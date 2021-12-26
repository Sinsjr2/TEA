using System.Windows;
using TEA.WPFTest.Message;
using TEA.WPFTest.Message.MainWindowMessage;
using TEA.WPFTest.Model;
using TEA.WPFTest.View;
using TEA.WPFTest.ViewModel;

namespace TEA.WPFTest {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var window = new MainWindow();
            var dispatcher = new BufferDispatcher<IMainWindowMessage>();
            var vm = new MainWindowViewModel();
            vm.Setup(dispatcher);
            var initial = new MainWindowModel(
                new RadioButtonTestModel(RadioButtonChecked.Btn1),
                new SelectorTestModel(),
                new CollectionRenderTestModel());
            var tea = new TEA<MainWindowModel, IMainWindowMessage>(initial, vm);
            dispatcher.Setup(tea);
            window.DataContext = vm;
            window.Show();
        }
    }
}
