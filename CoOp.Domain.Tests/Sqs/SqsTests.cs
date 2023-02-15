/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using CoOp.Domain.Sqs;
using NUnit.Framework;

namespace CoOp.Domain.Tests
{
    public class SqsTests
    {
        private IAmazonSQS _sqs;

        [SetUp]
        public void Connect()
        {
            _sqs = new SqsClientFactory(Conf.Init()).CreateSqsClient();
        }

        //[Test]
        public async Task CreateAllQueues()
        {
            await CreateFifoQueue("RegisterMember.fifo");
            await CreateFifoQueue("AddImmovables.fifo");
            await CreateFifoQueue("InitiateImmovablesPurchase.fifo");
            await CreateFifoQueue("1C-MarkImmovablesPurchased.fifo");
            await CreateFifoQueue("1C-AddImmovablesFee.fifo");
            await CreateFifoQueue("1C-DeleteImmovablesFee.fifo");
            await CreateFifoQueue("SplitImmovablesFeeBeforePurchase.fifo");
            await CreateFifoQueue("SplitImmovablesFeeAfterPurchase.fifo");
        }
        
        private async Task CreateFifoQueue(string queueName)
        {
            await _sqs.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = $"sample--{queueName}",
                Attributes =
                {
                    {"FifoQueue", "true"},
                    {"ContentBasedDeduplication", "true"},
                    {"MaximumMessageSize", (8*1024).ToString()}, // 8k (biggest event (for "RegisterMember") takes about 700 bytes)
                    {"MessageRetentionPeriod", (14*24*3600).ToString()} // 14 days 
                }
            });
        }
    }
}