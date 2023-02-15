/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.ReadStores;
using static CoOp.Domain.ImmovablesState;

namespace CoOp.Domain.Queries.InMemory
{
    public class CoOpReadModel : IReadModel,
        IAmReadModelFor<CoOpAR, CoOpId, MemberRegisteredEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, FirstImmovablesAddedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, ImmovablesFeeBeforePurchaseAddedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, ImmovablesFeeAfterPurchaseAddedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, ImmovablesFeeDeletedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, PercentageForPurchaseReachedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, PercentageForPurchaseReachedRollbackEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, EnoughMoneyToPurchaseEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, ImmovablesPurchaseInitiatedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, ImmovablesPurchasedEvent>,
        IAmReadModelFor<CoOpAR, CoOpId, CompletelyPaidEvent>
    {
        private readonly Dictionary<ImmovablesId, ImmovablesRm> _allImmovables = new Dictionary<ImmovablesId, ImmovablesRm>();
        
        public Dictionary<MemberId, MemberRm> AllMembers = new Dictionary<MemberId, MemberRm>();

        public IEnumerable<ImmovablesRm> NewImmovables => ImmovablesOfState(New);
        public IEnumerable<ImmovablesRm> EntranceFeePaidImmovables => ImmovablesOfState(EntranceFeePaid);
        public IEnumerable<ImmovablesRm> InQueueImmovables => ImmovablesOfState(InQueue).OrderByDescending(x => x.AccumulatedPercentage);
        public IEnumerable<ImmovablesRm> InPurchaseListImmovables => ImmovablesOfState(InPurchaseList);
        public IEnumerable<ImmovablesRm> PurchasedImmovables => ImmovablesOfState(Purchased);
        //public List<ImmovablesRm> CompletelyPaidImmovables = new List<ImmovablesRm>();

        public double CurrentTotalEntranceFees => 
            _allImmovables.Values.Select(x => x.Fees.EntranceFee).Sum();

        public double CurrentTotalAvailableFundForImmovables { get; private set; }
        
        public double CurrentTotalReservedForPurchases { get; private set; }

        public double TotalImmovablesFundInCoop =>
            CurrentTotalAvailableFundForImmovables + CurrentTotalReservedForPurchases;
        
        public double TotalPurchasedImmovables { get; private set; }

        private IEnumerable<ImmovablesRm> ImmovablesOfState(ImmovablesState state)
        {
            return _allImmovables.Values.Where(v => v.State == state);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, MemberRegisteredEvent> e)
        {
            var member = e.AggregateEvent;
            AllMembers.Add(member.MemberId, new MemberRm{RegInfo = member});
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, FirstImmovablesAddedEvent> e)
        {
            var immovables = e.AggregateEvent;
            var member = AllMembers[e.AggregateEvent.MemberId];
            var immovablesView = new ImmovablesRm
            {
                MemberId = immovables.MemberId,
                ImmovablesId = immovables.ImmovablesId,
                ContractNo = immovables.ContractNo,
                ContractDate = immovables.ContractDate,
                Price = immovables.Price,
                RequiredEntranceFee = immovables.RequiredEntranceFee,
                Currency = immovables.Currency,
                FirstName = member.RegInfo.FirstName,
                MiddleName = member.RegInfo.MiddleName,
                SurnameInitial = member.RegInfo.LastName[0],
                State = New,
                ContractTermYears = immovables.ContractTermYears
            };
            _allImmovables.Add(immovablesView.ImmovablesId, immovablesView);
            member.Immovables.Add(immovablesView);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeBeforePurchaseAddedEvent> e)
        {
            AddImmovablesFeeDerivative(e.AggregateEvent);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeAfterPurchaseAddedEvent> e)
        {
            AddImmovablesFeeDerivative(e.AggregateEvent);
        }

        private void AddImmovablesFeeDerivative(ImmovablesFeeAddedEvent e)
        {
            var view = _allImmovables[e.ImmovablesId];
            CurrentTotalAvailableFundForImmovables += view.Fees.AddImmovablesFeeDerivative(e);
            if (view.State == New && view.EntranceFeeDebt <= 0)
                SetImmovablesViewState(view.ImmovablesId, EntranceFeePaid);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeDeletedEvent> e)
        {
            var view = _allImmovables[e.AggregateEvent.ImmovablesId];
            CurrentTotalAvailableFundForImmovables += view.Fees.DeleteImmovablesFee(e.AggregateEvent.PaymentId);
            if (view.State == EntranceFeePaid && view.EntranceFeeAccountableDebt > 0) 
                SetImmovablesViewState(view.ImmovablesId, New);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, PercentageForPurchaseReachedEvent> e)
        {
            SetImmovablesViewState(e.AggregateEvent.ImmovablesId, InQueue);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, PercentageForPurchaseReachedRollbackEvent> e)
        {
            SetImmovablesViewState(e.AggregateEvent.ImmovablesId, EntranceFeePaid);
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, ImmovablesPurchasedEvent> e)
        {
            SetImmovablesViewState(e.AggregateEvent.ImmovablesId, Purchased);
            var view = _allImmovables[e.AggregateEvent.ImmovablesId];
            CurrentTotalReservedForPurchases -= view.Price;
            TotalPurchasedImmovables += view.Price;
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, EnoughMoneyToPurchaseEvent> e)
        {
            // just for notifications now
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, ImmovablesPurchaseInitiatedEvent> e)
        {
            SetImmovablesViewState(e.AggregateEvent.ImmovablesId, InPurchaseList);
            var view = _allImmovables[e.AggregateEvent.ImmovablesId];
            CurrentTotalAvailableFundForImmovables -= view.Price;
            CurrentTotalReservedForPurchases += view.Price;
        }

        public void Apply(IReadModelContext context, IDomainEvent<CoOpAR, CoOpId, CompletelyPaidEvent> e)
        {
            // it can be in any list - this causes error
            //Move(PurchasedImmovables, CompletelyPaidImmovables, e.AggregateEvent.ImmovablesId);
        }

        private void SetImmovablesViewState(ImmovablesId id, ImmovablesState state)
        {
            var view = _allImmovables[id];
            if (view.State == state) // just for debug possible issues with duplicate events
                throw new InvalidOperationException("Duplicate status change! Means duplicate status changing event for the same immovables!");
            view.State = state;
        }
    }

    // todo: create ImmovablesVM
    public class ImmovablesRm
    {
        public MemberId MemberId { get; set; }
        public ImmovablesId ImmovablesId { get; set; }
        public int ContractNo { get; set; }
        public DateTime ContractDate { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public char SurnameInitial { get; set; }
        public string AdditionalComment { get; set; } // for second immovables of the member
        public double AccumulatedTotal => Fees.Accumulated;
        public string AccumulatedTotalFormatted => AccumulatedTotal.ToString("C2");
        public double AccumulatedPercentage => AccumulatedTotal / Price;
        public string AccumulatedPercentageFormatted => AccumulatedPercentage.ToString("P2");
        public double Price { get; set; }
        public double RequiredEntranceFee { get; set; }
        public double EntranceFeeDebt => RequiredEntranceFee - Fees.EntranceFee;
        public double EntranceFeeAccountableDebt => RequiredEntranceFee - Fees.EntranceFeeAccountablePaymentsAmount;
        public string Currency { get; set; }
        public string PriceFormatted => Price.ToString("C0");
        public ImmovablesState State { get; set; }
        public ImmovablesFeeContainer Fees { get; } = new ImmovablesFeeContainer();
        public int ContractTermYears { get; set; }
        public string CurrentCoopInterestRate => 
            MembershipFeeRateCalc.CalculateFee(
                Time.PeriodInMonth(Fees.FeesBeforePurchase.FirstPaymentDate, Fees.FeesBeforePurchase.LastPaymentDate),
                ContractTermYears).ToString("P2"); 
    }
    
    public class MemberRm
    {
        public MemberRegisteredEvent RegInfo { get; internal set; }
        public List<ImmovablesRm> Immovables { get; } = new List<ImmovablesRm>();

        // todo: we need CloseContract event to rely on
        public bool HasActiveEntranceFeePaidImmovables => Immovables.Any(
            i => i.State >= EntranceFeePaid && 
                 i.State < ContractClosed);
        // todo: payments here
    }
    
    
}