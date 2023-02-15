/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;

namespace CoOp.Domain.Sqs
{
    // todo: store aws keys per CoopId

    public class SqsClientFactory
    {
        private readonly IConfiguration _configuration;

        public SqsClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IAmazonSQS CreateSqsClient()
        {
            return new AmazonSQSClient(new BasicAWSCredentials(
                    _configuration["Aws:AccessKey"],
                    _configuration["Aws:SecretKey"]),
                RegionEndpoint.GetBySystemName(_configuration["Aws:Region"]));
        }
    }
}