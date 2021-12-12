using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TEA.MVVM {

    /// <summary>
    ///  可変個の要素をViewに通知します。
    ///  各レンダーからの通知を１つにまとめ、
    ///  0から始める何番目のレンダーで発生したかをディスパッチします。
    /// </summary>
    public class CollectionRender<TState, TMessage>:
        ObservableCollection<ITEAComponent<TState, TMessage>>,
        ITEAComponent<IEnumerable<TState>, KeyValuePair<int, TMessage>> {

        readonly Func<ITEAComponent<TState, TMessage>> createRender;
        IDispatcher<KeyValuePair<int, TMessage>>? dispatcher;

        public CollectionRender(Func<ITEAComponent<TState, TMessage>> createRender) {
            this.createRender = createRender;
        }

        public void Setup(IDispatcher<KeyValuePair<int, TMessage>> dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Render(IEnumerable<TState> state) {
            var d = dispatcher ?? NullDispatcher<KeyValuePair<int, TMessage>>.Instance;
            var endIndex = this.ApplyToRender(d, createRender, state);
            // 配列の要素の移動を最小化するために後ろから削除する
            for (int i = Count; endIndex < i; i--) {
                RemoveItem(i);
            }
        }
    }
}
