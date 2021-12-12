using System.Windows;
using TEA.WPFTest.Message;
using TEA.WPFTest.Model;
using TEA.WPFTest.ViewModel;

namespace TEA.WPFTest {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var window = new MainWindow();
            var dispatcher = new BufferDispatcher<IRadioButtonTestMessage>();
            var vm = new RadioButtonTestViewModel();
            vm.Setup(dispatcher);
            var initial = new RadioButtonTestModel(Message.RadioButtonChecked.Btn1);
            var tea = new TEA<RadioButtonTestModel, IRadioButtonTestMessage>(initial, vm);
            dispatcher.Setup(tea);
            window.DataContext = vm;
            window.Show();
        }
    }
}
