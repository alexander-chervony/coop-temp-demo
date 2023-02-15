/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System.Threading;
using CoOp.Domain.Queries.InMemory;
using EventFlow.Configuration;
using EventFlow.Queries;

namespace CoOp.Domain.Tests
{
    public static class RootResolverExtensions
    {
        public static CoOpReadModel GetCoOpReadModel(this IRootResolver resolver)
        {
            var queryProcessor = resolver.Resolve<IQueryProcessor>();
            return queryProcessor.Process(
                new ReadModelByIdQuery<CoOpReadModel>(CoOpId.BlrRealty), CancellationToken.None);
        }
    }
}