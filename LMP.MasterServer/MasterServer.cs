﻿using LiteNetLib;
using LiteNetLib.Utils;
using LmpGlobal;
using LunaCommon;
using LunaCommon.Message;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using LunaCommon.Message.Types;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleLogger = LunaCommon.ConsoleLogger;
using LogLevels = LunaCommon.LogLevels;

namespace LMP.MasterServer
{
    public class MasterServer
    {
        public static int ServerMsTick { get; set; } = 100;
        public static int ServerMsTimeout { get; set; } = 15000;
        public static int ServerRemoveMsCheckInterval { get; set; } = 5000;
        public static ushort Port { get; set; } = 8700;
        public static bool RunServer { get; set; }
        public static ConcurrentDictionary<long, Server> ServerDictionary { get; } = new ConcurrentDictionary<long, Server>();
        private static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();

        private static NetManager Server { get; set; }

        public static async void Start()
        {
            var listener = new EventBasedNetListener();
            listener.NetworkReceiveEvent += ListenerOnNetworkReceiveEvent;
            var natPunchListener = new EventBasedNatPunchListener();
            Server = new NetManager(listener, 500, "masterserver")
            {
                UnconnectedMessagesEnabled = true,
                DisconnectTimeout = ServerMsTimeout,
                PingInterval = 500,
                NatPunchEnabled = true,
            };
            Server.Start();
            Server.NatPunchModule.Init(natPunchListener);
            CheckMasterServerListed();

            ConsoleLogger.Log(LogLevels.Normal, $"Master server {LmpVersioning.CurrentVersion} started! Поехали!");
            RemoveExpiredServers();


            while (RunServer)
            {
                Server.PollEvents();
                await Task.Delay(ServerMsTick);
            }

            Server.Stop();
        }
        
        private static void ListenerOnNetworkReceiveEvent(NetPeer peer, NetDataReader netDataReader)
        {
            if (FloodControl.AllowRequest(peer.EndPoint.Host))
            {
                var message = GetMessage(netDataReader.Data);
                if (message != null)
                    HandleMessage(message, peer);
            }
        }

        private static IMasterServerMessageBase GetMessage(byte[] messageBytes)
        {
            try
            {
                var message = MasterServerMessageFactory.Deserialize(messageBytes, LunaTime.UtcNow.Ticks) as IMasterServerMessageBase;
                return message;
            }
            catch (Exception e)
            {
                ConsoleLogger.Log(LogLevels.Error, $"Error deserializing message! :{e}");
                return null;
            }
        }

        private static void CheckMasterServerListed()
        {
            var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
            var ownEndpoint = $"{GetOwnIpAddress()}:{Port}";

            if(!servers.Contains(ownEndpoint))
            {
                ConsoleLogger.Log(LogLevels.Error, $"You're not in the master-servers URL ({RepoConstants.MasterServersListShortUrl}) " +
                    "Clients/Servers won't see you");
            }
            else
            {
                ConsoleLogger.Log(LogLevels.Normal, "Own ip correctly listed in master - servers URL");
            }
        }

        private static string GetOwnIpAddress()
        {
            var currentIpAddress = TryGetIpAddress("http://ip.42.pl/raw");

            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("https://api.ipify.org/");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://httpbin.org/ip");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://checkip.dyndns.org");

            return currentIpAddress;
        }

        private static string TryGetIpAddress(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream))
                {
                    var ipRegEx = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                    var result = ipRegEx.Matches(reader.ReadToEnd());

                    if (IPAddress.TryParse(result[0].Value, out var ip))
                        return ip.ToString();
                }
            }
            return null;
        }

        private static void HandleMessage(IMasterServerMessageBase message, NetPeer peer)
        {
            switch ((message?.Data as MsBaseMsgData)?.MasterServerMessageSubType)
            {
                case MasterServerMessageSubType.RegisterServer:
                    RegisterServer(message, peer.GetEndpoint());
                    break;
                case MasterServerMessageSubType.RequestServers:
                    var version = ((MsRequestServersMsgData)message.Data).CurrentVersion;
                    ConsoleLogger.Log(LogLevels.Normal, $"LIST REQUEST from: {peer.GetEndpoint()} v: {version}");
                    SendServerLists(peer);
                    break;
                case MasterServerMessageSubType.Introduction:
                    var msgData = (MsIntroductionMsgData)message.Data;
                    if (ServerDictionary.TryGetValue(msgData.Id, out var server))
                    {
                        var clientInternalEndpoint = Common.CreateEndpointFromString(msgData.InternalEndpoint);

                        ConsoleLogger.Log(LogLevels.Normal, $"INTRODUCTION request from: {peer.GetEndpoint()} to server: {server.ExternalEndpoint}");
                        Server.NatPunchModule.NatIntroduce(
                            new NetEndPoint(server.InternalEndpoint.Address.ToString(), server.InternalEndpoint.Port),
                            new NetEndPoint(server.ExternalEndpoint.Address.ToString(), server.ExternalEndpoint.Port),
                            new NetEndPoint(clientInternalEndpoint.Address.ToString(), clientInternalEndpoint.Port),
                            peer.EndPoint,// client external
                            msgData.Token); // request token
                    }
                    else
                    {
                        ConsoleLogger.Log(LogLevels.Error, "Client requested introduction to nonlisted host!");
                    }
                    break;
            }
        }

        /// <summary>
        /// Return the list of servers that match the version specified
        /// </summary>
        private static void SendServerLists(NetPeer peer)
        {
            var values = ServerDictionary.Values.OrderBy(v => v.Info.Id).ToArray();

            var msgData = MasterServerMessageFactory.CreateNewMessageData<MsReplyServersMsgData>();

            msgData.Id = values.Select(s => s.Info.Id).ToArray();
            msgData.ServerVersion = values.Select(s => s.Info.ServerVersion).ToArray();
            msgData.Cheats = values.Select(s => s.Info.Cheats).ToArray();
            msgData.Description = values.Select(s => s.Info.Description).ToArray();
            msgData.DropControlOnExit = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.DropControlOnExitFlight = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.DropControlOnVesselSwitching = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.ExternalEndpoint = values.Select(s => $"{s.ExternalEndpoint.Address}:{s.ExternalEndpoint.Port}").ToArray();
            msgData.GameMode = values.Select(s => s.Info.GameMode).ToArray();
            msgData.InternalEndpoint = values.Select(s => $"{s.InternalEndpoint.Address}:{s.InternalEndpoint.Port}").ToArray();
            msgData.MaxPlayers = values.Select(s => s.Info.MaxPlayers).ToArray();
            msgData.ModControl = values.Select(s => s.Info.ModControl).ToArray();
            msgData.PlayerCount = values.Select(s => s.Info.PlayerCount).ToArray();
            msgData.ServerName = values.Select(s => s.Info.ServerName).ToArray();
            msgData.VesselUpdatesSendMsInterval = values.Select(s => s.Info.VesselUpdatesSendMsInterval).ToArray();
            msgData.WarpMode = values.Select(s => s.Info.WarpMode).ToArray();
            msgData.TerrainQuality = values.Select(s => s.Info.TerrainQuality).ToArray();

            var msg = MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
            var data = msg.Serialize(true);

            peer.Send(data, SendOptions.ReliableOrdered);
        }

        private static void RegisterServer(IMessageBase message, IPEndPoint endpoint)
        {
            var msgData = (MsRegisterServerMsgData)message.Data;

            if (!ServerDictionary.ContainsKey(msgData.Id))
            {
                ServerDictionary.TryAdd(msgData.Id, new Server(msgData, endpoint));
                ConsoleLogger.Log(LogLevels.Normal, $"NEW SERVER: {endpoint}");
            }
            else
            {
                //Just update
                ServerDictionary[msgData.Id] = new Server(msgData, endpoint);
            }
        }

        private static void RemoveExpiredServers()
        {
            Task.Run(async () =>
            {
                while (RunServer)
                {
                    var serversIdsToRemove = ServerDictionary
                        .Where(s => LunaTime.UtcNow.Ticks - s.Value.LastRegisterTime >
                                    TimeSpan.FromMilliseconds(ServerMsTimeout).Ticks)
                        .ToArray();

                    foreach (var serverId in serversIdsToRemove)
                    {
                        ConsoleLogger.Log(LogLevels.Normal, $"REMOVING SERVER: {serverId.Value.ExternalEndpoint}");
                        ServerDictionary.TryRemove(serverId.Key, out var _);
                    }

                    await Task.Delay(ServerRemoveMsCheckInterval);
                }
            });
        }
    }
}
