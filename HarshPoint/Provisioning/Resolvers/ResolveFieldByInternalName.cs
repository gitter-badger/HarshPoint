﻿using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HarshPoint.Provisioning.Resolvers
{
    public sealed class ResolveFieldByInternalName
        : Implementation.Resolvable<Field, String, HarshProvisionerContext, ResolveFieldByInternalName>
    {
        public ResolveFieldByInternalName(IEnumerable<String> names)
            : base(names)
        {
        }

        protected override Task<IEnumerable<Field>> ResolveChainElement(ResolveContext<HarshProvisionerContext> context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            return this.ResolveClientObjectQuery(
                context,
                context.ProvisionerContext.Web.Fields,
                ClientObjectResolveQuery.FieldByInternalName
            );
        }
    }
}
