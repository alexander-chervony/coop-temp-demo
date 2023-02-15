/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using Microsoft.Extensions.Hosting;

namespace CoOp.Web.Extensions
{
    public static class HostEnvironmentEnvExtensions
    {
        public static bool IsDevOrLocal(this IHostEnvironment hostEnvironment)
        {
            if (hostEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostEnvironment));
            }

            return hostEnvironment.IsEnvironment(Environments.Development) || 
                   hostEnvironment.IsEnvironment("Local");
        }
    }
}