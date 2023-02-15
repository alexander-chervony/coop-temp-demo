/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace CoOp.Domain
{
    public class ImmovablesFeeBeforePurchaseAddedEvent : ImmovablesFeeAddedEvent
    {
        // it will probably go to CoopFundPart but should still be called entrance fee to avoid ambiguity
        public double EntranceFee { get; set; }
        /// <summary>
        /// Immovables inflation debt (negative value that should be paid off from purchased member payments)
        /// This and only this value (not PartToInflationFund should go to Accumulated/totals since there will be NO
        /// PartToInflationFund in the beginning of coop functioning)
        /// </summary>
        public double InflationDebt { get; set; }
        public double ImmovablesFundPart { get; set; }
    }
}