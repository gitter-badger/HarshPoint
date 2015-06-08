﻿using HarshPoint.Provisioning.Implementation;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace HarshPoint.Provisioning.Resolvers
{
    internal static class ClientObjectResolvableExtensions
    {
        public static Task<IEnumerable<T>> ResolveIdentifiers<T, TIdentifier, TSelf>(
            this Resolvable<T, TIdentifier, HarshProvisionerContext, TSelf> resolvable,
            HarshProvisionerContext context,
            IQueryable<T> query,
            Func<T, TIdentifier> idSelector
        )
            where T : ClientObject
            where TSelf : Resolvable<T, TIdentifier, HarshProvisionerContext, TSelf>
        {
            if (resolvable == null)
            {
                throw Error.ArgumentNull(nameof(resolvable));
            }

            return ResolveIdentifiers(
                context, 
                resolvable.Identifiers, 
                query, 
                idSelector,
                resolvable.IdentifierComparer
            );
        }

        public static Task<IEnumerable<T2>> ResolveIdentifiers<T1, T2, TIdentifier, TSelf>(
            this NestedResolvable<T1, T2, TIdentifier, HarshProvisionerContext, TSelf> resolvable,
            HarshProvisionerContext context,
            IQueryable<T2> query,
            Func<T2, TIdentifier> idSelector
        )
            where T2 : ClientObject
            where TSelf : NestedResolvable<T1, T2, TIdentifier, HarshProvisionerContext, TSelf>
        {
            if (resolvable == null)
            {
                throw Error.ArgumentNull(nameof(resolvable));
            }

            return ResolveIdentifiers(
                context,
                resolvable.Identifiers,
                query,
                idSelector,
                resolvable.IdentifierComparer
            );
        }

        private static async Task<IEnumerable<T>> ResolveIdentifiers<T, TIdentifier>(
            HarshProvisionerContext context,
            IEnumerable<TIdentifier> identifiers,
            IQueryable<T> query,
            Func<T, TIdentifier> idSelector,
            IEqualityComparer<TIdentifier> idComparer
        )
            where T : ClientObject
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (identifiers == null)
            {
                throw Error.ArgumentNull(nameof(identifiers));
            }

            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (idSelector == null)
            {
                throw Error.ArgumentNull(nameof(idSelector));
            }
            
            var items = context.ClientContext.LoadQuery(query);
            await context.ClientContext.ExecuteQueryAsync();

            var byId = items.ToImmutableDictionary(idSelector, idComparer);

            return identifiers.Select(id =>
            {
                T ct = null;
                byId.TryGetValue(id, out ct);
                return ct;
            });
        }
    }
}