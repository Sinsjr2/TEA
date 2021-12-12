namespace TEA {
    public interface IRender<State> {

        /// <summary>
        ///   stateの状態に従い描画を行います。
        ///   ディスパッチするとこのメソッドを呼び出します。
        /// </summary>
        void Render(State state);
    }
}
