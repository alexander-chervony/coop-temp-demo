/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Subscribers;

namespace CoOp.Domain.Tests
{
    public class AllEventsListener : ISubscribeSynchronousToAll
    {
        public static List<IDomainEvent> AllEventsReceived { get; } = new List<IDomainEvent>();
            
        public Task HandleAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            AllEventsReceived.AddRange(domainEvents);
            return Task.FromResult(0);
        }
    }
}