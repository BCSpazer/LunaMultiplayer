﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class MotdCliMsg : CliMsgBase<MotdBaseMsgData>
    {
        /// <inheritdoc />
        internal MotdCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)MotdMessageType.Request] = typeof(MotdRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Motd;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}