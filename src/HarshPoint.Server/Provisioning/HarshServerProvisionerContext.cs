﻿using HarshPoint.Provisioning.Implementation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HarshPoint.Server.Provisioning
{
    public sealed class HarshServerProvisionerContext : HarshProvisionerContextBase<HarshServerProvisionerContext>
    {
        private IReadOnlyDictionary<String, String> _upgradeArgs = EmptyUpgradeArguments;

        public HarshServerProvisionerContext(SPWeb web)
        {
            if (web == null)
            {
                throw Logger.Fatal.ArgumentNull("web");
            }

            SetWeb(web);
        }

        public HarshServerProvisionerContext(SPSite site)
        {
            if (site == null)
            {
                throw Logger.Fatal.ArgumentNull("site");
            }

            SetWeb(site.RootWeb);
        }

        public HarshServerProvisionerContext(SPWebApplication webApplication)
        {
            if (webApplication == null)
            {
                throw Logger.Fatal.ArgumentNull("webApplication");
            }

            WebApplication = webApplication;
            Farm = webApplication.Farm;
        }

        public HarshServerProvisionerContext(SPFarm farm)
        {
            if (farm == null)
            {
                throw Logger.Fatal.ArgumentNull("farm");
            }

            Farm = farm;
        }

        public SPFarm Farm
        {
            get;
            private set;
        }

        public SPSite Site
        {
            get;
            private set;
        }

        public SPWeb Web
        {
            get;
            private set;
        }

        public SPWebApplication WebApplication
        {
            get;
            private set;
        }

        public String UpgradeAction
        {
            get;
            private set;
        }

        public IReadOnlyDictionary<String, String> UpgradeArguments => _upgradeArgs;

        public override String ToString()
            => Web?.Url ?? WebApplication?.ToString() ?? Farm.ToString();

        private void SetWeb(SPWeb web)
        {
            Web = web;
            Site = web.Site;
            WebApplication = Site.WebApplication;
            Farm = WebApplication.Farm;
        }

        internal static HarshServerProvisionerContext FromProperties(
            SPFeatureReceiverProperties properties,
            String upgradeAction,
            IDictionary<String, String> upgradeArguments)
        {
            var result = FromProperties(properties);

            if (upgradeArguments != null)
            {
                result._upgradeArgs = upgradeArguments.ToImmutableDictionary(StringComparer.Ordinal);
            }

            result.UpgradeAction = upgradeAction;
            return result;
        }

        internal static HarshServerProvisionerContext FromProperties(SPFeatureReceiverProperties properties)
        {
            if (properties == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(properties));
            }

            if (properties.Feature == null)
            {
                throw Logger.Fatal.Argument(nameof(properties), SR.HarshServerProvisionerContext_PropertiesFeatureNull);
            }

            var parent = properties.Feature.Parent;

            var web = (parent as SPWeb);
            if (web != null)
            {
                return new HarshServerProvisionerContext(web);
            }

            var site = (parent as SPSite);
            if (site != null)
            {
                return new HarshServerProvisionerContext(site);
            }

            var webApplication = (parent as SPWebApplication);
            if (webApplication != null)
            {
                return new HarshServerProvisionerContext(webApplication);
            }

            var farm = (parent as SPFarm);
            if (farm != null)
            {
                return new HarshServerProvisionerContext(farm);
            }

            return null;
        }

        private static readonly IReadOnlyDictionary<String, String> EmptyUpgradeArguments =
            ImmutableDictionary<String, String>.Empty;

        private static readonly HarshLogger Logger = HarshLog.ForContext<HarshServerProvisionerContext>();
    }
}
