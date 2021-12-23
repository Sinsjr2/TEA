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
    }
}
