/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;

namespace CoOp.Web.Identity
{
    public class ClaimCantBeReassignedException : Exception
    {
        public ClaimCantBeReassignedException(string claimType) : base($"{claimType} can not be reassigned")
        {
        }
    }
}