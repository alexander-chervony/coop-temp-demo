/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoOp.Domain.Queries.InMemory;
using EventFlow;
using EventFlow.Extensions;
using EventFlow.Queries;

namespace CoOp.Domain.Tests.Data
{
    public static partial class TestData
    {
        public static MemberId RegisterRandomEmptyMember(this ICommandBus bus, string email = null)
        {
            var memberId = bus.RegisterEmptyMember(Guid.NewGuid().ToString(), email);
            return memberId;
        }

        public static MemberId RegisterRandomMemberWithImmovables(this ICommandBus bus, string email = null)
        {
            var (price, _) = GetImmovablesPrice();
            var memberId = bus.RegisterMemberWithImmovables(Guid.NewGuid().ToString(), price, email);
            return memberId;
        }

        public static async Task<MemberId> RegisterRandomMemberWithImmovablesAndEntranceFeePaid(this ICommandBus bus, IQueryProcessor queryProcessor, string email = null)
        {
            var (price, entranceFee) = GetImmovablesPrice();
            var memberId = bus.RegisterMemberWithImmovables(Guid.NewGuid().ToString(), price, email);
            var contractNo = (await GetReadModel(queryProcessor)).AllMembers[memberId].Immovables.First()
                .ContractNo;
            bus.Publish(new AddImmovablesFeeCommand(CoOpId.BlrRealty,
                Guid.NewGuid().ToString(), 
                    DateTime.Now,
                    contractNo,
                    entranceFee,
                    CoOpAR.CoOpCurrency));
            return memberId;
        }

        public static MemberId RegisterEmptyMember(this ICommandBus bus, string mid, string email = null)
        {
            bus.Publish(RegisterMemberCommand(mid, email));
            return GetMemberId(mid);
        }

        public static MemberId RegisterMemberWithImmovables(this ICommandBus bus, string mid, double price, string email = null)
        {
            var memberId = GetMemberId(mid);
                
            bus.Publish(RegisterMemberCommand(mid, email));
            bus.Publish(new AddFirstImmovablesCommand(
                CoOpId.BlrRealty, 
                memberId,
                price,
                "BYN",
                10));
            
            return memberId;
        }

        public static RegisterMemberCommand RegisterMemberCommand(string id, string email = null)
        {
            var (first, middle, last) = GetRandomName();
            return new RegisterMemberCommand(
                CoOpId.BlrRealty,
                first,
                middle,
                last,
                "000000000000",
                email ?? "x.y@gmail.com",
                "passport_by",
                DateTime.Now.AddYears(-20),
                "AA00000",
                id,
                "MIA",
                DateTime.Now,
                DateTime.Now.AddYears(10),
                "Belarus",
                "XXX",
                null);
        }

        // todo: make calculation closer to reality (with the moment immovables gets purchased and 0.5% stuff)
        private static (double price, double entranceFee) GetImmovablesPrice()
        {
            var price = new Random().Next((int) (CoOpAR.MinImmovablesPrice/1000), (int) (CoOpAR.MaxImmovablesPrice/1000)) * 1000;
            return (price, price*CoOpAR.EntranceFeeFromPriceRatio);
        }

        private static MemberId GetMemberId(string id) => 
            MemberId.NewDeterministic(id);

        private static (string, string, string) GetRandomName()
        {
            return (
                Names[new Random().Next(Names.Length)],
                Middlenames[new Random().Next(Middlenames.Length)],
                Lastnames[new Random().Next(Lastnames.Length)]);
        }
        
        private static async Task<CoOpReadModel> GetReadModel(IQueryProcessor queryProcessor)
        {
            return await queryProcessor.ProcessAsync(new ReadModelByIdQuery<CoOpReadModel>(CoOpId.BlrRealty), CancellationToken.None);
        }

    }
}