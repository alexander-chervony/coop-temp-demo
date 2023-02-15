/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CoOp.Domain.Tests
{
    // get payment size
    public class FeeCalculator
    {
        // public IEnumerable<> GetFeeAmounts
        
        // there is a need to know current payment amount. The amount should be calculatable from:
        // 1. Price
        // 2. Contract term 
        // So Payment before purchase is just Price/Contract term in month - ImmovablesFundPaymentSize
        // After purchase its ImmovablesFundPaymentSize + (Inflation and CoopFundPayment from remaining debt)
        
        // Number of month remaining? But what if paying faster? Remaining term in month decreases, payment amount should stay the same;
        // So there should be OverpaidMonth field (
        //     Positive if pays faster, 0 otherwise
        // )
        // For Debt there will be TotalDebtMonth but how to calc?
        
        // Lets find out if other users payments will be enough to UP the price
        // so there will be no need in increasing the users payment
    }
    
    [TestFixture]
    public class FeeCalculatorTest
    {
        //double _inflationRateYearly = 3;
        //double _inflationRateYearly = 1;
        //double _inflationRateYearly = 0.5;
        double _inflationRateYearly = 0.05;
        
        [Test]
        public void Show_PrePurchasePayments_When_OneThirdAccumulation_And_ConstPayments()
        {
            Show_PrePurchasePayments_When_OneThirdAccumulation_And_ConstPayments(108000d, 6, _inflationRateYearly);
            Show_PrePurchasePayments_When_OneThirdAccumulation_And_ConstPayments(108000d, 9, _inflationRateYearly);
            Show_PrePurchasePayments_When_OneThirdAccumulation_And_ConstPayments(108000d, 12, _inflationRateYearly);
            Show_PrePurchasePayments_When_OneThirdAccumulation_And_ConstPayments(108000d, 15, _inflationRateYearly);
        }
        
        [Test]
        public void Show_PrePurchasePayments_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments()
        {
            ShowAmountsForPurchase_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments(108000d, 3, _inflationRateYearly);
            //ShowAmountsForPurchase_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments(108000d, 6, _inflationRateYearly);
            //ShowAmountsForPurchase_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments(108000d, 9, _inflationRateYearly);
            //ShowAmountsForPurchase_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments(108000d, 12, _inflationRateYearly);
            //ShowAmountsForPurchase_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments(108000d, 15, _inflationRateYearly);
        }
        
        // todo: test for CoOpAR that shows that no matter what inflation (inflation changes and spikes)
        // purchase rate should be near-constant and near-constant purchased/total 
        
        // todo: add test to check that purchased member's payments are enough to fulfill non-purchased purchases 

        private void Show_PrePurchasePayments_When_OneThirdAccumulation_And_ConstPayments(
            double initialPrice, int contractTermYrs, double inflationRateYearly)
        {
            var contractTermMonth = contractTermYrs * 12;
            ShowAmountsForPurchase_When_OneThirdAccumulation_And_DefinedPayments(
                initialPrice, 
                contractTermYrs, 
                inflationRateYearly,
                () => Enumerable.Range(1, contractTermMonth/3)
                    .Select(x => initialPrice/contractTermMonth));
        }

        private void ShowAmountsForPurchase_When_OneThirdAccumulation_And_PeriodicallyIncreasedPayments(
            double initialPrice, int contractTermYrs, double inflationRateYearly)
        {
            var contractTermMonth = contractTermYrs * 12;
            const int periodLengthMonth = 1;
            ShowAmountsForPurchase_When_OneThirdAccumulation_And_DefinedPayments(
                initialPrice, 
                contractTermYrs, 
                inflationRateYearly,
                () => Enumerable.Range(1, contractTermMonth/3)
                    .Select(currentMonth =>
                    {
                        var currentPeriod = currentMonth / periodLengthMonth;
                        
                        var currentImmPrice = currentPeriod == 0
                            ? initialPrice
                            : Enumerable.Range(0, currentPeriod).Aggregate(
                                initialPrice, (sum, _) => sum + sum * periodLengthMonth*(inflationRateYearly/12));
                        
                        return currentImmPrice / contractTermMonth;
                    }));
        }

        private void ShowAmountsForPurchase_When_OneThirdAccumulation_And_DefinedPayments(
            double initialPrice, int contractTermYrs, double inflationRateYearly, Func<IEnumerable<double>> calcPayments)
        {
            var realPriceOfPurchase = Enumerable.Range(1, contractTermYrs/3)
                .Aggregate(initialPrice, (sum, _) => sum + sum * inflationRateYearly);
            
            var paymentsBeforePurchase = calcPayments().ToArray();
            
            var accumulated = AccumulatedSum(paymentsBeforePurchase, inflationRateYearly);

            var shouldBeAccumulated = realPriceOfPurchase / 3;
            
            Console.WriteLine($"initialPrice: {initialPrice}, contractTermYrs: {contractTermYrs}, _inflationRateYearly: {inflationRateYearly}; \r\n" +
                              $"realPriceOfPurchase: {realPriceOfPurchase}, diff from initial price: {Percentage(initialPrice, realPriceOfPurchase)}; \r\n" +
                              $"shouldBeAccumulated: {shouldBeAccumulated}, diff from payments sum: {Percentage(shouldBeAccumulated, paymentsBeforePurchase.Sum())} \r\n" +
                              $"payments sum without inflation accumulation: {paymentsBeforePurchase.Sum()} \r\n" +
                              $"actually accumulated: {(int)accumulated}, diff from shouldBeAccumulated: {Percentage(shouldBeAccumulated, accumulated)}; \r\n\r\n");
        }

        private string Percentage(double @base, double changed) => 
            $"{(changed - @base) * 100 / @base} %";

        private static double AccumulatedSum(double[] constPayments, double inflationRateYearly)
        {
            return constPayments.Aggregate((sum, payment) => 
                sum + sum*inflationRateYearly/12 + payment);
        }
    }
}