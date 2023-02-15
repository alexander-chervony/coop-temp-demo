/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Exceptions;
using static CoOp.Domain.ImmovablesState;

namespace CoOp.Domain
{
    public partial class CoOpAR : AggregateRoot<CoOpAR, CoOpId>,
        IEmit<InflationRateAddedEvent>,
        IEmit<InflationRateUpdatedEvent>,
        IEmit<MemberRegisteredEvent>,
        IEmit<FirstImmovablesAddedEvent>,
        IEmit<ImmovablesFeeBeforePurchaseAddedEvent>,
        IEmit<ImmovablesFeeAfterPurchaseAddedEvent>,
        IEmit<ImmovablesFeeDeletedEvent>,
        IEmit<PercentageForPurchaseReachedEvent>,
        IEmit<PercentageForPurchaseReachedRollbackEvent>,
        IEmit<EnoughMoneyToPurchaseEvent>,
        IEmit<ImmovablesPurchaseInitiatedEvent>,
        IEmit<ImmovablesPurchasedEvent>,
        IEmit<CompletelyPaidEvent>
    {
        // these constants must be set within InitCoOpCommand/Event
        private const byte MinPaidPctForPurchase = 35;
        public const string CoOpCurrency = "BYN";
        public const string MemberDefaultDocType = "passport_by";
        public const string MemberDefaultCountry = "Belarus";
        public const double EntranceFeeFromPriceRatio = 0.005;
        public const double DefaultImmovablesPrice = 85000;
        public const double MinImmovablesPrice = 20000;
        public const double MaxImmovablesPrice = 200000;
        public const double MembershipFeeRate = 0.05;
        public const int DefaultContractTermYears = 10;
        public const int MinContractTermYears = 1;
        public const int MaxContractTermYears = 20;
        
        
        
        private double _currentTotalAvailableFundForImmovables;
        private double _currentTotalReservedForPurchases;

        private InflationRate _currentInflationRate;

        
        private readonly Dictionary<MemberId, Member> _members = new Dictionary<MemberId, Member>();
        
        // contract numbers
        private Queue<int> _pctForPurchaseReachedQueue = new Queue<int>();

        // just for validation to avoid any duplicate payments
        private readonly HashSet<string> _allPayments = new HashSet<string>();
        // for faster lookup on command processing
        private readonly Dictionary<int, Member> _contractNoToMember = new Dictionary<int, Member>();

        public CoOpAR(CoOpId id) : base(id)
        {
        }
 
        public void AddInflationRate(AddInflationRateCommand cmd)
        {
            Emit(InflationRateEvent.From<InflationRateAddedEvent>(cmd));
        }
 
        public void UpdateInflationRate(UpdateInflationRateCommand cmd)
        {
            Emit(InflationRateEvent.From<InflationRateUpdatedEvent>(cmd));
        }
               
        public void RegisterMember(RegisterMemberCommand cmd)
        {
            if (_members.ContainsKey(cmd.GetMemberId()))
                throw DomainError.With("Member already registered");

            // todo: enforce uniquiness for more fields: email, phone, e.t.c.
            cmd.ReferralCode = GetRandomReferrerCode();

            Emit(MemberRegisteredEvent.From(cmd));
        }

        public void AddFirstImmovables(AddFirstImmovablesCommand command)
        {
            var member = _members[command.MemberId];
            
            if (member.Immovables.Count > 0)
                throw DomainError.With("First immovables already set!");

            var now = DateTime.Now;
            var nextContractNo = GetContractNo(now.Date);
            var immovablesId = ImmovablesId.NewDeterministic(Id, nextContractNo);
            
            // shouldn't be the case according to logic but just to ensure
            if (_members.Values.SelectMany(q => q.Immovables).Select(i => i.ContractNo).Contains(nextContractNo))
                throw DomainError.With($"Duplicate contract number generated {nextContractNo}!");

            // todo: take into account referral discounts
            // The least buggy way (the most predictable and straightforward) is to set this value here rather during first payment. Otherwise there are cases with deletion/re-addition of fee and further payments and changes in percentage.
            var requiredEntranceFee = command.Price * EntranceFeeFromPriceRatio;
           
            Emit(new FirstImmovablesAddedEvent(command.MemberId, member.GetUId(), immovablesId, nextContractNo, now, command.ContractTermYears, command.Price, requiredEntranceFee, command.Currency));
        }
        
        public void AddImmovablesFee(AddImmovablesFeeCommand cmd)
        {
            var member = GetMember(cmd.ContractNo);
            var immovables = EmitImmovablesFeeEvent<ImmovablesFeeAddedEvent>(member, cmd);
            EmitIfPercentageForPurchaseReached(immovables);
            EmitIfCompletelyPaid(immovables);
            EmitIfEnoughFundsToPurchase();
        }

        /// <summary>
        /// Only the case up to InQueue supported now. If member is in purchase list, it's hot handled.
        /// </summary>
        public void DeleteImmovablesFee(DeleteImmovablesFeeCommand cmd)
        {
            // todo: handle deletion of entrance fee
            
            
            var immovables = GetImmovables(cmd.ContractNo);
            Emit(ImmovablesFeeDeletedEvent.FromCommand(immovables.Id, cmd));
            EmitPercentageForPurchaseReachedRollbackIfNeeded(immovables);
            // note: due to delete (and addition back to another user some payment)
            // it may happen that users receive notifications twice
        }
        
        /// <summary>
        /// Split to several payments to add to different funds.
        /// </summary>
        private Immovables EmitImmovablesFeeEvent<T>(Member member, ImmovablesFeeCommand cmd)
            where T: ImmovablesFeeEvent, new()
        {
            var immovables = GetImmovables(cmd.ContractNo);

            EnsureCommonPaymentAssertions(cmd, member, immovables);

            Emit(ImmovablesFeeSplitter.Split(immovables, cmd, _currentInflationRate));

            return immovables;
        }

        private void EnsureCommonPaymentAssertions(FeeCommand payment, Member member, Immovables immovables)
        {
            switch (payment)
            {
                case IAdd _:
                {
                    if (_allPayments.Contains(payment.PaymentId1C))
                        throw DomainError.With($"Payment {payment.PaymentId1C} already registered");
                    break;
                }
                case IDelete _:
                {
                    // todo: decide - maybe ok to silently ignore deletion of non existent records? (consider
                    //      case when payment was not properly added and then command from 1C comes to delete
                    //      if the deletion fails here than there will be stuck message in deletion queue
                    if (!_allPayments.Contains(payment.PaymentId1C))
                        throw DomainError.With($"Unknown payment to delete {payment.PaymentId1C} for contract {payment.ContractNo}!");
                    break;
                }
                default:
                    throw DomainError.With($"Unknown payment type (Add or Delete?)!");
            }
            
            if (!member.Immovables.Contains(immovables))
                throw DomainError.With($"Member has no such immovables ({member.GetUId()}, {immovables.Id}, {immovables.ContractNo})!");

            if (payment.Currency != immovables.Currency)
                throw DomainError.With($"Payment currency {payment.Currency} doesn't match to immovables currency {immovables.Currency}");
            
            if (payment.Currency != CoOpCurrency)
                throw DomainError.With($"Payment currency {payment.Currency} doesn't match to coop currency {CoOpCurrency}");
        }

        private void EmitIfPercentageForPurchaseReached(Immovables immovables)
        {
            if (PercentageForPurchaseReached(immovables) && 
                !_pctForPurchaseReachedQueue.Contains(immovables.ContractNo) &&
                immovables.State < InQueue)
            {
                Emit(ImmovablesEvent.New<PercentageForPurchaseReachedEvent>(immovables));
            }
        }

        private void EmitPercentageForPurchaseReachedRollbackIfNeeded(Immovables immovables)
        {
            if (!PercentageForPurchaseReached(immovables) && 
                _pctForPurchaseReachedQueue.Contains(immovables.ContractNo) &&
                immovables.State == InQueue)
            {
                Emit(ImmovablesEvent.New<PercentageForPurchaseReachedRollbackEvent>(immovables));
            }
        }

        private void EmitIfCompletelyPaid(Immovables immovables)
        {
            // todo: additional event if overpaid too much? this would also need some "PartialRefundCommand"
            if (immovables.PaidFromPricePct >= 100)
            {
                Emit(ImmovablesEvent.New<CompletelyPaidEvent>(immovables));
            }
        }

        /// <summary>
        /// Check if enough funds for purchase immovables which is first in queue
        /// (not necessary the user that paid just now)
        /// </summary>
        private void EmitIfEnoughFundsToPurchase()
        {
            var firstInQueueContractNo = _pctForPurchaseReachedQueue.FirstOrDefault();
            if (firstInQueueContractNo == default)
                return;

            var immovables = GetImmovables(firstInQueueContractNo);
            if (EnoughFundsForPurchase(immovables) && StatusInQueue(immovables))
            {
                Emit(ImmovablesEvent.New<EnoughMoneyToPurchaseEvent>(immovables));
            }
        }

        private bool EnoughFundsForPurchase(Immovables immovables) => 
            _currentTotalAvailableFundForImmovables >= immovables.Price;

        private bool StatusInQueue(Immovables immovables) => 
            immovables.State == InQueue;

        private bool PercentageForPurchaseReached(Immovables immovables) => 
            immovables.PaidFromPricePct >= MinPaidPctForPurchase;

        public void InitiateImmovablesPurchase(InitiateImmovablesPurchaseCommand cmd)
        {
            if (!_pctForPurchaseReachedQueue.Contains(cmd.ContractNo))
                throw DomainError.With($"No such contract in purchase queue ({cmd.ContractNo})!");
               
            var immovables = GetImmovables(cmd.ContractNo);
               
            if (!EnoughFundsForPurchase(immovables))
                throw DomainError.With($"Not enough funds for purchase ({cmd.ContractNo}, {immovables.Price}, {_currentTotalAvailableFundForImmovables})!");
               
            if (!StatusInQueue(immovables))
                throw DomainError.With($"Immovables status should be InQueue. Actual status for {cmd.ContractNo} is {immovables.State}!");
            
            Emit(ImmovablesEvent.New<ImmovablesPurchaseInitiatedEvent>(immovables));
        }

        public void MarkImmovablesPurchased(MarkImmovablesPurchasedCommand cmd)
        {
            var immovables = GetImmovables(cmd.ContractNo);
            
            if(immovables.State != InPurchaseList)
                throw DomainError.With($"No such immovables {immovables.Id} in purchase list!");

            // todo: handle case when first completely paid and then purchased (prob needs separate list here in class as before instead of status)
            // todo: additional test cases for double event emission - avoid if possible
            
            if(_currentTotalReservedForPurchases < immovables.Price)
                throw DomainError.With($"Not enough funds in coop!");
            
            Emit(ImmovablesEvent.New<ImmovablesPurchasedEvent>(immovables));
        }

        // We apply the event as part of the event sourcing system. EventFlow
        // provides several different methods for doing this, e.g. state objects,
        // the Apply method is merely the simplest
        public void Apply(MemberRegisteredEvent e)
        {
            _members.Add(e.MemberId, new Member(e));
        }
        
        public void Apply(FirstImmovablesAddedEvent e)
        {
            var member = _members[e.MemberId];
            _contractNoToMember.Add(e.ContractNo, member);
            member.AddImmovables(new Immovables(e));
        }
        
        public void Apply(ImmovablesFeeBeforePurchaseAddedEvent e)
        {
            AddImmovablesFeeDerivative(e);
        }
        
        public void Apply(ImmovablesFeeAfterPurchaseAddedEvent e)
        {
            AddImmovablesFeeDerivative(e);
        }

        private void AddImmovablesFeeDerivative(ImmovablesFeeAddedEvent e)
        {
            _allPayments.Add(e.PaymentId1C);
            var added = GetImmovables(e.ContractNo)
                .AddImmovablesFeeDerivative(e);
            _currentTotalAvailableFundForImmovables += added;
        }

        public void Apply(ImmovablesFeeDeletedEvent e)
        {
            var difference = GetImmovables(e.ContractNo)
                .DeleteImmovablesFee(e.PaymentId);
            _currentTotalAvailableFundForImmovables += difference;
        }

        public void Apply(PercentageForPurchaseReachedEvent e)
        {
            GetImmovables(e.ContractNo)
                .ApplyInQueue();
            _pctForPurchaseReachedQueue.Enqueue(e.ContractNo);
        }

        public void Apply(PercentageForPurchaseReachedRollbackEvent e)
        {
            GetImmovables(e.ContractNo)
                .ApplyInQueueRollback();
            _pctForPurchaseReachedQueue = new Queue<int>(
                _pctForPurchaseReachedQueue.Where(p => p != e.ContractNo));
        }

        public void Apply(EnoughMoneyToPurchaseEvent e)
        {
            // note: nothing to do here: just needs for the notifications,
            // actual work happens on InitiateImmovablesPurchaseCommand
        }

        public void Apply(ImmovablesPurchaseInitiatedEvent e)
        {
            var imm = GetImmovables(e.ContractNo);
            imm.ApplyInPurchaseList();
            _pctForPurchaseReachedQueue.Dequeue();
            _currentTotalAvailableFundForImmovables -= imm.Price;
            _currentTotalReservedForPurchases += imm.Price;
        }

        public void Apply(ImmovablesPurchasedEvent e)
        {
            var imm = GetImmovables(e.ContractNo);
            imm.ApplyPurchased(e.Date);
            _currentTotalReservedForPurchases -= imm.Price;
        }

        public void Apply(CompletelyPaidEvent e)
        {
            // note: nothing to do here: just needs for the notifications
        }

        private Member GetMember(int contractNo) => 
            _contractNoToMember[contractNo];

        private Immovables GetImmovables(int contractNo) => 
            GetMemberImmovables(GetMember(contractNo), contractNo);

        private static Immovables GetMemberImmovables(Member member, int contractNo) => 
            member.Immovables.First(i => i.ContractNo == contractNo);

        public void Apply(InflationRateAddedEvent e)
        {
            _currentInflationRate = new InflationRate(e);
        }

        public void Apply(InflationRateUpdatedEvent e)
        {
            // update current. If older updated - don't care - only read model going to be updated
            if (_currentInflationRate?.Id == e.InflationRateId)
                _currentInflationRate = new InflationRate(e);
        }
    }
}

