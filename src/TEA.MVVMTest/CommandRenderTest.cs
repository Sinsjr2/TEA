using System;
using System.Collections.Generic;
using NUnit.Framework;
using TEA.MVVM;
using TestTools;

namespace TEA.MVVMTest {

    public class CommandRenderTest {

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void InitializeTest(bool canExecute) {
            var command = new CommandRender<string>(canExecute);
            command.CanExecute("").Is(canExecute);
        }

        [Test]
        public void CallExecuteTest() {
            void Check<T>(T? obj) {
                var command = new CommandRender<T>(true);
                var msgs = new List<T?>();
                command.Setup(new BufferDispatcher<T>(), (_, msg) => msgs.Add(msg));
                msgs.ToArray().Is(Array.Empty<T>());
                command.Execute(obj);
                msgs.ToArray().Is(new[] { obj });
            }
            Check<string>(null);
            Check("aaa");
            Check(8);
            Check<int?>(null);
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void RenderTest(bool initial, bool canExecute) {
            var command = new CommandRender(initial);

            var actualNotify = new List<bool>();
            command.CanExecuteChanged += (sender, _) => actualNotify.Add((sender as CommandRender)!.CanExecute(null));
            command.Render(canExecute);
            actualNotify.ToArray().Is(
                initial == canExecute
                ? Array.Empty<bool>()
                : new[] { canExecute }
            );
            command.CanExecute(null).Is(canExecute);
        }
    }
}
