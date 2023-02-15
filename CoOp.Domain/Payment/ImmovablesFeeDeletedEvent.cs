/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using EventFlow.Aggregates;

namespace CoOp.Domain
{
    public class ImmovablesFeeDeletedEvent : AggregateEvent<CoOpAR, CoOpId>
    {
        public string PaymentId1C { get; set; }
        public PaymentId PaymentId { get; set; }
        public int ContractNo { get; set; }
        public ImmovablesId ImmovablesId { get; set; }
        
        public static ImmovablesFeeDeletedEvent FromCommand(ImmovablesId immovablesId, DeleteImmovablesFeeCommand cmd)
        {
            return new ImmovablesFeeDeletedEvent
            {
                PaymentId1C = cmd.PaymentId1C,
                PaymentId = PaymentId.NewDeterministicFromId1C(cmd.PaymentId1C),
                ImmovablesId = immovablesId,
                ContractNo = cmd.ContractNo
            };
        }

    }
}