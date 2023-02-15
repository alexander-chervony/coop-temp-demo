/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;

namespace CoOp.Domain
{
    public class ImmovablesFeeCommand : FeeCommand
    {
        public ImmovablesFeeCommand(CoOpId id, string paymentId1C, DateTime paymentDate, int contractNo, double amount, string currency) 
            : base(id, paymentId1C, paymentDate, contractNo, amount, currency)
        {
        }
    }
}