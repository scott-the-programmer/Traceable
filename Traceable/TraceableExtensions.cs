using System;

namespace Traceable
{
    public static class TraceableExtensions
    {
        public static Traceable<TOutput> Transform<TInput, TOutput>(
            this ITraceable<TInput> source,
            string label,
            Func<TInput, TOutput> transformer)
        {
            ValidateLabel(label);
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));

            return new Traceable<TOutput>(label, new[] { (ITraceableBase)source }, () => transformer(source.Resolve()));
        }

        public static Traceable<TOutput> Transform<T1, T2, TOutput>(
            ITraceable<T1> first,
            ITraceable<T2> second,
            string label,
            Func<T1, T2, TOutput> transformer)
        {
            ValidateLabel(label);
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));

            return new Traceable<TOutput>(label, new[] { (ITraceableBase)first, (ITraceableBase)second },
                () => transformer(first.Resolve(), second.Resolve()));
        }

        public static Traceable<TOutput> Transform<T1, T2, T3, TOutput>(
            ITraceable<T1> first,
            ITraceable<T2> second,
            ITraceable<T3> third,
            string label,
            Func<T1, T2, T3, TOutput> transformer)
        {
            ValidateLabel(label);
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (third == null) throw new ArgumentNullException(nameof(third));
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));

            return new Traceable<TOutput>(label, new[] { (ITraceableBase)first, (ITraceableBase)second, (ITraceableBase)third },
                () => transformer(first.Resolve(), second.Resolve(), third.Resolve()));
        }

        private static void ValidateLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
        }
    }
}
