﻿using HarshPoint.Provisioning;
using Microsoft.SharePoint.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HarshPoint.Tests.Provisioning
{
    public class DateTimeFieldProvisioning : TestFieldBase<FieldDateTime, HarshDateTimeField>
    {
        public DateTimeFieldProvisioning(SharePointClientFixture fixture)
            : base(fixture, FieldType.DateTime)
        {
        }

        [Fact]
        public async Task DisplayFormat_is_set()
        {
            var prov = new HarshDateTimeField()
            {
                DisplayFormat = DateTimeFieldFormatType.DateOnly,
            };

            await RunWithField(prov, f =>
            {
                Assert.Equal(DateTimeFieldFormatType.DateOnly, f.DisplayFormat);
            });
        }
    }
}
