﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class HandshakeCliMsg : CliMsgBase<HandshakeBaseMsgData>
    {
        /// <inheritdoc />
        internal HandshakeCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)HandshakeMessageType.Request] = typeof(HandshakeRequestMsgData),
            [(ushort)HandshakeMessageType.Response] = typeof(HandshakeResponseMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Handshake;
        
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}