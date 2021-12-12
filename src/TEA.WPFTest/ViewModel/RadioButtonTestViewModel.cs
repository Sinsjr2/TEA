using System.ComponentModel;
using TEA.MVVM;
using TEA.WPFTest.Message;
using TEA.WPFTest.Model;

namespace TEA.WPFTest.ViewModel {

    public class RadioButtonTestViewModel : ITEAComponent<RadioButtonTestModel, IRadioButtonTestMessage>, INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        public OneSelectionRender<RadioButtonChecked?> RadioBtnType1 { get; } = new(null);

        public void Setup(IDispatcher<IRadioButtonTestMessage> dispatcher) {
            RadioBtnType1.Setup(dispatcher, selected => new OnChangedRadioButtonType1(selected));
        }

        public void Render(RadioButtonTestModel state) {
            RadioBtnType1.Render(state.BtnType1Selected);
        }
    }
}
