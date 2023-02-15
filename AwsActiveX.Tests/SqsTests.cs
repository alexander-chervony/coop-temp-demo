/*******************************************************
 * Copyright (C) 2018-2019 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AwsActiveX.Tests
{
    public class SqsTests
    {
        private ISqs _componentSqs = new Sqs();
        
        private IAmazonSQS _amazonSqs = new AmazonSQSClient(
            new BasicAWSCredentials(Aws.AccessKey, Aws.SecretKey), 
            Aws.Region);
        
        private const string FifoTestQueueName = "ActiveXTestQueue.fifo";

        [SetUp]
        public async Task Setup()
        {
            await _amazonSqs.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = FifoTestQueueName,
                Attributes =
                {
                    {"FifoQueue", "true"},
                    {"ContentBasedDeduplication", "true"}
                }
            });
        }
        
        [TearDown]
        public async Task TearDown()
        {
            await _amazonSqs.DeleteQueueAsync(new DeleteQueueRequest(Sqs.QueueUrl(FifoTestQueueName)));
        }

        [Test]
        public void Test()
        {
            // create test object
            var msg1 = new TestMsg();
            var msg2 = new TestMsg{ StrProp = "~!@#$%^&*()_+", DtProp = DateTime.MinValue };
            // serialize
            var msg1Serialized = JsonConvert.SerializeObject(msg1);
            var msg2Serialized = JsonConvert.SerializeObject(msg2);
            // send
            var statusCode1 = _componentSqs.Send(FifoTestQueueName, msg1Serialized);
            Assert.AreEqual(200, statusCode1);
            var statusCode2 = _componentSqs.Send(FifoTestQueueName, msg2Serialized);
            Assert.AreEqual(200, statusCode2);
            // read
            Thread.Sleep(3000); // wait 3 sec to get all messages available in queue
            var readResult = _componentSqs.Receive(FifoTestQueueName, 10);
            // compare deserialized
            var received = JsonConvert.DeserializeObject<ReceiveResponse>(readResult);
            Assert.AreEqual(200, received.HttpStatusCode);
            Assert.AreEqual(2, received.ReceivedMessages.Length);
            Assert.AreEqual(msg1Serialized, received.ReceivedMessages[0].Body);
            Assert.AreEqual(msg2Serialized, received.ReceivedMessages[1].Body);
            var msg1Deserialized = JsonConvert.DeserializeObject<TestMsg>(received.ReceivedMessages[0].Body);
            var msg2Deserialized = JsonConvert.DeserializeObject<TestMsg>(received.ReceivedMessages[1].Body);
            // delete
            Assert.AreEqual(200,_componentSqs.Delete(FifoTestQueueName, received.ReceivedMessages[0].ReceiptHandle));
            Assert.AreEqual(200,_componentSqs.Delete(FifoTestQueueName, received.ReceivedMessages[1].ReceiptHandle));
        }

        public class TestMsg
        {
            public string StrProp { get; set; } = "OLOLO / 'qwer' }";
            public DateTime DtProp { get; set; } = DateTime.Now;
        }
    }
}