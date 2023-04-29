using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Monads;

/// <summary>
/// Represents a monad wrapping an optional value
/// </summary>
/// <typeparam name="T">Type of value</typeparam>
[DebuggerNonUserCode]
public sealed class Maybe<T> : IEquatable<T>, IEquatable<Maybe<T>>
{

    [NotNull]
    internal bool HasValue { get; } = false;
    internal T Value { get; }

#nullable disable
    /// <summary>
    /// Initialize an empty maybe
    /// </summary>
    private Maybe() { }

    /// <summary>
    /// Initialize a maybe with the specified value
    /// </summary>
    /// <param name="item"></param>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public Maybe(T? item)
    {
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        if (item is not null)
        {
            Value = item;
            HasValue = true;
        }
    }
#nullable restore


    /// <summary>
    /// A standard no-value <c>Maybe&lt;<typeparamref name="T"/>&gt;</c>
    /// </summary>
    [NotNull]
    public static Maybe<T> Empty => new();

    /// <summary>
    /// Return a new <c>Maybe&lt;<typeparamref name="T"/>&gt;</c> with the
    /// value of <paramref name="selector"/> if this
    /// <c>Maybe&lt;<typeparamref name="T"/>&gt;</c> is empty.
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="selector">Value selector</param>
    [return: NotNull]
    public Maybe<TResult> Just<TResult>([NotNull] Func<T, TResult> selector)
        where TResult : notnull
    {
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return HasValue ? selector(Value) : Maybe<TResult>.Empty;
    }

    /// <summary>
    /// Asyncronously return a new <c>Maybe&lt;<typeparamref name="T"/>&gt;</c> with the
    /// value of <paramref name="selector"/> if this
    /// <c>Maybe&lt;<typeparamref name="T"/>&gt;</c> is empty
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="selector">Value selector</param>
    [return: NotNull]
    public Task<Maybe<TResult>> Just<TResult>([NotNull] Func<T, Task<TResult>> selector)
        where TResult : notnull
    {
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return JustImpl(selector);
    }

    [return: NotNull]
    private async Task<Maybe<TResult>> JustImpl<TResult>([NotNull] Func<T, Task<TResult>> selector)
        where TResult : notnull
    {
        return HasValue ? await selector(Value) : Maybe<TResult>.Empty;
    }

    /// <summary>
    /// If this is empty, return the value of <paramref name="nothing" />, otherwise,
    /// return the result of <paramref name="just" />.
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="nothing">Default value</param>
    /// <param name="just">Result value selector</param>
    public TResult Match<TResult>([NotNull] TResult nothing, [NotNull] Func<T, TResult> just)
        where TResult : notnull
    {
        if (nothing is null)
        {
            throw new ArgumentNullException(nameof(nothing));
        }
        if (just is null)
        {
            throw new ArgumentNullException(nameof(just));
        }
        return HasValue ? just(Value) : nothing;
    }

    /// <summary>
    /// Return the specified selector result or the default value provided.
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="lazyNothing">Default value factory</param>
    /// <param name="just">Result value selector</param>
    [return: NotNull]
    public TResult Match<TResult>([NotNull] Func<TResult> lazyNothing, [NotNull] Func<T, TResult> just)
        where TResult : notnull
    {
        if (lazyNothing is null)
        {
            throw new ArgumentNullException(nameof(lazyNothing));
        }
        if (just is null)
        {
            throw new ArgumentNullException(nameof(just));
        }
        return HasValue ? just(Value) : lazyNothing();
    }

    /// <summary>
    /// Return the specified selector result or the default value provided.
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="nothing">Default value</param>
    /// <param name="just">Result value selector</param>
    [return: NotNull]
    public Task<TResult> Match<TResult>([NotNull] TResult nothing, [NotNull] Func<T, Task<TResult>> just)
        where TResult : notnull
    {
        if (nothing is null)
        {
            throw new ArgumentNullException(nameof(nothing));
        }
        if (just is null)
        {
            throw new ArgumentNullException(nameof(just));
        }
        return HasValue ? just(Value) : Task.FromResult(nothing);
    }

    /// <summary>
    /// Return the specified selector result or the default value provided.
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="lazyNothing">Default value</param>
    /// <param name="just">Result value selector</param>
    [return: NotNull]
    public Task<TResult> Match<TResult>([NotNull] Func<TResult> lazyNothing, [NotNull] Func<T, Task<TResult>> just)
        where TResult : notnull
    {
        if (lazyNothing is null)
        {
            throw new ArgumentNullException(nameof(lazyNothing));
        }
        if (just is null)
        {
            throw new ArgumentNullException(nameof(just));
        }
        return HasValue ? just(Value) : Task.FromResult(lazyNothing());
    }

    /// <summary>
    /// Return the specified selector result or the default value provided.
    /// </summary>
    /// <typeparam name="TResult">Type of value returned</typeparam>
    /// <param name="lazyNothing">Default value</param>
    /// <param name="just">Result value selector</param>
    [return: NotNull]
    public Task<TResult> Match<TResult>([NotNull] Func<Task<TResult>> lazyNothing, [NotNull] Func<T, Task<TResult>> just)
        where TResult : notnull
    {
        if (lazyNothing is null)
        {
            throw new ArgumentNullException(nameof(lazyNothing));
        }
        if (just is null)
        {
            throw new ArgumentNullException(nameof(just));
        }
        return HasValue ? just(Value) : lazyNothing();
    }

#nullable disable
    /// <summary>
    /// Determines whether the specified object instances are considered equal.
    /// </summary>
    /// <param name="obj">Other object</param>
    public override bool Equals(object obj)
    {
        return obj is Maybe<T> other && Equals(Value, other.Value);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
    {
        return HasValue ? Value.GetHashCode() : default;
    }

    public bool Equals(T other)
    {
        return
            other is null
            && HasValue
            && EqualityComparer<T>.Default.Equals(Value, other);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Maybe{T}"/> instances are considered equal.
    /// </summary>
    /// <param name="obj">Other <see cref="Maybe{T}"/></param>
    public bool Equals(Maybe<T> other)
    {
        return EqualityComparer<T>.Default.Equals(Value, other.Value) && HasValue.Equals(other.HasValue);
    }
#nullable restore

    public static implicit operator Maybe<T>(T val)
    {
        return new Maybe<T>(val);
    }

    public static bool operator ==(Maybe<T> left, Maybe<T> right)
    {
        return !(left is null || right is null) && left?.Equals(right) == true;
    }

    public static bool operator !=(Maybe<T> left, Maybe<T> right)
    {
        return !(left is null || right is null) && left?.Equals(right) == false;
    }
}


[DebuggerNonUserCode]
public static partial class MaybeExtensions
{
    /// <summary>
    /// Return the result of <paramref name="resultSelector"/> if <paramref name="maybe"/> is non-empty
    /// and the result of <paramref name="evaluator"/> is non-empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOption"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="maybe">Initial value</param>
    /// <param name="evaluator">Second value to evaluate using initial value</param>
    /// <param name="resultSelector">Result selector</param>
    [return: NotNull]
    public static Task<Maybe<TResult>> SelectMany<T, TOption, TResult>(
        this Maybe<T> maybe,
        Func<T, Task<Maybe<TOption>>> evaluator,
        Func<T, TOption, TResult> resultSelector)
        where TResult : notnull
    {

        if (maybe is null)
        {
            throw new ArgumentNullException(nameof(maybe));
        }
        if (evaluator is null)
        {
            throw new ArgumentNullException(nameof(evaluator));
        }
        if (resultSelector is null)
        {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return SelectManyImpl(maybe, evaluator, resultSelector);
    }

    [return: NotNull]
    private static Task<Maybe<TResult>> SelectManyImpl<T, TOption, TResult>(Maybe<T> maybe, Func<T, Task<Maybe<TOption>>> selector, Func<T, TOption, TResult> resultSelector)
        where TResult : notnull
    {
        return maybe.Just(async v => (await selector(v)).Just(b => resultSelector(v, b)).Value);
    }


    /// <summary>
    /// Return the result of the <paramref name="selector"/> if <paramref name="maybe"/> is non-empty
    /// and the result of <paramref name="selector"/> is non-empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOption"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="maybe">Initial value</param>
    /// <param name="selector">Second value to evaluate using initial value</param>
    /// <param name="resultSelector">Result selector</param>
    [return: NotNull]
    public static Maybe<TResult> SelectMany<T, TOption, TResult>(
        this Maybe<T> maybe,
        Func<T, Maybe<TOption>> selector,
        Func<T, TOption, TResult> resultSelector)
        where TResult : notnull
    {

        if (maybe is null)
        {
            throw new ArgumentNullException(nameof(maybe));
        }
        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
        if (resultSelector is null)
        {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return maybe.Just(v => selector(v).Just(b => resultSelector(v, b)).Value);
    }

    /// <summary>
    /// Returns the result of <paramref name="selector"/> if <paramref name="maybe"/> is not empty. <para />
    /// If <paramref name="other"/> is not empty, the second argument of <paramref name="selector"/> is the value
    /// of <paramref name="other"/>, otherwise both arguments are <paramref name="maybe"/>.
    /// </summary>
    /// <typeparam name="T">Input type</typeparam>
    /// <typeparam name="TResult">Selected result</typeparam>
    /// <param name="maybe">Initial operating value.  First argument to the selector</param>
    /// <param name="other">Argument to pass to second <paramref name="selector"/> parameter if not empty</param>
    /// <param name="selector">Result selector</param>
    [return: NotNull]
    public static Maybe<TResult> SelectMatch<T, TResult>(this Maybe<T> maybe, Maybe<T> other, Func<T, T, TResult> selector)
        where TResult : notnull
        where T : notnull
    {
        if (maybe is null)
        {
            throw new ArgumentNullException(nameof(maybe));
        }

        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        return maybe.SelectMany(m => other.Match(m, o => o), selector);
    }

    /// <summary>
    /// Return the set of <see cref="Maybe{T}"/> elements where the elements are not empty and
    /// the <paramref name="predicate"/> evaluates to true for the value contained in those elements.
    /// </summary>
    /// <typeparam name="T">Type contained in the <see cref="Maybe{T}"/> elements</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="predicate">Function to evaluate selection of elements</param>
    [return: NotNull]
    public static IEnumerable<Maybe<T>> Where<T>(this IEnumerable<Maybe<T>> source, Func<T, bool> predicate)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(m => m.Match(false, v => predicate(v)));
    }

    /// <summary>
    /// Return the first non-empty element of a sequence
    /// </summary>
    /// <typeparam name="T">Type contained in the <see cref="Maybe{T}"/> elements</typeparam>
    /// <param name="source">Source sequence</param>
    [return: NotNull]
    public static Maybe<T> FirstOrDefault<T>(this IEnumerable<Maybe<T>> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var first = Enumerable.FirstOrDefault(source.WithValues());
        if (first is null)
        {
            return Maybe<T>.Empty;
        }
        return first.Value;
    }

    /// <summary>
    /// Return the first non-empty element of a sequence
    /// </summary>
    /// <typeparam name="T">Type contained in the <see cref="Maybe{T}"/> elements</typeparam>
    /// <param name="source">Source sequence</param>
    [return: NotNull]
    public static Maybe<T> FirstOrDefault<T>(this IEnumerable<Maybe<T>> source, Func<T, bool> predicate)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var first = source.WithValues().FirstOrDefault(e => predicate(e.Value));
        if (first is null)
        {
            return Maybe<T>.Empty;
        }
        return first.Value;
    }

    /// <summary>
    /// Return the set of elements from the sequence which are non-empty
    /// </summary>
    /// <typeparam name="T">Type contained in the <see cref="Maybe{T}"/> elements</typeparam>
    /// <param name="source">Source sequence</param>
    [return: NotNull]
    public static IEnumerable<Maybe<T>> WithValues<T>(this IEnumerable<Maybe<T>> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var result = source.Where(m => m.HasValue);
        return result;
    }

    /// <summary>
    /// Return the values of all non-empty elements in the sequence
    /// </summary>
    /// <typeparam name="T">Type contained in the <see cref="Maybe{T}"/> elements</typeparam>
    /// <param name="source">Source sequence</param>
    [return: NotNull]
    public static IEnumerable<T> Values<T>(this IEnumerable<Maybe<T>> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var result = source.WithValues().Select(m => m.Value);
        return result;
    }
}