using System;
using System.Collections.Generic;

namespace TEA {

    public static class RenderFactory {
        /// <summary>
        ///  キャッシュに対して、あれば描画し、そうでなければ
        ///  新しくオブジェクトを作ります。
        ///  戻り値で処理したキャッシュのインデックスの次のインデックスを返します。
        /// </summary>
        public static int ApplyToRender<TRender, TState, TMessage>(
            this IList<TRender> cachedRender,
            IDispatcher<KeyValuePair<int, TMessage>> dispatcher,
            Func<IDispatcher<TMessage>, TRender> createRender,
            IEnumerable<TState> state) where TRender : IRender<TState> {
            using var e = state.GetEnumerator();
            int i = 0;
            // キャッシュからrenderを呼び出す
            foreach (var r in cachedRender) {
                if (!e.MoveNext()) {
                    return i;
                }
                // ステートがあるうちはrenderに渡す
                r.Render(e.Current);
                i++;
            }
            // 足りない分をインスタンス化する
            for (; e.MoveNext(); i++) {
                var index = i;
                var render = createRender(dispatcher.Wrap((TMessage msg) => new KeyValuePair<int, TMessage>(index, msg)));
                cachedRender.Add(render);
                render.Render(e.Current);
            }
            return i;
        }

        public static int ApplyToRender<TRender, TState, TMessage>(
            this IList<TRender> cachedRender,
            IDispatcher<KeyValuePair<int, TMessage>> dispatcher,
            Func<TRender> createRender,
            IEnumerable<TState> state) where TRender : ITEAComponent<TState, TMessage> {
            return cachedRender.ApplyToRender(dispatcher, d => {
                var render = createRender();
                render.Setup(d);
                return render;
            },
            state);
        }

        public static int ApplyToList<T>(this IList<T> dest,
                                         IEnumerable<T> source,
                                         IEqualityComparer<T>? comparer = null) {
            return dest.ApplyToList(
                source,
                comparer is null
                ? (a, b) => EqualityComparer<T>.Default.Equals(a, b)
                : (a, b) => comparer.Equals(a, b));
        }

        /// <summary>
        ///  値が異なっていれば書き込み、そうでなければ何もしません。
        ///  要素数が足りない場合は追加します。
        ///  処理が完了した次のインデックスを返します。
        ///  比較器を指定しなければデフォルトの比較器を使用します。
        /// </summary>
        public static int ApplyToList<T>(this IList<T> dest,
                                         IEnumerable<T> source,
                                         Func<T, T, bool> isSame) {
            using var e = source.GetEnumerator();
            int i = 0;
            // 代入できるスペースがある場合は異なっていれば書き換える
            for (; e.MoveNext(); i++) {
                if (dest.Count <= i) {
                    dest.Add(e.Current);
                    continue;
                }
                var x = e.Current;
                if (!isSame(dest[i], x)) {
                    dest[i] = x;
                }
            }
            return i;
        }
    }
}
