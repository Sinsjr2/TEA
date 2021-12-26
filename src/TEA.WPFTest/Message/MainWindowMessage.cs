using TEA.WPFTest.Message.SelectorTest;

namespace TEA.WPFTest.Message.MainWindowMessage {
    public interface IMainWindowMessage { }

    public record InRadioButtonMessage(IRadioButtonTestMessage Message) : IMainWindowMessage;
    public record InSelectorTestMessage(ISelectorTestMessage Message) : IMainWindowMessage;
    public record InCollectionRenderTestMessage(CollectionRenderTestMessage.ICollectionRenderTestMessage Message) : IMainWindowMessage;
}
