using System.Collections.Generic;
using System.Linq;
using TEA.Model;
using TEA.WPFTest.Message.SelectorTest;

namespace TEA.WPFTest.Model {

    public record ListItem(string Str, int IntValue);

    public record SelectorTestModel : IUpdate<SelectorTestModel, ISelectorTestMessage> {

        int ListBoxPattern { get; init; } = 0;

        public ComboBoxModel<ListItem> SelectionAndItems => ListBoxPattern switch {
            0 => new(ListBoxPattern, new ListItem[] { new("this is 0", 0) }),
            1 => new(ListBoxPattern, new ListItem[] { new("this is 1", 1), new("x2", 2) }),
            2 => new(ListBoxPattern, new ListItem[] { new("this is 2", 2), new("x2_2", 2), new("x3", 3) }),
            3 => new(ListBoxPattern, new ListItem[] { new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100) }),
            4 => new(ListBoxPattern, new ListItem[] { new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100) }),
            5 => new(2             , new ListItem[] { new("other", 10), new("x2_3", 2), new("x3_2", 3), new("x4", 4) })
        };

        public ComboBoxModel<string> ForComboBox {
            get {
                var item = SelectionAndItems;
                return new(item.SelectedIndex, SelectionAndItems.Items.Select(x => x.Str).ToArray());
            }
        }

        public SelectorTestModel Update(ISelectorTestMessage message) {
            return message switch {
                OnChangedListBoxSelectedIndex msg => msg.SelectedIndex switch {
                    1 => this with { ListBoxPattern = 2 },
                    2 => this with { ListBoxPattern = 3 },
                    3 => this with { ListBoxPattern = 3 },
                    4 => this with { ListBoxPattern = 4 },
                    5 => this with { ListBoxPattern = 5 },
                    _ => this with { ListBoxPattern = 0 }
                },
                OnClickChangeTo3Button msg => this with { ListBoxPattern = 3 },
                _ => this
            };
        }
    }
}
