﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class AdminCliMsg : CliMsgBase<AdminBaseMsgData>
    {
        /// <inheritdoc />
        internal AdminCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {        
            [(ushort)AdminMessageType.ListRequest] = typeof(AdminListRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Admin;
        
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}