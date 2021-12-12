namespace TEA {

    /// <summary>
    ///  ディスパッチされた内容を捨てます。
    /// </summary>
    public class NullDispatcher<T> : IDispatcher<T> {
        public static readonly IDispatcher<T> Instance = new NullDispatcher<T>();
        public void Dispatch(T msg) { }
    }
}
