/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class AddFirstImmovablesCommand : Command<CoOpAR, CoOpId>
    {
        public MemberId MemberId { get; }
        public double Price { get; }
        public string Currency { get; }
        public int ContractTermYears { get; }
        
        public AddFirstImmovablesCommand(
            CoOpId id,
            MemberId memberId,
            double price,
            string currency,
            int contractTermYears)
            : base(id)
        {
            MemberId = memberId ?? throw new ArgumentNullException(nameof(memberId));
            if (price <= 0)
                throw new ArgumentNullException(nameof(price));
            Price = price;
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentNullException(nameof(currency));
            Currency = currency;
            if (contractTermYears <= 0)
                throw new ArgumentNullException(nameof(contractTermYears));
            ContractTermYears = contractTermYears;
        }
    }
}