﻿using HarshPoint.Provisioning;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.CodeDom;

namespace HarshPoint.ShellployGenerator
{
    internal sealed class HarshModifyFieldTaxonomyMetadata
        : ShellployMetadataObject<HarshModifyFieldTaxonomy>
    {
        protected override ShellployCommandBuilder<HarshModifyFieldTaxonomy> CreateCommandBuilder()
        {
            return base.CreateCommandBuilder()
                .AddNamedParameter<Guid>("TermSetId")
                .SetParameterValue(x => x.TermSet,
                    new CodeTypeReferenceExpression(typeof(Resolve))
                        .Call(nameof(Resolve.TermStoreSiteCollectionDefault))
                        .Call(nameof(Resolve.TermSet))
                        .Call(nameof(Resolve.ById), new CodeVariableReferenceExpression("TermSetId"))
                        .Call(nameof(ResolveBuilderExtensions.As), typeof(TermSet))
                )
                .AsChildOf<HarshField>()
                    .SetValue(x => x.TypeName, "TaxonomyFieldType")
                    .IgnoreParameter(x => x.Type)
                .End();
        }
    }
}