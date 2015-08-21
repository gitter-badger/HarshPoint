﻿using HarshPoint.Provisioning;
using Microsoft.SharePoint.Client;
using System;
using System.CodeDom;

namespace HarshPoint.ShellployGenerator.Commands
{
    internal sealed class BuildFieldLookup :
        HarshPointCommandBuilder<HarshModifyFieldLookup>
    {
        public BuildFieldLookup()
        {
            AsChildOf<HarshField>(parent =>
            {
                parent.Parameter(x => x.Type)
                    .SetFixedValue(FieldType.Lookup);

                parent.Parameter(x => x.TypeName)
                    .Ignore();
            });

            Parameter("TargetListUrl")
                .Synthesize(typeof(String));

            Parameter("TargetField")
                .SetDefaultValue("Title")
                .Synthesize(typeof(String));

            Parameter(x => x.LookupTarget)
                .SetFixedValue(
                new CodeTypeReferenceExpression(typeof(Resolve))
                    .Call(nameof(Resolve.List))
                    .Call(nameof(Resolve.ByUrl), new CodeVariableReferenceExpression("TargetListUrl"))
                    .Call(nameof(Resolve.Field))
                    .Call(nameof(Resolve.ByInternalName), new CodeVariableReferenceExpression("TargetField"))
                    .Call(nameof(ResolveBuilderExtensions.As), typeof(Tuple<List, Field>))
            );
        }
    }
}