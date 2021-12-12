using System;
using System.Collections.Generic;
using NUnit.Framework;
using TEA;
using TestTools;

namespace TEATest {

    public class BufferDispatcherTest {

        class DispachedValues : IDispatcher<int> {
            public List<int> Values { get; private set; } = new();
            public void Dispatch(int state) {
                Values.Add(state);
            }
        }

        [Test]
        public void SetupAndDispachTest() {
            // setupしてからディスパッチした場合
            var actual = new DispachedValues();
            var buffer = new BufferDispatcher<int>();
            buffer.Setup(actual);
            actual.Values.ToArray().Is(Array.Empty<int>());

            buffer.Dispatch(50);
            actual.Values.ToArray().Is(new[] { 50 });
        }


        [Test]
        public void BufferTest() {
            var buffer = new BufferDispatcher<int>();
            buffer.Dispatch(1);
            buffer.Dispatch(2);
            buffer.Dispatch(3);
            buffer.Dispatch(4);
            buffer.Dispatch(5);

            var actual = new DispachedValues();
            actual.Values.ToArray().Is(Array.Empty<int>());

            // ためたメッセージがレンダリングされること
            buffer.Setup(actual);
            actual.Values.ToArray().Is(new[] { 1, 2, 3, 4, 5 });

            // 素通しされることを確認
            actual.Values.Clear();
            buffer.Dispatch(10);
            actual.Values.ToArray().Is(new[] { 10 });

            actual.Values.Clear();
            buffer.Dispatch(101);
            actual.Values.ToArray().Is(new[] { 101 });
        }

    }
}
