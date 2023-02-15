/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Exceptions;

namespace CoOp.Domain
{
    public class AddSubsequentImmovablesCommandHandler
        : CommandHandler<CoOpAR, CoOpId, AddSubsequentImmovablesCommand>
    {
        public override Task ExecuteAsync(
            CoOpAR aggregate,
            AddSubsequentImmovablesCommand command,
            CancellationToken cancellationToken)
        {
            if (command.UpdatedPaymentPartPercentages.Sum(x=>x.Value) != 100)
            {
                throw DomainError.With("PaymentPartPct sum of all Immovables should be 100");
            }

            throw new NotImplementedException();
            //return Task.FromResult(0);
        }
    }
}