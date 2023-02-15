/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using CoOp.Domain;
using CoOp.Domain.Queries.InMemory;
using CoOp.Domain.Tests.Data;
using CoOp.Web.Models;
using EventFlow.Commands;
using EventFlow.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoOp.Web.Controllers
{
    /// <summary>
    /// todo: should work in dev environment only
    /// </summary>
    public partial class CoOpController
    {
        [Authorize(Policy = "Founder")]
        public async Task<ActionResult> ImpersonateNewEmptyMember()
        {
            var memberId = _commandBus.RegisterRandomEmptyMember(User.Identity.Name);
            await _claimsManager.ClearAllUserClaims();
            await AddClaimMemberId(memberId);
            return Redirect("/CoOp/Index");
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult> ImpersonateNewMemberWithImmovables()
        {
            var memberId = _commandBus.RegisterRandomMemberWithImmovables(User.Identity.Name);
            await _claimsManager.ClearAllUserClaims();
            await AddClaimMemberId(memberId);
            await AddClaimImmovablesAdded();
            return Redirect("/CoOp/Index");
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult> ImpersonateNewActiveMember()
        {
            var memberId = await _commandBus.RegisterRandomMemberWithImmovablesAndEntranceFeePaid(_queryProcessor, User.Identity.Name);
            await _claimsManager.ClearAllUserClaims();
            await AddClaimMemberId(memberId);
            await AddClaimImmovablesAdded();
            return Redirect("/CoOp/Index");
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> ImpersonateExistingMember(string id)
        {
            async Task<string> GetEmail()
            {
                var memberId = new MemberId(id);
                var readModel = await GetReadModel();
                return readModel.AllMembers[memberId].RegInfo.Email;
            }

            // todo: check if not already impersonated and implement stop impersonation as here (and see my comment - in fact we would ned to store all claims before replacing OR implement another impersonation option - just login from the user we want to impersonate): https://stackoverflow.com/questions/24161782/how-do-i-use-asp-net-identity-2-0-to-allow-a-user-to-impersonate-another-user
            await _claimsManager.ImpersonateUserByApplyingHisClaims(await GetEmail(), id);
            return Redirect("/CoOp/Index");
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> Add1ActiveMember()
        {
            await _commandBus.RegisterRandomMemberWithImmovablesAndEntranceFeePaid(_queryProcessor, User.Identity.Name);
            return await DisplayMembers();
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> Add10ActiveMembers()
        {
            for (int i = 0; i < 10; i++)
            {
                await _commandBus.RegisterRandomMemberWithImmovablesAndEntranceFeePaid(_queryProcessor, User.Identity.Name);
            }

            return await DisplayMembers();
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> Add1MonthPayment(string id)
        {
            var readModel = await GetReadModel();
            PaySameAsEntranceFee(GetImmovablesRm(readModel, id));
            return await DisplayMembers();
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> AddPayment(string immovablesId, int paymentPercent)
        {
            var readModel = await GetReadModel();
            await AddPaymentPercent(
                GetImmovablesRm(readModel, immovablesId),
                paymentPercent);
            return await DisplayMembers();
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> Add1MonthPayments()
        {
            PaySameAsEntranceFeeOnceForEachMember(await GetReadModel());
            return await DisplayMembers();
        }

        [Authorize(Policy = "Founder")]
        public async Task<ActionResult<CoOpVm>> Add12MonthPayments()
        {
            var readModel = await GetReadModel();
            for (int i = 0; i < 12; i++)
            {
                PaySameAsEntranceFeeOnceForEachMember(readModel);
            }

            return await DisplayMembers();
        }

        private void PaySameAsEntranceFeeOnceForEachMember(CoOpReadModel readModel)
        {
            // for each immovables pay same amount as entrance fee
            foreach (var immovables in GetAllImmovables(readModel))
            {
                PaySameAsEntranceFee(immovables);
            }
        }

        private void PaySameAsEntranceFee(ImmovablesRm immovables)
        {
            _commandBus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty,
                Guid.NewGuid().ToString(),
                DateTime.Now,
                immovables.ContractNo,
                immovables.RequiredEntranceFee,
                immovables.Currency));
        }

        private async Task AddPaymentPercent(ImmovablesRm iv, int paymentPercent)
        {
            //_commandBus.Publish
            await SendMessage("1C-AddImmovablesFee.fifo",
                new AddImmovablesFeeCommand(CoOpId.BlrRealty,
                    Guid.NewGuid().ToString(),
                    (iv.Fees.LastPaymentDate == default ? DateTime.Now : iv.Fees.LastPaymentDate).AddMonths(1),
                    iv.ContractNo,
                    iv.Price * paymentPercent / 100,
                    iv.Currency),
                CancellationToken.None);
        }

        private async Task SendMessage<TCommand>(string queueName, TCommand command,
            CancellationToken cancellationToken)
            where TCommand : Command<CoOpAR, CoOpId>
        {
            await _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _sqsQueueUrlBuilder.Build(queueName),
                MessageBody = JsonConvert.SerializeObject(command, Formatting.Indented),
                MessageGroupId = queueName,
            }, cancellationToken);
        }
        
        private static ImmovablesRm GetImmovablesRm(CoOpReadModel readModel, string immovablesId) => 
            GetAllImmovables(readModel).First(iv => iv.ImmovablesId.Value.Equals(immovablesId));

        private static IEnumerable<ImmovablesRm> GetAllImmovables(CoOpReadModel readModel) => 
            readModel.AllMembers.Values.SelectMany(m => m.Immovables);
    }
}