/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using CoOp.Domain.Sqs;
using EventFlow.Aggregates;
using EventFlow.Commands;
using EventFlow.Subscribers;

namespace CoOp.Domain.Tests
{
    public class TestTo1CEventsSender : To1CEventsSender,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, ImmovablesPurchasedEvent>,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, ImmovablesFeeDeletedEvent>
    {
        public TestTo1CEventsSender(IAmazonSQS sqs, SqsQueueUrlBuilder sqsQueueUrlBuilder) : base(sqs, sqsQueueUrlBuilder)
        {
        }

        public async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesPurchasedEvent> domainEvent, CancellationToken cancellationToken)
        {
            // we actually need sample for command, that coop would accept from 1C, so just create proper command
            await SendMessage("1C-MarkImmovablesPurchased.fifo", ToCommand(domainEvent.AggregateEvent), cancellationToken);
        }

        public override async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeBeforePurchaseAddedEvent> domainEvent, CancellationToken cancellationToken)
        {
            await base.HandleAsync(domainEvent, cancellationToken);
            // we actually need sample for command, that coop would accept from 1C, so just create proper command
            await SendMessage("1C-AddImmovablesFee.fifo", ToCommand(domainEvent.AggregateEvent), cancellationToken);
        }

        public override async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeAfterPurchaseAddedEvent> domainEvent, CancellationToken cancellationToken)
        {
            await base.HandleAsync(domainEvent, cancellationToken);
            // we actually need sample for command, that coop would accept from 1C, so just create proper command
            await SendMessage("1C-AddImmovablesFee.fifo", ToCommand(domainEvent.AggregateEvent), cancellationToken);
        }

        public async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeDeletedEvent> domainEvent, CancellationToken cancellationToken)
        {
            // we actually need sample for command, that coop would accept from 1C, so just create proper command
            await SendMessage("1C-DeleteImmovablesFee.fifo", ToCommand(domainEvent.AggregateEvent), cancellationToken);
        }
        
        private static MarkImmovablesPurchasedCommand ToCommand(ImmovablesPurchasedEvent e)
        {
            return new MarkImmovablesPurchasedCommand(CoOpId.BlrRealty, e.ContractNo);
        }
    }
}