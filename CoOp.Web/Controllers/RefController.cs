/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoOp.Domain;
using CoOp.Domain.Queries.InMemory;
using CoOp.Web.Identity;
using EventFlow.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoOp.Web.Controllers
{
    public class RefController : Controller
    {
        public const string ReferrerCookieName = "coOp.referrerHashedId";
        
        private readonly IQueryProcessor _queryProcessor;

        public RefController(
            IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }
        
        [Authorize(Policy = "Member")]
        public async Task<ActionResult<CoOpReadModel>> ReferralProgram()
        {
            var readModel = await _queryProcessor.ProcessAsync(new ReadModelByIdQuery<CoOpReadModel>(CoOpId.BlrRealty), CancellationToken.None);
            var memberId = new MemberId(User.GetMemberId());
            var curentMember = readModel.AllMembers[memberId];
            //ViewBag.ReferralLink = Url.Action("Index", "Ref", new {id = curentMember.RegInfo.ReferralCode},Request.Scheme).ToLower();
            ViewBag.ReferralLink = Url.Action("Index", "Ref", new {id = curentMember.RegInfo.ReferralCode},"https").ToLower();
            return View(readModel);
        }
        
        // todo: only for non-active member (or even non-member (simpler to implement) - direct to CoOp/RegisterMember)
        public async Task<ActionResult<CoOpReadModel>> Index(string id)
        {
            // todo: display error message "invalid referral link"
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            
            var readModel = await _queryProcessor.ProcessAsync(new ReadModelByIdQuery<CoOpReadModel>(CoOpId.BlrRealty), CancellationToken.None);
            if (null == readModel.AllMembers.Values.FirstOrDefault(m=>id.Equals(m.RegInfo.ReferralCode)))
                throw new Exception("Unknown referrer");
            
            Response.Cookies.Append(
                ReferrerCookieName,
                id, // ReferralCode
                new CookieOptions {Expires = DateTimeOffset.UtcNow.AddYears(20), IsEssential = true});
            
            return View();
        }
    }
}