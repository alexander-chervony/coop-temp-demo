/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.SQS;
using Autofac;
using CoOp.Domain.Queries.InMemory;
using CoOp.Domain.Sqs;
using EventFlow;
using EventFlow.Autofac.Extensions;
using EventFlow.Configuration;
using EventFlow.Extensions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CoOp.Domain.Tests
{
    public partial class CoOpTestsBase
    {
        [SetUp]
        public void BeforeEach()
        {
            AllEventsListener.AllEventsReceived.Clear();
        }

        protected IEnumerable<T> EventsOf<T>() =>
            AllEventsListener.AllEventsReceived.Select(x => x.GetAggregateEvent()).OfType<T>();

        protected void UsingResolver(Action<IRootResolver> test)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance<IConfiguration>(Conf.Init())
                .SingleInstance();

            builder.RegisterType<SqsClientFactory>()
                .SingleInstance();

            builder.RegisterType<SqsQueueUrlBuilder>()
                .SingleInstance();

            builder.Register<IAmazonSQS>(c => 
                    c.Resolve<SqsClientFactory>().CreateSqsClient())
                .SingleInstance();
            
            using var resolver = EventFlowOptions.New
                .UseAutofacContainerBuilder(builder)
                .Configure(c => c.ThrowSubscriberExceptions = true)
                .AddDefaults(typeof(CoOpAR).GetTypeInfo().Assembly)
                //.AddDefaults(typeof(TestTo1CEventsSender).GetTypeInfo().Assembly)
                .AddSubscribers(
                    typeof(AllEventsListener),
                    typeof(NotificationEventsListener)
                    //,typeof(TestTo1CEventsSender)
                    )
                .UseInMemoryReadStoreFor<CoOpReadModel>()
                .CreateResolver();
            
            test(resolver);
        }
    }
}