/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Linq;
using System.Reflection;
using CoOp.Domain.Queries.InMemory;
using CoOp.Web.Identity;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CoOp.Web.Infrastructure.JsonSerialization
{
    public class ConditionalSerializerContractResolver : DefaultContractResolver
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipalAccessor _claimsPrincipalAccessor;

        public ConditionalSerializerContractResolver(
            IAuthorizationService authorizationService,
            ClaimsPrincipalAccessor claimsPrincipalAccessor)
        {
            _authorizationService = authorizationService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            
            // todo: can be performance optimized
            if (member.IsDefined(typeof(AccessAttribute), false))
            {
                var access = (AccessAttribute)member.GetCustomAttributes(typeof(AccessAttribute), false).First();
                
                property.ShouldSerialize =
                    instance => _authorizationService.AuthorizeAsync(_claimsPrincipalAccessor.ClaimsPrincipal, access.Policy).Result.Succeeded;
            }

            return property;
        }
    }
}