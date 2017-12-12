﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class ScenarioSrvMsg : SrvMsgBase<ScenarioBaseMsgData>
    {
        /// <inheritdoc />
        internal ScenarioSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ScenarioMessageType.Data] = typeof(ScenarioDataMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Scenario;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}