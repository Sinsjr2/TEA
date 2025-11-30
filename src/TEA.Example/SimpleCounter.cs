using TEA;

namespace TEA.Example.SimpleCounter;

public interface ICounterMessage;

public record IncrementMessage : ICounterMessage;
public record DecrementMessage : ICounterMessage;
public record ResetMessage : ICounterMessage;

public record CounterModel(int Count) : IUpdate<CounterModel, ICounterMessage> {

    public bool IsEven => (Count & 1) == 0;

    public string EvenOddString => IsEven ? "even" : "odd";

    public CounterModel Update(ICounterMessage msg) {
        return msg switch {
            IncrementMessage => this with { Count = Count + 1 },
            DecrementMessage => this with { Count = Count - 1 },
            ResetMessage => this with { Count = 0 },
            _ => this,
        };
    }
}

public class CounterView : IRender<CounterModel> {
    public void Render(CounterModel model) {
        Console.WriteLine($"count: {model.Count} {model.EvenOddString}");
    }
}

public class Program {
    public static void EntryPoint() {
        Console.WriteLine($"SampleCode: {nameof(SimpleCounter)}");
        var tea = new TEA<CounterModel, ICounterMessage>(
            new CounterModel(0),
            new CounterView()
        );

        Console.WriteLine("Dispatch(Increment)");
        tea.Dispatch(Singleton<IncrementMessage>.Instance);
        Console.WriteLine("Dispatch(Decrement)");
        tea.Dispatch(Singleton<DecrementMessage>.Instance);
        Console.WriteLine("Dispatch(Decrement)");
        tea.Dispatch(Singleton<DecrementMessage>.Instance);
        Console.WriteLine("Dispatch(Reset)");
        tea.Dispatch(Singleton<ResetMessage>.Instance);
    }
}
