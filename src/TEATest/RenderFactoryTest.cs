using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TEA;
using TestTools;

namespace TEATest {

    public class RenderFactoryTest {

        [Test]
        public void ApplyToListTest() {
            var destList = new List<int>();

            destList.ApplyToList(Enumerable.Range(0, 1)).Is(1);
            destList.ToArray().Is(new [] {0});

            // list is same
            destList.ApplyToList(Enumerable.Range(0, 1)).Is(1);
            destList.ToArray().Is(new [] {0});

            destList.ApplyToList(Enumerable.Range(0, 0)).Is(0);
            destList.ToArray().Is(new [] {0});

            destList.ApplyToList(Enumerable.Range(0, 2)).Is(2);
            destList.ToArray().Is(new [] {0, 1});
        }

        public class SampleRender : ITEAComponent<int, int> {
            public List<int> List { get; } = new();
            public IDispatcher<int>? Dispatcher { get; private set; }
            public void Render(int state) {
                List.Add(state);
            }

            public void Setup(IDispatcher<int> dispatcher) {
                Dispatcher = dispatcher;
            }
        }

        [Test]
        public void ApplyToRenderTest() {
            var destList = new List<SampleRender>();
            var msgs = new List<KeyValuePair<int, int>>();
            var dispatcher = new BufferDispatcher<KeyValuePair<int, int>>();
            dispatcher.Setup(new BufferDispatcher<int>(), x => { msgs.Add(x); return 0; });
            msgs.ToArray().Is(Array.Empty<KeyValuePair<int, int>>());

            destList.ApplyToRender(dispatcher, () => new SampleRender(), Enumerable.Range(0, 1))
                .Is(1);
            destList.Count.Is(1);
            msgs.ToArray().Is(Array.Empty<KeyValuePair<int, int>>());
            destList[0].List.ToArray().Is(new[] { 0 });
            destList[0].List.Clear();

            destList[0].Dispatcher!.Dispatch(100);
            msgs.ToArray().Is(new KeyValuePair<int, int>[] { new(0, 100) });
            msgs.Clear();

            destList[0].Dispatcher!.Dispatch(50);
            msgs.ToArray().Is(new KeyValuePair<int, int>[] { new(0, 50) });
            msgs.Clear();

            destList.ApplyToRender(dispatcher, () => new SampleRender(), Enumerable.Range(0, 0))
                .Is(0);
            destList.Count.Is(1);
            destList[0].List.ToArray().Is(Array.Empty<int>());

            destList.ApplyToRender(dispatcher, () => new SampleRender(), Enumerable.Range(0, 2))
                .Is(2);
            destList.Count.Is(2);
            destList.SelectMany(x => x.List).ToArray().Is(new[] { 0, 1 });

            destList[1].Dispatcher!.Dispatch(99);
            msgs.ToArray().Is(new KeyValuePair<int, int>[] { new(1, 99)});
        }
    }
}
