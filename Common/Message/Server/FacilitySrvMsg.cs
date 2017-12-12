﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Facility;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class FacilitySrvMsg : SrvMsgBase<FacilityBaseMsgData>
    {
        /// <inheritdoc />
        internal FacilitySrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FacilityMessageType.Collapse] = typeof(FacilityCollapseMsgData),
            [(ushort)FacilityMessageType.Repair] = typeof(FacilityRepairMsgData),
            [(ushort)FacilityMessageType.Upgrade] = typeof(FacilityUpgradeMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.Facility;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}