using System;

namespace TEA {

    public class MessageWrapper<TSource, TResult> : IDispatcher<TSource> {
        readonly IDispatcher<TResult> dispatcher;
        readonly Action<IDispatcher<TResult>, TSource> dispatch;

        public MessageWrapper(IDispatcher<TResult> dispatcher, Action<IDispatcher<TResult>, TSource> dispatch) {
            this.dispatcher = dispatcher;
            this.dispatch = dispatch;
        }

        public void Dispatch(TSource msg) {
            dispatch(dispatcher, msg);
        }

    }

    public static class MessageWrapper {
        public static IDispatcher<TSource> Wrap<TSource, TResult>(
            this IDispatcher<TResult> dispatcher,
            Func<TSource, TResult> selector) {
            return new MessageWrapper<TSource, TResult>(dispatcher, (d, msg) => d.Dispatch(selector(msg)));
        }

        /// <summary>
        ///  ディスパッチャを呼び出すアクションを設定します。
        /// </summary>
        public static void Setup<TSource, TResult>(this ISetup<TSource> target,
                                                   IDispatcher<TResult> dispatcher,
                                                   Action<IDispatcher<TResult>, TSource> dispatch) {
            target.Setup(new MessageWrapper<TSource, TResult>(dispatcher, dispatch));
        }

        /// <summary>
        ///  メッセージの変換とディスパッチを設定します。
        /// </summary>
        public static void Setup<TSource, TResult>(this ISetup<TSource> target,
                                                   IDispatcher<TResult> dispatcher,
                                                   Func<TSource, TResult> selector) {
            target.Setup(dispatcher.Wrap((TSource msg) => selector(msg)));
        }

        /// <summary>
        ///  メッセージの変換と同時にディスパッチするかを判定するラムダ式を設定します。
        ///  nullを返すとディスパッチしません。
        /// </summary>
        public static void SetupMaybe<TSource, TResult>(this ISetup<TSource> target,
                                                        IDispatcher<TResult> dispatcher,
                                                        Func<TSource, TResult?> selector)
        // where TResult : class
        {
                target.Setup(new MessageWrapper<TSource, TResult>(dispatcher, (d, msg) => {
                    var x = selector(msg);
                    if (x is not null) {
                        d.Dispatch(x);
                    }
                }));
        }

        // /// <summary>
        // ///  メッセージの変換と同時にディスパッチするかを判定するラムダ式を設定します。
        // ///  nullを返すとディスパッチしません。
        // ///  値型用
        // /// </summary>
        // public static void SetupMaybe<TSource, TResult>(this ISetup<TSource> target,
        //                                                 IDispatcher<TResult> dispatcher,
        //                                                 Func<TSource, TResult?> selector)
        //     where TResult : struct {
        //         target.Setup(new MessageWrapper<TSource, TResult>(dispatcher, (d, msg) => {
        //             var x = selector(msg);
        //             if (x is not null) {
        //                 d.Dispatch(x.Value);
        //             }
        //         }));
        // }
    }
}
