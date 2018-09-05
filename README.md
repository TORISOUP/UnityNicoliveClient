# UnityNicoliveClient

UnityNicoliveClientはニコニコ生放送の新配信番組をUnityから操作するクライアントです。  
新配信のユーザ生放送にのみ対応しています。

UniRxの使用を前提にしています。

# 機能一覧

 * ログインしてユーザセッション取得
 * 番組開始/終了
 * 番組延長手段取得
 * 番組延長
 * 運営コメント投稿/削除
 * BSPコメント投稿
 * 番組情報取得
 * 番組統計情報取得（来場者数、コメント数）
 * コメント取得
 * アンケートの実行

# 使い方

## 基本的な使い方

 1. `NiconicoUserClient.LoginAsync` を実行して`NiconicoUser`を取得
 2. `NiconicoUser`を`NicoliveApiClient`に渡してクライアント作成
 3. `SetNicoliveProgramId`に番組IDを渡して操作対象番組を登録
 4. 各種メソッドを実行

```cs
IEnumerator LoginCoroutine()
{
    var mail = "ニコニコのメールアドレス";
    var pass = "ニコニコのパスワード";

    //ログイン実行
    var u = NiconicoUserClient.LoginAsync(mail, pass).ToYieldInstruction();
    yield return u; //ログイン処理を待機

    //ログインに成功するとユーザ情報が返ってくる
    NiconicoUser user = u.Result;

    //クライアントにユーザ情報を渡して初期化
    var client = new NicoliveApiClient(user);

    //操作したい番組ID登録
    client.SetNicoliveProgramId("lv123456");

    //運営コメントを非同期で投稿
    client.SendOperatorCommentAsync("テスト投稿");

    //投稿が終わるのを同期的に待つ場合はToYieldInstruction()
    yield return client.SendOperatorCommentAsync("テスト投稿").ToYieldInstruction();
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

`GetCurrentCommunityProgramIdAsync()` で取得可能。取得できない場合は`OnError()`が返る。  
**※番組作成後にAPIで取得できるようになるまで１分程度かかる点に注意。**


```cs
//現在放送中の番組ID取得
client.GetCurrentCommunityProgramIdAsync()
    .Subscribe(lv => Debug.Log(lv));
```

### チャンネル番組を含む場合

チャンネル番組を含めて放送中IDが欲しい場合は、 `GetScheduledProgramListAsync()` 使う必要がある。

 `GetScheduledProgramListAsync()`を利用すると、ユーザに紐付いた放送予定・放送中のコミュニティ・チャンネル番組を一覧で取得できる。
 その中から該当のチャンネルIDの番組IDを検索する必要がある。
 
 **※番組作成後にAPIで取得できるようになるまで１分程度かかる点に注意。**



```cs
var targetChannelId = "ch123456789";

client.GetScheduledProgramListAsync()
    .Subscribe(programs =>
    {
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
    });
```



## 番組の詳細情報取得

`GetProgramInfoAsync` で取得可能

```cs
var programInfo = default(ProgramInfo);
client.GetProgramInfoAsync().Subscribe(x => programInfo = x);
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
IEnumerator CommentCoroutine(NiconicoUser user, NicoliveApiClient apiClient)
{
    //番組情報取得
    var pi = apiClient.GetProgramInfoAsync().ToYieldInstruction();
    yield return pi;

    // 番組の部屋一覧
    // 自分が放送する番組の場合は全部屋取得できる
    // 他人の放送の場合は「座席を取得済み」の場合のみ、その座席のある部屋の情報が1つ取得できる
    var rooms = pi.Result.Rooms;

    //先頭の部屋に接続するコメントクライアントを作成
    var commentClient = new NicoliveCommentClient(rooms.First(), user.UserId);

    //コメント購読設定
    commentClient.OnMessageAsObservable.Subscribe(x => Debug.Log(x.Content));

    //クライアント接続
    commentClient.Connect(resFrom: 0);

    yield return new WaitForSeconds(10);

    //おかたづけ
    commentClient.Disconnect();
    commentClient.Dispose();
}
```

## アンケート

```cs
IEnumerator EnqueteCoroutine(NicoliveApiClient client)
{
    var title = "どれが好き？";
    var questions = new string[] { "き○この山", "た○のこの里", "コ○ラのマーチ" };

    //アンケートの実行
    client.StartEnqueteAsync(title, questions);


    //回答時間
    yield return new WaitForSeconds(10);


    //結果表示
    var ri = client.ShowResultEnqueteAsync().ToYieldInstruction();
    yield return ri; //APIの結果待ち
    var enqueteResult = ri.Result;

    //結果表示
    Debug.Log(enqueteResult.Title);
    foreach (var i in enqueteResult.Items)
    {
        //各要素の回答率
        Debug.Log(string.Format("{0}:{1}%", i.Name, i.Rate));
    }


    //結果表示待機
    yield return new WaitForSeconds(3);


    //アンケート終了
    client.FinishEnqueteAsync();
}
```


## エラーハンドリング

```cs
//失敗時はOnErrorが通知される
client.SendOperatorCommentAsync("テスト投稿")
    .Subscribe(_ => { }, ex => Debug.LogError(ex));
```

## その他

`NicoliveApiClient`が提供する各種メソッドは全てHot変換(PublishLast)済み。
そのため明示的な`Subscribe()`は省略可能。

```cs
client.SendOperatorCommentAsync("テスト投稿").Subscribe(); 
client.SendOperatorCommentAsync("テスト投稿"); //Subscribeを省略しても実行される
```

# 配布ライセンス

MITライセンス


# 権利表記

UniRx
Copyright (c) 2014 Yoshifumi Kawai https://github.com/neuecc/UniRx/blob/master/LICENSE

websocket-sharp
Copyright (c) 2010-2018 sta.blockhead https://github.com/sta/websocket-sharp/blob/master/LICENSE.txt
