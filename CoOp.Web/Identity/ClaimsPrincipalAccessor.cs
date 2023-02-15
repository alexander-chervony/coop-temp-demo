/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CoOp.Web.Identity
{
    public class ClaimsPrincipalAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipalAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal ClaimsPrincipal => _httpContextAccessor.HttpContext.User;
    }
}