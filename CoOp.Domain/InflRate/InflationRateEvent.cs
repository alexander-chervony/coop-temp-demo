/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Aggregates;

namespace CoOp.Domain
{
    public class InflationRateEvent : AggregateEvent<CoOpAR, CoOpId>
    {
        public InflationRateId InflationRateId { get; set; }
        public double Value { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public string Currency { get; set; }

        public static T From<T>(InflationRateCommand cmd)
            where T : InflationRateEvent, new()
        {
            return new T
            {
                InflationRateId = InflationRateId.New,
                Value = cmd.Value,
                EffectiveFrom = cmd.EffectiveFrom,
                Currency = cmd.Currency
            };
        }
    }
}