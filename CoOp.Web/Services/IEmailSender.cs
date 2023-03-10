/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoOp.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
