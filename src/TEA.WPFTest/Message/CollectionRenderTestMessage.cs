using TEA.WPFTest.Model;

namespace TEA.WPFTest.Message.CollectionRenderTestMessage {
    public interface ICollectionRenderTestMessage { }

    public record OnClickedItem(int Index, CollectionItem Item) : ICollectionRenderTestMessage;
}
