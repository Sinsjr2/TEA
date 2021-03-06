using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TEA.MVVM {

    /// <summary>
    ///  初期状態は未選択状態にしています。
    ///  指定した種類の要素のみtrueにそれ以外の要素をfalseにします。
    ///  ラジオボタンの選択に使用することを想定しています。
    ///  選択した要素の種類を通知します。
    ///  登録していない種類の値をレンダリングした場合は未選択になります。
    ///  また、レンダリングした時にValueに対しても変更通知をViewに行います。
    ///  これは、一つだけ選択状態にする管理を行いつつ、選択している種類を別のところに表示できるようにするためです。
    /// </summary>
    public class OneSelectionRender<TKind> : ITEAComponent<TKind, TKind>, IReadOnlyValue<TKind>, INotifyPropertyChanged {

        static readonly PropertyChangedEventArgs ValueProperty = new(nameof(Value));

#pragma warning disable CS8714 // キーがnullにならないようにメソッドを使用するときにnullチェックをしている
        readonly Dictionary<TKind, NotifyValue<bool>> selections = new();
#pragma warning restore CS8714
        readonly BufferDispatcher<TKind> dispatcher = new();
        readonly TKind noneSelection;
        readonly Func<TKind, TKind, bool> isSame;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///  現在選択している要素の種類
        ///  未選択の時はコンストラクタで渡した値になります。
        ///  バインディングできるようにしています。
        /// </summary>
        public TKind Value { get; private set; }

        /// <summary>
        ///  バインディングする値を取得します。
        ///  生成していない場合は新しく値を作ります。
        ///  取得したインスタンスに対して直接レンダリングしないでください。
        ///  コンストラクタで設定した未選択用の値やnullを取得すると例外が発生します。
        /// </summary>
        public NotifyValue<bool> this[TKind kind] {
            get {
                if (kind is null || isSame(kind, noneSelection)) {
                    throw new ArgumentException("can not get value: " + (kind?.ToString() ?? "null"), nameof(kind));
                }
                if (!selections.TryGetValue(kind, out var notify)) {
                    notify = new NotifyValue<bool>(false);
                    notify.Setup(new MessageWrapper<bool, TKind>(dispatcher, (d, isSelected) => {
                        var prevSelected = Value;
                        Value = kind;
                        if (isSame(prevSelected, kind)) {
                            return;
                        }
                        // １つだけtrueにするためにfalseにする
                        foreach (var notify in selections) {
                            notify.Value.Render(false);
                        }
                        if (Value != null && selections.TryGetValue(Value, out var currentNotify)) {
                            currentNotify.Render(!isSame(kind, noneSelection));
                        }
                        else {
                            Value = noneSelection;
                        }
                        if (!isSame(prevSelected, kind)) {
                            d.Dispatch(kind);
                        }
                    }));
                    selections[kind] = notify;
                }
                return notify;
            }
        }

        public OneSelectionRender(TKind noneSelection, IEqualityComparer<TKind>? comparer = null) {
            this.noneSelection = noneSelection;
            Value = noneSelection;
            isSame = comparer is null
                ? (a, b) => EqualityComparer<TKind>.Default.Equals(a, b)
                : (a, b) => comparer.Equals(a, b);
        }

        public OneSelectionRender(TKind noneSelection, Func<TKind, TKind, bool> isSame) {
            this.noneSelection = noneSelection;
            Value = noneSelection;
            this.isSame = isSame;
        }

        public void Setup(IDispatcher<TKind> dispatcher) {
            this.dispatcher.Setup(dispatcher);
        }

        public void Render(TKind state) {
            var prevSelected = Value;
            Value = state;
            if (isSame(prevSelected, state)) {
                return;
            }
            // １つだけtrueにするためにfalseにする
            foreach (var notify in selections) {
                notify.Value.Render(false);
            }
            if (Value != null && !isSame(Value, noneSelection)) {
                this[state].Render(true);
            }
            else {
                Value = noneSelection;
            }
            if (!isSame(prevSelected, state)) {
                PropertyChanged?.Invoke(this, ValueProperty);
            }
        }
    }
}
