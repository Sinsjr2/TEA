namespace TEA {

    /// <summary>
    ///  レンダーとディスパッチャを同時に使用して、
    ///  クラスを作成する際に使用します。
    /// </summary>
    public interface ITEAComponent<State, Message> : IRender<State>, ISetup<Message> {}
}
