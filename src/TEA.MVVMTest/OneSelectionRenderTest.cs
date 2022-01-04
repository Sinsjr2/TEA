using System;
using System.Collections.Generic;
using NUnit.Framework;
using TEA.MVVM;
using TestTools;

namespace TEA.MVVMTest {

    public class OneSelectionRenderTest {

        public enum Selection {
            NoneSelection,
            First,
            Second,
            Third,
        }

        [Test]
        [TestCase(Selection.NoneSelection)]
        [TestCase(Selection.First)]
        public void InitialTest(Selection noneSelection) {
            var render = new OneSelectionRender<Selection>(noneSelection);
            render.Value.Is(noneSelection);
        }

        public record NotifyData<TKind>(string PropertyName, TKind Kind);

        [Test]
        public void NoneSelectionRenderTest() {
            var render = new OneSelectionRender<Selection>(Selection.NoneSelection);
            render.Render(Selection.First);

            var actualNotify = new List<NotifyData<Selection>>();
            render.PropertyChanged += (sender, args) => {
                var val = (sender as OneSelectionRender<Selection>)!;
                actualNotify.Add(new(args.PropertyName!, val.Value));
            };

            var notify1 = render[Selection.First];
            notify1.Value.Is(true);
            var notify2 = render[Selection.Second];
            notify2.Value.Is(false);
            actualNotify.ToArray().Is(Array.Empty<NotifyData<Selection>>());

            render.Render(Selection.NoneSelection);
            notify1.Value.Is(false);
            notify2.Value.Is(false);
            actualNotify.ToArray().Is(new NotifyData<Selection>[] { new(nameof(OneSelectionRender<Selection>.Value), Selection.NoneSelection) });
        }

        [Test]
        [TestCase(Selection.NoneSelection)]
        public void ExceptionOnGetNoneSelection(Selection noneSelection) {
            var render = new OneSelectionRender<Selection>(noneSelection);
            Assert.That(() => { var _ = render[noneSelection]; }, Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        [TestCase(Selection.NoneSelection, Selection.First, false)]
        [TestCase(Selection.NoneSelection, Selection.First, true)]
        [TestCase(Selection.NoneSelection, Selection.Third, true)]
        [TestCase(Selection.NoneSelection, Selection.Third, false)]
        [TestCase(null, 8, true)]
        [TestCase(null, 8, false)]
        public void RenderTest<T>(T noneSelection, T nextSelection, bool renderAndGetValue, bool shouldNotify = true) {
            {
                var render = new OneSelectionRender<T>(noneSelection);
                var actualNotify = new List<NotifyData<T>>();
                render.PropertyChanged += (sender, args) => {
                    var val = (sender as OneSelectionRender<T>)!;
                    actualNotify.Add(new(args.PropertyName!, val.Value));
                };

                if (renderAndGetValue) {
                    // render and get value.
                    render.Render(nextSelection);
                    render[nextSelection].Value.Is(true);
                }
                else {
                    // get value and render.
                    var notify = render[nextSelection];
                    notify.Value.Is(false);
                    render.Render(nextSelection);
                    notify.Value.Is(true);
                }
                actualNotify.ToArray().Is(
                    shouldNotify ?
                    new NotifyData<T>[] { new(nameof(OneSelectionRender<Selection>.Value), nextSelection) }
                    : Array.Empty<NotifyData<T>>());
            }
        }
    }
}
