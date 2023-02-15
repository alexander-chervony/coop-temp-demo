/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class ImmovablesPurchaseCommand : Command<CoOpAR, CoOpId>
    {
        protected ImmovablesPurchaseCommand(
            CoOpId id,
            int contractNo)
            : base(id)
        {
            if (contractNo <= 0) throw new ArgumentNullException(nameof(contractNo));
            ContractNo = contractNo;
        }

        public int ContractNo { get; }
    }
}