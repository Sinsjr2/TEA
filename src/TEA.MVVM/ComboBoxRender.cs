using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TEA.Model;

namespace TEA.MVVM {

    /// <summary>
    ///  コンボボックスにバインディングするためのレンダー
    ///  選択が変化すると変化した要素のインデックスをディスパッチします。
    ///  リストの要素は書き込んだ値と異なれば書き換えます。
    ///  コンボボックスの中身をViewから変えることを想定していません。
    /// </summary>
    public class ComboBoxRender<T> : INotifyPropertyChanged, IReadOnlyValue<int>, ITEAComponent<ComboBoxModel<T>, int> {

        enum ChangeState {
            None,

            /// <summary>
            ///  SelectedIndex を変更中
            /// </summary>
            ChaingingIndex,

            /// <summary>
            ///  レンダリングで値が更新された
            /// </summary>
            Rendered
        }

        static readonly PropertyChangedEventArgs SelectedIndexProperty = new(nameof(Value));
        static readonly PropertyChangedEventArgs ItemsSourceProperty = new(nameof(ItemsSource));

        /// <summary>
        ///  Viewからインデックスを変更中にインデックスを変更した場合に
        ///  コンボボックスが更新するときのリストの入れ替えに仕様
        /// </summary>
        readonly ObservableCollection<T> tempList = new();

        readonly IEqualityComparer<T>? comparer;

        IDispatcher<int>? dispatcher;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///  インデックスを変更中に再度インデックスを変更したり、
        ///  要素を変更すると動作しないために使用しています。
        /// </summary>
        ChangeState changeState = ChangeState.None;

        int selectedIndex = 0;
        /// <summary>
        ///  ViewのコンボボックスのSelectedIndexにバインディングします。
        ///  セッターはViewからの通知のみで使用します。
        ///  ゲッターはViewModelから呼んでも問題ありません。
        /// </summary>
        public int Value {
            set {
                // 自身での変更中に通知される場合は現在のstateを真として新しい値を無視する
                if (changeState != ChangeState.None || selectedIndex == value) {
                    return;
                }
                selectedIndex = value;
                changeState = ChangeState.ChaingingIndex;
                try {
                    dispatcher?.Dispatch(value);

                    // Viewからインデックスの変更中にインデックスを変更すると
                    // 反映されないためその対策
                    if (changeState == ChangeState.Rendered) {
                        var backup = ItemsSource;
                        ItemsSource = tempList;
                        // 一度リストを入れ替えることでインデックスを設定できるようにする
                        PropertyChanged?.Invoke(this, ItemsSourceProperty);
                        // バインディングした時に何か値が入っているかもしれないのでクリア
                        tempList.Clear();
                        ItemsSource = backup;
                        // リストの要素数を変更した時のインデックスの通知を無視しているため
                        // 強制的にインデックスの通知を行う
                        RenderCurrentState(true);
                    }
                }
                finally {
                    changeState = ChangeState.None;
                }
            }
            get => selectedIndex;
        }

        /// <summary>
        ///  コンボボックスの中身
        /// </summary>
        IReadOnlyList<T>? items;
        public ObservableCollection<T> ItemsSource { get; private set; } = new();

        public ComboBoxRender(IEqualityComparer<T>? comparer = null) {
            this.comparer = comparer;
        }

        public void Setup(IDispatcher<int> dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Render(ComboBoxModel<T> state) {
            var prevIndex = selectedIndex;
            selectedIndex = state.SelectedIndex;
            items = state.Items;

            // インデックス変更中はViewへの通知を先送り
            if (changeState != ChangeState.None) {
                changeState = ChangeState.Rendered;
                return;
            }
            RenderCurrentState(prevIndex != state.SelectedIndex);
        }

        void RenderCurrentState(bool notifyIndex) {
            if (items is null) {
                // 呼ばれることは無いが念のため
                // nullである時コンボボックスは空になったほうが自然であるため
                ItemsSource.Clear();
                return;
            }
            var endIndex = ItemsSource.ApplyToList(items, comparer);
            // 配列の要素の移動を最小化するために後ろから削除する
            for (int i = ItemsSource.Count; endIndex < i; i--) {
                ItemsSource.RemoveAt(i);
            }
            if (notifyIndex) {
                PropertyChanged?.Invoke(this, SelectedIndexProperty);
            }
        }
    }
}
