using System;
using NUnit.Framework;
using TEA;
using TestTools;

namespace TEATest {
    public class TEATest {

        public enum SampleMsg {
            None,
            Set3,
            ChangedV1,
        }

        public record SampleState(int Value1, int Value2) : IUpdate<SampleState, SampleMsg> {
            public SampleState Update(SampleMsg msg) {
                return msg switch {
                    SampleMsg.ChangedV1 => new(6, 6),
                    SampleMsg.Set3 => new(3, 3),
                    _ => this,
                };
            }
        }

        public class DispathOnRenderingRender : ITEAComponent<SampleState, SampleMsg> {
            IDispatcher<SampleMsg>? dispatcher;

            public int V1 { get; set; }
            public int V2 { get; set; }
            public int RenderedCount { get; private set; }

            public void Setup(IDispatcher<SampleMsg> dispatcher) {
                this.dispatcher = dispatcher;
            }

            public void Render(SampleState state) {
                RenderedCount++;
                if (state.Value1 != V1) {
                    V1 = state.Value1;
                    dispatcher!.Dispatch(SampleMsg.ChangedV1);
                }
                V2 = state.Value2;
            }
        }

        [Test]
        public void DispathOnRendering() {
            var dispatcher = new BufferDispatcher<SampleMsg>();
            var render = new DispathOnRenderingRender();
            render.Setup(dispatcher);
            var initial = new SampleState(0, 2);
            var tea = new TEA<SampleState, SampleMsg>(initial, render);

            // 初期値が入っているか
            tea.Current.Is(initial);

            // 初回のレンダリングが行われているか
            render.RenderedCount.Is(1);
            render.V1.Is(0);
            render.V2.Is(2);
            dispatcher.Setup(tea);

            // レンダリングされていないことを確認
            render.RenderedCount.Is(1);

            // レンダリング中にディスパッチしても古い状態に戻らないことを確認
            dispatcher.Dispatch(SampleMsg.Set3);
            render.RenderedCount.Is(4);
            render.V1.Is(6);
            render.V2.Is(6);

            // 内部状態が同じに描画結果と同じことを確認
            tea.Current.Is(new(render.V1, render.V2));

            // 複数回読んでも変化しないことを確認
            tea.Current.Is(new(render.V1, render.V2));
        }

        /// <summary>
        ///  指定した回数ディスパッチします。
        /// </summary>
        public class RepeatDispatchRender : ITEAComponent<SampleState, SampleMsg> {
            readonly int count;

            int renderingCount = 0;

            public bool IgnoreRender = true;

            IDispatcher<SampleMsg>? dispatcher;

            public RepeatDispatchRender(int count) {
                this.count = count;
            }

            public void Setup(IDispatcher<SampleMsg> dispatcher) {
                this.dispatcher = dispatcher;
            }

            public void Render(SampleState state) {
                if (IgnoreRender) {
                    return;
                }
                renderingCount++;
                if (renderingCount <= count) {
                    dispatcher?.Dispatch(0);
                }
            }
        }

        /// <summary>
        ///  レンダリング中に指定回数以上ループする時に例外が発生する場合としない場合のテスト
        /// </summary>
        [Test]
        [TestCase(10, 10, true)]
        [TestCase(10, 9, false)]
        [TestCase(4, 4, true)]
        [TestCase(5, 4, false)]
        [TestCase(4, 3, false)]
        [TestCase(1, 0, false)]
        public void LoopErrorTest(int maxDispatchCount, int dispatchCount, bool shouldBeError) {
            var dispatcher = new BufferDispatcher<SampleMsg>();
            var render = new RepeatDispatchRender(dispatchCount);
            render.Setup(dispatcher);
            var initial = new SampleState(0, 2);
            var tea = new TEA<SampleState, SampleMsg>(initial, render, maxRendering : maxDispatchCount);
            dispatcher.Setup(tea);

            render.IgnoreRender = false;
            if (shouldBeError) {
                Assert.That(() => dispatcher.Dispatch(SampleMsg.None), Throws.InvalidOperationException);
            }
            else {
                dispatcher.Dispatch(SampleMsg.None);
            }
        }

        class CaptureValueMiddleware : IDispatcher<int> {
            readonly ITEA<CountUpModel, int> tea;
            int dispatchedCount = 0;

            public CountUpModel? GotValue { get; private set; }

            public CaptureValueMiddleware(ITEA<CountUpModel, int> tea) {
                this.tea = tea;
            }

            public void Dispatch(int msg) {
                dispatchedCount++;
                tea.Dispatch(msg);
                if (dispatchedCount == 9) {
                    GotValue = tea.Current;
                }
            }
        }

        record CountUpModel(int RenderedCount) : IUpdate<CountUpModel, int> {
            public bool ShouldDispatch => RenderedCount == 1;

            public CountUpModel Update(int msg) {
                return this with { RenderedCount = RenderedCount + 1 };
            }
        }

        class RepeatAndLatestRender : ITEAComponent<CountUpModel, int> {
            IDispatcher<int>? dispatcher;

            public CountUpModel? LatestValue { get; private set; }

            public void Setup(IDispatcher<int> dispatcher) {
                this.dispatcher = dispatcher;
            }

            public void Render(CountUpModel state) {
                LatestValue = state;
                if (state.ShouldDispatch) {
                    for (int i = 0; i < 10; i++) {
                        dispatcher?.Dispatch(0);
                    }
                }
            }
        }

        /// <summary>
        ///  レンダリング中にミドルウェアで現在の状態を取得できるか
        ///  TEAのバッファにためた状態で取得した値が期待値になっていることを確認する
        /// </summary>
        [Test]
        public void CurrentStateOnRenderingAndDispaching() {
            var dispatcher = new BufferDispatcher<int>();
            var render = new RepeatAndLatestRender();
            render.Setup(dispatcher);
            var initial = new CountUpModel(0);
            var tea = new TEA<CountUpModel, int>(initial, render);
            var middleware = new CaptureValueMiddleware(tea);
            dispatcher.Setup(middleware);

            // 何回もdispatch していなことを確認
            render.LatestValue!.RenderedCount.Is(0);
            middleware.GotValue.Is(null);

            // ディスパッチ中でも状態を取得できることを確認
            tea.Dispatch(0);
            render.LatestValue.RenderedCount.Is(11);
            middleware.GotValue!.RenderedCount.Is(10);
        }

        class ExceptionForTest : Exception { }

        record ExceptionModel(int RenderedCount) : IUpdate<ExceptionModel, bool> {
            public ExceptionModel Update(bool shouldThrow) {
                return shouldThrow
                    ? throw new ExceptionForTest()
                    : this with { RenderedCount = RenderedCount + 1 };
            }
        }

        class ExceptionTestRender : ITEAComponent<ExceptionModel, bool> {
            public ExceptionModel? LatestValue { get; private set; }

            public void Setup(IDispatcher<bool> dispatcher) {
            }
            public void Render(ExceptionModel state) {
                LatestValue = state;
            }
        }

        /// <summary>
        ///  Modelで例外が発生した時でもあとの処理を継続できることを確認
        /// </summary>
        [Test]
        public void ExceptionOccurredOccredTest() {
            var dispatcher = new BufferDispatcher<bool>();
            var render = new ExceptionTestRender();
            render.Setup(dispatcher);
            var initial = new ExceptionModel(10);
            var tea = new TEA<ExceptionModel, bool>(initial, render);
            dispatcher.Setup(tea);

            tea.Current.RenderedCount.Is(10);
            tea.Dispatch(false);
            tea.Current.RenderedCount.Is(11);

            Assert.That(() => tea.Dispatch(true), Throws.TypeOf<ExceptionForTest>());
            tea.Current.RenderedCount.Is(11);

            tea.Dispatch(false);
            tea.Current.RenderedCount.Is(12);

            Assert.That(() => tea.Dispatch(true), Throws.TypeOf<ExceptionForTest>());
            tea.Current.RenderedCount.Is(12);

            tea.Dispatch(false);
            tea.Current.RenderedCount.Is(13);

        }
    }
}
