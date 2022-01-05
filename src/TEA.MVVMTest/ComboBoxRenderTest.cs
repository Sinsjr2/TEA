using System;
using System.Collections.Generic;
using System.Collections.Generic.Immutable;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using TEA.Model;
using TEA.MVVM;
using TestTools;

namespace TEA.MVVMTest {
    public class ComboBoxRenderTest {

        [Test]
        [TestCase(0)]
        [TestCase("")]
        public void InitialValueTest<T>(T _) {
            var x = new ComboBoxRender<T>();
            x.Value.Is(0);
            x.ItemsSource.ToArray().Is(Array.Empty<T>());
        }

        public record NotifyData(string PropertyName, int SelectedIndex, EqList<int> Items);
        public record RenderNotifyPropertyChangedTestData(ComboBoxModel<int> InitialData, ComboBoxModel<int> InputData, IReadOnlyList<NotifyData> Expected, bool ShouldCallChanged);

        public static RenderNotifyPropertyChangedTestData[] GetRenderNotifyPropertyChangedTestData() {
            return new RenderNotifyPropertyChangedTestData[] {
                new(new(0, Array.Empty<int>()), new(0, new[] { 9, 10 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 0, new[] { 9, 10 }.ToEqList()) }, true),
                new(new(0, Array.Empty<int>()), new(1, new[] { 9, 10 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 1, new[] { 9, 10 }.ToEqList()) }, true),
                new(new(2, new[] { 1, 2, 3 }), new(1, new[] { 9, 10 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 1, new[] { 9, 10 }.ToEqList()) }, true),
                new(new(1, new[] { 3, 4, 5, 6 }), new(2, new[] { 8, 9, 10, 11 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 2, new[] { 8, 9, 10, 11 }.ToEqList()) }, true),
                new(new(1, new[] { 1, 2 }), new(2, new[] { 8, 9, 10 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 2, new[] { 8, 9, 10 }.ToEqList()) }, true),
                new(new(1, new[] { 1, 2 }), new(1, new[] { 8, 9, 10 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 1, new[] { 8, 9, 10 }.ToEqList()) }, true),
                new(new(0, new[] { 1, 2 }), new(0, new[] { 1 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 0, new[] { 1 }.ToEqList()) }, true),
                new(new(1, new[] { 1, 2 }), new(0, new[] { 1, 2 }), new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), 0, new[] { 1, 2 }.ToEqList()) }, false),
                new(new(1, new[] { 1, 2 }), new(1, new[] { 1, 2 }), Array.Empty<NotifyData>(), false),
            };
        }

        [Test]
        [TestCaseSource(nameof(GetRenderNotifyPropertyChangedTestData))]
        public void RenderNotifyPropertyChangedTest(RenderNotifyPropertyChangedTestData data) {
            var render = new ComboBoxRender<int>();
            var actualNotify = new List<NotifyData>();
            render.Render(data.InitialData);
            render.Value.Is(data.InitialData.SelectedIndex);
            render.ItemsSource.Is(data.InitialData.Items);

            render.PropertyChanged += (sender, args) => {
                var val = (sender as ComboBoxRender<int>)!;
                actualNotify.Add(new NotifyData(args.PropertyName!, val.Value, val.ItemsSource.ToEqList()));
            };
            bool calledChanged = false;
            render.ItemsSource.CollectionChanged += (_, __) => calledChanged = true;
            render.Render(data.InputData);
            calledChanged.Is(data.ShouldCallChanged);
            actualNotify.ToArray().Is(data.Expected);
            render.Value.Is(data.InputData.SelectedIndex);
            render.ItemsSource.ToArray().Is(data.InputData.Items);
        }

        public record SetIndexAndRenderOnDispatchTestData(ComboBoxModel<int> InitialData, int ChangedIndex, bool ShouldDispatch, ComboBoxModel<int> RenderData, IReadOnlyList<NotifyData> Expected);
        public static SetIndexAndRenderOnDispatchTestData[] GetSetIndexAndRenderOnDispatchTestData() {
            var dummyComboBox = new ComboBoxModel<int>(-1, Array.Empty<int>());
            return new SetIndexAndRenderOnDispatchTestData[] {
                new(new(1, new[] { 0, 1 }), 0, true, new(0, new[] { 0 }), new NotifyData[] {
                        new(nameof(ComboBoxRender<int>.Value), 0, new[] { 0 }.ToEqList()) }),
                new(new(1, new[] { 0, 1, 2 }), 0, true, new(2, new[] { 0, 1, 2 }), new NotifyData[] {
                        new(nameof(ComboBoxRender<int>.ItemsSource), -1, EqList<int>.Empty),
                        new(nameof(ComboBoxRender<int>.ItemsSource), -1, new[] { 0, 1, 2 }.ToEqList()),
                        new(nameof(ComboBoxRender<int>.Value), 2, new[] { 0, 1, 2 }.ToEqList())  }),
                new(new(1, new[] { 0, 1 }), 1, false, dummyComboBox, Array.Empty<NotifyData>()),
                new(new(0, new[] { 0, 1 }), 0, false, dummyComboBox, Array.Empty<NotifyData>()),
            };
        }

        [Test]
        [TestCaseSource(nameof(GetSetIndexAndRenderOnDispatchTestData))]
        public void SetIndexAndRenderOnDispatchTest(SetIndexAndRenderOnDispatchTestData data) {
            var render = new ComboBoxRender<int>();
            var dispatchedIndex = new List<int>();
            render.Setup(new BufferDispatcher<int>(), (_, index) => dispatchedIndex.Add(index));
            render.Render(data.InitialData with { SelectedIndex = -1 });
            dispatchedIndex.ToArray().Is(Array.Empty<int>());

            render.Value = data.InitialData.SelectedIndex;
            dispatchedIndex.Clear();

            var actualNotify = new List<NotifyData>();
            render.PropertyChanged += (sender, args) => {
                var val = (sender as ComboBoxRender<int>)!;
                actualNotify.Add(new NotifyData(args.PropertyName!, val.Value, val.ItemsSource.ToEqList()));
            };

            render.Setup(new BufferDispatcher<int>(), (_, index) => {
                render.Value.Is(index);
                dispatchedIndex.Add(index);
                render.Render(data.RenderData);
            });
            var expectedComboBoxData = data.ShouldDispatch
                ? data.RenderData
                : data.InitialData;
            render.Value = data.ChangedIndex;
            actualNotify.ToArray().Is(data.Expected);
            dispatchedIndex.ToArray().Is(data.ShouldDispatch ? new[] { data.ChangedIndex } : Array.Empty<int>());
            render.Value.Is(expectedComboBoxData.SelectedIndex);
            render.ItemsSource.Is(expectedComboBoxData.Items);
        }

        [Test]
        [TestCase(1, new[] { 0, 1, 2, 3 }, 2, new[] { 4, 5, 6 }, -1)]
        [TestCase(1, new[] { 0, 1, 2, }, 1, new[] { 4, 5, 6 }, -1)]
        [TestCase(2, new[] { 0, 1, 2, }, 2, new[] { 4, 5, 6 }, 0)]
        public void ItemsRemoveReturnMinus1(int initialIndex, int[] initialItems, int nextIndex, int[] nextItems, int setValue) {
            var render = new ComboBoxRender<int>();
            render.Render(new(initialIndex, initialItems));
            bool calledChanged = false;
            render.ItemsSource.CollectionChanged += (_, args) => {
                calledChanged = true;
                render.Value = setValue;
                render.Value.Is(setValue);
            };
            var actualNotify = new List<NotifyData>();
            render.PropertyChanged += (sender, args) => {
                var val = (sender as ComboBoxRender<int>)!;
                actualNotify.Add(new NotifyData(args.PropertyName!, val.Value, val.ItemsSource.ToEqList()));
            };

            var expected = new ComboBoxModel<int>(nextIndex, nextItems);
            render.Render(expected);
            render.Value.Is(expected.SelectedIndex);
            calledChanged.Is(true);
            actualNotify.ToArray().Is(new NotifyData[] { new(nameof(ComboBoxRender<int>.Value), expected.SelectedIndex, expected.Items.ToEqList()) });
        }
    }
}
