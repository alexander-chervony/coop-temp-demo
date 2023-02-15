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
    public class ImmovablesFeeContainer
    {
        public double EntranceFee
            => FeesBeforePurchase.Values.Any()
                ? FeesBeforePurchase.Values.Select(_ => _.EntranceFee.Amount).Sum()
                : default;

        /// <summary>
        /// This is needed to decide if immovable still in active (EntranceFeePaid) state or not:
        /// consider the fact that entrance fee payment can be deleted (for some reason, for example wrong user assignment), but member should still be active if he made
        /// enough payments afterwards. See prop usage.
        /// </summary>
        public double EntranceFeeAccountablePaymentsAmount =>
            EntranceFee + FeesBeforePurchase.Accumulated;

        public FeeDictionary<ImmovablesFeeBeforePurchase> FeesBeforePurchase { get; } = new FeeDictionary<ImmovablesFeeBeforePurchase> ();
        public FeeDictionary<ImmovablesFeeAfterPurchase> FeesAfterPurchase { get; } = new FeeDictionary<ImmovablesFeeAfterPurchase> ();
        public double Accumulated => FeesBeforePurchase.Accumulated + FeesAfterPurchase.Accumulated;
        public DateTime FirstPaymentDate => FeesBeforePurchase.FirstPaymentDate;
        public DateTime LastPaymentDate => new[] {FeesBeforePurchase.LastPaymentDate, FeesAfterPurchase.LastPaymentDate}.Max();
        
        public double AddImmovablesFeeDerivative(ImmovablesFeeAddedEvent e)
        {
            switch (e)
            {
                case ImmovablesFeeBeforePurchaseAddedEvent @event:
                {
                    var o = new ImmovablesFeeBeforePurchase(e);
                    FeesBeforePurchase.AddImmovablesFee(o);
                    o.EntranceFee = new ImmovablesFeeDerivative(@event.EntranceFee, false);
                    o.InflationDebt = new ImmovablesFeeDerivative(@event.InflationDebt, true);
                    o.ImmovablesFundPart = new ImmovablesFeeDerivative(@event.ImmovablesFundPart, true);
                    return o.Accumulated;
                }
                case ImmovablesFeeAfterPurchaseAddedEvent @event:
                {
                    var o = new ImmovablesFeeAfterPurchase(e);
                    FeesAfterPurchase.AddImmovablesFee(o);
                    o.InflationFundPart = new ImmovablesFeeDerivative(@event.InflationFundPart, false);
                    o.CoopFundPart = new ImmovablesFeeDerivative(@event.CoopFundPart, false);
                    o.ImmovablesFundPart = new ImmovablesFeeDerivative(@event.ImmovablesFundPart, true);
                    return o.Accumulated;
                }
                default:
                    throw new NotImplementedException();
            }
        }
        
        public double DeleteImmovablesFee(PaymentId id)
        {
            if (FeesBeforePurchase.ContainsKey(id))
                return FeesBeforePurchase.DeleteImmovablesFee(id);
            if (FeesAfterPurchase.ContainsKey(id))
                return FeesAfterPurchase.DeleteImmovablesFee(id);
            throw new KeyNotFoundException($"Payment not found with id {id}");
        }
    }
}