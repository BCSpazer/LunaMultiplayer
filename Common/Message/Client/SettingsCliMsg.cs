﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class SettingsCliMsg : CliMsgBase<SettingsBaseMsgData>
    {
        /// <inheritdoc />
        internal SettingsCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)SettingsMessageType.Request] = typeof(SettingsRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Settings;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}
