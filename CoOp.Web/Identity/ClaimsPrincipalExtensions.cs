/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Security.Claims;

namespace CoOp.Web.Identity
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetMemberId(this ClaimsPrincipal principal)
        {
            return GetClaim(principal, "MemberId");
        }
        
        public static string GetClaim(this ClaimsPrincipal principal, string claimType)
        {
            // use Identity instead of principal to evaluate only Main identity inside ClaimPrincipal,
            // to guarantee the same result of calling User.Identity.FindFirst and principal.FindFirst in case if they used in mix 
            var claim = ((ClaimsIdentity)principal.Identity).FindFirst(claimType);
            return claim?.Value;
        }
    }
}