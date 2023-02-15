/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace CoOp.Domain
{
    public class ImmovablesFeeBeforePurchase : ImmovablesFee, IAccumulator
    {
        public ImmovablesFeeBeforePurchase(ImmovablesFeeEvent e) : base(e)
        {
        }
        
        public ImmovablesFeeDerivative EntranceFee { get; set; }
        public ImmovablesFeeDerivative InflationDebt { get; set; }
        public ImmovablesFeeDerivative ImmovablesFundPart { get; set; }
        public double Accumulated => 
            (InflationDebt?.Accumulated ?? 0) + (ImmovablesFundPart?.Accumulated ?? 0);

        public string AccumulatedFormatted => Accumulated.ToString("C2");
    }
}