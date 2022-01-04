using System;
using System.Linq;
using NUnit.Framework;
using TEA.MVVM;
using TestTools;

namespace TEA.MVVMTest {

    public class CollectionRenderTest {

        [Test]
        [TestCase(6)]
        [TestCase("")]
        public void InitialValueTest<T>(T defaultValue) {
            var render = new CollectionRender<NotifyValue<T>, T, T>(() => new NotifyValue<T>(defaultValue));
            render.Select(x => x.Value).Is(Array.Empty<T>());
        }

        [Test]
        [TestCase(new int[] {}, new int[] {})]
        [TestCase(new int[] { 6 }, new int[] {})]
        [TestCase(new int[] {}, new int[] { 8 })]
        [TestCase(new int[] { 1, 2 }, new int[] { 1, 2, 3 })]
        [TestCase(new int[] { 1, 2 }, new int[] { 1, 2 })]
        public void RenderTest(int[] initialValues, int[] inputValues) {
            var render = new CollectionRender<NotifyValue<int>, int, int>(() => new NotifyValue<int>(0));
            bool calledChanged = false;
            render.CollectionChanged += (_, __) => calledChanged = true;
            render.Render(initialValues);
            calledChanged.Is(initialValues.Any());
            render.ToArray().Select(x => x.Value).Is(initialValues);
            calledChanged = false;

            render.Render(inputValues);
            render.ToArray().Select(x => x.Value).Is(inputValues);
            calledChanged.Is(!initialValues.SequenceEqual(inputValues));
        }
    }
}
