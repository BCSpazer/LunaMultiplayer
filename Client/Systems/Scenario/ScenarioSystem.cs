using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon;
using FinePrint.Utilities;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.Scenario
{
    public class ScenarioSystem : MessageSystem<ScenarioSystem, ScenarioMessageSender, ScenarioMessageHandler>
    {
        public override void Update()
        {
            base.Update();
            if (!Enabled) return;

            if (DateTime.UtcNow.Ticks - LastSendTime > TimeSpan.FromSeconds(SettingsSystem.ServerSettings.SendScenarioDataSecInterval).Ticks)
            {
                LastSendTime = DateTime.UtcNow.Ticks;
                SendScenarioModules();
            }
        }

        #region Fields

        private Dictionary<string, string> CheckData { get; } = new Dictionary<string, string>();
        public Queue<ScenarioEntry> ScenarioQueue { get; } = new Queue<ScenarioEntry>();
        private long LastSendTime { get; set; }
        private Dictionary<string, Type> AllScenarioTypesInAssemblies { get; } = new Dictionary<string, Type>();
        
        #endregion

        #region Public methods
        
        public void LoadMissingScenarioDataIntoGame()
        {
            var validScenarios = KSPScenarioType.GetAllScenarioTypesInAssemblies()
                .Where(s => 
                    !HighLogic.CurrentGame.scenarios.Exists(psm => psm.moduleName == s.ModuleType.Name) && 
                    LoadModuleByGameMode(s));

            foreach (var validScenario in validScenarios)
            {
                Debug.Log("Creating new scenario module " + validScenario.ModuleType.Name);
                HighLogic.CurrentGame.AddProtoScenarioModule(validScenario.ModuleType,
                    validScenario.ScenarioAttributes.TargetScenes);
            }
        }

        /// <summary>
        /// Check if the scenario has changed and sends it to the server
        /// </summary>
        public void SendScenarioModules()
        {
            var scenarioName = new List<string>();
            var scenarioData = new List<byte[]>();

            foreach (var scenarioModule in ScenarioRunner.GetLoadedModules())
            {
                var scenarioType = scenarioModule.GetType().Name;

                if (!IsScenarioModuleAllowed(scenarioType))
                    continue;

                var scenarioNode = new ConfigNode();
                scenarioModule.Save(scenarioNode);

                var scenarioBytes = ConfigNodeSerializer.Singleton.Serialize(scenarioNode);
                var scenarioHash = Common.CalculateSha256Hash(scenarioBytes);

                if (scenarioBytes.Length == 0)
                {
                    Debug.Log("Error writing scenario data for " + scenarioType);
                    continue;
                }

                //Data is the same since last time - Skip it.
                if (CheckData.ContainsKey(scenarioType) && (CheckData[scenarioType] == scenarioHash)) continue;

                CheckData[scenarioType] = scenarioHash;

                scenarioName.Add(scenarioType);
                scenarioData.Add(scenarioBytes);
            }

            if (scenarioName.Any())
                MessageSender.SendScenarioModuleData(scenarioName.ToArray(), scenarioData.ToArray());
        }

        public void LoadScenarioDataIntoGame()
        {
            while (ScenarioQueue.Count > 0)
            {
                var scenarioEntry = ScenarioQueue.Dequeue();
                if (scenarioEntry.ScenarioName == "ContractSystem")
                {
                    SpawnStrandedKerbalsForRescueMissions(scenarioEntry.ScenarioNode);
                    CreateMissingTourists(scenarioEntry.ScenarioNode);
                }
                if (scenarioEntry.ScenarioName == "ProgressTracking")
                    CreateMissingKerbalsInProgressTrackingSoTheGameDoesntBugOut(scenarioEntry.ScenarioNode);

                CheckForBlankSceneSoTheGameDoesntBugOut(scenarioEntry);

                var psm = new ProtoScenarioModule(scenarioEntry.ScenarioNode);
                if (IsScenarioModuleAllowed(psm.moduleName))
                {
                    Debug.Log($"Loading {psm.moduleName} scenario data");
                    HighLogic.CurrentGame.scenarios.Add(psm);
                }
                else
                {
                    Debug.Log($"Skipping {psm.moduleName} scenario data in {SettingsSystem.ServerSettings.GameMode} mode");
                }
            }
        }

        public void UpgradeTheAstronautComplexSoTheGameDoesntBugOut()
        {
            var sm = HighLogic.CurrentGame.scenarios.Find(psm => psm.moduleName == "ScenarioUpgradeableFacilities");
            if ((sm != null) && ScenarioUpgradeableFacilities.protoUpgradeables.ContainsKey("SpaceCenter/AstronautComplex"))
            {
                foreach (var uf in ScenarioUpgradeableFacilities.protoUpgradeables["SpaceCenter/AstronautComplex"].facilityRefs)
                {
                    Debug.Log("Setting astronaut complex to max level");
                    uf.SetLevel(uf.MaxLevel);
                }
            }
        }

        #endregion

        #region Private methods

        private static void CreateMissingTourists(ConfigNode contractSystemNode)
        {
            var contractsNode = contractSystemNode.GetNode("CONTRACTS");

            var kerbalNames = contractsNode.GetNodes("CONTRACT")
                .Where(c => (c.GetValue("type") == "TourismContract") && (c.GetValue("state") == "Active"))
                .SelectMany(c => c.GetNodes("PARAM"))
                .SelectMany(p => p.GetValues("kerbalName"));

            foreach (var kerbalName in kerbalNames)
            {
                Debug.Log($"Spawning missing tourist ({kerbalName}) for active tourism contract");
                var pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Tourist);
                pcm.ChangeName(kerbalName);
            }
        }

        //Defends against bug #172
        private static void SpawnStrandedKerbalsForRescueMissions(ConfigNode contractSystemNode)
        {
            var rescueContracts = contractSystemNode.GetNode("CONTRACTS").GetNodes("CONTRACT").Where(c => c.GetValue("type") == "RescueKerbal");
            foreach (var contractNode in rescueContracts)
            {
                if (contractNode.GetValue("state") == "Offered")
                {
                    var kerbalName = contractNode.GetValue("kerbalName");
                    if (!HighLogic.CurrentGame.CrewRoster.Exists(kerbalName))
                    {
                        Debug.Log("Spawning missing kerbal (" + kerbalName + ") for offered KerbalRescue contract");
                        var pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Unowned);
                        pcm.ChangeName(kerbalName);
                    }
                }
                if (contractNode.GetValue("state") == "Active")
                {
                    var kerbalName = contractNode.GetValue("kerbalName");
                    Debug.Log("Spawning stranded kerbal (" + kerbalName + ") for active KerbalRescue contract");
                    var bodyId = int.Parse(contractNode.GetValue("body"));
                    if (!HighLogic.CurrentGame.CrewRoster.Exists(kerbalName))
                        GenerateStrandedKerbal(bodyId, kerbalName);
                }
            }
        }

        private static void GenerateStrandedKerbal(int bodyId, string kerbalName)
        {
            //Add kerbal to crew roster.
            Debug.Log("Spawning missing kerbal, Name: " + kerbalName);

            var pcm = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Unowned);
            pcm.ChangeName(kerbalName);
            pcm.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

            //Create protovessel
            var newPartId = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
            var contractBody = FlightGlobals.Bodies[bodyId];

            //Atmo: 10km above atmo, to half the planets radius out.
            //Non-atmo: 30km above ground, to half the planets radius out.
            var minAltitude = CelestialUtilities.GetMinimumOrbitalDistance(contractBody, 1.1f);
            var maxAltitude = minAltitude + contractBody.Radius*0.5;

            var strandedOrbit = Orbit.CreateRandomOrbitAround(FlightGlobals.Bodies[bodyId], minAltitude, maxAltitude);

            var kerbalPartNode = new[] {ProtoVessel.CreatePartNode("kerbalEVA", newPartId, pcm)};

            var protoVesselNode = ProtoVessel.CreateVesselNode(kerbalName, VesselType.EVA, strandedOrbit, 0,
                kerbalPartNode);
            var discoveryNode = ProtoVessel.CreateDiscoveryNode(DiscoveryLevels.Unowned, UntrackedObjectClass.A,
                double.PositiveInfinity,
                double.PositiveInfinity);

            var protoVessel = new ProtoVessel(protoVesselNode, HighLogic.CurrentGame)
            {
                discoveryInfo = discoveryNode
            };

            //It's not supposed to be infinite, but you're crazy if you think I'm going to decipher the values field of the rescue node.
            HighLogic.CurrentGame.flightState.protoVessels.Add(protoVessel);
        }

        //Defends against bug #172
        private void CreateMissingKerbalsInProgressTrackingSoTheGameDoesntBugOut(ConfigNode progressTrackingNode)
        {
            foreach (ConfigNode possibleNode in progressTrackingNode.nodes)
                CreateMissingKerbalsInProgressTrackingSoTheGameDoesntBugOut(possibleNode);

            //The kerbals are kept in a ConfigNode named 'crew', with 'crews' as a comma space delimited array of names.
            if (progressTrackingNode.name == "crew")
            {
                var kerbalNames = progressTrackingNode.GetValue("crews");
                if (!string.IsNullOrEmpty(kerbalNames))
                {
                    var kerbalNamesSplit = kerbalNames.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var kerbalName in kerbalNamesSplit.Where(k => !HighLogic.CurrentGame.CrewRoster.Exists(k)))
                    {
                        Debug.Log("Generating missing kerbal from ProgressTracking: " + kerbalName);
                        var pcm = CrewGenerator.RandomCrewMemberPrototype();
                        pcm.ChangeName(kerbalName);
                        HighLogic.CurrentGame.CrewRoster.AddCrewMember(pcm);

                        //Also send it off to the server
                        KerbalSystem.Singleton.MessageSender.SendKerbalIfDifferent(pcm);
                    }
                }
            }
        }

        //If the scene field is blank, KSP will throw an error while starting the game, meaning players will be unable to join the server.
        private static void CheckForBlankSceneSoTheGameDoesntBugOut(ScenarioEntry scenarioEntry)
        {
            if (scenarioEntry.ScenarioNode.GetValue("scene") == string.Empty)
            {
                var nodeName = scenarioEntry.ScenarioName;
                ScreenMessages.PostScreenMessage(nodeName + " is badly behaved!");
                Debug.Log(nodeName + " is badly behaved!");
                scenarioEntry.ScenarioNode.SetValue("scene", "7, 8, 5, 6, 9");
            }
        }

        private static bool LoadModuleByGameMode(KSPScenarioType validScenario)
        {
            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.CAREER:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewCareerGames);
                case Game.Modes.SCIENCE_SANDBOX:
                    return
                        validScenario.ScenarioAttributes.HasCreateOption(
                            ScenarioCreationOptions.AddToNewScienceSandboxGames);
                case Game.Modes.SANDBOX:
                    return validScenario.ScenarioAttributes.HasCreateOption(ScenarioCreationOptions.AddToNewSandboxGames);
            }
            return false;
        }

        private void LoadScenarioTypes()
        {
            AllScenarioTypesInAssemblies.Clear();

            var scenarioTypes = AssemblyLoader.loadedAssemblies
                .SelectMany(a => a.assembly.GetTypes())
                .Where(s => s.IsSubclassOf(typeof(ScenarioModule)) && !AllScenarioTypesInAssemblies.ContainsKey(s.Name));

            foreach (var scenarioType in scenarioTypes)
                AllScenarioTypesInAssemblies.Add(scenarioType.Name, scenarioType);
        }

        private bool IsScenarioModuleAllowed(string scenarioName)
        {
            //Blacklist asteroid module from every game mode
            //We hijack this and enable / disable it if we need to.
            if (string.IsNullOrEmpty(scenarioName) || (scenarioName == "ScenarioDiscoverableObjects")) return false;

            if (!AllScenarioTypesInAssemblies.Any()) LoadScenarioTypes(); //Load type dictionary on first use

            if (!AllScenarioTypesInAssemblies.ContainsKey(scenarioName)) return false; //Module missing

            var scenarioType = AllScenarioTypesInAssemblies[scenarioName];

            var scenarioAttributes = (KSPScenario[]) scenarioType.GetCustomAttributes(typeof(KSPScenario), true);
            if (scenarioAttributes.Length > 0)
            {
                var attribute = scenarioAttributes[0];
                var protoAllowed = false;
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    protoAllowed = attribute.HasCreateOption(ScenarioCreationOptions.AddToExistingCareerGames);
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToNewCareerGames);
                }
                if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                {
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToExistingScienceSandboxGames);
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToNewScienceSandboxGames);
                }
                if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
                {
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToExistingSandboxGames);
                    protoAllowed |= attribute.HasCreateOption(ScenarioCreationOptions.AddToNewSandboxGames);
                }
                return protoAllowed;
            }

            //Scenario is not marked with KSPScenario - let's load it anyway.
            return true;
        }

        #endregion
    }
}