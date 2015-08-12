﻿using HarshPoint.Provisioning;
using Microsoft.SharePoint.Client;

namespace HarshPoint.ShellployGenerator
{
    internal sealed class HarshModifyFieldMultilineTextMetadata
        : ShellployMetadataObject<HarshModifyFieldMultilineText>
    {
        protected override ShellployCommandBuilder<HarshModifyFieldMultilineText> CreateCommandBuilder()
        {
            return base.CreateCommandBuilder()
                .AsChildOf<HarshField>()
                    .SetValue(x => x.Type, FieldType.Note)
                .End();
        }
    }
}