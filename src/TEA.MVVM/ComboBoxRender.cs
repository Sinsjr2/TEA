using System;
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
            Chainging,
        }

        static readonly PropertyChangedEventArgs SelectedIndexProperty = new(nameof(Value));
        static readonly PropertyChangedEventArgs ItemsSourceProperty = new(nameof(ItemsSource));

        readonly Func<T, T, bool> isSame;

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
                changeState = ChangeState.Chainging;
                try {
                    dispatcher?.Dispatch(value);

                    // Viewからインデックスの変更中にインデックスを変更すると
                    // 反映されないためその対策
                    if (value != selectedIndex) {
                        var itemsBackup = ItemsSource;
                        var indexBackup = selectedIndex;
                        ItemsSource = new ObservableCollection<T>();
                        selectedIndex = -1;
                        // 一度リストを入れ替えることでインデックスを設定できるようにする
                        PropertyChanged?.Invoke(this, ItemsSourceProperty);
                        ItemsSource = itemsBackup;
                        PropertyChanged?.Invoke(this, ItemsSourceProperty);
                        selectedIndex = indexBackup;
                    }
                    RenderCurrentState(value != selectedIndex);
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
            isSame = comparer is null
                ? (a, b) => EqualityComparer<T>.Default.Equals(a, b)
                : (a, b) => comparer.Equals(a, b);
        }

        public ComboBoxRender(Func<T, T, bool> isSame) {
            this.isSame = isSame;
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
                return;
            }
            // コンボボックスの要素数を変化されるとViewから通知されるので選択しているインデックスを捨てるため
            changeState = ChangeState.Chainging;
            try {
                RenderCurrentState(prevIndex != state.SelectedIndex);
            }
            finally {
                changeState = ChangeState.None;
            }
        }

        void RenderCurrentState(bool notifyIndex) {
            if (items is null) {
                // 呼ばれることは無いが念のため
                // nullである時コンボボックスは空になったほうが自然であるため
                selectedIndex = -1;
                ItemsSource.Clear();
                return;
            }
            var prevItemsCount = ItemsSource.Count;
            var backupIndex = selectedIndex;
            // set Value -1 so return -1.
            selectedIndex = -1;
            var endIndex = ItemsSource.ApplyToList(items, isSame);
            // 配列の要素の移動を最小化するために後ろから削除する
            for (int i = ItemsSource.Count - 1; endIndex <= i; i--) {
                ItemsSource.RemoveAt(i);
            }
            selectedIndex = backupIndex;
            if (items.Count != prevItemsCount || notifyIndex) {
                PropertyChanged?.Invoke(this, SelectedIndexProperty);
            }
        }
    }
}
