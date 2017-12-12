﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class HandshakeSrvMsg : SrvMsgBase<HandshakeBaseMsgData>
    {
        /// <inheritdoc />
        internal HandshakeSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)HandshakeMessageType.Challenge] = typeof(HandshakeChallengeMsgData),
            [(ushort)HandshakeMessageType.Reply] = typeof(HandshakeReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Handshake;

        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}