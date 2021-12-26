using System.ComponentModel;
using TEA.WPFTest.Message.MainWindowMessage;
using TEA.WPFTest.Model;

namespace TEA.WPFTest.ViewModel {
    public class MainWindowViewModel : ITEAComponent<MainWindowModel, IMainWindowMessage>, INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        public RadioButtonTestViewModel RadioButtonTest { get; } = new();
        public SelectorTestViewModel SelectorTest { get; } = new();
        public CollectionRenderTestViewModel CollectionRenderTest { get; } = new();

        public void Setup(IDispatcher<IMainWindowMessage> dispatcher) {
            RadioButtonTest.Setup(dispatcher, msg => new InRadioButtonMessage(msg));
            SelectorTest.Setup(dispatcher, msg => new InSelectorTestMessage(msg));
            CollectionRenderTest.Setup(dispatcher, msg => new InCollectionRenderTestMessage(msg));
        }

        public void Render(MainWindowModel state) {
            RadioButtonTest.Render(state.RadioButton);
            SelectorTest.Render(state.SelectorTest);
            CollectionRenderTest.Render(state.CollectionRenderTest);
        }
    }
}
