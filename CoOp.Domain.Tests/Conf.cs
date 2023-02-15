/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.IO;
using Microsoft.Extensions.Configuration;

namespace CoOp.Domain.Tests
{
    public static class Conf
    {
        public static IConfiguration Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.tests.json", optional: false, reloadOnChange: true)
                .Build();
            return config;
        }
    }
}