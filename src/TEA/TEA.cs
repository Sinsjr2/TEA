using System;
using System.Collections.Generic;

namespace TEA {
    public class TEA<TState, TMessage> : ITEA<TState, TMessage>
        where TState : IUpdate<TState, TMessage> {

        /// <summary>
        ///  レンダリングの状況を表します。
        /// </summary>
        enum State {
            None,

            /// <summary>
            ///  レンダリング中
            /// </summary>
            Rendering,

            /// <summary>
            ///  レンダリング中にディスパッチされた
            /// </summary>
            Dispached,
        }

        readonly IRender<TState> render;
        readonly int maxRendering;
        State teaState = State.None;

        /// <summary>
        ///   実行するべきメッセージ
        /// </summary>
        readonly List<TMessage> messages = new(16);


        TState currentState;
        public TState Current {
            get {
                try {
                    foreach (var msg in messages) {
                        currentState = currentState.Update(msg);
                    }
                }
                finally {
                    messages.Clear();
                }
                return currentState;
            }
        }

        /// <summary>
        ///   初期化と同時に初期状態をレンダリングします。
        /// </summary>
        /// <param name="initialState">レンダリングする初期値</param>
        /// <param name="render">レンダリング先</param>
        /// <param name="cacheMessageListSize">レンダリング中にディスパッチ時にメッセージをキャッシュするサイズです。</param>
        /// <param name="maxRendering">
        /// 無限ループをした時に検出するために、最大のレンダリング回数をしています。
        /// 1以上でないと一回でもレンダリングすると例外が発生します。
        /// この回数以上レンダリングすると無限ループしている可能性があるので例外を発生させます。
        /// </param>
        public TEA(TState initialState, IRender<TState> render, int cacheMessageListSize = 16, int maxRendering = 10) {
            this.maxRendering = maxRendering;
            messages = new(cacheMessageListSize);
            this.render = render;
            currentState = initialState;
            Render();
        }

        public void Dispatch(TMessage msg) {
            messages.Add(msg);
            if (teaState != State.None) {
                teaState = State.Dispached;
                return;
            }
            Render();
        }

        void Render() {
            teaState = State.Rendering;
            try {
                // 無限ループを回避するため
                // レンダリングした回数
                for (int renderCount = 0; renderCount < maxRendering; renderCount++) {
                    render.Render(Current);
                    // レンダー呼び出し中にディスパッチされたか
                    if (teaState != State.Dispached) {
                        return;
                    }
                    teaState = State.Rendering;
                }
                throw new InvalidOperationException($"レンダリングが指定された回数以上行われました。最大回数:{maxRendering}/n現在の状態:{currentState}");
            } finally {
                // 例外が発生したあとでもdispatchが呼び出せるようにしておく
                // もしくは、正常にreturnされた場合
                teaState = State.None;
            }
        }
    }
}
