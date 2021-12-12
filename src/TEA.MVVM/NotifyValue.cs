using System.Collections.Generic;
using System.ComponentModel;

namespace TEA.MVVM {

    /// <summary>
    ///  Viewから変更された値を受け取ったり、Viewへの変更通知を行います。
    ///  値が同じであればViewへ通知しません。
    ///  ただし、キャッシュと異なっている場合は通知します。
    /// </summary>
    public class NotifyValue<T> : INotifyPropertyChanged, ITEAComponent<T, T> {

        static PropertyChangedEventArgs ValueProeprty = new(nameof(Value));

        public event PropertyChangedEventHandler? PropertyChanged;

        IDispatcher<T>? dispatcher;

        /// <summary>
        ///  通知するかを判定するための比較器
        /// </summary>
        readonly IEqualityComparer<T> comparer;

        T currentValue;

        /// <summary>
        ///  Viewとのバインディング用変数
        /// </summary>
        public T Value {
            get => currentValue;
            set {
                if (comparer.Equals(currentValue, value)) {
                    return;
                }
                currentValue = value;
                dispatcher?.Dispatch(value);
            }
        }

        public NotifyValue(T initial, IEqualityComparer<T>? comparer = null) {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            currentValue = initial;
        }

        public void Setup(IDispatcher<T> dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Render(T state) {
            // 同値判定で参照が同じであればtrueを返せるように毎回代入する
            var prev = currentValue;
            currentValue = state;
            if (comparer.Equals(prev, state)) {
                return;
            }
            PropertyChanged?.Invoke(this, ValueProeprty);
        }
    }
}
