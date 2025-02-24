using System.ComponentModel;
using TEA.MVVM;
using TEA.WPFTest.Message.CollectionRenderTestMessage;
using TEA.WPFTest.Model;

namespace TEA.WPFTest.ViewModel {

    public class CollectionItemViewModel : ITEAComponent<CollectionItem, CollectionItem>, INotifyPropertyChanged {
        public WriteNotify<CollectionItem> CurrentState { get; } = new(new("", 0));
        public CommandRender Button { get; } = new(true);

        public event PropertyChangedEventHandler? PropertyChanged { add {} remove {} }

        public void Setup(IDispatcher<CollectionItem> dispatcher) {
            Button.Setup(dispatcher, _ => CurrentState.Value);
        }

        public void Render(CollectionItem state) {
            CurrentState.Render(state);
        }
    }

    public class CollectionRenderTestViewModel : ITEAComponent<CollectionRenderTestModel, ICollectionRenderTestMessage>, INotifyPropertyChanged {

        public CollectionRender<CollectionItemViewModel, CollectionItem, CollectionItem> List1 { get; } = new(() => new CollectionItemViewModel());

        public event PropertyChangedEventHandler? PropertyChanged { add {} remove {} }

        public void Setup(IDispatcher<ICollectionRenderTestMessage> dispatcher) {
            List1.Setup(dispatcher, pair => new OnClickedItem(pair.Key, pair.Value));
        }

        public void Render(CollectionRenderTestModel state) {
            List1.Render(state.List1Items);
        }
    }
}
