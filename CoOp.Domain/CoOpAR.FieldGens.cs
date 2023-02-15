/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Linq;
using EventFlow.Exceptions;

namespace CoOp.Domain
{
    public partial class CoOpAR
    {
        private int GetContractNo(DateTime today)
        {
            var todayContracts = _members.Values
                .SelectMany(q => q.Immovables.Where(x => x.ContractDate.Date == today)
                    .Select(i => i.ContractNo)).ToArray();
            var todayNo = todayContracts.Length > 0 
                ? todayContracts.Max() 
                : int.Parse(today.ToString("yyMMdd") + "00");
            return todayNo + 1;
        }

        private string GetRandomReferrerCode()
        {
            var allCodes = _members.Values.Select(m => m.ReferralCode).ToDictionary(x=>x);
            for (int i = 3; i <= 5; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    var code = RandomIdGenerator.GetBase36(i);
                    if (!allCodes.ContainsKey(code))
                    {
                        return code;
                    }
                }
            }
            throw DomainError.With("Cant generate unique Random Referrer Code for reasonable time.");
        }
    }
}