using System;
using System.Collections.Generic;
using NUnit.Framework;
using TEA;
using TestTools;

namespace TEATest {

    public class MessageWrapperTest {

        [Test]
        public void SetupDispatchActionTest() {
            var dispatcher = new BufferDispatcher<int>();
            var inner = new BufferDispatcher<int>();
            dispatcher.Setup(inner, (d, msg) => d.Dispatch(msg + 1));

            var msgs = new List<int>();
            var inner2 = new BufferDispatcher<int>();
            inner.Setup(inner2, (_, msg) => msgs.Add(msg));

            // 呼ばれないことを確認
            inner2.Setup(new BufferDispatcher<int>(), (_, __) => Assert.Fail("must not call"));

            dispatcher.Dispatch(20);
            msgs.ToArray().Is(new[] { 21 });
            msgs.Clear();

            dispatcher.Dispatch(-3);
            msgs.ToArray().Is(new[] { -2 });
        }

        [Test]
        public void SetupMaybeWithClassTest() {
            var dispatcher = new BufferDispatcher<string>();
            var inner = new BufferDispatcher<string>();
            dispatcher.SetupMaybe(inner, msg => msg == "hoge" ? null : msg);

            var msgs = new List<string>();
            inner.Setup(new BufferDispatcher<string>(), (_, msg) => msgs.Add(msg));

            dispatcher.Dispatch("1");
            msgs.ToArray().Is(new[] { "1" });
            msgs.Clear();

            dispatcher.Dispatch("hoge");
            msgs.ToArray().Is(Array.Empty<string>());

            dispatcher.Dispatch("2");
            msgs.ToArray().Is(new[] { "2" });
        }

        [Test]
        [TestCase(true, 5)]
        [TestCase(true, 0)]
        [TestCase(false, 5)]
        public void SetupMaybeWithStructTest(bool pass, int dispatchValue) {
            var dispatcher = new BufferDispatcher<int>();
            var inner = new BufferDispatcher<int?>();
            dispatcher.SetupMaybe(inner, msg => pass ? msg : null);

            var msgs = new List<int>();
            inner.Setup(new BufferDispatcher<int>(), (_, msg) => msgs.Add(msg!.Value));
            dispatcher.Dispatch(dispatchValue);
            msgs.ToArray().Is(pass ? new[] { dispatchValue } : Array.Empty<int>());
        }
    }
}
