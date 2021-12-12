namespace TEA {
    public interface IDispatcher<Message> {
        void Dispatch(Message msg);
    }
}
