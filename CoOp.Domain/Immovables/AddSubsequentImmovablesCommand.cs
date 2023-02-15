/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;

namespace CoOp.Domain
{
    public class AddSubsequentImmovablesCommand : AddFirstImmovablesCommand
    {
        public KeyValuePair<ImmovablesId, byte>[] UpdatedPaymentPartPercentages { get; }

        public AddSubsequentImmovablesCommand(CoOpId id, MemberId memberId, double price, string currency, int contractTermYears, KeyValuePair<ImmovablesId, byte>[] updatedPaymentPartPercentages) 
            : base(id, memberId, price, currency, contractTermYears)
        {
            UpdatedPaymentPartPercentages = updatedPaymentPartPercentages;
            throw new NotImplementedException();
        }
    }
}