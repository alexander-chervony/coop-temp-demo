/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using CoOp.Domain.Queries.InMemory;
using AutoMapper;
using CoOp.Web.Models;

namespace CoOp.Web.Infrastructure
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<CoOpReadModel, CoOpVm>();
            CreateMap<CoOpReadModel, FoundersCoOpVm>();
        }
    }
}