using System;
using System.Collections.Generic;
using TEA.WPFTest.Message.CollectionRenderTestMessage;

namespace TEA.WPFTest.Model {

    public record CollectionItem(string Str1, int Integer);
    public record CollectionRenderTestModel : IUpdate<CollectionRenderTestModel, ICollectionRenderTestMessage> {

        int ListBoxPattern { get; init; } = 0;

        public IEnumerable<CollectionItem> List1Items => ListBoxPattern switch {
            0 => new CollectionItem[] { new("this is 0", 0), new("dummy", 777) },
            1 => new CollectionItem[] { new("this is 1", 1), new("x2", 2) },
            2 => new CollectionItem[] { new("this is 2", 2), new("x2_2", 2), new("x3", 3) },
            3 => new CollectionItem[] { new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100) },
            4 => new CollectionItem[] { new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100), new("sample", 100) },
            5 => new CollectionItem[] { new("other", 10), new("x2_3", 2), new("x3_2", 3), new("x4", 4) },
            _ => throw new ArgumentOutOfRangeException(nameof(List1Items), List1Items, null)
        };

        public CollectionRenderTestModel Update(ICollectionRenderTestMessage message) {
            return message switch {
                OnClickedItem msg => msg.Index switch {
                    1 => this with { ListBoxPattern = 2 },
                    2 => this with { ListBoxPattern = 3 },
                    3 => this with { ListBoxPattern = 3 },
                    4 => this with { ListBoxPattern = 4 },
                    5 => this with { ListBoxPattern = 5 },
                    _ => this with { ListBoxPattern = 0 }
                },
                _ => throw new ArgumentOutOfRangeException(nameof(message), message, null)
            };
        }
    }
}
