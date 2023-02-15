/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Aggregates;

namespace CoOp.Domain
{
    public class ImmovablesEvent : AggregateEvent<CoOpAR, CoOpId>
    {
        public ImmovablesId  ImmovablesId { get; set; }
        public int ContractNo { get; set; }
        public DateTime Date { get; set; }
        
        public static T New<T>(Immovables i)
            where T : ImmovablesEvent, new()
        {
            return new T
            {
                ImmovablesId = i.Id,
                ContractNo = i.ContractNo,
                Date = DateTime.Now
            };
        }
    }
}