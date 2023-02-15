/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using EventFlow.Commands;

namespace CoOp.Domain
{
    public class RegisterMemberCommand : Command<CoOpAR, CoOpId>, IMemberUIdData
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        // Symbol "+" + Country code + operator code + phone number, for ex: +375291234567
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

        public RegisterMemberCommand(CoOpId aggregateId, string firstName, string middleName, string lastName,
            string cellPhoneNumber, string email, string documentType, DateTime birthDate, string passportSn,
            string passportPn, string passportIssuedBy, DateTime passportIssuedDate, DateTime passportExpDate,
            string country, string address, MemberId refererId) : base(aggregateId)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            MiddleName = middleName ?? throw new ArgumentNullException(nameof(middleName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            CellPhoneNumber = cellPhoneNumber ?? throw new ArgumentNullException(nameof(cellPhoneNumber));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            DocumentType = documentType ?? throw new ArgumentNullException(nameof(documentType));
            if (birthDate == default) throw new ArgumentNullException(nameof(birthDate));
            BirthDate = birthDate;
            PassportSn = passportSn ?? throw new ArgumentNullException(nameof(passportSn));
            PassportPn = passportPn ?? throw new ArgumentNullException(nameof(passportPn));
            PassportIssuedBy = passportIssuedBy ?? throw new ArgumentNullException(nameof(passportIssuedBy));
            if (passportIssuedDate == default) throw new ArgumentNullException(nameof(passportIssuedDate));
            PassportIssuedDate = passportIssuedDate;
            if (passportExpDate == default) throw new ArgumentNullException(nameof(passportExpDate));
            PassportExpDate = passportExpDate;
            Country = country ?? throw new ArgumentNullException(nameof(country));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            RefererId = refererId;
        }
    }
}