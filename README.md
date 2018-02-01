# UnityNicoliveClient

UnityNicoliveClientはニコニコ生放送の新配信番組をUnityから操作するクライアントです。  
ユーザ生放送（たぶんチャンネル生放送も）にのみ対応しています。

UniRxの使用を前提にしています。

# 機能一覧

 * ログインしてユーザセッション取得
 * 番組開始/終了
 * 番組延長手段取得
 * 番組延長
 * 運営コメント投稿/削除
 * BSPコメント投稿
 * 番組情報取得

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

## 現在放送中の番組取得

```cs
//現在放送中の番組ID取得
client.GetCurrentNicoliveProgramIdAsync()
    .Subscribe(lv => Debug.Log(lv));
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

#配布ライセンス

MITライセンス


# 権利表記

UniRx
Copyright (c) 2014 Yoshifumi Kawai https://github.com/neuecc/UniRx/blob/master/LICENSE