﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class SetingsSrvMsg : SrvMsgBase<SettingsBaseMsgData>
    {
        /// <inheritdoc />
        internal SetingsSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)SettingsMessageType.Reply] = typeof(SettingsReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Settings;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}