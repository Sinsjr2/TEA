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
        ///  通知するかを判定するための比較器
        /// </summary>
        readonly IEqualityComparer<T> comparer;

        /// <summary>
        ///  Viewとのバインディング用変数
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        ///  比較器がnullであればデフォルトの比較を行います。
        /// </summary>
        public WriteNotify(T initial, IEqualityComparer<T>? comparer = null) {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            Value = initial;
        }

        public void Render(T state) {
            // 同値判定で参照が同じであればtrueを返せるように毎回代入する
            var prev = Value;
            Value = state;
            if (comparer.Equals(Value, state)) {
                return;
            }
            PropertyChanged?.Invoke(this, ValueProeprty);
        }
    }
}
