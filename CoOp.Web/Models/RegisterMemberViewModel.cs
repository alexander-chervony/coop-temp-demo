/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using CoOp.Domain;
using CoOp.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CoOp.Web.Models
{
    public class RegisterMemberViewModel : IMemberUIdData
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        // Symbol "+" + Country code + operator code + phone number, for ex: +375291234567
        public string CellPhoneNumber { get; set; }
        public string Email { get; set; }
        
        
        public string DocumentType { get; set; } = "passport_by";
        public DateTime BirthDate { get; set; }
        public string PassportSn { get; set; }
        public string PassportPn { get; set; }
        public string PassportIssuedBy { get; set; }
        public DateTime PassportIssuedDate { get; set; }
        public DateTime PassportExpDate { get; set; }

        public string Country { get; set; } = "Belarus";
        public string Address { get; set; }

        public MemberId RefererId { get; set; }
    }
}


public class RegisterMemberViewModelValidator : AbstractValidator<RegisterMemberViewModel>
{
    public RegisterMemberViewModelValidator(IStringLocalizerFactory factory, IUrlHelper urlHelper)
    {
        var controller = urlHelper.ActionContext.RouteData.Values["Controller"];
        var action = urlHelper.ActionContext.RouteData.Values["Action"];
        var str = $"Views.{controller}.{action}";
        var localizer = factory.Create(str,System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

    
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(2, 100)
            .WithName(localizer["FirstName.Label"].Value);
        
        RuleFor(x => x.FirstName)
            .Matches(@"^[\p{IsCyrillic}]+$")
            .WithName(localizer["FirstName.Label"].Value);
        
        RuleFor(x => x.MiddleName)
            .NotNull()
            .Length(2, 100)
            .WithName(localizer["MiddleName.Label"].Value);
        
        RuleFor(x => x.MiddleName)
            .Matches(@"^[\p{IsCyrillic}]+$")
            .WithName(localizer["MiddleName.Label"].Value);
        
        RuleFor(x => x.LastName)
            .NotNull()
            .Length(2, 100)
            .WithName(localizer["LastName.Label"].Value);
        
        RuleFor(x => x.LastName)
            .Matches(@"^[\p{IsCyrillic}]+$")
            .WithName(localizer["LastName.Label"].Value);
        
        RuleFor(x => x.CellPhoneNumber)
            .NotNull()
            .Length(13)
            .WithName(localizer["CellPhoneNumber.Label"].Value);
        
        RuleFor(x => x.CellPhoneNumber)
            .Matches(@"^\+\d{12}$")
            .WithName(localizer["CellPhoneNumber.Label"].Value)
            .WithMessage("Phone number must be like +375291234567");
        
        RuleFor(x => x.Email)
            .NotNull()
            .EmailAddress()
            .WithName(localizer["Email.Label"].Value);

        RuleFor(x => x.BirthDate)
            .NotNull()
            .NotEmpty()
            .LessThan(DateTime.Now.AddYears(-18))
            .WithName(localizer["BirthDate.Label"].Value)
            .WithMessage("You must be 18 years old");
        
        RuleFor(x => x.PassportSn)
            .NotNull()
            .Length(9)
            .WithName(localizer["PassportSn.Label"].Value);
        
        RuleFor(x => x.PassportPn)
            .NotNull()
            .Length(14)
            .WithName(localizer["PassportPn.Label"].Value);
        
        RuleFor(x => x.PassportIssuedBy)
            .NotNull()
            .NotEmpty()
            .WithName(localizer["PassportIssuedBy.Label"].Value)
            .WithMessage("Passport issuer must be specified");
        
        RuleFor(x => x.PassportIssuedDate)
            .NotNull().NotEmpty()
            .WithName(localizer["PassportIssuedDate.Label"].Value)
            .WithMessage("Date should be specified");
        
        RuleFor(x => x.PassportIssuedDate)
            .LessThan(DateTime.Now)
            .WithName(localizer["PassportIssuedDate.Label"].Value)
            .WithMessage("Date should be less than now");
        
        RuleFor(x => x.PassportExpDate)
            .NotNull()
            .NotEmpty()
            .GreaterThan(DateTime.Now)
            .WithName(localizer["PassportExpDate.Label"].Value)
            .WithMessage("Date should greater than now");

        RuleFor(x => x.Address)
            .NotNull()
            .NotEmpty()
            .WithName(localizer["Address.Label"].Value);
    }
}