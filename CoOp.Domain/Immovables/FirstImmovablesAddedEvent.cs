/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Aggregates;

namespace CoOp.Domain
{
    public class FirstImmovablesAddedEvent : AggregateEvent<CoOpAR, CoOpId>
    {
        public FirstImmovablesAddedEvent(
            MemberId memberId, 
            string memberUId,
            ImmovablesId immovablesId, 
            int contractNo,
            DateTime contractDate, 
            int contractTermYears,
            double price, 
            double requiredEntranceFee, 
            string currency)
        {
            MemberId = memberId;
            MemberUId = memberUId;
            ImmovablesId = immovablesId;
            ContractNo = contractNo;
            ContractDate = contractDate;
            ContractTermYears = contractTermYears;
            Price = price.RoundToCents();
            RequiredEntranceFee = requiredEntranceFee.RoundToCents();
            Currency = currency;
        }

        public MemberId MemberId { get; set; }
        public string MemberUId { get; set; }
        public ImmovablesId ImmovablesId { get; set; }
        public int ContractNo { get; set; }
        public DateTime ContractDate { get; set; }
        public int ContractTermYears { get; set; }
        public double Price { get; set; }
        public double RequiredEntranceFee { get; set; }
        public string Currency { get; set; }
    }
}