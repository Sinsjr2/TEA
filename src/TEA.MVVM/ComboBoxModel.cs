using System;
using System.Collections.Generic;

namespace TEA.Model {

    /// <summary>
    ///  コンボボックスに表示する内容
    /// </summary>
    [Serializable]
    public record ComboBoxModel<T>(int SelectedIndex, IReadOnlyList<T> Items);
}
