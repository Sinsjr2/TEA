using System;
using System.Collections.Generic;
using NUnit.Framework;
using TEA.MVVM;
using TestTools;

namespace TEA.MVVMTest {

    public class WriteNotifyTest {

        [Test]
        [TestCase(8, 8)]
        [TestCase("", null)]
        public void InitialValueTest<T>(T _, T initial) {
            var writer = new WriteNotify<T>(initial);
            writer.Value.Is(initial);
        }

        public record NotifyData<TKind>(string PropertyName, TKind Kind);

        [Test]
        [TestCase(null, "1", "1")]
        [TestCase("99", "77", "99")]
        [TestCase("1", null, null)]
        public void ComparerTest(string? initial, string? val, string? expected) {
            bool IsSame(string? a, string? b) => a?.Length == b?.Length;
            var writer = new WriteNotify<string?>(initial, IsSame);

            var actualNotify = new List<NotifyData<string?>>();
            writer.PropertyChanged += (sender, args) => {
                var val = (sender as WriteNotify<string?>)!;
                actualNotify.Add(new(args.PropertyName!, val.Value));
            };

            writer.Render(val);
            writer.Value.Is(expected);
            actualNotify.ToArray()
                .Is(IsSame(initial, val)
                    ? Array.Empty<NotifyData<string?>>()
                    : new NotifyData<string?>[] { new(nameof(WriteNotify<string>.Value), expected) });
        }
    }
}
