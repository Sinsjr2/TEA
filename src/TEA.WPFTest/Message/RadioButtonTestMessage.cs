using System;

namespace TEA.WPFTest.Message {

    public interface IRadioButtonTestMessage { }

    /// <summary>
    /// 選択しているラジオボタン
    /// </summary>
    public enum RadioButtonChecked {
        Btn1,
        Btn2,
        Btn3
    }

    [Serializable]
    public record OnChangedRadioButtonType1(RadioButtonChecked? Selected) : IRadioButtonTestMessage;


}