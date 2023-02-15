/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using CoOp.Domain.Tests.Data;
using EventFlow;
using EventFlow.Extensions;
using FluentAssertions;
using NUnit.Framework;
using _ = CoOp.Domain.Tests.Data.TestData;

namespace CoOp.Domain.Tests
{
    public class CoOpTests : CoOpTestsBase
    {
        [Test]
        public void WhenMembersRegistered_ReadModelContainsAllMembers()
        {
            UsingResolver(resolver =>
            {
                var bus = resolver.Resolve<ICommandBus>();
                bus.Publish(_.RegisterMemberCommand("1"));
                bus.Publish(_.RegisterMemberCommand("2"));
                
                var readModel = resolver.GetCoOpReadModel();
                
                readModel.AllMembers.Should().ContainKey(MemberId.NewDeterministic("1"));
                readModel.AllMembers.Should().ContainKey(MemberId.NewDeterministic("2"));
            });
        }

        [Test]
        public void WhenImmovablesAdded_ContractNumberGeneratedCorrectly()
        {
            UsingResolver(resolver =>
            {
                var bus = resolver.Resolve<ICommandBus>();

                bus.RegisterMemberWithImmovables("1", 100000);

                EventsOf<FirstImmovablesAddedEvent>().Should()
                    .ContainSingle(x =>
                        x.ContractNo == int.Parse(DateTime.Now.ToString("yyMMdd") + "01"));

                bus.RegisterMemberWithImmovables("2", 200000);
 
                EventsOf<FirstImmovablesAddedEvent>().Should()
                    .ContainSingle(x =>
                        x.ContractNo == int.Parse(DateTime.Now.ToString("yyMMdd") + "02"));
            });
        }

        [Test]
        public void SingleCoopMember_WhenPaidPrice_CanPurchase()
        {
            UsingResolver(resolver =>
            {
                var bus = resolver.Resolve<ICommandBus>();
                
                var now = DateTime.Now;
                
                bus.Publish(new AddInflationRateCommand(CoOpId.BlrRealty, 
                    0.12, now.AddDays(-60), "BYN"));
               
                var memberId = bus.RegisterMemberWithImmovables("1", 100000);
                
                var immovablesAdded = EventsOf<FirstImmovablesAddedEvent>().First();
                var immovablesId = immovablesAdded.ImmovablesId;
                var contractNo = immovablesAdded.ContractNo;

                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_0", now.AddMonths(-12), contractNo, immovablesAdded.RequiredEntranceFee,"BYN"));

                EventsOf<ImmovablesFeeBeforePurchaseAddedEvent>().Should()
                    .ContainSingle(x => 
                        x.ImmovablesId == immovablesId && x.EntranceFee.Equals(immovablesAdded.RequiredEntranceFee));

                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_1", now.AddMonths(-11), contractNo, 20000,"BYN"));

                EventsOf<PercentageForPurchaseReachedEvent>().Should()
                    .BeEmpty();
                   
                // check read model
                resolver.GetCoOpReadModel().EntranceFeePaidImmovables.First(m=>m.ImmovablesId == immovablesId).AccumulatedTotal.Should().Be(20000);
             
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_2", now.AddMonths(-10), contractNo,15000, "BYN"));
                
                // previous payment amount + inflation (1% from prev payment for 1 month is approx 200) + this payment
                resolver.GetCoOpReadModel().CurrentTotalAvailableFundForImmovables.Should().BeApproximately(35200, 20);

                EventsOf<PercentageForPurchaseReachedEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);

                resolver.GetCoOpReadModel().EntranceFeePaidImmovables.Count().Should().Be(0);
                resolver.GetCoOpReadModel().InQueueImmovables.First(m=>m.ImmovablesId == immovablesId).AccumulatedTotal.Should().BeApproximately(35200, 20);
                
                EventsOf<EnoughMoneyToPurchaseEvent>().Should()
                    .BeEmpty();
                
                // <delete test> <immovables fee delete causes user to be removed from queue>
                bus.Publish(new DeleteImmovablesFeeCommand(CoOpId.BlrRealty,
                    "1C_Payment_2", contractNo));

                EventsOf<PercentageForPurchaseReachedRollbackEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);
                
                resolver.GetCoOpReadModel().CurrentTotalAvailableFundForImmovables.Should().Be(20000);
                resolver.GetCoOpReadModel().InQueueImmovables.Should().BeEmpty();
                
                bus.Publish(new DeleteImmovablesFeeCommand(CoOpId.BlrRealty,
                    "1C_Payment_1", contractNo));
                
                resolver.GetCoOpReadModel().CurrentTotalAvailableFundForImmovables.Should().Be(0);
                
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_2_Again", now.AddMonths(-10), contractNo,35000, "BYN"));

                // 1 before update and one again. after rollback and second update
                EventsOf<PercentageForPurchaseReachedEvent>().Count().Should().Be(2);
                EventsOf<PercentageForPurchaseReachedEvent>().Should()
                    .Contain(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);

                // </delete test>
                    
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty,
                    "1C_Payment_3", now.AddMonths(-9), contractNo, 65000, "BYN"));
                
                EventsOf<EnoughMoneyToPurchaseEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);

                EventsOf<CompletelyPaidEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);

                var total = resolver.GetCoOpReadModel().CurrentTotalAvailableFundForImmovables;
                total.Should().BeGreaterThan(100000); // + inflation
                bus.Publish(new InitiateImmovablesPurchaseCommand(CoOpId.BlrRealty, contractNo));
                resolver.GetCoOpReadModel().CurrentTotalReservedForPurchases.Should().Be(100000);
                var rest = total - resolver.GetCoOpReadModel().CurrentTotalReservedForPurchases;
                resolver.GetCoOpReadModel().CurrentTotalAvailableFundForImmovables.Should().Be(rest);

                bus.Publish(new MarkImmovablesPurchasedCommand(CoOpId.BlrRealty, contractNo));
                resolver.GetCoOpReadModel().CurrentTotalReservedForPurchases.Should().Be(0);
                resolver.GetCoOpReadModel().CurrentTotalAvailableFundForImmovables.Should().Be(rest);
     
                EventsOf<ImmovablesPurchasedEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);
           
                resolver.GetCoOpReadModel().PurchasedImmovables.First(m=>m.ImmovablesId == immovablesId).AccumulatedTotal.Should().BeGreaterThan(100000); // + inflation
            });
        }

        [Test]
        public void ThreeMembers_WhenPercentReachedAndEnoughMoney_FirstPurchases()
        {
            UsingResolver(resolver =>
            {
                var bus = resolver.Resolve<ICommandBus>();
 
                bus.Publish(new AddInflationRateCommand(CoOpId.BlrRealty, 
                    0.12, DateTime.Now.AddDays(-60), "BYN"));
               
                bus.RegisterMemberWithImmovables("1", 100000);
                bus.RegisterMemberWithImmovables("2", 100000);
                bus.RegisterMemberWithImmovables("3", 100000);
                
                var immovablesAdded = EventsOf<FirstImmovablesAddedEvent>().First();
                var immovablesId = immovablesAdded.ImmovablesId;
                var contractNo = immovablesAdded.ContractNo;

                var otherImmovablesAdded = EventsOf<FirstImmovablesAddedEvent>().ToArray();

                var contractNo2 = otherImmovablesAdded[1].ContractNo;
                var contractNo3 = otherImmovablesAdded[2].ContractNo;
                
                // entrance fees
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_01", DateTime.Now.AddMonths(-12), contractNo, 1000,"BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_02", DateTime.Now.AddMonths(-12), contractNo2, 1000,"BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_03", DateTime.Now.AddMonths(-12), contractNo3, 1000,"BYN"));

                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_11", DateTime.Now.AddMonths(-11), contractNo, 20000,"BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_12", DateTime.Now.AddMonths(-11), contractNo2, 20000,"BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_13", DateTime.Now.AddMonths(-11), contractNo3, 20000,"BYN"));

                EventsOf<PercentageForPurchaseReachedEvent>().Should()
                    .BeEmpty();

                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_21", DateTime.Now.AddMonths(-10), contractNo,15000, "BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_22", DateTime.Now.AddMonths(-10), contractNo2,14000, "BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_23", DateTime.Now.AddMonths(-10), contractNo3,14000, "BYN"));
                
                EventsOf<PercentageForPurchaseReachedEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);
                
                EventsOf<EnoughMoneyToPurchaseEvent>().Should()
                    .ContainSingle(x => x.ImmovablesId == immovablesId && x.ContractNo == contractNo);
                
                bus.Publish(new InitiateImmovablesPurchaseCommand(CoOpId.BlrRealty, contractNo));
                bus.Publish(new MarkImmovablesPurchasedCommand(CoOpId.BlrRealty, contractNo));
                
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_31", DateTime.Now, contractNo, 20000,"BYN"));
                //bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                //   "1C_Payment_32", DateTime.Now, contractNo2, 20000,"BYN"));
                //bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                //    "1C_Payment_33", DateTime.Now, contractNo3, 20000,"BYN"));
                
                bus.Publish(new DeleteImmovablesFeeCommand(CoOpId.BlrRealty,
                    "1C_Payment_31", contractNo));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_31_Again", DateTime.Now, contractNo,22000, "BYN"));
                
                // after purchase
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_41", DateTime.Now.AddMonths(1), contractNo, 10000,"BYN"));
                bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty, 
                    "1C_Payment_42", DateTime.Now.AddMonths(2), contractNo, 10000,"BYN"));

                // todo: add some checks for inflation and other "PartTo" events   
            });
        }
    }
}