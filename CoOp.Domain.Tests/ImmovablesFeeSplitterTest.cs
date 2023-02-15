/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace CoOp.Domain.Tests
{
    public class ImmovablesFeeSplitterTest
    {
        [Test]
        public void FirstPayment_Produce_ImmovablesInflationDebt_EqualTo_Zero()
        {
            var @event = ImmovablesFeeSplitter.Split(
                ImmovablesWithEntranceFee(),
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid", DateTime.Now, _contractNo, 123, "BYN"), 
                _inflation12PrcYearly);

            @event.Should().BeOfType<ImmovablesFeeBeforePurchaseAddedEvent>();
            var actual = (ImmovablesFeeBeforePurchaseAddedEvent) @event;
            Assert.AreEqual(0, actual.InflationDebt);
            Assert.Greater(actual.ImmovablesFundPart, 0);
        }

        [Test]
        public void SameDayPayment_Produce_ImmovablesInflationDebt_EqualTo_Zero()
        {
            var today = DateTime.Now;
            
            var immovables = ImmovablesWithEntranceFee()
                .WithPayment(100, today)
                .WithPayment(200, today);
            
            var @event = ImmovablesFeeSplitter.Split(
                immovables,
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid", today, _contractNo, 123, "BYN"), 
                _inflation12PrcYearly);

            @event.Should().BeOfType<ImmovablesFeeBeforePurchaseAddedEvent>();
            var actual = (ImmovablesFeeBeforePurchaseAddedEvent) @event;
            Assert.AreEqual(0, actual.InflationDebt);
        }

        [Test]
        public void WhenNotPurchased_ShouldSplitCorrectly()
        {
            var today = DateTime.Now;
            
            var immovables = ImmovablesWithEntranceFee()
                .WithPayment(100, today.AddDays(-60))
                .WithPayment(200, today.AddDays(-Time.DaysInMonth));

            var amount = 123;
            var @event = ImmovablesFeeSplitter.Split(
                immovables,
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid", today, _contractNo, amount, "BYN"), 
                _inflation12PrcYearly);

            @event.Should().BeOfType<ImmovablesFeeBeforePurchaseAddedEvent>();
            var actual = (ImmovablesFeeBeforePurchaseAddedEvent) @event;
            
            var expectedInflationAmount = CalcInflation(300, Time.DaysInMonth).RoundToCents();
            Assert.AreEqual(expectedInflationAmount, actual.InflationDebt);
            
            var expectedImmovablesFundAmount = amount;
            Assert.AreEqual(expectedImmovablesFundAmount, actual.ImmovablesFundPart);
        }
        
        [Test]
        public void WhenPurchased_ShouldSplitCorrectly()
        {
            var immovables = ImmovablesWithEntranceFee();
            var purchaseDateTime = DateTime.Now;
            immovables.WithPayment(immovables.Price * 0.5, purchaseDateTime);
            immovables.ApplyPurchased(purchaseDateTime);
            
            var remainingDebt = immovables.Price * 0.5;
            
            var amountPaid = ((immovables.Price * 0.1)/3).RoundToCents();
            var daysUsed = 30;
            var @event = ImmovablesFeeSplitter.Split(
                immovables,
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid", 
                    purchaseDateTime.AddDays(daysUsed), _contractNo, amountPaid, "BYN"),
                _inflation12PrcYearly);

            @event.Should().BeOfType<ImmovablesFeeAfterPurchaseAddedEvent>();
            var actual = (ImmovablesFeeAfterPurchaseAddedEvent) @event;
            
            var expectedInflationFundPart = CalcInflation(remainingDebt, daysUsed).RoundToCents();
            Assert.AreEqual(expectedInflationFundPart, actual.InflationFundPart);
            
            var expectedCoopFundPart = CalcCoopFee(remainingDebt, daysUsed, immovables).RoundToCents();
            Assert.AreEqual(expectedCoopFundPart, actual.CoopFundPart);
            
            var expectedImmovablesFundPart = amountPaid - expectedInflationFundPart - expectedCoopFundPart;
            Assert.AreEqual(expectedImmovablesFundPart, actual.ImmovablesFundPart);
            
            // check that final sum with roundings of parts match
            Assert.AreEqual(amountPaid, 
                (actual.InflationFundPart + actual.CoopFundPart + actual.ImmovablesFundPart).RoundToCents());
        }

        [Test]
        public void EntranceFees_AreAdded_Until_FullyPaid()
        {
            var feePart = 10;
            var immovables = Immovables()
                .WithEntranceFee(feePart, DateTime.Now)
                .WithEntranceFee(feePart, DateTime.Now);
            var @event = ImmovablesFeeSplitter.Split(
                immovables,
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid", DateTime.Now, _contractNo, feePart, "BYN"), 
                _inflation12PrcYearly);
            immovables.AddImmovablesFeeDerivative(@event);

            @event.Should().BeOfType<ImmovablesFeeBeforePurchaseAddedEvent>();
            var actual = (ImmovablesFeeBeforePurchaseAddedEvent) @event;
            Assert.AreEqual(0, actual.InflationDebt);
            Assert.AreEqual(0, actual.ImmovablesFundPart);
            Assert.AreEqual(feePart, actual.EntranceFee);
            immovables.Fees.EntranceFee.Should().Be(3*feePart);
            
            var @event2 = ImmovablesFeeSplitter.Split(
                immovables,
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid2", DateTime.Now, _contractNo, 
                    immovables.RequiredEntranceFee, "BYN"), 
                _inflation12PrcYearly);
            immovables.AddImmovablesFeeDerivative(@event2);

            var restOfEntranceFee = immovables.RequiredEntranceFee - 3 * feePart;
            @event2.Should().BeOfType<ImmovablesFeeBeforePurchaseAddedEvent>();
            var actual2 = (ImmovablesFeeBeforePurchaseAddedEvent) @event2;
            Assert.AreEqual(0, actual2.InflationDebt);
            Assert.AreEqual(immovables.RequiredEntranceFee - restOfEntranceFee, actual2.ImmovablesFundPart);
            Assert.AreEqual(restOfEntranceFee, actual2.EntranceFee);
            immovables.Fees.EntranceFee.Should().Be(immovables.RequiredEntranceFee);
            immovables.Fees.Accumulated.Should().Be(3*feePart);
        }

        [Test]
        public void EntranceFee_LastPaymentDate_Doesnt_Influence_ImmovablesInflationDebt()
        {
            var now = DateTime.Now;
            var paidAmount = 1000;
            var @event = ImmovablesFeeSplitter.Split(
                ImmovablesWithEntranceFee()
                    .WithPayment(paidAmount, now.AddDays(-Time.DaysInMonth))
                    .WithPayment(paidAmount, now.AddDays(-Time.DaysInMonth)),
                new AddImmovablesFeeCommand(CoOpId.BlrRealty, "1cid", now, _contractNo, 123, "BYN"), 
                _inflation12PrcYearly);

            @event.Should().BeOfType<ImmovablesFeeBeforePurchaseAddedEvent>();
            var actual = (ImmovablesFeeBeforePurchaseAddedEvent) @event;
            var expectedInflationDebt = CalcInflation(paidAmount*2, Time.DaysInMonth).RoundToCents();
            actual.InflationDebt.Should().Be(expectedInflationDebt);
        }

        [Test]
        public void DoubleToInt()
        {
            double d = 1.5;
            Assert.AreEqual(1, (int) d);
        }

        private double CalcInflation(double accumulatedAmount, int daysFromLastPayment) => 
            accumulatedAmount * _inflation12PrcYearly.Value
                              * daysFromLastPayment / Time.DaysInYear;

        private double CalcCoopFee(double remainingDebtAmount, int daysFromLastPayment, Immovables immovables) => 
            remainingDebtAmount * MembershipFeeRateCalc.CalculateFee(immovables.AccumulationMonthBeforePurchase, immovables.ContractTermYears)
                                * daysFromLastPayment / Time.DaysInYear;

        private readonly InflationRate _inflation12PrcYearly = new InflationRate(
            new InflationRateEvent{InflationRateId = InflationRateId.New, Value = 0.12});
        
        private static int _contractNo = 123123;

        private static Immovables ImmovablesWithEntranceFee()
        {
            var immovables = Immovables();
            return immovables.WithEntranceFee(immovables.RequiredEntranceFee, immovables.ContractDate);
        }

        private static Immovables Immovables()
        {
            var uid = "123";

            var prices = new[] {20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200}
                .Select(x => x * 1000).ToArray();

            var price = prices[new Random().Next(prices.Length)];
            var entranceFee = price * CoOpAR.EntranceFeeFromPriceRatio;
            var date = DateTime.Today.AddYears(-1);
            var testImmovables = new Immovables(
                new FirstImmovablesAddedEvent(
                    MemberId.NewDeterministic(uid),
                    uid,
                    ImmovablesId.NewDeterministic(CoOpId.BlrRealty, _contractNo),
                    _contractNo,
                    date,
                    10,
                    price,
                    entranceFee,
                    "BYN"));
            
            return testImmovables;
        }
    }

    static class TestImmovablesExtensions
    {
        public static Immovables WithPayment(this Immovables immovables, double amount, DateTime date)
        {
            // here we use payment without inflation part just for test setup simplicity,
            // to produce predictable inflation amount values
            var @event = new ImmovablesFeeBeforePurchaseAddedEvent
            {
                PaymentId = PaymentId.NewDeterministicFromId1C(Guid.NewGuid().ToString()),
                Amount = amount,
                ImmovablesFundPart = amount,
                PaymentDate = date
            };
            immovables.AddImmovablesFeeDerivative(@event);
            return immovables;
        }
        
        public static Immovables WithEntranceFee(this Immovables immovables, double feeAmount, DateTime feeDate)
        {
            var @event = new ImmovablesFeeBeforePurchaseAddedEvent
            {
                PaymentId = PaymentId.NewDeterministicFromId1C(Guid.NewGuid().ToString()),
                Amount = feeAmount,
                EntranceFee = feeAmount,
                PaymentDate = feeDate
            };
            immovables.AddImmovablesFeeDerivative(@event);
            return immovables;
        }

    }
}