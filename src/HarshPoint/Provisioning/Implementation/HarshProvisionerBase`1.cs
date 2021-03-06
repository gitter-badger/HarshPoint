﻿using HarshPoint.ObjectModel;
using HarshPoint.Provisioning.Records;
using Serilog.Core.Enrichers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HarshPoint.Provisioning.Implementation
{
    /// <summary>
    /// Provides common initialization and completion logic for 
    /// classes provisioning SharePoint artifacts.
    /// </summary>
    public abstract class HarshProvisionerBase<TContext> : HarshProvisionerBase, ITrackValueSource
        where TContext : HarshProvisionerContextBase<TContext>
    {
        private readonly HarshScopedValue<TContext> _context;
        private readonly HarshScopedValue<HarshLogger> _logger;
        private readonly HarshScopedValue<ParameterSet> _parameterSet;
        private readonly HarshScopedValue<RecordWriter<TContext>> _recordWriter;

        private ICollection<HarshProvisionerBase> _children;
        private ICollection<Func<Object>> _childrenContextStateModifiers;
        private ManualResolver _manualResolver;
        private HarshProvisionerMetadata _metadata;
        private PropertyValueSourceTracker _valueSourceTracker;

        protected HarshProvisionerBase()
        {
            _context = new HarshScopedValue<TContext>();
            _parameterSet = new HarshScopedValue<ParameterSet>();
            _recordWriter = new HarshScopedValue<RecordWriter<TContext>>();

            _logger = new HarshScopedValue<HarshLogger>(
                HarshLog.ForContext(GetType())
            );
        }

        public TContext Context => _context.Value;

        public ICollection<HarshProvisionerBase> Children
            => HarshLazy.Initialize(ref _children, CreateChildrenCollection);

        public HarshLogger Logger => _logger.Value;

        public Boolean MayDeleteUserData { get; set; }

        protected ManualResolver ManualResolver
            => HarshLazy.Initialize(
                ref _manualResolver,
                () => CreateManualResolver(CreateResolveContext)
            );

        protected String ParameterSetName => ParameterSet?.Name;

        protected RecordWriter<TContext> WriteRecord
        {
            get
            {
                if (_recordWriter == null)
                {
                    throw Logger.Fatal.InvalidOperation(
                        SR.HarshProvisionerBase_NoContext
                    );
                }

                return _recordWriter.Value;
            }
        }

        internal HarshProvisionerBase<TContext> ForwardTarget { get; private set; }

        internal Boolean HasChildren
            => (_children != null) && _children.Any();

        internal ParameterSet ParameterSet
            => _parameterSet.Value;

        internal HarshProvisionerMetadata Metadata
            => HarshLazy.Initialize(
                ref _metadata,
                () => HarshProvisionerMetadataRepository.Get(GetType())
            );

        internal PropertyValueSourceTracker ValueSourceTracker
            => HarshLazy.Initialize(ref _valueSourceTracker);

        public void ModifyChildrenContextState(Func<Object> modifier)
        {
            if (modifier == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(modifier));
            }

            if (_childrenContextStateModifiers == null)
            {
                _childrenContextStateModifiers = new Collection<Func<Object>>();
            }

            _childrenContextStateModifiers.Add(modifier);
        }

        public void ModifyChildrenContextState(Object state)
        {
            if (state == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(state));
            }

            ModifyChildrenContextState(() => state);
        }

        public Task ProvisionAsync(TContext context)
            => ProvisionAsync(context, CancellationToken.None);

        public Task ProvisionAsync(TContext context, CancellationToken token)
        {
            if (context == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(context));
            }

            return RunSelfAndChildren(
                context,
                token,
                OnProvisioningAsync,
                ProvisionChildrenAsync
            );
        }

        public Task UnprovisionAsync(TContext context)
            => UnprovisionAsync(context, CancellationToken.None);

        public async Task UnprovisionAsync(TContext context, CancellationToken token)
        {
            if (context == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(context));
            }

            if (MayDeleteUserData || context.MayDeleteUserData || !Metadata.UnprovisionDeletesUserData)
            {
                await RunSelfAndChildren(
                    context,
                    token,
                    OnUnprovisioningAsync,
                    UnprovisionChildrenAsync
                );
            }
        }

        protected internal Boolean IsValueDefaultFromContext(Expression<Func<Object>> expression)
            => _valueSourceTracker?.GetValueSource(expression)
                == DefaultFromContextPropertyValueSource.Instance;

        protected void ForwardsTo(HarshProvisionerBase<TContext> target)
        {
            ForwardTarget = target;
        }

        protected void ValidateMandatoryWhenCreatingParameters()
        {
            var mandatory = ParameterSet
                .Parameters
                .Where(Metadata.IsMandatoryWhenCreating);

            foreach (var param in mandatory)
            {
                ValidateHasNonDefaultValue(param);
            }
        }

        protected virtual Task InitializeAsync() => HarshTask.Completed;

        protected virtual void Complete() { }

        protected virtual Task OnProvisioningAsync() => HarshTask.Completed;

        protected virtual void OnValidating() { }

        [NeverDeletesUserData]
        protected virtual Task OnUnprovisioningAsync() => HarshTask.Completed;

        protected virtual ICollection<HarshProvisionerBase> CreateChildrenCollection()
            => new Collection<HarshProvisionerBase>();

        protected virtual ResolveContext<TContext> CreateResolveContext()
            => new ResolveContext<TContext>(Context);

        internal virtual ManualResolver CreateManualResolver(Func<IResolveContext> resolveContextFactory)
            => new ManualResolver(resolveContextFactory);

        internal virtual Task OnResolvedPropertiesBound() => HarshTask.Completed;

        protected abstract Task ProvisionChild(
            HarshProvisionerBase provisioner,
            TContext context,
            CancellationToken cancellationToken
        );

        protected abstract Task UnprovisionChild(
            HarshProvisionerBase provisioner,
            TContext context,
            CancellationToken cancellationToken
        );

        private ParameterSet ResolveParameterSet()
        {
            var resolver = new ParameterSetResolver(this, Metadata.ParameterSets);
            return resolver.Resolve();
        }

        private void ValidateParameters()
        {
            foreach (var parameter in ParameterSet.Parameters)
            {
                if (parameter.IsMandatory)
                {
                    ValidateHasNonDefaultValue(parameter);
                }

                foreach (var attr in parameter.ValidationAttributes)
                {
                    attr.Validate(parameter, parameter.GetValue(this));
                }
            }
        }

        private void ValidateHasNonDefaultValue(Parameter parameter)
        {
            if (parameter.HasDefaultValue(this))
            {
                throw Logger.Error.ParameterValidationFormat(
                    parameter,
                    SR.HarshProvisionerBase_ParameterMandatory,
                    parameter
                );
            }
        }

        private TContext PrepareChildrenContext()
        {
            if (_childrenContextStateModifiers == null)
            {
                return Context;
            }

            return _childrenContextStateModifiers
                .Select(fn => fn())
                .Where(state => state != null)
                .Aggregate(
                    Context, (ctx, state) => (TContext)ctx.PushState(state)
                );
        }

        private Task ProvisionChildrenAsync(CancellationToken token)
            => RunChildren(ProvisionChild, token);

        private Task UnprovisionChildrenAsync(CancellationToken token)
            => RunChildren(UnprovisionChild, token, reverse: true);

        private async Task RunChildren(
            Func<HarshProvisionerBase, TContext, CancellationToken, Task> action,
            CancellationToken token,
            Boolean reverse = false)
        {
            if (!HasChildren)
            {
                return;
            }

            var children = reverse ? _children.Reverse() : _children;
            var context = PrepareChildrenContext();

            foreach (var child in children)
            {
                token.ThrowIfCancellationRequested();
                await action(child, context, token);
            }
        }

        private async Task RunWithContext(
            TContext context,
            Func<Task> action
        )
        {
            var contextLogger = Logger.ForContext(
                new PropertyEnricher("ProvisionerContext", context),
                new PropertyEnricher("MayDeleteUserData", MayDeleteUserData)
            );

            var recordWriter = new RecordWriter<TContext>(context);

            using (_context.Enter(context))
            using (_logger.Enter(contextLogger))
            using (_recordWriter.Enter(recordWriter))
            {
                await action();
            }
        }

        private Task RunSelfAndChildren(
            TContext context,
            CancellationToken token,
            Func<Task> action,
            Func<CancellationToken, Task> childAction
        )
            => RunWithContext(context, async delegate
            {
                token.ThrowIfCancellationRequested();

                Metadata.DefaultFromContextPropertyBinder.Bind(
                    this,
                    Context
                );

                Metadata.ResolvedPropertyBinder.Bind(
                    this,
                    CreateResolveContext
                );

                await OnResolvedPropertiesBound();
                token.ThrowIfCancellationRequested();

                // parameter set resolving depends on values
                // from context being already set

                using (_parameterSet.Enter(ResolveParameterSet()))
                {
                    try
                    {
                        ValidateParameters();
                        OnValidating();

                        await InitializeAsync();
                        token.ThrowIfCancellationRequested();

                        await action();
                        token.ThrowIfCancellationRequested();
                    }
                    finally
                    {
                        Complete();
                    }
                }

                await childAction(token);
            });

        PropertyValueSource ITrackValueSource.GetValueSource(PropertyInfo property)
            => _valueSourceTracker?.GetValueSource(property);

        void ITrackValueSource.SetValueSource(PropertyInfo property, PropertyValueSource source)
            => ValueSourceTracker.SetValueSource(property, source);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        protected static readonly ICollection<HarshProvisionerBase> NoChildren =
            ImmutableArray<HarshProvisionerBase>.Empty;
    }
}
