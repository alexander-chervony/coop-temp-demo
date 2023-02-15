/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Collections.Generic;
using CoOp.Domain;

namespace CoOp.Web.Models
{
    public class FoundersCoOpVm : CoOpVm
    {
        public double CurrentTotalEntranceFees { get; set; }
        
        public string CurrentTotalEntranceFeesString => CurrentTotalEntranceFees.ToString("C0");
    }
}