﻿using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageSender : SubSystem<CraftLibrarySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<CraftLibraryCliMsg>(msg)));
        }
    }
}