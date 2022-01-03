using System.Collections;
using NUnit.Framework;

namespace TestTools {
    public static class Assertion {

        public static void Is<T>(this T? actual, T? expected) {
            if (typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T))) {
                Assert.That(actual, NUnit.Framework.Is.EquivalentTo(expected as IEnumerable));
            } else {
                Assert.That(actual, NUnit.Framework.Is.EqualTo(expected));
            }
        }

        public static void IsNot<T>(this T? actual, T? notExpected) {
            if (typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T))) {
                Assert.That(actual, NUnit.Framework.Is.Not.EquivalentTo(notExpected as IEnumerable));
            } else {
                Assert.That(actual, NUnit.Framework.Is.Not.EqualTo(notExpected));
            }
        }
    }
}
