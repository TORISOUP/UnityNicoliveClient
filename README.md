# UnityNicoliveClient

UnityNicoliveClientはニコニコ生放送の新配信番組をUnityから操作するクライアントです。  


# 導入方法

## R3版（v1.0.0~）

UPMから導入してください。

```
https://github.com/TORISOUP/UnityNicoliveClient.git?path=Assets/TORISOUP/NicoliveClient/Plugins
```

## UniRx版

[v2023.09.06](https://github.com/TORISOUP/UnityNicoliveClient/releases/tag/v2023.09.06)

# 依存ライブラリ

次のライブラリを別途導入する必要があります。

* R3
* R3.Unity
* UniTask

# 機能一覧

 * ログインしてユーザセッション取得
 * 番組開始/終了
 * 番組延長手段取得
 * 番組延長
 * 運営コメント投稿/削除
 * 番組情報取得
 * 番組統計情報取得（来場者数、コメント数）
 * ~コメント取得~ 
 * アンケートの実行/終了

コメントの取得については[NdgrClientSharp](https://github.com/TORISOUP/NdgrClientSharp)を利用してください。

またコミュニティの概念がなくなったため、それに関係する機能を削除しました。

# 使い方

## 基本的な使い方

 1. `NiconicoUserClient.LoginAsync` を実行して`NiconicoUser`を取得
 2. `NiconicoUser`を`NicoliveApiClient`に渡してクライアント作成
 3. `SetNicoliveProgramId`に番組IDを渡して操作対象番組を登録
 4. 各種メソッドを実行

```cs
public async UniTask LoginAsync(string mail, string pass, CancellationToken ct)
{
    //ログイン実行
    NiconicoUser user = await NiconicoUserClient.LoginAsync(mail, pass, ct);

    //クライアントにユーザ情報を渡して初期化
    var client = new NicoliveApiClient(user);

    //操作したい番組ID登録
    client.SetNicoliveProgramId("lv123456");

    //運営コメントを非同期で投稿
    await client.SendOperatorCommentAsync("名前", "テスト投稿", "white", false, ct);
}
```

## 操作する番組の設定

`SetNicoliveProgramId()`を実行する。これを実行しないとApiClientは動作しません。

```cs
client.SetNicoliveProgramId("lv123456");
```

## UserAgentを設定する（推奨）

デフォルトでは`UnityNicoliveClient`がUAに設定されています。
変更したい場合は`SetCustomUserAgent`から設定可能（できるだけ自身のアプリ名を設定してください）

```cs
client.SetCustomUserAgent("YourApplicationNameHere");
```

## 番組の詳細情報取得

`GetProgramInfoAsync` で取得可能

```cs
ProgramInfo result = await client.GetProgramInfoAsync("lv123456", ct);
```

## コメントを取得する

コメントの取得については[NdgrClientSharp](https://github.com/TORISOUP/NdgrClientSharp)を利用してください。

1. `GetProgramInfoAsync` で番組情報(`ProgramInfo`)を取得する
2. `ProgramInfo`の中の`Room.ViewUei`を使ってNDGR（ニコ生の新コメントサーバー）に接続する
3. `NdgrClientSharp`の`NdgrLiveCommentFetcher`を使ってコメントを取得する


```cs
// 番組情報取得
var programInfo = await client.GetProgramInfoAsync("lv12345", ct);

// 新仕様にニコ生では常にRoomは1つ
// そこに入っているViewUriを使う
var viewUri = programInfo.Rooms[0].ViewUri

// 生放送コメント取得用のクライアントを生成
var liveCommentFetcher = new NdgrLiveCommentFetcher();

// コメントの受信準備
liveCommentFetcher
    .OnMessageReceived
    .Subscribe(chukedMessage =>
    {
        switch (chukedMessage.PayloadCase)
        {
            case ChunkedMessage.PayloadOneofCase.Message:
                // コメントやギフトの情報などはMessage
                Debug.Log(chukedMessage.Message);
                break;
            case ChunkedMessage.PayloadOneofCase.State:
                // 番組他状態の変更などはStateから取得可能
                Debug.Log(chukedMessage.State);
                break;

            default:
                break;
        }
    });

// コメントの受信開始
liveCommentFetcher.Connect(viewUri);

// ---

// コメントの受信停止
liveCommentFetcher.Disconnect();

// リソースの解放(忘れずに)
liveCommentFetcher.Dispose();
```

詳しくは[NdgrClientSharp](https://github.com/TORISOUP/NdgrClientSharp)のREADMEを参照してください。

## アンケートの実行

```cs
// アンケート開始
await client.StartEnqueteAsync(
    "lv12345",
    "好きな食べ物は？",
    new[] { "バナナ", "りんご", "カレー" }, ct);


// 結果表示＆取得
var result = await client.ShowResultEnqueteAsync("lv12345", ct);

foreach (var data in result.Items)
{
    Debug.Log($"{data.Name} : {data.Rate}%");
}

// アンケート終了
await client.FinishEnqueteAsync("lv12345", ct);
```


# 配布ライセンス

MITライセンス


# 権利表記

UniRx
Copyright (c) 2014 Yoshifumi Kawai https://github.com/neuecc/UniRx/blob/master/LICENSE

R3
Copyright (c) 2024 Cysharp, Inc. https://github.com/Cysharp/R3/blob/main/LICENSE

UniTask
Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc. https://github.com/Cysharp/UniTask/blob/master/LICENSE

NugetForUnity
Copyright (c) 2018 Patrick McCarthy https://github.com/GlitchEnzo/NuGetForUnity/blob/master/LICENSE