﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class KerbalSrvMsg : SrvMsgBase<KerbalBaseMsgData>
    {
        /// <inheritdoc />
        internal KerbalSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)KerbalMessageType.Reply] = typeof(KerbalReplyMsgData),
            [(ushort)KerbalMessageType.Proto] = typeof(KerbalProtoMsgData),
            [(ushort)KerbalMessageType.Remove] = typeof(KerbalRemoveMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Kerbal;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}