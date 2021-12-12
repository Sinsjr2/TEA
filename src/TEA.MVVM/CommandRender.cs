using System;
using System.ComponentModel;
using System.Windows.Input;

namespace TEA.MVVM {

    /// <summary>
    ///  コマンドの実行で指定された引数のディスパッチと
    ///  コマンドを実行できるかをレンダリングします。
    /// </summary>
    public class CommandRender<T> : INotifyPropertyChanged, ICommand, ITEAComponent<bool, T?> {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CanExecuteChanged;

        IDispatcher<T?>? dispatcher;
        bool canExecute;

        public CommandRender(bool initialCanExecute) {
            canExecute = initialCanExecute;
        }

        public void Setup(IDispatcher<T?> dispatcher) {
            this.dispatcher = dispatcher;
        }

        public void Render(bool state) {
            if (canExecute == state) {
                return;
            }
            canExecute = state;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter) {
            return canExecute;
        }

        public void Execute(object? parameter) {
            var param = parameter is null
                ? default
                : (T)parameter;
            dispatcher?.Dispatch(param);
        }
    }

    /// <summary>
    ///  ボタンのコマンドへのバインディングなど
    ///  ディスパッチ時のインスタンスを必要としない場合に使用します。
    /// </summary>
    public class CommandRender : CommandRender<object> {
        public CommandRender(bool initialCanExecute) : base(initialCanExecute) { }
    }
}
