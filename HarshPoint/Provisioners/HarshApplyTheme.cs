﻿using HarshPoint.Provisioning;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HarshPoint.Provisioners
{
    public sealed class HarshApplyTheme : HarshProvisioner
    {
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public String BackgroundImageUrl
        {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public String ColorPaletteUrl
        {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public String FontSchemeUrl
        {
            get;
            set;
        }

        public Boolean ShareGenerated
        {
            get;
            set;
        }

        protected override async Task OnProvisioningAsync()
        {
            await base.OnProvisioningAsync();
            await Web.EnsurePropertyAvailable(w => w.ServerRelativeUrl);

            var backgroundImageUrl = await EnsureServerRelativeOrNull(BackgroundImageUrl);
            var colorPaletteUrl = await EnsureServerRelativeOrNull(ColorPaletteUrl);
            var fontSchemeUrl = await EnsureServerRelativeOrNull(FontSchemeUrl);

            Web.ApplyTheme(
                colorPaletteUrl, 
                fontSchemeUrl, 
                backgroundImageUrl, 
                ShareGenerated
            );

            await ClientContext.ExecuteQueryAsync();
        }
        
        private async Task<String> EnsureServerRelativeOrNull(String url)
        {
            if (url == null)
            {
                return null;
            }

            return await HarshUrl.EnsureServerRelative(Site, url);
        }
    }
}