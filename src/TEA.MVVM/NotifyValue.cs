using System;
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
        ///  通知するかを判定するための同値判定
        /// </summary>
        readonly Func<T, T, bool> isSame;

        T currentValue;

        /// <summary>
        ///  Viewとのバインディング用変数
        /// </summary>
        public T Value {
            get => currentValue;
            set {
                if (isSame(currentValue, value)) {
                    return;
                }
                currentValue = value;
                dispatcher?.Dispatch(value);
            }
        }

        public NotifyValue(T initial, IEqualityComparer<T>? comparer = null) {
            isSame = comparer is null ? (a, b) => EqualityComparer<T>.Default.Equals(a, b) : comparer.Equals;
            currentValue = initial;
        }

        public NotifyValue(T initial, Func<T, T, bool> isSame) {
            this.isSame = isSame;
            currentValue = initial;
        }

        public void Setup(IDispatcher<T> dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Render(T state) {
            // 同値判定で参照が同じであればtrueを返せるように毎回代入する
            var prev = currentValue;
            currentValue = state;
            if (isSame(prev, state)) {
                return;
            }
            PropertyChanged?.Invoke(this, ValueProeprty);
        }
    }
}
