using System.ComponentModel;
using TEA.MVVM;
using TEA.WPFTest.Message.SelectorTest;
using TEA.WPFTest.Model;

namespace TEA.WPFTest.ViewModel {
    public class SelectorTestViewModel : ITEAComponent<SelectorTestModel, ISelectorTestMessage>, INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        public CommandRender Select3Btn { get; } = new(true);
        public ComboBoxRender<ListItem> ListItems { get; } = new();
        public ComboBoxRender<string> ComboBoxItems { get; } = new();

        public void Setup(IDispatcher<ISelectorTestMessage> dispatcher) {
            Select3Btn.Setup(dispatcher, _ => Singleton<OnClickChangeTo3Button>.Instance);
            ListItems.Setup(dispatcher, index => new OnChangedListBoxSelectedIndex(index));
            ComboBoxItems.Setup(dispatcher, index => new OnChangedListBoxSelectedIndex(index));
        }
 
        public void Render(SelectorTestModel state) {
            ListItems.Render(state.SelectionAndItems);
            ComboBoxItems.Render(state.ForComboBox);
        }
    }
}
