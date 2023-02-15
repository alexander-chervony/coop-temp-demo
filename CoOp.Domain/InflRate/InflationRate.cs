/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Entities;

namespace CoOp.Domain
{
    /// <summary>
    /// Yearly inflation rate.
    /// </summary>
    public class InflationRate : Entity<InflationRateId>
    {
        public double Value { get; }
        // todo: only year and month number matters
        public DateTime EffectiveFrom { get; }
        public string Currency { get; }
        
        public InflationRate(InflationRateEvent e) : base(e.InflationRateId)
        {
            Value = e.Value;
            EffectiveFrom = e.EffectiveFrom;
            Currency = e.Currency;
        }
    }
}