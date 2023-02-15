/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class AddImmovablesFeeCommandHandler
        : CommandHandler<CoOpAR, CoOpId, AddImmovablesFeeCommand>
    {
        public override Task ExecuteAsync(
            CoOpAR aggregate,
            AddImmovablesFeeCommand command,
            CancellationToken cancellationToken)
        {
            aggregate.AddImmovablesFee(command);
            return Task.FromResult(0);
        }
    }
}