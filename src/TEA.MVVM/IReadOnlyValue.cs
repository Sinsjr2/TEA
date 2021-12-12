namespace TEA.MVVM {

    /// <summary>
    ///  値の取得を行うためのインターフェース
    /// </summary>
    interface IReadOnlyValue<T> {
        T Value { get; }
    }
}
