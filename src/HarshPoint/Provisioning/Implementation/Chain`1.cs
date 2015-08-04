﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace HarshPoint.Provisioning.Implementation
{
    public class Chain<TElement>
    {
        private static readonly HarshLogger Logger = HarshLog.ForContext<Chain<TElement>>();

        protected Chain<TElement> Append(Chain<TElement> other)
        {
            if (other == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(other));
            }

            var thisElements = GetChainElements().ToImmutableHashSet();
            var otherElements = other.GetChainElements();

            if (thisElements.Overlaps(otherElements))
            {
                throw Logger.Fatal.Argument(
                    nameof(other),
                    SR.Chain_ElementAlreadyContained
                );
            }

            var clone = Clone();
            clone.GetChainElements().Last().Next = other.Clone();
            return clone;
        }

        protected IEnumerable<TElement> Elements => GetChainElements().Cast<TElement>();

        private Chain<TElement> Next { get; set; }

        private Chain<TElement> Clone()
        {
            var clone = (Chain<TElement>)MemberwiseClone();
            clone.Next = Next?.Clone();
            return clone;
        }

        private IEnumerable<Chain<TElement>> GetChainElements()
        {
            var current = this;

            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }
    }
}
