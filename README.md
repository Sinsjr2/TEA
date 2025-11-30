# 1. はじめに

## 概要
TEA(The Elm Architecture)は **Redux/ElmアーキテクチャをC#で実現**するためのライブラリです。    
従来のUI開発では、複数クラス(UIコンポーネント)間で状態を連動させる場合、実装が煩雑になりがちです。  
TEAは **単方向データフロー**を採用し、状態の更新を一元化することで以下のメリットを提供します:  

- 状態管理の簡素化  
- 複数クラス間でのデータ共有が容易  
- テストがしやすい（ModelクラスのUpdateメソッドを単体テスト可能）

## 導入方法
NuGet にて以下のパッケージを公開しています。

### [TEA.Base](https://www.nuget.org/packages/TEA.Base)

TEA のコア部分です。
以下のようにメッセージの通知から状態の更新、状態の反映を行います。
1. Dispatch
2. Update
3. Render

### [TEA.MVVM](https://www.nuget.org/packages/TEA.MVVM)
WPFやAvaloniaなどのUIフレームワークで利用しやすいようにMVVMパターンを補助を行うための機能を提供します。
例えば、ViewModelとバインディングを組み合わせてUI更新を簡潔に記述可能にします。

## TEAのフロー

### 概略
下記図に示すように、すべての動作の起点は、[IDispatcher](src/TEA/IDispatcher.cs)インターフェースのDispatchメソッドです。  
[TEA](src/TEA/TEA.cs)は、そのインターフェースを実装しているので、最終的にTEAクラスのDispatchメソッドが実行されます。
なぜなら、IDispatcherインターフェースはデザインパターンのDecoratorやC#のLinqのに入れ子にできるためです。  
その後、TEAクラスの中で、[IUpdate](src/TEA/IUpdate.cs)のUpdateメソッドにより、メッセージから次の「状態」(Model)を作ります。  
最後に、[IRender](src/TEA/IRender.cs)のRenderに計算した「状態」を渡し、描画をします。  
以上が大まかな処理の流れです。  

``` mermaid
sequenceDiagram
    actor User
    participant TEA
    participant Model
    participant IRender

    User ->> TEA: Dispatch(Message)
    TEA ->> Model: Update(Message)
    Model -->> TEA: new Model
    TEA ->> IRender: Render(Model)
    IRender -->> TEA:
    TEA -->> User:
```

## TEAが解決する問題

### Modelのフィールド変更時の逐次描画
とりわけ、UIの実装においては、複数のボタンから一つの値を操作し、その結果を反映することがあります。
たとえば、下記のようなシンプルなカウンターアプリを考えます。

- ボタン
  - 増加ボタン: カウンターを1増加させます。
  - 減少ボタン: カウンターを1減少させます。
  - リセットボタン: カウンターを0にします。

- 表示
  - カウンター: カウント値を10進数で表示します。
  - 奇数・偶数: カウンターが奇数か偶数を「odd」「even」と表示します。

この場合、「表示」の「奇数・偶数」は、「カウンター」の値に依存して計算されます。  
ModelクラスのUpdateメソッド内で、「カウンターの更新処理」と一緒に「奇数・偶数を文字列に変換」をしてもいいですが、「カウンター」を更新するボタンを増やすごとに、全てのボタン処理に追加するのは面倒で修正が漏れる可能性もあります。  
TEAでは、「Dispatch」メソッドで「メッセージ」を送るといずれ、「Render」メソッドが呼ばれます。  
そのため、「Render」メソッド内で、「奇数・偶数を文字列に変換」する処理を呼ぶと「カウンター」に依存して更新が可能になります。  

[サンプルコード](#サンプルコード)に例があります。

### クラス間のデータ共有
状態を一元管理するため、クラス間でのデータ同期が容易になります。  
UIアプリケーションにおいては、細かく分けたUIコンポーネント間のデータのやり取りを用意にする効果があります。

### UIリスト表示とコールバック設定
ボタン付きリストなどのUI要素を生成し、各要素に対してメッセージをDispatchする仕組みを簡潔に記述可能です。

## サンプルコード
### シンプルカウンター
#### コンソール版
[SimpleCunter](src/TEA.Example/SimpleCounter.cs)  
`Program.Entry()`からプログラムが開始します。  
[TEAが解決する問題](#TEAが解決する問題)の[Modelのフィールド変更時の逐次描画](#Modelのフィールド変更時の逐次描画)について実装しています。

### 複数カウンター
[TEAが解決する問題](#TEAが解決する問題)の[クラス間のデータ共有](#クラス間のデータ共有)について実装しています。
「シンプルカウンター」を複数管理します。  
カウンターの追加、削除を行い、カウンターが更新されるごとに、最小値と最大値を更新します。
「シンプルカウンター」で作成した「モデル」複数配置し、そのモデルへのメッセージの送り方を説明するサンプルコードとなります。

#### コンソール版
[MultiCunter](src/TEA.Example/MultiCounter.cs)  
`Program.Entry()`からプログラムが開始します。  

本来は、「SimpleCounterView」を使用してRenderするところですが、
コンソール版は、Console.WriteLineするだけであり、SimpleCounterViewであると余計な位置で改行されてわかりにくいため、MultiCounterView内で文字列を組み立てています。

## TEAの推奨事項
- IUpdateのUpdateメソッドが返すオブジェクト(Model)はイミュータブルにする。  
  ミュータブルでも問題なく動作はしますが、Render処理中のDispatchよびだしへの対応やLinqの処理を最大限活用、コードの複雑度低減などから、戻り値はイミュータブルにすることをおすすめします。
- IUpdateの処理は、副作用なし(とりわけIO処理は書かない)
  Update内の処理はファイルアクセスや外部へのアクセスなどを避け、Updateに同じメッセージを渡す限り同じ値を返すいわゆる「純粋な関数」としてください。
  もし、外部へのアクセスが必要な場合は、Render内やDispatchが呼ばれたり、Dispatchを呼ぶタイミングで行ってください。
- (可能であれば、)Render中にDispatchを呼ばない
  TEAでは、Render中にDispatchしても、再度Renderを呼ばないように、メッセージをキャッシュし、Renderが終了したタイミングでためたメッセージで状態を更新し、再度Renderする機構があります。
  これは、Renderした結果、値が変化し、その変更通知が発生する可能性があるためです。  
  よって、避けられる場合は、避けるのが望ましいです。

## Q&A
- 新しいクラスを追加方法  
  以下に示すのは一例であり、プロジェクトに応じて適宜修正してください。  
  以下のものを追加する  
    - 描画先クラスの制御を行う IUpddate を実装した 〇〇Modelクラスの追加
    - 描画部分 以下のいずれか
      - ITEAComponent を実装した〇〇ViewModel (MVVM で binding を使用する場合) 
      - IRender を実装した 〇〇View (C# markup を使用する場合)クラスを追加
    - Dispatch 時に使用する メッセージの種類を以下のいずれかの方法で作成
      - enum
      - interface を使用した Union
        - 画面が入れ子になる場合は、入れ子の画面からのメッセージをラップする必要があるのでおすすめ
      - 単なる class
      - 単なる struct(ex: int, string, ユーザー定義のstruct)

- Dispatchするメッセージが class かつパラメーターを持たず、gcを抑えたい場合の場合  
  それぞれの クラス でフィールドを定義するのは面倒であるので Singleton<>.Instance を使うと良い。
- TEAで自動テストを行うときは?  
  ModelのUpdateのみを単体テストする。(pureなクラスになるのでUpdateとプロパティのみをすれば良いので簡単)  
  必要であれば、UI込みで自動テストを行うことで、UIとModelをつなぐ処理に抜けがないことが確認できる。

