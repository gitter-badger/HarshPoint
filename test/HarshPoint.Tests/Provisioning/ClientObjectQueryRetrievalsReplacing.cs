﻿using HarshPoint.Provisioning.Implementation;
using Microsoft.SharePoint.Client;
using System;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace HarshPoint.Tests.Provisioning
{
    public class ClientObjectQueryRetrievalsReplacing : SeriloggedTest
    {
        public ClientObjectQueryRetrievalsReplacing(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Does_not_modify_expression_without_Include()
        {
            var ctx = new ClientObjectResolveContext();
            ctx.Include<List>(l => l.Title);

            var visitor = new ClientObjectResolveQueryProcessor(ctx);
            var expression = GetExpression(w => w.Lists);

            var actual = visitor.AddContextRetrievals(expression);

            Assert.Equal(expression.ToString(), actual.ToString());
        }

        [Fact]
        public void Retrievals_of_two_types_are_added()
        {
            var ctx = new ClientObjectResolveContext();
            ctx.Include<List>(l => l.Title);
            ctx.Include<Field>(f => f.InternalName);

            var visitor = new ClientObjectResolveQueryProcessor(ctx);
            var expression = GetExpression(
                w => w.Lists.Include(
                    l => l.Fields.Include()
                )
            );

            var expected = GetExpression(
                w => w.Lists.Include(
                    l => l.Fields.Include(f => f.InternalName),
                    l => l.Title
                )
            );

            var actual = visitor.AddContextRetrievals(expression);

            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void Empty_Include_is_replaced_with_retrievals()
        {
            var ctx = new ClientObjectResolveContext();
            ctx.Include<List>(l => l.Title);

            var visitor = new ClientObjectResolveQueryProcessor(ctx);
            var expression = GetExpression(
                w => w.Lists.Include()
            );

            var visited = visitor.AddContextRetrievals(expression);
            Assert.NotSame(expression, visited);

            var lambda = visited as LambdaExpression;
            Assert.NotNull(lambda);

            var call = lambda.Body as MethodCallExpression;
            Assert.NotNull(call);

            var arguments = call.Arguments;
            Assert.Equal(2, arguments.Count);

            var secondArg = arguments[1] as NewArrayExpression;
            Assert.NotNull(secondArg);

            var retrieval = Assert.Single(secondArg.Expressions);
            Assert.NotNull(retrieval);
            Assert.Equal("Title", retrieval.TryExtractSinglePropertyAccess().Name);
        }

        [Fact]
        public void Retrievals_are_added_to_existing_Include()
        {
            var ctx = new ClientObjectResolveContext();
            ctx.Include<List>(l => l.Title);

            var visitor = new ClientObjectResolveQueryProcessor(ctx);

            var expression = GetExpression(
                w => w.Lists.Include(l => l.Description)
            );

            var visited = visitor.AddContextRetrievals(expression);
            Assert.NotSame(expression, visited);

            var lambda = visited as LambdaExpression;
            Assert.NotNull(lambda);

            var call = lambda.Body as MethodCallExpression;
            Assert.NotNull(call);

            var arguments = call.Arguments;
            Assert.Equal(2, arguments.Count);

            var secondArg = arguments[1] as NewArrayExpression;
            Assert.NotNull(secondArg);

            Assert.Equal(2, secondArg.Expressions.Count);
            Assert.Equal("Description", secondArg.Expressions[0].TryExtractSinglePropertyAccess().Name);
            Assert.Equal("Title", secondArg.Expressions[1].TryExtractSinglePropertyAccess().Name);
        }

        [Fact]
        public void Empty_IncludeWitDefaultProperties_is_replaced_with_retrievals()
        {
            var ctx = new ClientObjectResolveContext();
            ctx.Include<List>(l => l.Title);

            var visitor = new ClientObjectResolveQueryProcessor(ctx);

            var expression = GetExpression(
                w => w.Lists.IncludeWithDefaultProperties()
            );

            var visited = visitor.AddContextRetrievals(expression);
            Assert.NotSame(expression, visited);

            var lambda = visited as LambdaExpression;
            Assert.NotNull(lambda);

            var call = lambda.Body as MethodCallExpression;
            Assert.NotNull(call);

            var arguments = call.Arguments;
            Assert.Equal(2, arguments.Count);

            var secondArg = arguments[1] as NewArrayExpression;
            Assert.NotNull(secondArg);

            var retrieval = Assert.Single(secondArg.Expressions);
            Assert.NotNull(retrieval);
            Assert.Equal("Title", retrieval.TryExtractSinglePropertyAccess().Name);
        }

        [Fact]
        public void Include_call_is_removed_when_no_retrievals()
        {
            var ctx = new ClientObjectResolveContext();
            ctx.Include<List>();

            var visitor = new ClientObjectResolveQueryProcessor(ctx);

            var expression = GetExpression(
                w => w.Lists.Include()
            );

            var visited = visitor.AddContextRetrievals(expression);
            Assert.NotSame(expression, visited);

            var lambda = visited as LambdaExpression;
            Assert.NotNull(lambda);

            var listsAccess = lambda.Body as MemberExpression;
            Assert.NotNull(listsAccess);
            Assert.Equal("Lists", listsAccess.Member.Name);
        }


        private static Expression GetExpression(Expression<Func<Web, Object>> expr)
        {
            return expr;
        }
    }
}
