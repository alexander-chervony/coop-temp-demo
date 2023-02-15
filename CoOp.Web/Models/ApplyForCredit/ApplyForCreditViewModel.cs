/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoOp.Web.Models.ApplyForCredit;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NUglify.Helpers;

namespace CoOp.Web.Models.ApplyForCredit
{

    
    public class ApplyForCreditInputModel
    {
            public string   Fio                          { get; set;}
            public string   LastNameOld                  { get; set;}
            public string   CellPhoneNumber              { get; set;}
            public string   DocumentType                 { get; set;}
            public DateTime? BirthDate                   { get; set;}
            public string   PassportSn                   { get; set;}
            public string   PassportPn                   { get; set;}
            public string   PassportIssuedBy             { get; set;}
            public DateTime?   PassportIssuedDate        { get; set;}
            public DateTime?   PassportExpDate           { get; set;}
            public string   Address                      { get; set;}
            public string   Product                      { get; set;}
            public int      SumToCredit                  { get; set;}
            public int      Period                       { get; set;}
            public string   LivingIn                     { get; set;}
            public string   LivingAddress                { get; set;}
            public string   Education                    { get; set;}
            public string   MaritalStatus                { get; set;}
            public string   MaritalPartnerName           { get; set;}
            public string   MaritalPartnerCell           { get; set;}
            public int      ChildrenUnder18              { get; set;}
            public string   OrganizationType             { get; set;}
            public string   OrganizationNameAndAddress   { get; set;}
            public string   JobTitle                     { get; set;}
            public int      JobExpirienceLast            { get; set;}
            public int      JobExpirienceTotal           { get; set;}
            public string   JobPartnerNameAndCell        { get; set;}
            public int      IncomeAverage                { get; set;}
            public bool?    HaveACar                     { get; set;}
            public bool?    HaveAProperty                { get; set;}
            public string   PropertyType                 { get; set;}
            public bool?    HaveActiveCredit             { get; set;}
            public int      MonthlyPaymentCredit         { get; set;}
            public string   RelativeNameAndCell          { get; set;}
            public string   Passphrase                   { get; set;}
            
    }
    
}

public class ApplyForCreditInputModelValidator : AbstractValidator<ApplyForCreditInputModel>
{
    public ApplyForCreditInputModelValidator(IStringLocalizerFactory factory, IUrlHelper urlHelper)
    {
        var controller = urlHelper.ActionContext.RouteData.Values["Controller"];
        var action = urlHelper.ActionContext.RouteData.Values["Action"];
        var str = $"Views.{controller}.Index";
        var localizer = factory.Create(str,System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

        RuleFor(x => x.Fio)
            .Must(x => x?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 2)
            .WithMessage("Фамилия, имя и отчество должны быть указаны");

        RuleFor(x => x.CellPhoneNumber)
            .Must(x=> !x.IsNullOrWhiteSpace() )
            .WithMessage("Номер указывается в формате 375291234567")
            .Matches(@"^\d{12}$")
            .WithMessage("Номер указывается в формате 375291234567");
        
        RuleFor(x => x.DocumentType)
            .NotNull()
            .WithMessage("Необходимо указать тип документа");

        RuleFor(x => x.BirthDate)
            .NotNull()
            .WithMessage("Необходимо указать дату рождения")
            .LessThan(DateTime.Now.AddYears(-18))
            .WithMessage("Клиент должен быть не младше 18 лет");

        RuleFor(x => x.PassportSn)
            .NotNull()
            .WithMessage("Необходимо указать номер документа")
            .Length(9)
            .WithMessage("Номер документа должен содержать 9 символов");

        RuleFor(x => x.PassportPn)
            .NotNull()
            .WithMessage("Необходимо указать личный номер")
            .Length(14)
            .WithMessage("Личный номер должен содержать 14 символов");
        
        RuleFor(x => x.PassportIssuedBy)
            .NotEmpty()
            .WithMessage("Необходимо указать кем выдан документ");
        
        RuleFor(x => x.PassportIssuedDate)
            .NotNull()
            .WithMessage("Необходимо указать дату выдачи документа")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Дата выдачи документа не может быть больше сегодняшней");

        RuleFor(x => x.PassportExpDate)
            .NotNull()
            .WithMessage("Необходимо указать срок действия документа")
            .GreaterThanOrEqualTo(DateTime.Now)
            .WithMessage("Срок действия должен быть больше сегодняшней даты");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Необходимо указать адрес регистрации");

        RuleFor(x => x.Product)
            .NotEmpty()
            .WithMessage("Необходимо указать приобретаемый товар");

        RuleFor(x => x.SumToCredit)
            .GreaterThanOrEqualTo(50) // 50 и более рублей
            .WithMessage("Сумма кредита должна быть не менее 50 рублей");

        RuleFor(x => x.Period)
            .GreaterThanOrEqualTo(3) // 3 месяца и более рублей
            .WithMessage("Укажите срок кредитования от 3 до 60 месяцев")
            .LessThanOrEqualTo(60)
            .WithMessage("Укажите срок кредитования от 3 до 60 месяцев");

        RuleFor(x => x.LivingIn)
            .NotNull() // тип жилья
            .WithMessage("Необходимо указать тип жилья проживания");
        
        RuleFor(x => x.LivingAddress)
            .NotEmpty() // тип жилья
            .WithMessage("Необходимо указать адрес жилья проживания");

        RuleFor(x => x.Education)
            .NotEmpty() 
            .WithMessage("Необходимо указать образование");
        
        RuleFor(x => x.MaritalStatus)
            .NotNull() 
            .WithMessage("Необходимо указать семейное положение");

        RuleFor(x => x.ChildrenUnder18)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Укажите в диапазоне от 0 до 10")
            .LessThanOrEqualTo(10)
            .WithMessage("Укажите в диапазоне от 0 до 10");

        RuleFor(x => x.OrganizationType)
            .NotEmpty()
            .WithMessage("Укажите основное направление деятельности организации");

        RuleFor(x => x.OrganizationNameAndAddress)
            .NotEmpty()
            .WithMessage("Укажите наименование и адрес ИП/организации");

        RuleFor(x => x.JobExpirienceLast)
            .GreaterThanOrEqualTo(3) // месяца
            .WithMessage("Укажите количество месяцев в диапазоне 3-600")
            .LessThanOrEqualTo(600) // месяцев (50 лет)
            .WithMessage("Укажите количество месяцев в диапазоне 3-600");

        RuleFor(x => x.JobExpirienceTotal)
            .GreaterThanOrEqualTo(3) // месяца
            .WithMessage("Укажите количество месяцев в диапазоне 3-600")
            .LessThanOrEqualTo(600) // месяцев (50 лет)
            .WithMessage("Укажите количество месяцев в диапазоне 3-600");

        RuleFor(x => x.JobPartnerNameAndCell)
            .NotEmpty()
            .WithMessage("Укажите имя и номер тел коллеги");

        RuleFor(x => x.IncomeAverage)
            .GreaterThanOrEqualTo(0) // от 0 рублей (в декрете)
            .WithMessage("Укажите средний доход в месяц в диапазоне от 0 до 10000 руб")
            .LessThanOrEqualTo(10000) // до 10000 рублей
            .WithMessage("Укажите средний доход в месяц в диапазоне от 0 до 10000 руб");

        RuleFor(x => x.HaveACar)
            .NotNull()
            .WithMessage("Укажите, имеете ли автомобиль в собственности");
        
        RuleFor(x => x.HaveAProperty)
            .NotNull()
            .WithMessage("Укажите, имеете ли недвижимость в собственности");

        RuleFor(x => x.HaveActiveCredit)
            .NotNull()
            .WithMessage("Укажите, имеете ли кредит в данный момент");

        RuleFor(x => x.MonthlyPaymentCredit)
            .GreaterThanOrEqualTo(0) // от 0 рублей (нет кредита)
            .WithMessage("Сумма ежемесячного платежа по кредиту должна быть в диапазоне 0 - 10000 руб")
            .LessThanOrEqualTo(10000) // до 10000 рублей
            .WithMessage("Сумма ежемесячного платежа по кредиту должна быть в диапазоне 0 - 10000 руб");

        RuleFor(x => x.Passphrase)
            .NotEmpty() 
            .WithMessage("Необходимо указать кодовое слово");
    }
}