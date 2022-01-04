using System.Linq;

namespace System.Collections.Generic.Immutable {

    public class EqList<T> : IReadOnlyList<T>, IEquatable<EqList<T>> {

        public static readonly EqList<T> Empty = new(Array.Empty<T>());

        readonly IReadOnlyList<T> elements;

        static readonly int initialHash = 10;

        int hash = initialHash;

        public EqList(IReadOnlyList<T> elements) {
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public T this[int index] => elements[index];

        public int Count => elements.Count;

        public IEnumerator<T> GetEnumerator() {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return elements.GetEnumerator();
        }

        public override int GetHashCode() {
            if (hash != initialHash) {
                return hash;
            }
            foreach (var x in elements) {
                hash = unchecked((hash * 31) + x?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public override bool Equals(object? obj) {
            return obj is EqList<T> list && this == list;
        }

        public bool Equals(EqList<T>? other) {
            return this == other;
        }

        public override string ToString() {
            return "[" + string.Join(",\n", elements.Select(x => x?.ToString() ?? "null")) + "]\n";
        }

        public static bool operator !=(EqList<T>? a, EqList<T>? b)
        {
            return !(a == b);
        }

        public static bool operator ==(EqList<T>? a, EqList<T>? b)
        {
            return
            ReferenceEquals(a, b) ||
            (a is not null && b is not null &&
             a.Count == b.Count &&
             a.elements.SequenceEqual(b.elements));
        }
    }

    public static class EqList {

        public static EqList<T> ToEqList<T>(this IEnumerable<T> source) {
            return new(source.ToArray());
        }

        public static EqList<T> ToEqList<T>(this IReadOnlyList<T> source) {
            return new(source);
        }
    }
}
