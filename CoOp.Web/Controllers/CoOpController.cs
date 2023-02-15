/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using AutoMapper;
using CoOp.Domain;
using CoOp.Domain.Queries.InMemory;
using CoOp.Domain.Sqs;
using CoOp.Web.Controllers.Filters;
using CoOp.Web.Identity;
using CoOp.Web.Models;
using EventFlow;
using EventFlow.Extensions;
using EventFlow.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace CoOp.Web.Controllers
{
    [Authorize]
    public partial class CoOpController : Controller
    {
        private readonly ClaimsManager _claimsManager;
        private readonly IWebHostEnvironment _env;
        private readonly ICommandBus _commandBus;
        private readonly IQueryProcessor _queryProcessor;
        private readonly IAmazonSQS _sqs;
        private readonly SqsQueueUrlBuilder _sqsQueueUrlBuilder;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        public CoOpController(
            ClaimsManager claimsManager,
            IWebHostEnvironment env,
            ICommandBus commandBus,
            IQueryProcessor queryProcessor,
            IAmazonSQS sqs,
            SqsQueueUrlBuilder sqsQueueUrlBuilder,
            IMapper mapper,
            IAuthorizationService authorizationService)
        {
            _claimsManager = claimsManager;
            _env = env;
            _commandBus = commandBus;
            _queryProcessor = queryProcessor;
            _sqs = sqs;
            _sqsQueueUrlBuilder = sqsQueueUrlBuilder;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            return View("DisplayMembers");
        }

        public async Task<ActionResult<CoOpVm>> DisplayMembers()
        {
            var readModel = await GetReadModel();
            return Ok(await ToVm(readModel) ?? new CoOpVm());
        }

        public IActionResult RegisterMember()
        {
            // todo: validate referralCode cookie and show message "go find another referral" if its wrong
            return View();
        }
        
        [HttpPost]
        [AjaxValidation]
        public async Task<ActionResult> RegisterMember([FromBody]RegisterMemberViewModel vm)
        {
            var referralCode = Request.Cookies[RefController.ReferrerCookieName];
            var referrerId = !string.IsNullOrEmpty(referralCode)
                ? (await GetReadModel()).AllMembers.Values.First(m => referralCode.Equals(m.RegInfo.ReferralCode)).RegInfo.MemberId
                : null;

            // todo: tryPublish method returning bool and out errors and return json error
            _commandBus.Publish(new RegisterMemberCommand(
                CoOpId.BlrRealty,
                vm.FirstName,
                vm.MiddleName,
                vm.LastName,
                vm.CellPhoneNumber,
                vm.Email,
                CoOpAR.MemberDefaultDocType,
                vm.BirthDate,
                vm.PassportSn,
                vm.PassportPn,
                vm.PassportIssuedBy,
                vm.PassportIssuedDate,
                vm.PassportExpDate,
                vm.Country,
                vm.Address,
                referrerId));

            await AddClaimMemberId(vm.GetMemberId());

            return View("AddFirstImmovables");
        }

        [Authorize(Policy = "Member")]
        public IActionResult AddFirstImmovables()
        {
            return View();
        }
        
        [Authorize(Policy = "Member")]
        public IActionResult AddFirstImmovablesInit()
        {
            return Ok(new AddFirstImmovablesViewModel
            {
                MinImmovablesPrice = CoOpAR.MinImmovablesPrice,
                MaxImmovablesPrice = CoOpAR.MaxImmovablesPrice,
                MinContractTermYears = CoOpAR.MinContractTermYears,
                MaxContractTermYears = CoOpAR.MaxContractTermYears,
                ImmovablesPrice = CoOpAR.DefaultImmovablesPrice,
                ContractTermYears = CoOpAR.DefaultContractTermYears,
                Currency = CoOpAR.CoOpCurrency
            });
        }

        [HttpPost]
        [Authorize(Policy = "Member")]
        [AjaxValidation]
        public async Task<ActionResult> AddFirstImmovables([FromBody]AddFirstImmovablesViewModel vm)
        {
            if (string.IsNullOrEmpty(User.GetMemberId()))
                throw new Exception("Unknown MemberId. Did you add at least 1 user?");
            
            _commandBus.Publish(new AddFirstImmovablesCommand(CoOpId.BlrRealty,
                new MemberId(User.GetMemberId()),
                vm.ImmovablesPrice,
                vm.Currency,
                vm.ContractTermYears));
            
            await AddClaimImmovablesAdded();
            
            return Ok(vm);
        }
        
        [HttpPost]
        [Authorize(Policy = "Founder")]
        public async Task<IActionResult> InitiateImmovablesPurchase(int id)
        {
            _commandBus.Publish(new InitiateImmovablesPurchaseCommand(CoOpId.BlrRealty, id));
            
            var readModel = await GetReadModel();
            
            return Ok(await ToVm(readModel));
        }

        [HttpPost]
        [Authorize(Policy = "Founder")]
        public async Task<IActionResult> MarkImmovablesPurchased(int id)
        {
            _commandBus.Publish(new MarkImmovablesPurchasedCommand(CoOpId.BlrRealty, id));
            
            var readModel = await GetReadModel();
            
            return Ok(await ToVm(readModel));
        }
        
        
        
        [Authorize(Policy = "Member")]
        [Authorize(Policy = "ImmovablesAdded")]
        public IActionResult PleasePayEntranceFeeToProceed()
        {
            return View();
        }

        [Authorize(Policy = "ActivePaidMember")]
        public IActionResult ShowPayments()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize(Policy = "ActivePaidMember")]
        public async Task<ActionResult> ShowPayments([FromBody] string filter)
        {
            var readModel = await GetReadModel();
            var memberView = readModel.AllMembers[new MemberId(User.GetMemberId())];
            return Ok(memberView.Immovables);
        }

        
        private async Task AddClaimMemberId(MemberId memberId)
        {
            await _claimsManager.AddClaim("MemberId", memberId.Value);
        }

        private async Task AddClaimImmovablesAdded()
        {
            await _claimsManager.AddClaim("ImmovablesAdded", bool.TrueString);
        }

        private async Task<CoOpReadModel> GetReadModel()
        {
            return await _queryProcessor.ProcessAsync(new ReadModelByIdQuery<CoOpReadModel>(CoOpId.BlrRealty), CancellationToken.None);
        }
        
        private async Task<CoOpVm> ToVm(CoOpReadModel readModel)
        {
            if (readModel == null)
                return null;

            bool founder = (await _authorizationService.AuthorizeAsync(User, "Founder")).Succeeded;
            var vm = founder
                ? _mapper.Map<FoundersCoOpVm>(readModel)
                : _mapper.Map<CoOpVm>(readModel);
            
            vm.TotalPurchasedImmovablesFormattedPercent = 
                (readModel.TotalPurchasedImmovables / readModel.AllMembers.Values.SelectMany(m => m.Immovables)
                     .Where(v => v.State != ImmovablesState.Purchased)
                     .Sum(iv=> iv.Price)).ToString("P2");

            if (!founder)
            {
                vm.AllMembers = vm.AllMembers.Where(pair =>
                    pair.Key.Equals(new MemberId(User.GetMemberId()))).ToDictionary(pair=>pair.Key,pair => pair.Value);
            }
            
            return vm;
        }
    }
}