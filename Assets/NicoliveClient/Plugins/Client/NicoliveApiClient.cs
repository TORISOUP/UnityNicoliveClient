using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets.NicoliveClient.Plugins.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace NicoliveClient
{
    public class NicoliveApiClient
    {
        private NiconicoUser _niconicoUser;

        private Regex _lvRegex = new Regex(@"lv\d+");

        /// <summary>
        /// 現在の操作対象となっている番組ID
        /// </summary>
        public string NicoliveProgramId { get; private set; }


        #region Setup

        public NicoliveApiClient(NiconicoUser niconicoUser)
        {
            _niconicoUser = niconicoUser;
        }

        public NicoliveApiClient(NiconicoUser niconicoUser, string nicoliveProgramId)
        {
            _niconicoUser = niconicoUser;
            NicoliveProgramId = nicoliveProgramId;
        }

        /// <summary>
        /// 操作対象とする番組IDを設定する
        /// </summary>
        public void SetNicoliveProgramId(string id)
        {
            NicoliveProgramId = id;
        }

        #endregion


        #region 放送中番組取得

        /// <summary>
        /// 現在放送中の番組IDを取得する
        /// </summary>
        public IObservable<string> GetCurrentNicoliveProgramIdAsync()
        {
            return Observable.FromCoroutine<string>(GetPlayerStatusCoroutine).Kick();
        }

        private IEnumerator GetPlayerStatusCoroutine(IObserver<string> observer)
        {
            var url = "http://live.nicovideo.jp/api/getpublishstatus";

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();

                var result = www.downloadHandler.text;

                var match = _lvRegex.Match(result);
                if (match.Success)
                {
                    observer.OnNext(match.Value);
                    observer.OnCompleted();
                }
                else
                {
                    observer.OnError(new NicoliveApiClientException("番組IDの取得に失敗しました"));
                }
            }
        }

        #endregion

        #region 運営コメント

        /// <summary>
        /// 運営コメントを投稿する
        /// </summary>
        /// <param name="text">本文</param>
        /// <param name="name">投稿者名（nullで非表示)</param>
        /// <param name="color">表示色</param>
        /// <param name="isPermanent">永続表示するか</param>
        /// <returns></returns>
        public IObservable<Unit> SendOperatorCommentAsync(string text, string name = "", string color = "white", bool isPermanent = false)
        {
            return Observable.FromCoroutine<Unit>(o => PutOperatorCommentCoroutine(o, name, text, color, isPermanent)).Kick();
        }

        private IEnumerator PutOperatorCommentCoroutine(IObserver<Unit> observer, string name, string text, string color, bool isPermanent)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/operator_comment", NicoliveProgramId);

            var json = JsonUtility.ToJson(new OperatorCommentRequest
            {
                text = text,
                userName = name,
                isPermanent = isPermanent,
                color = color
            });

            using (var www = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(json)))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();
                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
        }

        /// <summary>
        /// 運営コメントの表示をやめる
        /// </summary>
        public IObservable<Unit> DeleteOperatorCommentAsync()
        {
            return Observable.FromCoroutine<Unit>(DeleteOperatorCommentCoroutine).Kick();
        }

        private IEnumerator DeleteOperatorCommentCoroutine(IObserver<Unit> observer)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/operator_comment", NicoliveProgramId);

            using (var www = UnityWebRequest.Delete(url))
            {
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();
                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
        }

        #endregion

        #region BSPコメント

        /// <summary>
        /// BSPコメントを投稿する
        /// </summary>
        /// <param name="text">本文</param>
        /// <param name="name">投稿者名（nullで非表示)</param>
        /// <param name="color">表示色</param>
        /// <returns></returns>
        public IObservable<Unit> SendBspCommentAsync(string text, string name = "", string color = "white")
        {
            return Observable.FromCoroutine<Unit>(o => PostBspCommentCoroutine(o, name, text, color)).Kick();
        }

        private IEnumerator PostBspCommentCoroutine(IObserver<Unit> observer, string name, string text, string color)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/bsp_comment", NicoliveProgramId);

            var json = JsonUtility.ToJson(new BspCommentRequest
            {
                text = text,
                userName = name,
                color = color
            });

            using (var www = UnityWebRequest.Post(url, "POST"))
            {
                var data = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(data);
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();
                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
        }

        #endregion

        #region 番組開始/終了

        /// <summary>
        /// 番組を開始する
        /// </summary>
        /// <returns></returns>
        public IObservable<bool> StartProgramAsync()
        {
            return Observable.FromCoroutine<bool>(o => SegmentCoroutine(o, "on_air")).Kick();
        }

        /// <summary>
        /// 番組を終了する
        /// </summary>
        /// <returns></returns>
        public IObservable<bool> EndProgramAsync()
        {
            return Observable.FromCoroutine<bool>(o => SegmentCoroutine(o, "end")).Kick();
        }

        private IEnumerator SegmentCoroutine(IObserver<bool> observer, string state)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/segment", NicoliveProgramId);

            var json = "{\"state\":\"" + state + "\"}";


            using (var www = UnityWebRequest.Put(url, json))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();
                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }
                observer.OnNext(!www.isHttpError);
                observer.OnCompleted();
            }
        }


        #endregion

        #region 番組延長
        /// <summary>
        /// 延長手段を取得する
        /// </summary>
        /// <returns></returns>
        public IObservable<ExtensionMethods[]> GetExtensionAsync()
        {
            return Observable.FromCoroutine<ExtensionMethods[]>(GetExtensionCoroutine).Kick();
        }

        private IEnumerator GetExtensionCoroutine(IObserver<ExtensionMethods[]> observer)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/extension", NicoliveProgramId);

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();
                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;
                var extensions = JsonUtility.FromJson<ApiResponseDto<MethodsDto>>(json);

                observer.OnNext(extensions.data.methods.Select(x => x.ToExtensionMethods()).ToArray());
                observer.OnCompleted();
            }
        }

        /// <summary>
        /// 番組延長を行う
        /// </summary>
        /// <param name="minutes">分数（30分単位)</param>
        /// <returns></returns>
        public IObservable<ExtendResult> ExtendProgramAsync(int minutes)
        {
            return Observable.FromCoroutine<ExtendResult>(o => ExtendProgramCoroutine(o, minutes)).Kick();
        }

        private IEnumerator ExtendProgramCoroutine(IObserver<ExtendResult> observer, int minutes)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/extension", NicoliveProgramId);

            using (var www = UnityWebRequest.Post(url, "POST"))
            {
                var data = Encoding.UTF8.GetBytes("{\"minutes\":" + minutes + "}");
                www.uploadHandler = new UploadHandlerRaw(data);
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();
                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;
                var extendDto = JsonUtility.FromJson<ExtendDto>(json);
                observer.OnNext(new ExtendResult(extendDto.end_time));
                observer.OnCompleted();
            }
        }

        #endregion

        #region 番組情報取得

        /// <summary>
        /// 番組情報を取得する
        /// </summary>
        public IObservable<ProgramInfo> GetProgramInfoAsync()
        {
            return Observable.FromCoroutine<ProgramInfo>(GetProgramInfo).Kick();
        }

        private IEnumerator GetProgramInfo(IObserver<ProgramInfo> observer)
        {
            var url = string.Format("http://live2.nicovideo.jp/watch/{0}/programinfo", NicoliveProgramId);

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);

                yield return www.SendWebRequest();

                if (www.isHttpError)
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;

                var dto = JsonUtility.FromJson<ApiResponseDto<ProgramInfoDto>>(json);
                var programInfo = dto.data.ToProgramInfo();

                observer.OnNext(programInfo);
                observer.OnCompleted();
            }
        }

        #endregion
    }

    public class NicoliveApiClientException : Exception
    {
        public NicoliveApiClientException(string message) : base(message)
        {
        }
    }
}
