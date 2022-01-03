using NUnit.Framework;
using TestTools;
using TEA.MVVM;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace TEA.MVVMTest {
    public class NotifyValueTest {

        [Test]
        public void InitialValueTest() {
            new NotifyValue<int>(5).Value.Is(5);
            new NotifyValue<int?>(null).Value.Is(null);
            new NotifyValue<string?>(null).Value.Is(null);
            new NotifyValue<string>("aaa").Value.Is("aaa");
        }

        [Test]
        public void RenderPropertyChangedCallTest() {
            // Renderを呼ぶ場合
            PropertyChangedEventArgs? args = null;
            int? val = null;
            var render = new NotifyValue<int>(0);
            int count = 0;
            render.PropertyChanged += (sender, b) => {
                args = b;
                val = (sender as NotifyValue<int>)!.Value;
                count++;
            };
            render.Render(5);
            val.Is(5);
            args!.PropertyName!.Is(nameof(NotifyValue<int>.Value));
            count.Is(1);
        }

        [Test]
        public void ValuePropertyChangedCallTest() {
            // Valueに代入する場合
            // 複数にバインディングする場合に通知するための確認
            PropertyChangedEventArgs? args = null;
            int? val = null;
            var render = new NotifyValue<int>(0);
            int count = 0;
            render.PropertyChanged += (sender, b) => {
                args = b;
                count++;
                val = (sender as NotifyValue<int>)!.Value;
            };
            render.Value = 10;
            val.Is(10);
            args!.PropertyName!.Is(nameof(NotifyValue<int>.Value));
            count.Is(1);
        }

        [Test]
        public void RenderOnSetValuePropertyChangedCallTest() {
            // Valueに代入してRenderを呼ぶ場合
            PropertyChangedEventArgs? args = null;
            int? val = null;
            var render = new NotifyValue<int>(0);
            int count = 0;
            render.PropertyChanged += (sender, b) => {
                args = b;
                count++;
                val = (sender as NotifyValue<int>)!.Value;
            };
            int inputState = 20;
            var msgs = new List<int>();
            render.Setup(new BufferDispatcher<int>(), (_, x) => {
                msgs.Add(x);
                msgs.Add(render.Value);
                render.Render(inputState);
            });
            render.Value = 5;
            val.Is(20);
            count.Is(1);
            args!.PropertyName!.Is(nameof(NotifyValue<int>.Value));
            msgs.ToArray().Is(new[] { 5, 5 });
            msgs.Clear();
            count = 0;

            // 同じ値をValueに入れた場合
            render.Value = 20;
            count.Is(0);
            msgs.ToArray().Is(Array.Empty<int>());

            // 違う値を入れると通知される
            render.Value = 6;
            inputState = 7;
            count.Is(1);
            msgs.ToArray().Is(new[] { 6, 6 });
        }

        [Test]
        public void SetupAndSetValuePropertyChangedCallTest() {
            // setupしていてもPropertyChangedが呼ばれることを確認
            var render = new NotifyValue<string>("");
            string? val = null;
            int count = 0;
            render.Setup(new BufferDispatcher<string>());
            render.PropertyChanged += (sender, b) => {
                count++;
                val = (sender as NotifyValue<string>)!.Value;
            };

            render.Value = "abc";
            val.Is("abc");
            render.Value.Is("abc");
            count.Is(1);

            // 同じ値であれば通知されないことを確認
            render.Value = "abc";
            render.Value.Is("abc");
            count.Is(1);
        }

        [Test]
        public void NoDispatchOnRenderTest() {
            // レンダリングした時にディスパッチしないことを確認
            var render = new NotifyValue<string>("");
            var count = 0;
            render.Setup(new BufferDispatcher<string>(), (d, msg) => count++);
            render.Render("");
            render.Render("1");
            count.Is(0);
        }

        public enum DoSomethingOnceTestPattern {
            Render,
            SetValue
        }

        [Test]
        public void RenderOnPropertyChangedTest() {
            // プロパティが変更された時にレンダリングできることを確認
            var render = new NotifyValue<int>(6);
            var count = 0;
            int inputValue = 5;
            int? notifyValueAfter = null;
            render.PropertyChanged += (sender, _) => {
                count++;
                render.Render(inputValue);
                notifyValueAfter = (sender as NotifyValue<int>)!.Value;
            };

            // case of Render. 1 notify
            render.Render(5);
            count.Is(1);
            notifyValueAfter.Is(5);

            // case of Value. 1 notify
            count = 0;
            inputValue = 100;
            render.Value = 100;
            count.Is(1);
            notifyValueAfter.Is(100);
            render.Value.Is(100);

            // case of Render. 2 notify
            count = 0;
            inputValue = 25;
            render.Render(3);
            count.Is(2);
            notifyValueAfter.Is(25);
            render.Value.Is(25);

            // case of Value. 2 notify
            count = 0;
            inputValue = 111;
            render.Value = 79;
            count.Is(2);
            notifyValueAfter.Is(111);
            render.Value.Is(111);
        }

        [Test]
        public void DoOnceOnPropertyChangedTest() {
            // プロパティが変更された時にValueに代入できることを確認
            // また、ディスパッチしたメッセージが期待値であることを確認する
            var render = new NotifyValue<int>(6);
            var count = 0;
            int inputValue = 5;
            int? notifyValueAfter = null;
            var msgs = new List<int>();
            render.Setup(new BufferDispatcher<int>(), (d, msg) => msgs.Add(msg));
            render.PropertyChanged += (sender, _) => {
                count++;
                render.Value = inputValue;
                notifyValueAfter = (sender as NotifyValue<int>)!.Value;
            };

            // case of Render. 1 notify
            render.Render(5);
            count.Is(1);
            msgs.ToArray().Is(Array.Empty<int>());
            notifyValueAfter.Is(5);

            // case of Value. 1 notify
            count = 0;
            inputValue = 100;
            render.Value = 100;
            count.Is(1);
            msgs.ToArray().Is(new[] { 100 });
            notifyValueAfter.Is(100);
            render.Value.Is(100);

            // case of Render. 2 notify
            msgs.Clear();
            count = 0;
            inputValue = 25;
            render.Render(3);
            msgs.ToArray().Is(new[] { 25 });
            notifyValueAfter.Is(25);
            render.Value.Is(25);
            count.Is(2);

            // case of Value. 2 notify
            msgs.Clear();
            count = 0;
            inputValue = 111;
            render.Value = 79;
            msgs.ToArray().Is(new[] { 79, 111 });
            notifyValueAfter.Is(111);
            render.Value.Is(111);
            count.Is(2);
        }

        public enum DoTwiceOnPropertyChangedPattern {
            SetValueTwice,
            RenderTwice,
            SetValueAndRender,
            RenderAndSetValue
        }

        [Test]
        [TestCase(DoTwiceOnPropertyChangedPattern.SetValueTwice     , new[] { 6 } , new[] { 7, 9 })]
        [TestCase(DoTwiceOnPropertyChangedPattern.RenderTwice       , new int[0]  , new[] { 7 })]
        [TestCase(DoTwiceOnPropertyChangedPattern.SetValueAndRender , new[] { 5 } , new[] { 7, 5 })]
        [TestCase(DoTwiceOnPropertyChangedPattern.RenderAndSetValue , new[] { 6 } , new[] { 7, 9 })]
        public void DoTwiceOnPropertyChangedTest(DoTwiceOnPropertyChangedPattern pattern,
                                                 int[] expectedCaseOfRender, int[] expectedCaseOfValue) {
            var render = new NotifyValue<int>(0);
            var count = 0;
            int inputValue1 = 5;
            int inputValue2 = 6;
            int? notifyValueAfter = null;
            var msgs = new List<int>();
            render.Setup(new BufferDispatcher<int>(), (_, msg) => msgs.Add(msg));
            // confirm to avoid loop
            // NotifyValue -> (PropertyChanged) View -> (setter) NotifyValue
            render.PropertyChanged += (sender, _) => {
                count++;
                switch (pattern) {
                    case DoTwiceOnPropertyChangedPattern.SetValueTwice:
                        render.Value = inputValue1;
                        render.Value = inputValue2;
                        break;
                    case DoTwiceOnPropertyChangedPattern.RenderTwice:
                        render.Render(inputValue1);
                        render.Render(inputValue2);
                        break;
                    case DoTwiceOnPropertyChangedPattern.SetValueAndRender:
                        render.Value = inputValue1;
                        render.Render(inputValue2);
                        break;
                    case DoTwiceOnPropertyChangedPattern.RenderAndSetValue:
                        render.Render(inputValue1);
                        render.Value = inputValue2;
                        break;
                }
                notifyValueAfter = (sender as NotifyValue<int>)!.Value;
            };

            // case of Render
            render.Render(2);
            count.Is(2);
            render.Value.Is(6);
            notifyValueAfter.Is(6);
            msgs.ToArray().Is(expectedCaseOfRender);

            // case of Value
            count = 0;
            msgs.Clear();
            inputValue2 = 9;
            render.Value = 7;
            render.Value.Is(9);
            notifyValueAfter.Is(9);
            msgs.ToArray().Is(expectedCaseOfValue);
            count.Is(2);
        }

        [Test]
        public void EqualsLambdaTest() {
            // ラムダ式で同値比較が行えることを確認
            var render = new NotifyValue<string>("1", (a, b) => a.Length == b.Length);
            var count = 0;
            render.PropertyChanged += (sender, b) => {
                count++;
            };

            // case of Value. non change
            render.Value = "6";
            render.Value.Is("1");
            count.Is(0);

            // case of Render.  non change
            render.Render("9");
            render.Value.Is("1");
            count.Is(0);

            // case of Value. should change
            render.Value = "77";
            render.Value.Is("77");
            count.Is(1);

            // case of Render. should change
            count = 0;
            render.Render("777");
            render.Value.Is("777");
            count.Is(1);
        }
    }
}
