﻿using LunaCommon.Xml;
using Server.Context;
using Server.Log;
using Server.Settings;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// Here we keep a copy of all the player vessels in XML format and we also save them to files at a specified rate
    /// </summary>
    public class VesselStoreSystem
    {
        public static string VesselsFolder = Path.Combine(ServerContext.UniverseDirectory, "Vessels");

        public static ConcurrentDictionary<Guid, string> CurrentVesselsInXmlFormat = new ConcurrentDictionary<Guid, string>();

        private static readonly object BackupLock = new object();

        public static bool VesselExists(Guid vesselId) => CurrentVesselsInXmlFormat.ContainsKey(vesselId);

        /// <summary>
        /// Removes a vessel from the store
        /// </summary>
        public static void RemoveVessel(Guid vesselId)
        {
            CurrentVesselsInXmlFormat.TryRemove(vesselId, out _);

            Task.Run(() =>
            {
                lock (BackupLock)
                {
                    FileHandler.FileDelete(Path.Combine(VesselsFolder, $"{vesselId}.xml"));
                }
            });
        }

        /// <summary>
        /// Returns a XML vessel in the standard KSP format
        /// </summary>
        public static string GetVesselInConfigNodeFormat(Guid vesselId)
        {
            return CurrentVesselsInXmlFormat.TryGetValue(vesselId, out var vesselInXmlFormat) ?
                ConfigNodeXmlParser.ConvertToConfigNode(vesselInXmlFormat) : null;
        }

        /// <summary>
        /// Load the stored vessels into the dictionary
        /// </summary>
        public static void LoadExistingVessels()
        {
            lock (BackupLock)
            {
                foreach (var file in Directory.GetFiles(VesselsFolder).Where(f => Path.GetExtension(f) == ".xml"))
                {
                    if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var vesselId))
                    {
                        CurrentVesselsInXmlFormat.TryAdd(vesselId, FileHandler.ReadFileText(file));
                    }
                }
            }
        }

        /// <summary>
        /// This multithreaded function backups the vessels from the internal dictionary to a file at a specified interval
        /// </summary>
        public static void BackupVesselsThread()
        {
            while (ServerContext.ServerRunning)
            {
                BackupVessels();
                Thread.Sleep(GeneralSettings.SettingsStore.VesselsBackupIntervalMs);
            }

            //Do a last backup before quitting
            BackupVessels();
        }

        /// <summary>
        /// Actually performs the backup of the vessels to file
        /// </summary>
        private static void BackupVessels()
        {
            lock (BackupLock)
            {
                LunaLog.Debug("Backing up vessels to the disk...");
                var vesselsInXml = CurrentVesselsInXmlFormat.ToArray();
                foreach (var vessel in vesselsInXml)
                {
                    FileHandler.WriteToFile(Path.Combine(VesselsFolder, $"{vessel.Key}.xml"), vessel.Value);
                }
            }
        }
    }
}
