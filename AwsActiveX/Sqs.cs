/*******************************************************
 * Copyright (C) 2018-2019 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace AwsActiveX 
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Sqs : ISqs
    {
        private readonly IAmazonSQS _sqs;
        private const int ReceiveWaitTimeSec = 1;
        public static string QueueUrl(string name) => 
            $"https://sqs.{Aws.Region.SystemName}.amazonaws.com/cleaned/dev--{name}";
        
        /// <summary>
        /// Create client with keys hardcoded, later maybe read from conf
        /// Note: pub def ctor should be only ctor (required by com/activeX)
        /// </summary>
        public Sqs()
        {
            _sqs = new AmazonSQSClient(
                new BasicAWSCredentials(Aws.AccessKey, Aws.SecretKey), 
                Aws.Region);
        }

        public int Send(string queueName, string messageBody)
        {
            var result = _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = QueueUrl(queueName),
                MessageBody = messageBody,
                MessageGroupId = queueName
            }).Result;
            return (int) result.HttpStatusCode;
        }

        public int Delete(string queueName, string receiptHandle)
        {
            var result = _sqs.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = QueueUrl(queueName),
                ReceiptHandle = receiptHandle
            }).Result;
            return (int) result.HttpStatusCode;
        }

        public string Receive(string queueName, int maxNumberOfMessages)
        {
            var result = _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = QueueUrl(queueName),
                MaxNumberOfMessages = maxNumberOfMessages,
                WaitTimeSeconds = ReceiveWaitTimeSec
            }).Result;
            return SerializeReceivedMessages(result.HttpStatusCode, result.Messages);
        }

        private static string SerializeReceivedMessages(HttpStatusCode httpStatus, List<Message> messages)
        {
            return JsonConvert.SerializeObject(new ReceiveResponse
            {
                HttpStatusCode = (int) httpStatus,
                ReceivedMessages = messages.Select(m =>
                    new ReceivedMessage
                    {
                        ReceiptHandle = m.ReceiptHandle,
                        Body = m.Body
                    }).ToArray()
            });
        }
    }
}