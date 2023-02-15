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
    public class ImmovablesId : Identity<ImmovablesId>
    {
        private static readonly Guid Namespace = Guid.Parse("B7FFE9A9-6CBF-4F09-B40B-11C3EFA7BBBB");

        /// <summary>
        /// Immovables id is created from both coop id, for example "BlrRealty" and contract no,
        /// so it will almost guarantee that there will be no accidental corruption of data in this coop
        /// by data from another country or environment coop.
        /// It also allows to store different coops in same DB (and there will be no overlap if the key read or write model snapshot will be contractNo)
        /// </summary>
        public static ImmovablesId NewDeterministic(
            CoOpId coOpId,
            int contractNo) => NewDeterministic(Namespace, $"{coOpId}__{contractNo}");

        public ImmovablesId(string value) : base(value)
        {
        }
    }
}