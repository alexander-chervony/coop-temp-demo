/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Entities;

namespace CoOp.Domain
{
    public class Payment1C : Entity<PaymentId>
    {
        public string Id1C { get; }
        public DateTime PaymentDate { get; }
        public string PaymentDateFormatted => PaymentDate.ToString("dd MMMM yyyy");
        public double Amount { get; }
        public string AmountFormatted => Amount.ToString("C2");
        /// <summary>
        /// String abbreviation from https://www.iban.com/currency-codes
        /// </summary>
        public string Currency { get; }
        
        protected Payment1C(FeeEvent e)
            : base(e.PaymentId)
        {
            Id1C = e.PaymentId1C;
            PaymentDate = e.PaymentDate;
            Amount = e.Amount;
            Currency = e.Currency;
        }
    }
}