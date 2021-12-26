using TEA.WPFTest.Message.MainWindowMessage;

namespace TEA.WPFTest.Model {
    public record MainWindowModel(
        RadioButtonTestModel RadioButton,
        SelectorTestModel SelectorTest,
        CollectionRenderTestModel CollectionRenderTest) : IUpdate<MainWindowModel, IMainWindowMessage> {
        public MainWindowModel Update(IMainWindowMessage message) {
            return message switch {
                InRadioButtonMessage msg => this with { RadioButton = RadioButton.Update(msg.Message) },
                InSelectorTestMessage msg => this with { SelectorTest = SelectorTest.Update(msg.Message) },
                InCollectionRenderTestMessage msg => this with { CollectionRenderTest = CollectionRenderTest.Update(msg.Message) }
            };
        }
    }
}
