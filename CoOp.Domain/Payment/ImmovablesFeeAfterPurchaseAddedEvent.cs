/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace CoOp.Domain
{
    public class ImmovablesFeeAfterPurchaseAddedEvent : ImmovablesFeeAddedEvent
    {
        /// <summary>
        /// Fund to pay off Immovables inflation debt
        /// </summary>
        public double InflationFundPart { get; set; }
        public double CoopFundPart { get; set; }
        public double ImmovablesFundPart { get; set; }
    }
}