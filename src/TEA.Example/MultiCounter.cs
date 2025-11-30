using System.Text;
using TEA.Example.SimpleCounter;

namespace TEA.Example.MultiCounter;

public interface IMultiCounterMessage;

/// <summary>
///  指定した配列のインデックスの後ろにカウンターを追加します。
///  カウンターの名前とカウンターの初期値も指定します。
/// </summary>
public record AddCounterAfterIndexMessage(int Index, string CounterName, int InitialCount) : IMultiCounterMessage;

/// <summary>
/// 指定したインデックスのカウンターを削除します。
/// </summary>
public record RemoveCounterMessage(int Index) : IMultiCounterMessage;

/// <summary>
/// 指定したインデックスのカウンターを更新します。
/// </summary>
public record CounterMessageOnMultiCounter(int Index, ICounterMessage Message) : IMultiCounterMessage;

public record CounterPair(string CounterName, CounterModel Counter);

public record MultiCounterModel(IReadOnlyList<CounterPair> Counters)
    : IUpdate<MultiCounterModel, IMultiCounterMessage> {

    /// <summary>
    /// 最小のカウンターを返します。
    /// </summary>
    public (int index, CounterPair pair)? MinCounter =>
        Counters.Count <= 0
        ? null
        : Counters.Index().MinBy(x => x.Item.Counter.Count);

    /// <summary>
    /// 最大のカウンターを返します。
    /// </summary>
    public (int index, CounterPair pair)? MaxCounter =>
        Counters.Count <= 0
        ? null
        : Counters.Index().MaxBy(x => x.Item.Counter.Count);

    public MultiCounterModel Update(IMultiCounterMessage message) {
        return message switch {
            AddCounterAfterIndexMessage msg => AddCounterAfterIndex(msg),
            RemoveCounterMessage msg => RemoveCounterMessage(msg),
            CounterMessageOnMultiCounter msg => UpdateCounter(msg),
            _ => this
        };
    }

    MultiCounterModel AddCounterAfterIndex(AddCounterAfterIndexMessage message) {
        return this with {
            Counters = [
                .. Counters.Take(message.Index),
                new CounterPair(message.CounterName, new CounterModel(message.InitialCount)),
                .. Counters.Skip(message.Index)
            ]
        };
    }

    MultiCounterModel RemoveCounterMessage(RemoveCounterMessage message) {
        return this with {
            Counters = [
                .. Counters.Take(message.Index - 1),
                .. Counters.Skip(message.Index + 1)]
        };
    }

    MultiCounterModel UpdateCounter(CounterMessageOnMultiCounter msg) {
        if (Counters.Count <= msg.Index) {
            return this;
        }
        var newCounter = Counters[msg.Index];
        return this with {
            Counters = [
                .. Counters.Take(msg.Index),
                newCounter with { Counter = newCounter.Counter.Update(msg.Message)},
                .. Counters.Skip(msg.Index)
            ]
        };
    }
}

public class MultiCounterView : IRender<MultiCounterModel> {

    static string CounterString((int index, CounterPair pair)? counter) {
        return counter == null
            ? "null"
            : $"[{counter.Value.index}] name: {counter.Value.pair.CounterName}";
    }

    public void Render(MultiCounterModel state) {
        var sb = new StringBuilder();
        int i = -1;
        sb.AppendLine("----");
        foreach (var pair in state.Counters) {
            i++;
            sb.AppendLine($"[{i}] name: {pair.CounterName}, count: {pair.Counter.Count} {pair.Counter.EvenOddString}");
        }
        sb.AppendLine($"min: {CounterString(state.MinCounter)}, max: {CounterString(state.MaxCounter)}");
        sb.AppendLine("----");
        Console.Write(sb.ToString());
    }
}

public class Program {
    public static void EntryPoint() {
        Console.WriteLine($"SampleCode: {nameof(MultiCounterModel)}");
        var tea = new TEA<MultiCounterModel, IMultiCounterMessage>(
            new MultiCounterModel([]),
            new MultiCounterView()
        );

        Console.WriteLine("Dispatch(AddCounterAfterIndex(0, A, 1))");
        tea.Dispatch(new AddCounterAfterIndexMessage(0, "A", 1));

        Console.WriteLine("Dispatch(AddCounterAfterIndex(1, B, 2))");
        tea.Dispatch(new AddCounterAfterIndexMessage(1, "B", 2));

        Console.WriteLine("Dispatch(IncrementCounter(0))");
        tea.Dispatch(new CounterMessageOnMultiCounter(0, Singleton<IncrementMessage>.Instance));

        Console.WriteLine("Dispatch(IncrementCounter(1))");
        tea.Dispatch(new CounterMessageOnMultiCounter(1, Singleton<IncrementMessage>.Instance));

        Console.WriteLine("Dispatch(DecrementCounter(0))");
        tea.Dispatch(new CounterMessageOnMultiCounter(0, Singleton<DecrementMessage>.Instance));

        Console.WriteLine("Dispatch(RemoveCounter(1))");
        tea.Dispatch(new RemoveCounterMessage(1));
    }
}