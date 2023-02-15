/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Aws4RequestSigner;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Wmhelp.XPath2;

namespace CoOp.Domain.Tests
{
    public class SqsHttpTests
    {
        private readonly IConfiguration _configuration = Conf.Init();
        
        //[Test]
        public async Task Send_By_Get_Method()
        {
            var signer = new AWS4RequestSigner(_configuration["Aws:AccessKey"], _configuration["Aws:SecretKey"]);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://sqs.eu-central-1.amazonaws.com/804778673837/test-string-queue.fifo?Action=SendMessage&MessageBody=hello3&MessageGroupId=1")
            };

            request = await signer.Sign(request, "sqs", _configuration["Aws:Region"]);

            var client = new HttpClient();
            var response = await client.SendAsync(request);
        }
        
        //[Test]
        public async Task Send_By_Post_Method()
        {
            var signer = new AWS4RequestSigner(_configuration["Aws:AccessKey"], _configuration["Aws:SecretKey"]);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://sqs.eu-central-1.amazonaws.com/804778673837/test-string-queue.fifo")
            };

            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Action", "SendMessage"),
                new KeyValuePair<string, string>("MessageBody", "hello fifo queue 2"),
                new KeyValuePair<string, string>("MessageGroupId", "1")
            };

            request.Content = new FormUrlEncodedContent(parameters);

            request = await signer.Sign(request, "sqs", _configuration["Aws:Region"]);

            var client = new HttpClient();
            var response = await client.SendAsync(request);
        }
        
        //[Test]
        public async Task ReceiveAndDelete()
        {
            // receive
            var signer = new AWS4RequestSigner(_configuration["Aws:AccessKey"], _configuration["Aws:SecretKey"]);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://sqs.eu-central-1.amazonaws.com/804778673837/test-string-queue.fifo?Action=ReceiveMessage&MaxNumberOfMessages=1")
            };

            request = await signer.Sign(request, "sqs", _configuration["Aws:Region"]);

            var client = new HttpClient();
            var response = await client.SendAsync(request);
            var responseStr = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseStr);
            
            // delete
            var doc = XDocument.Parse(responseStr);
            var receiptHandle = doc.XPath2SelectElement("//*:ReceiptHandle").Value;
            
            var deleteRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://sqs.eu-central-1.amazonaws.com/804778673837/test-string-queue.fifo?Action=DeleteMessage&ReceiptHandle={HttpUtility.UrlEncode(receiptHandle)}")
            };
            deleteRequest = await signer.Sign(deleteRequest, "sqs", _configuration["Aws:Region"]);
            var deleteResponse = await client.SendAsync(deleteRequest);
            Console.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
        }
    }
}