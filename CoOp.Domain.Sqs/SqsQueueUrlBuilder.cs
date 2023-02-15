/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using Microsoft.Extensions.Configuration;

namespace CoOp.Domain.Sqs
{
    public class SqsQueueUrlBuilder
    {
        private readonly IConfiguration _configuration;

        public SqsQueueUrlBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }
            
        public string Build(string name) => 
            $"https://sqs.{_configuration["Aws:Region"]}.amazonaws.com/{_configuration["Aws:AccountId"]}/{_configuration["Aws:SqsPrefix"]}--{name}";
    }
}