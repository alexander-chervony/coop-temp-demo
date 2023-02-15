/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Entities;
using static CoOp.Domain.ImmovablesState;

namespace CoOp.Domain
{
    public class Immovables : Entity<ImmovablesId>
    {
        public int ContractNo { get; }
        
        public int ContractTermYears { get; }
        
        public DateTime ContractDate { get; }
        
        public double Price { get; }
        
        public double RequiredEntranceFee { get; }

        public double EntranceFeeDebt => RequiredEntranceFee - Fees.EntranceFee;

        private double EntranceFeeAccountableDebt => RequiredEntranceFee - Fees.EntranceFeeAccountablePaymentsAmount;
        
        public string Currency { get; }

        public double PaidFromPricePct => (Fees.Accumulated * 100) / Price;
        
        public bool CompletelyPaid => PaidFromPricePct >= 100;
        
        public ImmovablesState State { get; private set; }
        
        private DateTime _purchaseDateTime;
        
        public int AccumulationMonthBeforePurchase { get; set; }

        public ImmovablesFeeContainer Fees { get; } = new ImmovablesFeeContainer();
        
        public Immovables(FirstImmovablesAddedEvent e) 
            : base(e.ImmovablesId)
        {
            ContractNo = e.ContractNo;
            ContractDate = e.ContractDate;
            ContractTermYears = e.ContractTermYears;
            Price = e.Price;
            RequiredEntranceFee = e.RequiredEntranceFee;
            Currency = e.Currency;
            State = New;
        }

        public double AddImmovablesFeeDerivative(ImmovablesFeeAddedEvent derivative)
        {
            var addedAmount = Fees.AddImmovablesFeeDerivative(derivative);
            if (State == New && EntranceFeeDebt <= 0) 
                State = EntranceFeePaid;
            return addedAmount;
        }
        
        public double DeleteImmovablesFee(PaymentId id)
        {
            var negativeResult = Fees.DeleteImmovablesFee(id);
            if (State == EntranceFeePaid && EntranceFeeAccountableDebt > 0)
                State = New;
            return negativeResult;
        }

        public void ApplyPurchased(DateTime purchaseDateTime)
        {
            _purchaseDateTime = purchaseDateTime;
            AccumulationMonthBeforePurchase = Time.PeriodInMonth(Fees.FeesBeforePurchase.FirstPaymentDate, purchaseDateTime);
            State = Purchased;
        }

        public void ApplyInQueue()
        {
            State = InQueue;
        }

        public void ApplyInQueueRollback()
        {
            State = EntranceFeePaid;
        }

        public void ApplyInPurchaseList()
        {
            State = InPurchaseList;
        }
    }

    public enum ImmovablesState
    {
        New = 10,
        EntranceFeePaid = 20,
        InQueue = 30,
        InPurchaseList = 40,
        Purchased = 50,
        ContractClosed = 60 // todo: implement CloseContract command
    }
}