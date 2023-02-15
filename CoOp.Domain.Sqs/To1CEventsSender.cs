/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using EventFlow.Aggregates;
using EventFlow.Commands;
using EventFlow.Subscribers;
using Newtonsoft.Json;

namespace CoOp.Domain.Sqs
{
    // todo: handle send error so event is not persisted AND source message not deleted
    // - should be by default according to eventflow conf but needs to be checked
    public class To1CEventsSender :
        ISubscribeSynchronousTo<CoOpAR, CoOpId, MemberRegisteredEvent>,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, FirstImmovablesAddedEvent>,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, ImmovablesFeeBeforePurchaseAddedEvent>,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, ImmovablesFeeAfterPurchaseAddedEvent>,
        ISubscribeSynchronousTo<CoOpAR, CoOpId, ImmovablesPurchaseInitiatedEvent>
    {
        private readonly IAmazonSQS _sqs;
        private readonly SqsQueueUrlBuilder _sqsQueueUrlBuilder;

        public To1CEventsSender(IAmazonSQS sqs, SqsQueueUrlBuilder sqsQueueUrlBuilder)
        {
            _sqs = sqs;
            _sqsQueueUrlBuilder = sqsQueueUrlBuilder;
        }
        
        public async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, MemberRegisteredEvent> domainEvent, CancellationToken cancellationToken)
        {
            await SendMessage("RegisterMember.fifo", domainEvent, cancellationToken);
        }

        public async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, FirstImmovablesAddedEvent> domainEvent, CancellationToken cancellationToken)
        {
            await SendMessage("AddImmovables.fifo", domainEvent, cancellationToken);
        }

        public async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesPurchaseInitiatedEvent> domainEvent, CancellationToken cancellationToken)
        {
            await SendMessage("InitiateImmovablesPurchase.fifo", domainEvent, cancellationToken);
        }

        public virtual async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeBeforePurchaseAddedEvent> domainEvent, CancellationToken cancellationToken)
        {
            await SendMessage("SplitImmovablesFeeBeforePurchase.fifo", domainEvent, cancellationToken);
        }

        public virtual async Task HandleAsync(IDomainEvent<CoOpAR, CoOpId, ImmovablesFeeAfterPurchaseAddedEvent> domainEvent, CancellationToken cancellationToken)
        {
            await SendMessage("SplitImmovablesFeeAfterPurchase.fifo", domainEvent, cancellationToken);
        }
        
        protected static ImmovablesFeeCommand ToCommand(FeeEvent e)
        {
            return new ImmovablesFeeCommand(CoOpId.BlrRealty,
                e.PaymentId1C, e.PaymentDate, e.ContractNo, e.Amount, e.Currency);
        }

        protected static DeleteImmovablesFeeCommand ToCommand(ImmovablesFeeDeletedEvent e)
        {
            return new DeleteImmovablesFeeCommand(CoOpId.BlrRealty,
                e.PaymentId1C, e.ContractNo);
        }
                
        protected async Task SendMessage<TCommand>(string queueName, TCommand command, CancellationToken cancellationToken) 
            where TCommand : Command<CoOpAR, CoOpId>
        {
            await _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _sqsQueueUrlBuilder.Build(queueName),
                MessageBody = JsonConvert.SerializeObject(command, Formatting.Indented),
                MessageGroupId = queueName
            }, cancellationToken);
        }

        private async Task SendMessage<TEvent>(string queueName, IDomainEvent<CoOpAR, CoOpId, TEvent> domainEvent, CancellationToken cancellationToken) 
            where TEvent : IAggregateEvent<CoOpAR, CoOpId>
        {
            await _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _sqsQueueUrlBuilder.Build(queueName),
                MessageBody = JsonConvert.SerializeObject(domainEvent.AggregateEvent, Formatting.Indented),
                MessageGroupId = queueName,
                
            }, cancellationToken);
        }
    }
}