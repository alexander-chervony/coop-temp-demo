/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;

namespace CoOp.Domain
{
    public static class ImmovablesFeeSplitter
    {
        public static ImmovablesFeeAddedEvent Split(
            Immovables immovables, 
            ImmovablesFeeCommand cmd, 
            InflationRate currentInflationRate)
        {
            var (inflationRateForNDays, daysFromLastPayment) = GetInflationRateForLastNDays(
                immovables.Fees.LastPaymentDate, 
                cmd.PaymentDate,
                currentInflationRate);
            
            if (immovables.State < ImmovablesState.Purchased)
            {
                var remaining = cmd.Amount;
                var @event = FeeEvent.FromImmovablesFeeCommand<ImmovablesFeeBeforePurchaseAddedEvent>(immovables.Id, cmd);

                var entranceFeeDebt = immovables.EntranceFeeDebt;
                if (entranceFeeDebt > 0)
                {
                    @event.EntranceFee = entranceFeeDebt > remaining ? remaining : entranceFeeDebt;
                    remaining -= entranceFeeDebt;
                }
                if (remaining > 0)
                    @event.ImmovablesFundPart = remaining;
                
                // inflation for savings should be payed for period no mater what is remaining value
                @event.InflationDebt = (immovables.Fees.Accumulated * inflationRateForNDays).RoundToCents();
                return @event;
            }
            else
            {
                var @event = FeeEvent.FromImmovablesFeeCommand<ImmovablesFeeAfterPurchaseAddedEvent>(immovables.Id, cmd);

                // todo: if not enough - create debt event for PartToInflation and for PartToCoopFund
                // events should add to inflationDebtAmount and coopFundDebtAmount

                // when processing payment - first pay off debt for PartToInflation and for PartToCoopFund
                // then proceed to payments as usual
                
                var remaining = cmd.Amount;
                
                var debtAmount = immovables.Price - immovables.Fees.Accumulated;
                
                // inflation for remaining debt should be payed for period
                var inflationAmount = (debtAmount * inflationRateForNDays).RoundToCents();
                if (inflationAmount > remaining)
                    throw new NotImplementedException();
                remaining -= inflationAmount;
                @event.InflationFundPart = inflationAmount;

                // to coop fund
                var yearlyMembershipRate = MembershipFeeRateCalc.CalculateFee(
                    immovables.AccumulationMonthBeforePurchase, 
                    immovables.ContractTermYears);

                var coopMembershipFeeAmount =
                    (debtAmount * GetRateForDays(daysFromLastPayment, yearlyMembershipRate)).RoundToCents();

                if (coopMembershipFeeAmount > remaining)
                    throw new NotImplementedException();
                remaining -= coopMembershipFeeAmount;
                @event.CoopFundPart = coopMembershipFeeAmount;
                
                // to immovables fund/ accumulation
                @event.ImmovablesFundPart = remaining;
                
                return @event;
            }
        }

        /// <summary>
        /// Period can span 2 inflation values - one option is to count period as if it consists of 2 parts.
        /// Take into account leap year? what if period is half-way leap/non-leap? - one option is to count period as if it consists of 2 parts.
        /// in general for both these cases - split period for the parts where any of the params differ: either inflation rate or days in year
        /// 
        /// Another option (currently chosen) is to take simplistic approach - whenever the payment is made -
        /// just take latest known inflation value ( for previous month and it will be ok since inflation itself is
        /// approximation. In fact, to make it more precise we would need to use inflation for realty/immovables rather
        /// than common one calculated for common consumer goods. This can be an enhancement in future)
        /// </summary>
        private static (double, double) GetInflationRateForLastNDays(
            DateTime previousPaymentDate,
            DateTime currentPaymentDate,
            InflationRate currentYearlyInflation)
        {
            if (previousPaymentDate == default)
                return (0, 0);
            
            // how to round days so no day will be lost? probably ok without rounding, so no days lost
            var daysFromLastPayment = (currentPaymentDate - previousPaymentDate).TotalDays;
            if (daysFromLastPayment <= 0)
                return (0, 0);
            
            return (GetRateForDays(daysFromLastPayment,currentYearlyInflation.Value), daysFromLastPayment);
        }

        private static double GetRateForDays(double daysFromLastPayment, double yearlyRate) => 
            (daysFromLastPayment / Time.DaysInYear) * yearlyRate;
    }
}