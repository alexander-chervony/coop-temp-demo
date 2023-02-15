/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace CoOp.Domain
{
    public class FeeDictionary<TFee> : Dictionary<PaymentId, TFee>
        where TFee : ImmovablesFee, IAccumulator
    {
        public double Accumulated => Values.Any()
            ? Values.Select(_ => _.Accumulated).Sum()
            : default;
        
        public DateTime LastPaymentDate => Values.Any() 
            ? Values.Select(_ => _.PaymentDate).Max()
            : default;

        // accumulated filtering is needed to distinguish accumulation from entrance fee, for example for AccumulationMonthBeforePurchase
        public DateTime FirstPaymentDate => Values.Any(_ => _.Accumulated > 0)
            ? Values.Where(_ => _.Accumulated > 0).Select(_ => _.PaymentDate).Min()
            : default;
        
        // adds container for further derivative assignments
        public void AddImmovablesFee(TFee fee)
        {
            Add(fee.Id, fee);
        }

        public double DeleteImmovablesFee(PaymentId id)
        {
            var toRemove = this[id];
            Remove(id);
            return -toRemove.Accumulated;
        }
    }
}