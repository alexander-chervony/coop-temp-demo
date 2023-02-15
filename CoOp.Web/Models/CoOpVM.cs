/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Collections.Generic;
using CoOp.Domain;
using CoOp.Domain.Queries.InMemory;

namespace CoOp.Web.Models
{
    public class CoOpVm
    {
        // todo: filter by memberId
        public Dictionary<MemberId, MemberRm> AllMembers = new Dictionary<MemberId, MemberRm>();

        public IEnumerable<ImmovablesRm> NewImmovables { get; set; }
        public IEnumerable<ImmovablesRm> EntranceFeePaidImmovables { get; set; }
        public IEnumerable<ImmovablesRm> InQueueImmovables { get; set; }
        public IEnumerable<ImmovablesRm> InPurchaseListImmovables { get; set; }
        public IEnumerable<ImmovablesRm> PurchasedImmovables { get; set; }
        //public List<ImmovablesRm> CompletelyPaidImmovables = new List<ImmovablesRm>();

        public double CurrentTotalAvailableFundForImmovables { get; set; }
        public string CurrentTotalAvailableFundForImmovablesString =>
            CurrentTotalAvailableFundForImmovables.ToString("C0");
        
        public double CurrentTotalReservedForPurchases { get; set; }
        public string CurrentTotalReservedForPurchasesString => CurrentTotalReservedForPurchases.ToString("C0");

        public double TotalImmovablesFundInCoop =>
            CurrentTotalAvailableFundForImmovables + CurrentTotalReservedForPurchases;
        public string TotalImmovablesFundInCoopString =>
            (CurrentTotalAvailableFundForImmovables + CurrentTotalReservedForPurchases).ToString("C0");
        
        public double TotalPurchasedImmovables { get; set; }
        public string TotalPurchasedImmovablesFormatted => TotalPurchasedImmovables.ToString("C0");
        public string TotalPurchasedImmovablesFormattedPercent { get; set; }
    }
}