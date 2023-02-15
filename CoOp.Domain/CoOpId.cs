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
    public class CoOpId : Identity<CoOpId>
    {
        private static readonly Guid Namespace = Guid.Parse("3c0afe50-d7e5-4121-b67e-d9ea85b4fb5d");

        // todo: to coop initialization command, different names for dev/ prod
        public static CoOpId BlrRealty => NewDeterministic(Namespace, "BlrRealty");
        
        public CoOpId(string value) : base(value) { }
    }
}