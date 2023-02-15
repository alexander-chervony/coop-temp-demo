/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using EventFlow.Commands;

namespace CoOp.Domain
{
    public class DeleteImmovablesFeeCommand :  Command<CoOpAR, CoOpId>, IDelete
    {
        public DeleteImmovablesFeeCommand(CoOpId id, string paymentId1C, int contractNo) : base(id)
        {
            PaymentId1C = paymentId1C;
            ContractNo = contractNo;
        }
        
        public string PaymentId1C { get; }
        public int ContractNo { get; }
    }
}