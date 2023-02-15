/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class FeeCommand : Command<CoOpAR, CoOpId>
    {
        protected FeeCommand(
            CoOpId id,
            string paymentId1C,
            DateTime paymentDate,
            int contractNo,
            double amount,
            string currency)
            : base(id)
        {
            if (string.IsNullOrEmpty(paymentId1C)) throw new ArgumentNullException(nameof(paymentId1C));
            PaymentId1C = paymentId1C;
            if (paymentDate == default) throw new ArgumentNullException(nameof(paymentDate));
            PaymentDate = paymentDate;
            if (contractNo <= 0) throw new ArgumentNullException(nameof(contractNo));
            ContractNo = contractNo;
            if (amount < 0) throw new ArgumentNullException(nameof(amount) + " cant be negative");
            Amount = amount;
            if (string.IsNullOrEmpty(currency)) throw new ArgumentNullException(nameof(currency));
            Currency = currency;
        }

        public string PaymentId1C { get; set; }
        public DateTime PaymentDate { get; set; }
        public int ContractNo { get; }
        public double Amount { get; }
        public string Currency { get; }
    }
}