﻿using HarshPoint.Provisioning;
using HarshPoint.Provisioning.Implementation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace HarshPoint.Shellploy
{
    public abstract class HarshProvisionerCmdlet : PSCmdlet
    {
        internal static void AddChildren<TContext>(HarshProvisionerBase<TContext> parent, Object children)
            where TContext : HarshProvisionerContextBase
        {
            if (children == null)
            {
                return;
            }

            if ((children is HarshProvisionerBase) || (children is IDefaultFromContextTag))
            {
                AddChild(parent, children);
            }

            var scriptBlock = children as ScriptBlock;
            if (scriptBlock != null)
            {
                children = scriptBlock.Invoke()
                    .Select(c => c.BaseObject);
            }

            var enumerable = children as IEnumerable;
            if (children != null)
            {
                AddChildren(parent, children);
            }

            AddChild(parent, children);
        }

        internal static void AddChildren<TContext>(HarshProvisionerBase<TContext> parent, IEnumerable<Object> children)
            where TContext : HarshProvisionerContextBase
        {
            foreach (var child in children)
            {
                AddChild(parent, child);
            }
        }

        internal static void AddChild<TContext>(HarshProvisionerBase<TContext> parent, Object child)
            where TContext : HarshProvisionerContextBase
        {
            if (parent == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(parent));
            }

            if (child == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(child));
            }

            var provisioner = (child as HarshProvisionerBase);
            var defaultFromContextTag = (child as IDefaultFromContextTag);

            if (provisioner != null)
            {
                parent.Children.Add(provisioner);
            }
            else if (defaultFromContextTag != null)
            {
                parent.ModifyChildrenContextState(defaultFromContextTag);
            }
            else
            {
                throw Logger.Fatal.ArgumentNotAssignableTo(
                    nameof(child),
                    child,
                    typeof(HarshProvisionerBase),
                    typeof(IDefaultFromContextTag)
                );
            }
        }

        private static readonly HarshLogger Logger = HarshLog.ForContext(typeof(HarshProvisionerCmdlet));
    }
}