/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoOp.Web.Models.Calc
{

    public class PaymentModel
    {
        public int Number { get; set; }
        public string Date { get; set; }
        public double ValueBody { get; set; }
        public string ValueBodyString => ValueBody.ToString("C0");
        public double ValuePercent { get; set;  }
        public string ValuePercentString => ValuePercent.ToString("C0");
        public double ValueTotal { get; set; }
        public string ValueTotalString => ValueTotal.ToString("C0");
        public bool IsAccumulation { get; set; }
    }

    
    public class CalcInputModel
    {
        public double ImmovablesValue { get; set; }
        public int TermInYears { get; set; }
        public double BankInterestRate { get; set; }
        public double CoopInflationRate { get; set; }
        public int AccumulationTime { get; set; }
    }

    public class CalcOutputModel
    {
        public double ImmovablesValue { get; set; }
        public string ImmovablesValueString => ImmovablesValue.ToString("C0");
        
        public double BankAdditionalFee { get; set; }
        public string BankAdditionalFeeString => BankAdditionalFee.ToString("C0");
        
        public double CoopEntranceFee { get; set; }
        public string CoopEntranceFeeString => CoopEntranceFee.ToString("C0");
        
        public double CoopInterestRate { get; set; }
        public string CoopInterestRateString => (CoopInterestRate/12).ToString("P3");
        public IList<PaymentModel> BankMonthlyPayments { get; set; }
        public IList<PaymentModel> CoopMonthlyPayments { get; set; }

        public string BankTotalPayments =>
            (BankMonthlyPayments.Sum(p => p.ValueTotal) + BankAdditionalFee).ToString("C0");

        public string BankOverPaymentString =>
            (BankMonthlyPayments.Sum(p => p.ValueTotal) + BankAdditionalFee - ImmovablesValue).ToString("C0");

        
        public string CoopTotalPayments => 
            (
                CoopMonthlyPayments.Where(p=>!p.IsAccumulation).Sum(p => p.ValueTotal) +
                CoopMonthlyPayments.Where(p=>p.IsAccumulation).Sum(p => p.ValueBody) + 
                CoopEntranceFee
            ).ToString("C0");

        public string CoopOverPaymentString =>
        (
            CoopMonthlyPayments.Where(p=>!p.IsAccumulation).Sum(p => p.ValueTotal) +
            CoopMonthlyPayments.Where(p=>p.IsAccumulation).Sum(p => p.ValueBody) + 
            CoopEntranceFee -
            ImmovablesValue
        ).ToString("C0");

        public string OverPaymentTimesString =>
            (
                (BankMonthlyPayments.Sum(p => p.ValueTotal) + BankAdditionalFee - ImmovablesValue) /
                (
                    CoopMonthlyPayments.Where(p=>!p.IsAccumulation).Sum(p => p.ValueTotal) +
                    CoopMonthlyPayments.Where(p=>p.IsAccumulation).Sum(p => p.ValueBody) + 
                    CoopEntranceFee -
                    ImmovablesValue
                )       
             ).ToString("0.##");
            


    }


    
}