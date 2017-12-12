﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class FlagSrvMsg : SrvMsgBase<FlagBaseMsgData>
    {
        /// <inheritdoc />
        internal FlagSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FlagMessageType.ListRequest] = typeof(FlagListRequestMsgData),
            [(ushort)FlagMessageType.ListResponse] = typeof(FlagListResponseMsgData),
            [(ushort)FlagMessageType.FlagData] = typeof(FlagDataMsgData),
            [(ushort)FlagMessageType.FlagDelete] = typeof(FlagDeleteMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Flag;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}