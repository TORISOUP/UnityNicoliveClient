# UnityNicoliveClient

UnityNicoliveClientはニコニコ生放送の新配信番組をUnityから操作するクライアントです。  
新配信のユーザ生放送にのみ対応しています。


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

* UniRx
* UniTask

# 機能一覧

 * ログインしてユーザセッション取得
 * 番組開始/終了
 * 番組延長手段取得
 * 番組延長
 * 運営コメント投稿/削除
 * 番組情報取得
 * 番組統計情報取得（来場者数、コメント数）
 * コメント取得
 * アンケートの実行/終了

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

`SetNicoliveProgramId()`を実行する。これを実行しないとApiClientは動作しない。

```cs
client.SetNicoliveProgramId("lv123456");
```

## UserAgentを設定する（推奨）

デフォルトでは`UnityNicoliveClient`がUAに設定されています。
変更したい場合は`SetCustomUserAgent`から設定可能（できるだけ自身のアプリ名を設定してください）

```cs
client.SetCustomUserAgent("YourApplicationNameHere");
```

## 自分が今放送している番組のID取得


### コミュニティ番組のみでよい場合

`GetCurrentCommunityProgramIdAsync()` で取得可能。
**※番組作成後にAPIで取得できるようになるまで１分程度かかる点に注意。**


```cs
//現在放送中の番組ID取得
string[] programs = await client.GetCurrentCommunityProgramIdAsync(ct);
```

### チャンネル番組を含む場合

チャンネル番組を含めて放送中IDが欲しい場合は、 `GetScheduledProgramListAsync()` 使う必要がある。

 `GetScheduledProgramListAsync()`を利用すると、ユーザに紐付いた放送予定・放送中のコミュニティ・チャンネル番組を一覧で取得できる。
 その中から該当のチャンネルIDの番組IDを検索する必要がある。
 
 **※番組作成後にAPIで取得できるようになるまで１分程度かかる点に注意。**


```cs
var targetChannelId = "ch123456789";
var programs = await client.GetScheduledProgramListAsync(ct);

foreach (var programSchedule in programs)
{
    if (programSchedule.SocialGroupId == targetChannelId
        && programSchedule.Status == ProgramStatus.OnAir
        && programSchedule.Status == ProgramStatus.Test //テスト放送も判定に含めるなら必要
       )
    {
        Debug.Log(targetChannelId + "は現在、" + programSchedule.ProgramId + "で配信中です。");
        return;
    }
}
Debug.Log(targetChannelId + "は現在配信していません。");
```


## 番組の詳細情報取得

`GetProgramInfoAsync` で取得可能

```cs
ProgramInfo result = await client.GetProgramInfoAsync("lv123456", ct);
```

## コメントを取得する

1. `GetProgramInfoAsync` で番組情報(`ProgramInfo`)を取得する
2. `ProgramInfo`の中の`Room`を使って`NicoliveCommentClient`を初期化する
3. `NicoliveCommentClient.OnMessageAsObservable()` を購読してコメントを受け取る
4. `NicoliveCommentClient.Connect()` でコメントサーバに接続
5. `NicoliveCommentClient.Disconnect()` で一時切断
6. `NicoliveCommentClient.Dispose()` で破棄

**使い終わったら必ずDispose()を実行すること！**

```cs
//番組情報取得
var pi = await client.GetProgramInfoAsync("lv12345", ct);

// 番組の部屋一覧
// 自分が放送する番組の場合は全部屋取得できる
// 他人の放送の場合は「座席を取得済み」の場合のみ、その座席のある部屋の情報が1つ取得できる
var rooms = pi.Rooms;

//先頭の部屋に接続するコメントクライアントを作成
using var commentClient = new NicoliveCommentClient(rooms.First(), user.UserId);

//コメント購読設定
commentClient.OnMessageAsObservable.Subscribe(x => Debug.Log(x.Content));

//クライアント接続
commentClient.Connect(resFrom: 0);

await UniTask.Delay(TimeSpan.FromSeconds(10));

//おかたづけ
commentClient.Disconnect();
```

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

UniTask
Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc. https://github.com/Cysharp/UniTask/blob/master/LICENSE

websocket-sharp
Copyright (c) 2010-2018 sta.blockhead https://github.com/sta/websocket-sharp/blob/master/LICENSE.txt
