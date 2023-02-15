/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Aggregates;

namespace CoOp.Domain
{
    public class MemberRegisteredEvent : AggregateEvent<CoOpAR, CoOpId>
    {
        public MemberId MemberId { get; set; }
        public string MemberUId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string CellPhoneNumber { get; set; }
        public string Email { get; set; }
        public string DocumentType { get; set; }
        public DateTime BirthDate { get; set; }
        public string PassportSn { get; set; }
        public string PassportPn { get; set; }
        public string PassportIssuedBy { get; set; }
        public DateTime PassportIssuedDate { get; set; }
        public DateTime PassportExpDate { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string ReferralCode { get; set; }
        public MemberId RefererId { get; set; }
        
        public static MemberRegisteredEvent From(RegisterMemberCommand m)
        {
            return new MemberRegisteredEvent
            {
                MemberId = m.GetMemberId(),
                MemberUId = m.GetUId(),
                FirstName = m.FirstName,
                MiddleName = m.MiddleName,
                LastName = m.LastName,
                CellPhoneNumber = m.CellPhoneNumber,
                Email = m.Email,
                DocumentType = m.DocumentType,
                BirthDate = m.BirthDate,
                PassportSn = m.PassportSn,
                PassportPn = m.PassportPn,
                PassportIssuedBy = m.PassportIssuedBy,
                PassportIssuedDate = m.PassportIssuedDate,
                PassportExpDate = m.PassportExpDate,
                Country = m.Country,
                Address = m.Address,
                ReferralCode = m.ReferralCode,
                RefererId = m.RefererId,
            };
        }
    }
}