﻿using HarshPoint.Provisioning;
using HarshPoint.Provisioning.Implementation;
using Microsoft.SharePoint.Client;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace HarshPoint.Tests.Provisioning.Implementation
{
    public class ManualResolving : SharePointClientTest
    {
        public ManualResolving(SharePointClientFixture fixture, ITestOutputHelper output) : base(fixture, output)
        {
        }

        [Fact]
        public void Resolve_gets_resolved()
        {
            var mr = new ManualResolver(Fixture.CreateResolveContext);
            var result = mr.Resolve(MockResolve.Build<String>("32", "42", "52"));

            Assert.Equal(new[] { "32", "42", "52" }, result);
        }

        [Fact]
        public void ResolveSingle_gets_resolved()
        {
            var mr = new ManualResolver(Fixture.CreateResolveContext);
            var result = mr.ResolveSingle(MockResolve.Build<String>("42"));

            Assert.Equal("42", result.Value);
        }

        [Fact]
        public void ResolveSingleOrDefault_gets_resolved()
        {
            var mr = new ManualResolver(Fixture.CreateResolveContext);
            var result = mr.ResolveSingleOrDefault(MockResolve.Build<String>("42"));

            Assert.Equal("42", result.Value);
        }

        [Fact]
        public void Resolve_gets_resolved_with_retrievals()
        {
            var mr = new ClientObjectManualResolver(Fixture.CreateResolveContext);

            var field = Web.Fields.GetById(HarshBuiltInFieldId.Title);
            var mock = MockResolve.Mock<Field>(field);

            mock.Setup(x => x.ToEnumerable(It.IsAny<Object>(), It.IsAny<IResolveContext>()))
                .Callback<Object, IResolveContext>((state, ctx) =>
                {
                    var corc = Assert.IsType<ClientObjectResolveContext>(ctx);
                    var retrievals = corc.QueryProcessor
                        .GetRetrievals<Field>()
                        .Select(Convert.ToString);

                    var actual = Assert.Single(retrievals);
                    var expected = Convert.ToString(
                        GetExpression<Field>(f => f.FieldTypeKind)
                    );

                    Assert.Equal(expected, actual);
                });

            var result = mr.Resolve(
                mock.Object,
                f => f.FieldTypeKind
            );

            Assert.Same(field, Assert.Single(result));
        }

        private static Expression<Func<T, Object>> GetExpression<T>(Expression<Func<T, Object>> expr)
            => expr;
    }
}
