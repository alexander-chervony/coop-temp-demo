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
    public class DeleteImmovablesFeeCommandHandler
        : CommandHandler<CoOpAR, CoOpId, DeleteImmovablesFeeCommand>
    {
        public override Task ExecuteAsync(
            CoOpAR aggregate,
            DeleteImmovablesFeeCommand command,
            CancellationToken cancellationToken)
        {
            aggregate.DeleteImmovablesFee(command);
            return Task.FromResult(0);
        }
    }
}