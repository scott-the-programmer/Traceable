using System;

namespace EntityTrace
{
    /// <summary>
    /// Extension methods for transforming traceable entities.
    /// </summary>
    public static class TraceableExtensions
    {
        /// <summary>
        /// Transforms a traceable entity from one type to another using a custom function.
        /// </summary>
        /// <typeparam name="TInput">The input type of the source traceable.</typeparam>
        /// <typeparam name="TOutput">The output type of the transformed traceable.</typeparam>
        /// <param name="source">The source traceable to transform.</param>
        /// <param name="label">The label to display in dependencies and graph (e.g., "Round", "ToInt").</param>
        /// <param name="transformer">The function to apply to the source value.</param>
        /// <returns>A new traceable entity representing the transformation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source or transformer is null.</exception>
        /// <exception cref="ArgumentException">Thrown when label is null or whitespace.</exception>
        public static Traceable<TOutput> Transform<TInput, TOutput>(
            this ITraceable<TInput> source,
            string label,
            Func<TInput, TOutput> transformer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            var sourceBase = (ITraceableBase)source;

            return new Traceable<TOutput>(
                operation: label,
                operands: new[] { sourceBase },
                computeFunc: () => transformer(source.Resolve())
            );
        }

        /// <summary>
        /// Transforms two traceable entities into a new type using a custom function.
        /// </summary>
        /// <typeparam name="T1">The type of the first traceable.</typeparam>
        /// <typeparam name="T2">The type of the second traceable.</typeparam>
        /// <typeparam name="TOutput">The output type of the transformed traceable.</typeparam>
        /// <param name="first">The first traceable to transform.</param>
        /// <param name="second">The second traceable to transform.</param>
        /// <param name="label">The label to display in dependencies and graph (e.g., "Sum", "Combine").</param>
        /// <param name="transformer">The function to apply to the two source values.</param>
        /// <returns>A new traceable entity representing the transformation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any traceable or transformer is null.</exception>
        /// <exception cref="ArgumentException">Thrown when label is null or whitespace.</exception>
        public static Traceable<TOutput> Transform<T1, T2, TOutput>(
            ITraceable<T1> first,
            ITraceable<T2> second,
            string label,
            Func<T1, T2, TOutput> transformer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            var firstBase = (ITraceableBase)first;
            var secondBase = (ITraceableBase)second;

            return new Traceable<TOutput>(
                operation: label,
                operands: new[] { firstBase, secondBase },
                computeFunc: () => transformer(first.Resolve(), second.Resolve())
            );
        }

        /// <summary>
        /// Transforms three traceable entities into a new type using a custom function.
        /// </summary>
        /// <typeparam name="T1">The type of the first traceable.</typeparam>
        /// <typeparam name="T2">The type of the second traceable.</typeparam>
        /// <typeparam name="T3">The type of the third traceable.</typeparam>
        /// <typeparam name="TOutput">The output type of the transformed traceable.</typeparam>
        /// <param name="first">The first traceable to transform.</param>
        /// <param name="second">The second traceable to transform.</param>
        /// <param name="third">The third traceable to transform.</param>
        /// <param name="label">The label to display in dependencies and graph (e.g., "Average", "Combine").</param>
        /// <param name="transformer">The function to apply to the three source values.</param>
        /// <returns>A new traceable entity representing the transformation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any traceable or transformer is null.</exception>
        /// <exception cref="ArgumentException">Thrown when label is null or whitespace.</exception>
        public static Traceable<TOutput> Transform<T1, T2, T3, TOutput>(
            ITraceable<T1> first,
            ITraceable<T2> second,
            ITraceable<T3> third,
            string label,
            Func<T1, T2, T3, TOutput> transformer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (third == null)
                throw new ArgumentNullException(nameof(third));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            var firstBase = (ITraceableBase)first;
            var secondBase = (ITraceableBase)second;
            var thirdBase = (ITraceableBase)third;

            return new Traceable<TOutput>(
                operation: label,
                operands: new[] { firstBase, secondBase, thirdBase },
                computeFunc: () => transformer(first.Resolve(), second.Resolve(), third.Resolve())
            );
        }
    }
}
