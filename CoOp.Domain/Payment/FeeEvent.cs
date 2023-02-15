/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Aggregates;

namespace CoOp.Domain
{
    public class FeeEvent : AggregateEvent<CoOpAR, CoOpId>
    {
        public string PaymentId1C { get; set; }
        public PaymentId PaymentId { get; set; } // - todo: do we need these additional ids? or maybe only source
        public int ContractNo { get; set; }
        public ImmovablesId ImmovablesId { get; set; } // - todo: do we need these additional ids? or maybe only source
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        
        public static T Copy<T>(FeeEvent e, double amount)
            where T : FeeEvent, new()
        {
            return new T
            {
                PaymentId1C = e.PaymentId1C,
                PaymentId = e.PaymentId,
                ImmovablesId = e.ImmovablesId,
                ContractNo = e.ContractNo,
                PaymentDate = e.PaymentDate,
                Amount = amount,
                Currency = e.Currency
            };
        }
        
        public static T FromFeeCommand<T>(ImmovablesId immovablesId, FeeCommand cmd)
            where T : FeeEvent, new()
        {
            return new T
            {
                PaymentId1C = cmd.PaymentId1C,
                PaymentId = PaymentId.NewDeterministicFromId1C(cmd.PaymentId1C),
                ImmovablesId = immovablesId,
                ContractNo = cmd.ContractNo,
                PaymentDate = cmd.PaymentDate,
                Amount = cmd.Amount,
                Currency = cmd.Currency
            };
        }
                
        public static T FromImmovablesFeeCommand<T>(ImmovablesId immovablesId, ImmovablesFeeCommand cmd)
            where T : FeeEvent, new()
        {
            var e = FromFeeCommand<T>(immovablesId, cmd);
            return e;
        }

    }
}