#nullable enable

namespace TEA {

    /// <summary>
    ///  ディスパッチ先と現在の状態の取得用のインターフェース
    /// </summary>
    public interface ITEA<State, Message> : IDispatcher<Message> {
        State Current { get; }
    }
}
