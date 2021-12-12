namespace TEA {
    public interface IUpdate<State, Message> {
        /// <summary>
        ///  メッセージにより新しい状態に更新し返します。
        /// </summary>
        State Update(Message msg);
    }
}
