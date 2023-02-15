/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace CoOp.Domain
{
    public class ImmovablesFeeAfterPurchase : ImmovablesFee, IAccumulator
    {
        public ImmovablesFeeAfterPurchase(ImmovablesFeeEvent e) : base(e)
        {
        }
        public ImmovablesFeeDerivative InflationFundPart { get; set; }
        public ImmovablesFeeDerivative CoopFundPart { get; set; }
        public ImmovablesFeeDerivative ImmovablesFundPart { get; set; }
        public double Accumulated => (ImmovablesFundPart?.Accumulated ?? 0)
                                     // actually these two supposed to be 0, but as we now maintain logic of Accumulated
                                     // inside ImmovablesFeeDerivative.accumulated than this is a must
                                     + (InflationFundPart?.Accumulated ?? 0)
                                     + (CoopFundPart?.Accumulated ?? 0);
        public string AccumulatedFormatted => Accumulated.ToString("C2");
    }
}