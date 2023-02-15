/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CoOp.Domain;
using CoOp.Domain.Queries.InMemory;
using CoOp.Web.Extensions;
using EventFlow.Queries;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;

namespace CoOp.Web.Identity
{
    /// <summary>
    /// Changes/updates claims according to read model state.
    /// Note: Performed on each request, so can cause certain performance penalty.
    ///  todo: add LastUpdatedDateTime claim and perform update once in 5 min only
    /// </summary>
    public class MemberClaimsTransformer : IClaimsTransformation
    {
        public static readonly IEnumerable<string> AllClaims = new[] { "MemberId", "ImmovablesAdded", "ActivePaidMember" }; 
        
        private readonly ClaimsManager _claimsManager;
        private readonly IQueryProcessor _queryProcessor;
        private readonly IWebHostEnvironment _env;
        
        public MemberClaimsTransformer(
            ClaimsManager claimsManager,
            IQueryProcessor queryProcessor,
            IWebHostEnvironment env)
        {
            _claimsManager = claimsManager;
            _queryProcessor = queryProcessor;
            _env = env;
        }

        // Each time HttpContext.AuthenticateAsync() or HttpContext.SignInAsync(...) is called the claims transformer is invoked. So this might be invoked multiple times. 
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identities.FirstOrDefault(x => x.IsAuthenticated);
            if (identity == null) 
                return principal;
            
            var mid = principal.GetClaim("MemberId");
            // registered but member not created yet
            if (mid == null) 
                return principal;
            
            // cleanup obsolete claims for dev (state of the user for previous read only model is not actual now)
            if (_env.IsDevOrLocal())
            {
                var rm = await GetReadModel();
                if (rm == null || !rm.AllMembers.ContainsKey(new MemberId(mid)))
                {
                    await _claimsManager.RemoveClaims(principal, AllClaims);
                }
            }

            bool needToCheckActivePaidMemberClaim =
                bool.TrueString.Equals(principal.GetClaim("ImmovablesAdded")) &&
                !bool.TrueString.Equals(principal.GetClaim("ActivePaidMember"));
            
            if (!needToCheckActivePaidMemberClaim)
                return principal;
            
            var readModel = await GetReadModel();
            
            // on application start it's ok to skip once this check
            if (readModel == null)
                return principal;

            var memberId = new MemberId(mid);
            if (readModel.AllMembers[memberId].HasActiveEntranceFeePaidImmovables)
                await _claimsManager.AddClaim(principal,"ActivePaidMember", bool.TrueString, false);
            else
               await _claimsManager.RemoveClaim(principal, "ActivePaidMember", false);

            return new ClaimsPrincipal(identity);
        }

        private Task<CoOpReadModel> GetReadModel()
        {
            return _queryProcessor.ProcessAsync(new ReadModelByIdQuery<CoOpReadModel>(CoOpId.BlrRealty), CancellationToken.None);
        }
    }
}