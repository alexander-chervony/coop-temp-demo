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
    public class AddFirstImmovablesViewModel
    {
        public double ImmovablesPrice { get; set; }
        public double MinImmovablesPrice { get; set; } 
        public double MaxImmovablesPrice { get; set; } 
        public int ContractTermYears { get; set; }
        public int MinContractTermYears { get; set; }  
        public int MaxContractTermYears { get; set; } 
        public string Currency { get; set; }
    }
}

public class AddFirstImmovablesViewModelValidator : AbstractValidator<AddFirstImmovablesViewModel>
{
    public AddFirstImmovablesViewModelValidator(IStringLocalizerFactory factory, IUrlHelper urlHelper)
    {
        var controller = urlHelper.ActionContext.RouteData.Values["Controller"];
        var action = urlHelper.ActionContext.RouteData.Values["Action"];
        var str = $"Views.{controller}.{action}";
        var localizer = factory.Create(str,System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

        RuleFor(x => x.ImmovablesPrice)
            .NotEmpty()
            .GreaterThanOrEqualTo(x=>x.MinImmovablesPrice)
            .LessThanOrEqualTo(x=>x.MaxImmovablesPrice)
            .WithName(localizer["ImmovablesPrice.Label"].Value);
        RuleFor(x => x.ContractTermYears)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.MinContractTermYears)
            .LessThanOrEqualTo(x => x.MaxContractTermYears)
            .WithName(localizer["ContractTermYears.Label"].Value);
    }
}