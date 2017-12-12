﻿using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Facility;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class FacilityCliMsg : CliMsgBase<FacilityBaseMsgData>
    {
        /// <inheritdoc />
        internal FacilityCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FacilityMessageType.Collapse] = typeof(FacilityCollapseMsgData),
            [(ushort)FacilityMessageType.Repair] = typeof(FacilityRepairMsgData),
            [(ushort)FacilityMessageType.Upgrade] = typeof(FacilityUpgradeMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Facility;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}