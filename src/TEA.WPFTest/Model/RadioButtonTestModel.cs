using TEA.WPFTest.Message;

namespace TEA.WPFTest.Model {

    public record RadioButtonTestModel(RadioButtonChecked? BtnType1Selected) : IUpdate<RadioButtonTestModel, IRadioButtonTestMessage> {
        public RadioButtonTestModel Update(IRadioButtonTestMessage message) {
            return message switch {
                OnChangedRadioButtonType1 msg => msg.Selected switch {
                    RadioButtonChecked.Btn1 => this with { BtnType1Selected = RadioButtonChecked.Btn1 },
                    RadioButtonChecked.Btn2 => this with { BtnType1Selected = RadioButtonChecked.Btn1 },
                    RadioButtonChecked.Btn3 => this with { BtnType1Selected = null }
                },
                _ => this
            };
        }
    }
}
