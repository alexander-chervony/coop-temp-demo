/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using EventFlow;
using EventFlow.Commands;
using EventFlow.Extensions;
using Newtonsoft.Json;

namespace CoOp.Domain.Sqs
{
    public class From1CEventsReceiver
    {
        private readonly ICommandBus _bus;
        private readonly IAmazonSQS _sqs;
        private readonly SqsQueueUrlBuilder _sqsQueueUrlBuilder;

        // waits when no messages. If messages in queue return instantly even if there are less than MaxNumberOfMessages
        private const int ReceiveWaitTimeSec = 20;
        private const int BetweenAddFeesQueriesWaitTimeSec = 1;
        // todo after Jan 2020: make each query read in separate StartPolling and same config:
        // ReceiveWaitTimeSec = 20, BetweenAddFeesQueriesWaitTimeSec = 1 or even 0
        private const int BetweenOtherQueriesWaitTimeSec = 20;
        private const int MaxNumberOfMessages = 10;

        public From1CEventsReceiver(ICommandBus bus, IAmazonSQS sqs, SqsQueueUrlBuilder sqsQueueUrlBuilder)
        {
            _bus = bus;
            _sqs = sqs;
            _sqsQueueUrlBuilder = sqsQueueUrlBuilder;
        }

        public void StartPolling()
        {
            StartPolling(Read1CAddImmovablesFee, BetweenAddFeesQueriesWaitTimeSec);
            StartPolling(Read1COtherQueues, BetweenOtherQueriesWaitTimeSec);
        }

        private static void StartPolling(Func<Task> read1CQueue, int betweenQueriesWaitTimeSec)
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        await read1CQueue();
                    }
                    catch (Exception e)
                    {
                        // todo: logging
                        Console.WriteLine(e);
                    }

                    await Task.Delay(betweenQueriesWaitTimeSec * 1000);
                }
            });
        }

        private async Task Read1CAddImmovablesFee()
        {
            await ProcessQueue<ImmovablesFeeAddedEvent, AddImmovablesFeeCommand>(
                "1C-AddImmovablesFee.fifo",
                e => 
                    new AddImmovablesFeeCommand(CoOpId.BlrRealty, e.PaymentId1C, e.PaymentDate, e.ContractNo, e.Amount, e.Currency));
        }

        private async Task Read1COtherQueues()
        {
            await ProcessQueue<ImmovablesPurchasedEvent, MarkImmovablesPurchasedCommand>(
                "1C-MarkImmovablesPurchased.fifo",
                e => 
                    new MarkImmovablesPurchasedCommand(CoOpId.BlrRealty, e.ContractNo));
            
            await ProcessQueue<ImmovablesFeeDeletedEvent, DeleteImmovablesFeeCommand>(
                "1C-DeleteImmovablesFee.fifo",
                e => 
                    new DeleteImmovablesFeeCommand(CoOpId.BlrRealty, e.PaymentId1C, e.ContractNo));
        }

        private async Task ProcessQueue<TEvent, TCommand>(string queueName, Func<TEvent, TCommand> createCommand)
            where TCommand: Command<CoOpAR, CoOpId>
        {
            foreach (var msg in await ReceiveAll<TEvent>(queueName))
            {
                if (_bus.Publish(createCommand(msg.Body)).IsSuccess)
                {
                    // todo: make retriable? if command is published but msg not deleted
                    // command should get into dead letter queue
                    await Delete(queueName, msg.Handle);
                }
            }
        }

        private async Task<IEnumerable<Msg<T>>> ReceiveAll<T>(string queueName)
        {
            async Task<List<Msg<T>>> Recv() => new List<Msg<T>>(await Receive<T>(queueName));

            List<Msg<T>> all = new List<Msg<T>>();
            List<Msg<T>> received;
            do
            {
                received = await Recv();
                all.AddRange(received);
            } while (received.Count == MaxNumberOfMessages);

            return all;
        }

        private async Task<IEnumerable<Msg<T>>> Receive<T>(string queueName)
        {
            var result = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _sqsQueueUrlBuilder.Build(queueName),
                MaxNumberOfMessages = MaxNumberOfMessages,
                WaitTimeSeconds = ReceiveWaitTimeSec
            });

            return result.Messages.Select(m =>
                new Msg<T>
                {
                    Handle = m.ReceiptHandle,
                    Body = JsonConvert.DeserializeObject<T>(m.Body)
                });
        }

        private class Msg<T>
        {
            public string Handle { get; set; }
            public T Body { get; set; }
        }

        private async Task<int> Delete(string queueName, string receiptHandle)
        {
            var result = await _sqs.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = _sqsQueueUrlBuilder.Build(queueName),
                ReceiptHandle = receiptHandle
            });
            return (int) result.HttpStatusCode;
        }
    }
}