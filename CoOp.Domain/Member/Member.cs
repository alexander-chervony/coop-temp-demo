/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using EventFlow.Entities;

namespace CoOp.Domain
{
    public class Member : Entity<MemberId>, IMemberUIdData
    {
        public string FirstName { get; }
        public string MiddleName { get; }
        public string LastName { get; }
        public string CellPhoneNumber { get; }
        public string Email { get; }
        public string DocumentType { get; }
        public DateTime BirthDate { get; }
        public string PassportSn { get; }
        public string PassportPn { get; }
        public string PassportIssuedBy { get; }
        public DateTime PassportIssuedDate { get; }
        public DateTime PassportExpDate { get; }
        public string Country { get; }
        public string Address { get; }
        /// <summary>
        /// Reffered by this member id. There will be another structure for attraction by consultants.
        /// </summary>
        public MemberId RefererId { get; }
        public string ReferralCode { get; }
        public List<Immovables> Immovables { get; } = new List<Immovables>();

        public Member(MemberRegisteredEvent e) : base(e.MemberId)
        {
            FirstName = e.FirstName;
            MiddleName = e.MiddleName;
            LastName = e.LastName;
            CellPhoneNumber = e.CellPhoneNumber;
            Email = e.Email;
            DocumentType = e.DocumentType;
            BirthDate = e.BirthDate;
            PassportSn = e.PassportSn;
            PassportPn = e.PassportPn;
            PassportIssuedBy = e.PassportIssuedBy;
            PassportIssuedDate = e.PassportIssuedDate;
            PassportExpDate = e.PassportExpDate;
            Country = e.Country;
            Address = e.Address;
            ReferralCode = e.ReferralCode;
            RefererId = e.RefererId;
        }

        public void AddImmovables(Immovables immovables)
        {
            Immovables.Add(immovables);
        }
    }
}