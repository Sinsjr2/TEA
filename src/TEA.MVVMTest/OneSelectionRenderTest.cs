using System;
using System.Collections.Generic;
using System.Linq;
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
        [TestCase("")]
        public void ExceptionOnGetNoneSelection<T>(T noneSelection) {
            var render = new OneSelectionRender<T>(noneSelection);
            Assert.That(() => { var _ = render[noneSelection]; }, Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void ExceptionOnGetNull() {
            void Check<T>(T noneSelection, T? nullValue) {
                var render = new OneSelectionRender<T?>(noneSelection);
                Assert.That(() => { var _ = render[nullValue]; }, Throws.Exception.TypeOf<ArgumentException>());
            }
            Check("", null);
            Check<int?>(1, null);
        }

        [Test]
        [TestCase(Selection.NoneSelection, Selection.First, false)]
        [TestCase(Selection.NoneSelection, Selection.First, true)]
        [TestCase(Selection.NoneSelection, Selection.Third, true)]
        [TestCase(Selection.NoneSelection, Selection.Third, false)]
        [TestCase(null, "", true)]
        [TestCase(null, "", false)]
        public void RenderTest<T>(T noneSelection, T nextSelection, bool renderAndGetValue) {
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
                new NotifyData<T>[] { new(nameof(OneSelectionRender<Selection>.Value), nextSelection) });
        }

        [Test]
        public void RenderNullTest() {
            void Check<T>(T noneSelection, T initialSelection, T nextSelection) {
                var render = new OneSelectionRender<T>(noneSelection);
                var actualNotify = new List<NotifyData<T>>();
                render.PropertyChanged += (sender, args) => {
                    var val = (sender as OneSelectionRender<T>)!;
                    actualNotify.Add(new(args.PropertyName!, val.Value));
                };

                void CheckNotiry(T prevValue, T renderedValue) {
                    actualNotify.ToArray().Is(
                        EqualityComparer<T>.Default.Equals(prevValue, renderedValue)
                        ? Array.Empty<NotifyData<T>>()
                        : new[] { new NotifyData<T>(nameof(OneSelectionRender<T>.Value), renderedValue) });
                }

                render.Render(initialSelection);
                render.Value.Is(initialSelection);
                CheckNotiry(noneSelection, initialSelection);
                actualNotify.Clear();

                render.Render(nextSelection);
                render.Value.Is(nextSelection);
                CheckNotiry(initialSelection, nextSelection);
            }
            Check<int?>(null, 8, null);
            Check<string?>(null, "", null);
        }

        [Test]
        public void SetValueTest() {
            var render = new OneSelectionRender<Selection>(Selection.NoneSelection);

            var values = new[] {
                render[Selection.First],
                render[Selection.Second],
                render[Selection.Third]
            };

            values[2].Value = true;
            values.Select(x => x.Value).Is(new[] { false, false, true});

            values[0].Value = true;
            values.Select(x => x.Value).Is(new[] { true, false, false});

            values[0].Value = false;
            values.Select(x => x.Value).Is(new[] { false, false, false});

        }
    }
}
