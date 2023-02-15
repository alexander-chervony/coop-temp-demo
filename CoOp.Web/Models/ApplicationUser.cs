/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CoOp.Domain;
using Microsoft.AspNetCore.Identity;

namespace CoOp.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        // currently MemberId is in claim (AspNetUserClaims table)
        // public MemberId MemberId { get; set; }
    }
}