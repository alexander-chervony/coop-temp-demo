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
    public class MemberId : Identity<MemberId>
    {
        private static readonly Guid Namespace = Guid.Parse("82c39079-45a9-4060-986c-beaf6b885adb");

        public static MemberId NewDeterministic(string value) => NewDeterministic(Namespace, value);

        public MemberId(string value) : base(value) { }
    }
}