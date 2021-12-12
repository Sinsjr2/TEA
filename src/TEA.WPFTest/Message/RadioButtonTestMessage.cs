using System;

namespace TEA.WPFTest.Message {

    public interface IRadioButtonTestMessage { }

    /// <summary>
    /// �I�����Ă��郉�W�I�{�^��
    /// </summary>
    public enum RadioButtonChecked {
        Btn1,
        Btn2,
        Btn3
    }

    [Serializable]
    public record OnChangedRadioButtonType1(RadioButtonChecked? Selected) : IRadioButtonTestMessage;


}