/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Subscribers;

namespace CoOp.Domain.Tests
{
    public class NotificationEventsListener : 
        ISubscribeSynchronousTo<CoOpAR, CoOpId, PercentageForPurchaseReachedEvent>,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, EnoughMoneyToPurchaseEvent>
    {
        public Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, PercentageForPurchaseReachedEvent> domainEvent, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, EnoughMoneyToPurchaseEvent> domainEvent, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}