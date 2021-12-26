using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TEA.MVVM {

    /// <summary>
    ///  レンダリングをした内容をViewに通知するだけのクラス
    ///  値が同じであれば、Viewへ通知しません。
    /// </summary>
    public class WriteNotify<T> : INotifyPropertyChanged, IReadOnlyValue<T>, IRender<T> {

        static PropertyChangedEventArgs ValueProeprty = new(nameof(Value));

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///  通知するかを判定するための同値判定
        /// </summary>
        readonly Func<T, T, bool> isSame;

        /// <summary>
        ///  Viewとのバインディング用変数
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        ///  比較器がnullであればデフォルトの比較を行います。
        /// </summary>
        public WriteNotify(T initial, IEqualityComparer<T>? comparer = null) {
            isSame = comparer is null ? (a, b) => EqualityComparer<T>.Default.Equals(a, b) : comparer.Equals;
            Value = initial;
        }

        /// <summary>
        ///  ラムダ式で比較を行います。
        /// </summary>
        public WriteNotify(T initial, Func<T, T, bool> isSame) {
            this.isSame = isSame;
            Value = initial;
        }

        public void Render(T state) {
            // 同値判定で参照が同じであればtrueを返せるように毎回代入する
            var prev = Value;
            Value = state;
            if (isSame(prev, state)) {
                return;
            }
            PropertyChanged?.Invoke(this, ValueProeprty);
        }
    }
}
