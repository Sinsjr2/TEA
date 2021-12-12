using System;
using System.Collections.Generic;

namespace TEA {
    public class TEA<State, Message> : ITEA<State, Message>
        where State : IUpdate<State, Message> {

        readonly IRender<State> render;
        readonly int maxRendering;
        readonly int cacheMessageListSize;
        bool isCallingRender = false;

        /// <summary>
        ///   実行するべきメッセージ
        /// </summary>
        readonly List<Message> messages = new List<Message>(16);


        State currentState;
        public State Current {
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
        public TEA(State initialState, IRender<State> render, int cacheMessageListSize = 16, int maxRendering = 10) {
            this.maxRendering = maxRendering;
            this.cacheMessageListSize = cacheMessageListSize;
            this.render = render;
            currentState = initialState;
            Render();
        }

        public void Dispatch(Message msg) {
            messages.Add(msg);
            if (isCallingRender) {
                return;
            }
            Render();
        }

        void Render() {
            isCallingRender = true;
            try {
                // 無限ループを回避するため
                // レンダリングした回数
                for (int renderCount = 0; renderCount < maxRendering; renderCount++) {
                    render.Render(Current);
                    // レンダー呼び出し中にdispacherが呼ばれたかが変更されたか
                    if (messages.Count <= 0) {
                        return;
                    }
                }
                throw new InvalidOperationException($"レンダリングが指定された回数以上行われました。最大回数:{maxRendering}/n現在の状態:{currentState}");
            } finally {
                // 例外が発生したあとでもdispatchが呼び出せるようにしておく
                // もしくは、正常にreturnされた場合
                isCallingRender = false;
            }
        }
    }
}
