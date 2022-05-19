# What's this?
"TEA" is C# library, inspilered by [Redux](https://github.com/reduxjs/redux) and [The Elm Archtecture](https://guide.elm-lang.org/architecture/).
TEA consists of Three elements (Message Update Render), and Manage state.

# Three elements
# Example (with Unity)
https://github.com/Sinsjr2/BallFall

## Initialize TEA instance

``` c#
GameSceneRender gameSceneRender;

void Start() {
    // "gameScenRender" requires tea instance to call "Setup".
    // Also, "tea" requires "gemeSceneRender" to call constructor.
    // so, I use BufferDispatcher.
    var buffer = new BufferDispatcher<IGameSceneMessage>();
    gameSceneRender.Setup(buffer);
    var inputSubscription = new GameObject(nameof(InputSubscription))
        .AddComponent<InputSubscription>();
    var tea = new TEA<GameSceneState, IGameSceneMessage>(
        // initial state
        // if you use "inspector" in unity for set initial state,
        // you get initial state from render.
        // Also, you don't want to use inspector,
        // you can set initial value with C# expression.
        gameSceneRender.CreateState(),
        gameSceneRender);
    buffer.SetDispatcher(tea);
}
```

## Message
"Message" is called "Action" in Redux.
The role of "Message" is to notify from View to Model.
``` c#
public interface IGameSceneMessage { }

public enum BarPosition {
    Left,
    Center,
    Right,
}

public record InputState(BarPosition BarPosition,
                         bool PushedEnterButton,
                         bool PushedEscapeButton);

public record WrapBallMessage(IBallMessage Message): IGameSceneMessage;
public InitGame: IGameSceneMessage;
public OnInput(InputState State): IGameSceneMessage;
public record OnChangedCanvasSize(Vector2 CanvasSize) : IGameSceneMessage;

public interface IBallMessage: 
public record OnCollisionBar: IBallMessage;
public record OnOutOfArea: IballMessage;
```

## State (== Model)
"Update" is recommended to make it pure function.
``` c#
public record GameSceneState : IUpdate<GameSceneState, IGameSceneMessage> {
    public GameSceneState Update(IGameSceneMessage msg) {
        ...
    }
}
```

## Render (== View)
View 
``` c#
public class BallRender: MonoBehaviour, IRender<BallState> { ... }
public class BarRender: MonoBehaviour, ITEAComponent<BarState, IBarMessage> { ... }
public class GameSceneRender: MonoBehaviour, ITEAComponent<GameSceneState, IGameSceneMessage> {
    [SerializeField]
    MonoBehaviourRenderFactory<BallRender, BallState, IBallMessage> ballRender;

    public void Setup(IDispatcher<IGameSceneMessage> dispatcher) {
        ...
        ballRender.Setup(dispatcher, indexAndMsg => new WrapBallMessage { id = indexAndMsg.Key, message = indexAndMsg.Value });
        ...
    }
    
    public void 
}
```

# Smilar libraries
- [Unidux](https://github.com/mattak/Unidux)
- [UniTEA](https://github.com/uzimaru0000/UniTEA)
- [Redux.NET](https://github.com/GuillaumeSalles/redux.NET)
