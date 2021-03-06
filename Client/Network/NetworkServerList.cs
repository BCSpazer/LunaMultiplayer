﻿using Lidgren.Network;
using LunaClient.Systems.Ping;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using UniLinq;
using Random = System.Random;

namespace LunaClient.Network
{
    public class NetworkServerList
    {
        public static string Password { get; set; } = string.Empty;

        private static readonly List<IPEndPoint> PrivMasterServers = new List<IPEndPoint>();
        public static List<IPEndPoint> MasterServers
        {
            get
            {
                lock (PrivMasterServers)
                {
                    if (!PrivMasterServers.Any())
                    {
                        var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
                        PrivMasterServers.AddRange(servers.Select(Common.CreateEndpointFromString));
                    }
                    return PrivMasterServers;
                }
            }
        }

        public static ConcurrentDictionary<string, ServerInfo> Servers { get; } = new ConcurrentDictionary<string, ServerInfo>();
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// Sends a request servers to the master servers
        /// </summary>
        public static void RequestServers()
        {
            Servers.Clear();
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsRequestServersMsgData>();
            var requestMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);
            NetworkSender.QueueOutgoingMessage(requestMsg);
        }

        /// <summary>
        /// Handles a server list response from the master servers
        /// </summary>
        public static void HandleServersList(NetIncomingMessage msg)
        {
            try
            {
                var msgDeserialized = NetworkMain.MstSrvMsgFactory.Deserialize(msg, LunaTime.UtcNow.Ticks);
                
                //Sometimes we receive other type of unconnected messages. 
                //Therefore we assert that the received message data is of MsReplyServersMsgData
                if (msgDeserialized.Data is MsReplyServersMsgData data)
                {
                    //Filter servers with diferent version
                    if (data.ServerVersion != LmpVersioning.CurrentVersion)
                        return;

                    if (!Servers.ContainsKey(data.ExternalEndpoint))
                    {
                        var server = new ServerInfo
                        {
                            Id = data.Id,
                            InternalEndpoint = data.InternalEndpoint,
                            ExternalEndpoint = data.ExternalEndpoint,
                            Description = data.Description,
                            Password = data.Password,
                            Cheats = data.Cheats,
                            ServerName = data.ServerName,
                            DropControlOnExit = data.DropControlOnExit,
                            MaxPlayers = data.MaxPlayers,
                            WarpMode = data.WarpMode,
                            TerrainQuality = data.TerrainQuality,
                            PlayerCount = data.PlayerCount,
                            GameMode = data.GameMode,
                            ModControl = data.ModControl,
                            DropControlOnExitFlight = data.DropControlOnExitFlight,
                            VesselUpdatesSendMsInterval = data.VesselUpdatesSendMsInterval,
                            DropControlOnVesselSwitching = data.DropControlOnVesselSwitching,
                            ServerVersion = data.ServerVersion
                        };

                        if (Servers.TryAdd(data.ExternalEndpoint, server))
                            PingSystem.QueuePing(data.ExternalEndpoint);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Invalid server list reply msg: {e}");
            }
        }

        /// <summary>
        /// Send a request to the master server to introduce us and do the nat punchtrough to the selected server
        /// </summary>
        public static void IntroduceToServer(long currentEntryId)
        {
            try
            {
                var token = RandomString(10);
                var ownEndpoint = new IPEndPoint(LunaNetUtils.GetMyAddress(), NetworkMain.Config.Port);

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsIntroductionMsgData>();
                msgData.Id = currentEntryId;
                msgData.Token = token;
                msgData.InternalEndpoint = Common.StringFromEndpoint(ownEndpoint);

                var introduceMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);

                LunaLog.Log($"[LMP]: Sending NAT introduction to server. Token: {token}");
                NetworkSender.QueueOutgoingMessage(introduceMsg);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error connecting to server: {e}");
            }
        }

        /// <summary>
        /// We received a nat punchtrough response so connect to the server
        /// </summary>
        public static void HandleNatIntroduction(NetIncomingMessage msg)
        {
            try
            {
                var token = msg.ReadString();
                LunaLog.Log($"[LMP]: Nat introduction success to {msg.SenderEndPoint} token is: {token}");
                NetworkConnection.ConnectToServer(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port, Password);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error handling NAT introduction: {e}");
            }
        }

        /// <summary>
        /// Generates a random string, usefull for token
        /// </summary>
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}
