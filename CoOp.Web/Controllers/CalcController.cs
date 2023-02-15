/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using CoOp.Domain;
using CoOp.Web.Models.Calc;
using EventFlow.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;


namespace CoOp.Web.Controllers
{
    public class CalcController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly IStringLocalizer _localizer;

        public CalcController(IQueryProcessor queryProcessor, IStringLocalizerFactory factory)
        {
            _queryProcessor = queryProcessor;
            _localizer = factory.Create("Views.Calc.Index", 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate([FromBody] CalcInputModel i)
        {
            const double bankAdditionalFee = 500; // оформление договора займа
            var coopEntranceFee = i.ImmovablesValue * 0.005; // 0.5% в данный момент

            var bankMonthlyPayments = BuildMonthlyPayments(i, false);
            var coopMonthlyPayments = BuildMonthlyPayments(i, true);


            var outputModel = new CalcOutputModel
            {
                ImmovablesValue = i.ImmovablesValue,
                BankAdditionalFee = bankAdditionalFee,
                CoopEntranceFee = coopEntranceFee,
                BankMonthlyPayments = bankMonthlyPayments,
                CoopMonthlyPayments = coopMonthlyPayments,
                CoopInterestRate = CalculateMembershipFeeRate(i),
            };

            outputModel.CoopMonthlyPayments.Insert(0, new PaymentModel { 
                Number = 0, 
                Date = _localizer["EntranceFee.Label"].Value,
                ValueBody = coopEntranceFee,
                ValueTotal = coopEntranceFee
            });
            
            return Json(outputModel);
        }

        private static double CalculateMembershipFeeRate(CalcInputModel i)
        {
            return MembershipFeeRateCalc.CalculateFee(i.AccumulationTime, i.TermInYears);
        }
        
        private static IList<PaymentModel> BuildMonthlyPayments(CalcInputModel model, bool isForCoop)
        {
            // Процентная ставка
            var rate = isForCoop 
                ? (model.CoopInflationRate + CalculateMembershipFeeRate(model) * 100) / 100 
                : model.BankInterestRate / 100; 
            
            var termInMonths = model.TermInYears * 12; // Срок кредита
            
            // Сумма кредита
            var body = isForCoop  
                ? model.ImmovablesValue - model.ImmovablesValue * 0.35  
                : model.ImmovablesValue - model.ImmovablesValue * 0.10;
            
            var payments = new List<PaymentModel>();

            var i = 0;
            if ( isForCoop )
            {
                var payment = model.ImmovablesValue * 0.35 / (model.AccumulationTime > 0 ? model.AccumulationTime : 1);
                
                //for (var t = 0; t < model.AccumulationTime; t++)
                var t = 1;
                var accumulated = 0d;
                while(true) 
                {
                    payments.Add(new PaymentModel
                    {
                        Number = t,
                        Date = 
                            model.AccumulationTime == 0
                                ? " 1ый взнос" 
                                : DateTime.Now.AddMonths(t-1).ToString("MM yyyy") + " накопление",
                        ValueBody = payment - accumulated * (model.CoopInflationRate/100/12),
                        ValuePercent = accumulated * (model.CoopInflationRate/100/12),
                        ValueTotal = payment,
                        IsAccumulation = true
                    });
                    accumulated += payment;
                    if( t++ >= model.AccumulationTime)
                        break;
                }
                i = t-1;
            }
            else
            {
                payments.Add(new PaymentModel
                {
                    Number = i,
                    Date = DateTime.Now.ToString("MM yyyy") + " 1ый взнос",
                    ValueBody = model.ImmovablesValue * 0.10,
                    ValuePercent = 0,
                    ValueTotal = model.ImmovablesValue * 0.10
                });
            }
           
            
            var paidMonth = Math.Round(body / (termInMonths - (isForCoop ? model.AccumulationTime : 0)), 2); // Месячный платеж
            
            while (Math.Round(body, 2) > 0)
            {
                // выплата по телу кредита:
                var paidBody = paidMonth > body ? Math.Round(body, 2) : paidMonth;
                // выплата процентов на тело кредита:
                var paidPercent = Math.Round(body * rate / 12, 2);

                payments.Add(new PaymentModel
                {
                    Number = ++i, 
                    Date = DateTime.Now.AddMonths(i - 1).ToString("MM yyyy") + " выплата", 
                    ValueBody = paidBody,
                    ValuePercent = paidPercent,
                    ValueTotal = paidBody + paidPercent
                });

                //totalSum += paidBody + paidPercent;
                //Console.WriteLine("body={0}\tpaid={1}\tpercent={2}\tSum={3}", Math.Round(body, 2), Math.Round(paidBody, 2), Math.Round(paidPercent, 2), Math.Round(totalSum, 2));
                body -= paidBody;
            }

            return payments;
        }

    }
}