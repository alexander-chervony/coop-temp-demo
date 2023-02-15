/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class InflationRateCommand : Command<CoOpAR, CoOpId>
    {
        public double Value { get; }
        public DateTime EffectiveFrom { get; }
        public string Currency { get; }

        protected InflationRateCommand(
            CoOpId id,
            double value,
            DateTime effectiveFrom,
            string currency)
            : base(id)
        {
            if (value <= 0)
                throw new ArgumentNullException(nameof(value));
            if (effectiveFrom == default)
                throw new ArgumentNullException(nameof(effectiveFrom));
            if (string.IsNullOrEmpty(currency))
                throw new ArgumentNullException(nameof(currency));
            Value = value;
            EffectiveFrom = effectiveFrom;
            Currency = currency;
        }
    }
}