﻿using HarshPoint.Provisioning;
using HarshPoint.Provisioning.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HarshPoint.Tests.Provisioning
{
    public class DefaultFromContextTests : SharePointClientTest
    {
        public DefaultFromContextTests(SharePointClientFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        [Fact]
        public void Metadata_finds_DefaultFromContext_single_resolver()
        {
            var metadata = new HarshProvisionerMetadata(typeof(SingleResolverProvisioner));

            var param = Assert.Single(
                metadata.DefaultParameterSet.Parameters
                    .Where(p => p.IsDefaultFromContext)
            );

            Assert.Equal(
                typeof(SingleResolverProvisioner).GetProperty("SingleResolver"),
                param.PropertyInfo
            );

            Assert.Equal(
                typeof(String),
                param.ResolvedType
            );
        }

        [Fact]
        public void Metadata_finds_DefaultFromContext_resolver()
        {
            var metadata = new HarshProvisionerMetadata(typeof(ResolverProvisioner));

            var param = Assert.Single(
                metadata.DefaultParameterSet.Parameters
                    .Where(p => p.IsDefaultFromContext)
            );

            Assert.Equal(
                typeof(ResolverProvisioner).GetProperty("Resolver"),
                param.PropertyInfo
            );

            Assert.Equal(
                typeof(String),
                param.ResolvedType
            );
        }

        [Fact]
        public async Task Resolve_property_gets_assigned()
        {
            var prov = new ResolverProvisioner();

            Assert.Null(prov.Resolver);

            await prov.ProvisionAsync(Fixture.Context);

            Assert.NotNull(prov.Resolver);
            Assert.IsType<ContextStateResolver<String>>(prov.Resolver);
        }

        [Fact]
        public async Task ResolveSingle_property_gets_assigned()
        {
            var prov = new SingleResolverProvisioner();

            Assert.Null(prov.SingleResolver);

            await prov.ProvisionAsync(Fixture.Context);

            Assert.NotNull(prov.SingleResolver);
            Assert.IsType<ContextStateResolver<String>>(prov.SingleResolver);
        }

        [Fact]
        public async Task String_property_gets_assigned()
        {
            var prov = new StringProvisioner();

            Assert.Null(prov.StringProperty);

            await prov.ProvisionAsync(Fixture.Context.PushState("42"));

            Assert.Equal("42", prov.StringProperty);
        }

        [Fact]
        public async Task Tagged_property_gets_assigned()
        {
            var prov = new TaggedProvisioner();
            Assert.Null(prov.TaggedStringProperty);

            var state = Fixture.Context
                .PushState("red herring")
                .PushState(new DummyTag() { Value = "424242" });

            await prov.ProvisionAsync(state);

            Assert.Equal("424242", prov.TaggedStringProperty);
        }

        [Fact]
        public async Task Assigned_property_doesnt_get_overwritten()
        {
            var prov = new ResolverProvisioner();
            var resolver = new DummyResolver();
            prov.Resolver = resolver;

            await prov.ProvisionAsync(Fixture.Context);

            Assert.Same(resolver, prov.Resolver);
        }

        [Fact]
        public void Fails_if_TagType_doesnt_implement_IDefaultFromContextTag()
        {
            Assert.Throws<HarshProvisionerMetadataException>(
                () => new ParameterSetBuilder(typeof(WrongTagType)).Build()
            );
        }

        private class StringProvisioner : HarshProvisioner
        {
            [Parameter]
            [DefaultFromContext]
            public String StringProperty
            {
                get;
                set;
            }
        }

        private class TaggedProvisioner : HarshProvisioner
        {
            [Parameter]
            [DefaultFromContext(typeof(DummyTag))]
            public String TaggedStringProperty
            {
                get;
                set;
            }
        }

        private class SingleResolverProvisioner : HarshProvisioner
        {
            [Parameter]
            [DefaultFromContext]
            public IResolve<String> SingleResolver
            {
                get;
                set;
            }
        }

        private class ResolverProvisioner : HarshProvisioner
        {
            [Parameter]
            [DefaultFromContext]
            public IResolve<String> Resolver
            {
                get;
                set;
            }
        }

        private class DummyTag : DefaultFromContextTag<String>
        {
        }

        private class DummyResolver : IResolve<String>
        {
            public Task<IEnumerable<string>> TryResolveAsync(IResolveContext context)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class WrongTagType : HarshProvisioner
        {
            [Parameter]
            [DefaultFromContext(typeof(String))]
            public String Param { get; set; }
        }
    }
}
