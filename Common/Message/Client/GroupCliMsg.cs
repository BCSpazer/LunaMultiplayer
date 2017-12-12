﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class GroupCliMsg : CliMsgBase<GroupBaseMsgData>
    {
        /// <inheritdoc />
        internal GroupCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)GroupMessageType.ListRequest] = typeof(GroupListRequestMsgData),
            [(ushort)GroupMessageType.ListResponse] = typeof(GroupListResponseMsgData),
            [(ushort)GroupMessageType.CreateGroup] = typeof(GroupCreateMsgData),
            [(ushort)GroupMessageType.RemoveGroup] = typeof(GroupRemoveMsgData),
            [(ushort)GroupMessageType.GroupUpdate] = typeof(GroupUpdateMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Groups;
        
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}