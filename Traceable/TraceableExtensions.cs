using System;

namespace Traceable;

public static class TraceableExtensions
{
    public static IDisposable AsScope(this ITraceableBase traceable)
    {
        if (traceable == null) throw new ArgumentNullException(nameof(traceable));
        return TraceableScope.Push(traceable);
    }

    public static Traceable<TOutput> Transform<TInput, TOutput>(
        this ITraceable<TInput> source,
        Func<TInput, TOutput> transformer,
        string label)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (transformer == null) throw new ArgumentNullException(nameof(transformer));
        ValidateLabel(label);

        return new Traceable<TOutput>(label, new[] { (ITraceableBase)source }, () => transformer(source.Resolve()));
    }

    public static Traceable<TOutput> Transform<T1, T2, TOutput>(
        ITraceable<T1> first,
        ITraceable<T2> second,
        Func<T1, T2, TOutput> transformer,
        string label)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));
        if (transformer == null) throw new ArgumentNullException(nameof(transformer));
        ValidateLabel(label);

        return new Traceable<TOutput>(label, new[] { (ITraceableBase)first, (ITraceableBase)second },
            () => transformer(first.Resolve(), second.Resolve()));
    }

    public static Traceable<TOutput> Transform<T1, T2, T3, TOutput>(
        ITraceable<T1> first,
        ITraceable<T2> second,
        ITraceable<T3> third,
        Func<T1, T2, T3, TOutput> transformer,
        string label)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));
        if (third == null) throw new ArgumentNullException(nameof(third));
        if (transformer == null) throw new ArgumentNullException(nameof(transformer));
        ValidateLabel(label);

        return new Traceable<TOutput>(label, new[] { (ITraceableBase)first, (ITraceableBase)second, (ITraceableBase)third },
            () => transformer(first.Resolve(), second.Resolve(), third.Resolve()));
    }

    public static (Traceable<TOut1>, Traceable<TOut2>) Split<TInput, TOut1, TOut2>(
        this ITraceable<TInput> source,
        Func<TInput, (TOut1, TOut2)> splitter,
        string label1,
        string label2)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (splitter == null) throw new ArgumentNullException(nameof(splitter));
        ValidateLabel(label1);
        ValidateLabel(label2);

        var result1 = new Traceable<TOut1>(label1, new[] { (ITraceableBase)source },
            () => splitter(source.Resolve()).Item1);
        var result2 = new Traceable<TOut2>(label2, new[] { (ITraceableBase)source },
            () => splitter(source.Resolve()).Item2);

        return (result1, result2);
    }

    public static (Traceable<TOut1>, Traceable<TOut2>, Traceable<TOut3>) Split<TInput, TOut1, TOut2, TOut3>(
        this ITraceable<TInput> source,
        Func<TInput, (TOut1, TOut2, TOut3)> splitter,
        string label1,
        string label2,
        string label3)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (splitter == null) throw new ArgumentNullException(nameof(splitter));
        ValidateLabel(label1);
        ValidateLabel(label2);
        ValidateLabel(label3);

        var result1 = new Traceable<TOut1>(label1, new[] { (ITraceableBase)source },
            () => splitter(source.Resolve()).Item1);
        var result2 = new Traceable<TOut2>(label2, new[] { (ITraceableBase)source },
            () => splitter(source.Resolve()).Item2);
        var result3 = new Traceable<TOut3>(label3, new[] { (ITraceableBase)source },
            () => splitter(source.Resolve()).Item3);

        return (result1, result2, result3);
    }

    private static void ValidateLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
    }
}
