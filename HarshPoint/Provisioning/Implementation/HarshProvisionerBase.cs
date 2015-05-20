﻿using System.Threading.Tasks;

namespace HarshPoint.Provisioning.Implementation
{
    public abstract class HarshProvisionerBase
    {
        protected abstract Task InitializeAsync();

        protected virtual void Complete()
        {
        }

        protected abstract Task OnProvisioningAsync();

        [NeverDeletesUserData]
        protected abstract Task OnUnprovisioningAsync();
    }
}