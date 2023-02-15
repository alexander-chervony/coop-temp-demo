/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoOp.Web.Extensions;
using CoOp.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace CoOp.Web.Identity
{
    public class ClaimsManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly ClaimsPrincipalAccessor _currentPrincipalAccessor;

        public ClaimsManager(
            ClaimsPrincipalAccessor currentPrincipalAccessor,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env)
        {
            _currentPrincipalAccessor = currentPrincipalAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        public async Task ImpersonateUserByApplyingHisClaims(string email, string memberId)
        {
            var impersonatedUser = await _userManager.FindByEmailAsync(email);
            var currentUser = await _userManager.GetUserAsync(_currentPrincipalAccessor.ClaimsPrincipal);
            const string memberIdClaimType = "MemberId";
           // if same user by email, change only member id
            if (currentUser.Email.Equals(email))
            {
                await AddUpdateClaim(memberIdClaimType, memberId, false);
            }
            else
            {
                var impersonatedUserClaims = (await _userManager.GetClaimsAsync(impersonatedUser))
                    .Where(c => !c.Type.Equals(memberIdClaimType)).ToList();
                // this is needed to workaround situation when one admin tries to impersonate another one which also impersonated third user
                // so just assign this member id that is passed to user
                impersonatedUserClaims.Add(new Claim(memberIdClaimType, memberId));
                await ClearAllUserClaims();
                foreach (var claim in impersonatedUserClaims)
                {
                    await AddUpdateClaim(claim.Type, claim.Value, false);
                }
            }
            await _signInManager.RefreshSignInAsync(currentUser);
        }

        public async Task ClearAllUserClaims(bool refreshSignIn = false)
        {
            await RemoveClaims(MemberClaimsTransformer.AllClaims, refreshSignIn);
        }

        /// <param name="refreshSignin">Sometimes (e.g. when adding multiple claims at once) it is desirable to refresh cookie only once, for the last one </param>
        public async Task AddUpdateClaim(string claimType, string claimValue, bool refreshSignin = true)
        {
            await AddClaim(
                _currentPrincipalAccessor.ClaimsPrincipal,
                claimType,
                claimValue, 
                async user =>
                {
                    await RemoveClaim(_currentPrincipalAccessor.ClaimsPrincipal, user, claimType);
                },
                refreshSignin);
        }

        public async Task AddClaim(string claimType, string claimValue, bool refreshSignin = true)
        {
            await AddClaim(_currentPrincipalAccessor.ClaimsPrincipal, claimType, claimValue, refreshSignin);
        }
        
        /// <summary>
        /// At certain stages of user auth there is no user yet in context but there is one to work with in client code (e.g. calling from ClaimsTransformer)
        /// that's why we have principal as param
        /// </summary>
        public async Task AddClaim(ClaimsPrincipal principal, string claimType, string claimValue, bool refreshSignin = true)
        {
            await AddClaim(
                principal,
                claimType,
                claimValue, 
                async user =>
                {
                    // allow reassignment in dev
                    if (_env.IsDevOrLocal()) 
                        await RemoveClaim(principal, user, claimType);

                    if (GetClaim(principal, claimType) != null)
                        throw new ClaimCantBeReassignedException(claimType);                
                },
                refreshSignin);
        }

        public async Task RemoveClaims(IEnumerable<string> claimTypes, bool refreshSignin = true)
        {
            await RemoveClaims(_currentPrincipalAccessor.ClaimsPrincipal, claimTypes, refreshSignin);
        }
        
        public async Task RemoveClaims(ClaimsPrincipal principal, IEnumerable<string> claimTypes, bool refreshSignin = true)
        {
            AssertAuthenticated(principal);
            foreach (var claimType in claimTypes)
            {
                await RemoveClaim(principal, claimType);
            }
            // reflect the change in the Identity cookie
            if (refreshSignin)
                await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(principal));
        }

        public async Task RemoveClaim(string claimType, bool refreshSignin = true)
        {
            await RemoveClaim(_currentPrincipalAccessor.ClaimsPrincipal, claimType, refreshSignin);
        }
        
        public async Task RemoveClaim(ClaimsPrincipal principal, string claimType, bool refreshSignin = true)
        {
            AssertAuthenticated(principal);
            var user = await _userManager.GetUserAsync(principal);
            await RemoveClaim(principal, user, claimType);
            // reflect the change in the Identity cookie
            if (refreshSignin)
                await _signInManager.RefreshSignInAsync(user);
        }
        
        private async Task AddClaim(ClaimsPrincipal principal, string claimType, string claimValue, Func<ApplicationUser, Task> processExistingClaims, bool refreshSignin)
        {
            AssertAuthenticated(principal);
            var user = await _userManager.GetUserAsync(principal);
            await processExistingClaims(user);
            var claim = new Claim(claimType, claimValue);
            ClaimsIdentity(principal).AddClaim(claim);
            await _userManager.AddClaimAsync(user, claim);
            // reflect the change in the Identity cookie
            if (refreshSignin)
                await _signInManager.RefreshSignInAsync(user);
        }

        /// <summary>
        /// Due to bugs or as result of debug it can be more than one identity of the same type.
        /// The method removes all the claims of a given type.
        /// </summary>
        private async Task RemoveClaim(ClaimsPrincipal principal, ApplicationUser user, string claimType)
        {
            AssertAuthenticated(principal);
            var identity = ClaimsIdentity(principal);
            var claims = identity.FindAll(claimType).ToArray();
            if (claims.Length > 0)
            {
                await _userManager.RemoveClaimsAsync(user, claims);
                foreach (var c in claims)
                {
                    identity.RemoveClaim(c);
                }
            }
        }
        
        private static Claim GetClaim(ClaimsPrincipal principal, string claimType)
        {
            return ClaimsIdentity(principal).FindFirst(claimType);    
        }    

        /// <summary>
        /// This kind of bugs has to be found during testing phase
        /// </summary>
        private static void AssertAuthenticated(ClaimsPrincipal principal)
        {
            if (!principal.Identity.IsAuthenticated)
                throw new InvalidOperationException("User should be authenticated in order to update claims");
        }

        private static ClaimsIdentity ClaimsIdentity(ClaimsPrincipal principal)
        {
            return (ClaimsIdentity) principal.Identity;
        }
    }
}