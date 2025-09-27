using System;
using System.Diagnostics.CodeAnalysis;

namespace Parking.Domain.Repositories.Filters;

public sealed class RangeFilter<T>
    where T : IComparable<T>
{
    public RangeFilter(T from, T to)
    {
        if (from.CompareTo(to) > 0)
        {
            throw new ArgumentException("The range start must be less than or equal to the end.", nameof(from));
        }

        From = from;
        To = to;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by EF Core materialization.")]
    public T From { get; }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by EF Core materialization.")]
    public T To { get; }
}
