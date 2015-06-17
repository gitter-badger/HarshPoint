﻿using HarshPoint.Provisioning;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HarshPoint.Tests.Provisioning
{
    public class LookupProvisioning : IClassFixture<SharePointClientFixture>
    {
        private const String TargetListUrl = "Lists/2c6280a9273e441abecf2909379712be";
        public LookupProvisioning(SharePointClientFixture fix)
        {
            Fixture = fix;
        }

        public SharePointClientFixture Fixture { get; private set; }

        //[Fact]
        public async Task Lookup_has_correct_list_id()
        {
            var targetList = await EnsureTargetList();

            var lookupField = new HarshLookupField()
            {
               // LookupTarget = Resolve.ListByUrl(TargetListUrl).FieldById(HarshBuiltInFieldId.Title)
            };

            var fieldId = Guid.NewGuid();
            var field = new HarshField()
            {
                Id = fieldId,
                InternalName = fieldId.ToString("n"),
                Type = FieldType.Lookup,
            };
        }

        private async Task<List> EnsureTargetList()
        {
            var list = new HarshList()
            {
                Title = "HarshPoint Tests Lookup Target List",
                Url = TargetListUrl,
            };

            await list.ProvisionAsync(Fixture.Context);

            return list.List;
        }
    }
}