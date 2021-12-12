using System.Collections.Generic;

#nullable enable

namespace TEA {

    /// <summary>
    ///  ディスパッチャが設定されるまでメッセージをためます。
    /// </summary>
    public class BufferDispatcher<T> : IDispatcher<T>, ISetup<T> {

        readonly int capacity;

        /// <summary>
        ///  メッセージのバッファ
        ///  不要にもかかわらずメモリに残り続けるのを防ぐためにnullにしています。
        /// </summary>
        List<T>? messages;

        IDispatcher<T>? dispatcher;

        /// <summary>
        ///  初期サイズを指定します。
        /// </summary>
        public BufferDispatcher(int size = 4) {
            capacity = size;
        }

        /// <summary>
        ///  引数で指定したディスパッチャに置き換えます。
        ///  溜まっているメッセージがあればディスパッチします。
        /// </summary>
        public void Setup(IDispatcher<T> dispatcher) {
            this.dispatcher = dispatcher;
            if (messages is null) {
                return;
            }
            foreach (var msg in messages) {
                dispatcher.Dispatch(msg);
            }
            messages = null;
        }

        public void Dispatch(T msg) {
            if (dispatcher is not null)
            {
                dispatcher.Dispatch(msg);
                return;
            }
            messages ??= new(capacity);
            messages.Add(msg);
        }

    }
}
