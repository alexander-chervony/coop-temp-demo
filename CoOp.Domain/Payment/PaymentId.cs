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
    public class PaymentId : Identity<PaymentId>
    {
        private static readonly Guid Namespace = Guid.Parse("121abc51-9038-4aee-91c1-22a1fd0d0997");

        public static PaymentId NewDeterministicFromId1C(string value) => NewDeterministic(Namespace, value);
        
        public PaymentId(string value) : base(value)
        {
        }
    }
}