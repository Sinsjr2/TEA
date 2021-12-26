namespace TEA {
    public interface IUpdate<TState, TMessage> {
        /// <summary>
        ///  メッセージにより新しい状態に更新し返します。
        /// </summary>
        TState Update(TMessage message);
    }
}
