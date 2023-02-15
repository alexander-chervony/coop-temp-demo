/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class UpdateInflationRateCommandHandler
        : CommandHandler<CoOpAR, CoOpId, UpdateInflationRateCommand>
    {
        public override Task ExecuteAsync(
            CoOpAR aggregate,
            UpdateInflationRateCommand cmd,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException("requires recalculation logic implementation");
            //aggregate.UpdateInflationRate(cmd);
            //return Task.FromResult(0);
        }
    }
}