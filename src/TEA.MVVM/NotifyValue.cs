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

        enum InnerState {
            None,
            Notifying,
            SetValue,
            RetryRender,
        }

        static PropertyChangedEventArgs ValueProeprty = new(nameof(Value));

        /// <summary>
        ///  通知するかを判定するための同値判定
        /// </summary>
        readonly Func<T, T, bool> isSame;

        public event PropertyChangedEventHandler? PropertyChanged;
        IDispatcher<T>? dispatcher;
        InnerState innerState;

        T? latestSetValue = default;
        T currentValue;
        /// <summary>
        ///  Viewとのバインディング用変数
        /// </summary>
        public T Value {
            get => currentValue;
            set {
                latestSetValue = value;
                NotifyCore(value, true);
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
            NotifyCore(state, false);
        }

        void NotifyCore(T state, bool isFromSetter) {
            if (isSame(currentValue, state)) {
                    return;
            }
            currentValue = state;
            // avoid loop;
            if (innerState != InnerState.None) {
                innerState = isFromSetter ? InnerState.SetValue
                    : innerState == InnerState.SetValue ? InnerState.SetValue
                    : InnerState.RetryRender;
                return;
            }
            innerState = InnerState.Notifying;
            try {
                if (isFromSetter) {
                    dispatcher?.Dispatch(currentValue);
                }
                T prevState;
                do {
                    innerState = InnerState.Notifying;
                    prevState = currentValue;
                    PropertyChanged?.Invoke(this, ValueProeprty);
                    var prevInnerState = innerState;
                    if (innerState == InnerState.SetValue) {
                        innerState = InnerState.Notifying;
                        dispatcher?.Dispatch(latestSetValue!);
                    }
                    if (prevInnerState != InnerState.Notifying || innerState != InnerState.Notifying) {
                        innerState = InnerState.Notifying;
                        prevState = currentValue;
                        PropertyChanged?.Invoke(this, ValueProeprty);
                    }
                }
                while (innerState != InnerState.Notifying && !isSame(currentValue, prevState));
            }
            finally {
                innerState = InnerState.None;
                latestSetValue = default;
            }
        }
    }
}
