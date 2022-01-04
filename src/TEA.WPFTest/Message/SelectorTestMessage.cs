namespace TEA.WPFTest.Message.SelectorTest {
    public interface ISelectorTestMessage { }

    public record OnChangedListBoxSelectedIndex(int SelectedIndex) : ISelectorTestMessage;
    public record OnClickChangeTo3Button : ISelectorTestMessage;
}
