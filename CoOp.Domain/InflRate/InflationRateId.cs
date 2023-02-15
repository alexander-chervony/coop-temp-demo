/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Core;
using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace CoOp.Domain
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class InflationRateId : Identity<InflationRateId>
    {
        //private static readonly Guid Namespace = Guid.Parse("159B01AB-F9E7-4564-BC11-FCF31AA01580");

        //public static InflationRateId NewDeterministic(string value) => 
        //    NewDeterministic(Namespace, value);

        public InflationRateId(string value) : base(value)
        {
        }
    }
}