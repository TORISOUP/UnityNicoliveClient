using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// 現在の操作対象となっている番組ID
        /// </summary>
        public string NicoliveProgramId { get; private set; }

        /// <summary>
        /// pgから始まる方の番組ID
        /// </summary>
        private string _pgProgramId;

        private string _userAgent = "UnityNicoliveClient";

        #region Setup

        /// <summary>
        /// NicoliveApiClientを初期化する
        /// </summary>
        /// <param name="niconicoUser">ユーザ情報</param>
        public NicoliveApiClient(NiconicoUser niconicoUser)
        {
            _niconicoUser = niconicoUser;
        }

        /// <summary>
        /// NicoliveApiClientを初期化する
        /// </summary>
        /// <param name="niconicoUser">ユーザ情報</param>
        /// <param name="nicoliveProgramId">操作対象の番組ID(lv)</param>
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
            if (NicoliveProgramId != id) _pgProgramId = null;
            NicoliveProgramId = id;
        }

        /// <summary>
        /// UserAgentを設定する
        /// </summary>
        public void SetCustomUserAgent(string userAgent)
        {
            _userAgent = userAgent;
        }

        #endregion


        #region 放送中番組取得

        /// <summary>
        /// 現在放送中の番組IDを取得する
        /// </summary>
        [Obsolete("放送中のコミュニティ番組IDを取得する場合はGetCurrentCommunityProgramIdAsync()を利用して下さい")]
        public IObservable<string> GetCurrentNicoliveProgramIdAsync()
        {
            return GetCurrentCommunityProgramIdAsync();
        }

        /// <summary>
        /// 現在放送中(テスト中含む)のコミュニティ番組の番組IDを取得する。
        /// 更新頻度が遅めのAPIなので、数分待たないと最新情報が取得できない場合がある。
        /// チャンネル番組の放送中IDを取得する場合はGetScheduledProgramListAsync()を使うこと。
        /// </summary>
        public IObservable<string> GetCurrentCommunityProgramIdAsync()
        {
            return Observable.FromCoroutine<ProgramSchedule[]>(GetScheduledProgramListCoroutine)
                .Select(schedules =>
                {
                    foreach (var schedule in schedules)
                    {
                        if (
                            schedule.SocialGroupType == SocialGroupType.Community &&
                            (schedule.Status == ProgramStatus.Test || schedule.Status == ProgramStatus.OnAir))
                        {
                            return schedule.ProgramId;
                        }
                    }

                    throw new NicoliveApiClientException("現在放送中の番組はありません");
                }).Kick();
        }

        #endregion

        #region 放送予定番組一覧取得

        /// <summary>
        /// 放送予定、現在放送中の番組一覧を取得する
        /// （更新頻度が遅めのAPIなので、数分待たないと最新値が取得できない場合がある）
        /// </summary>
        public IObservable<ProgramSchedule[]> GetScheduledProgramListAsync()
        {
            return Observable.FromCoroutine<ProgramSchedule[]>(GetScheduledProgramListCoroutine).Kick();
        }

        private IEnumerator GetScheduledProgramListCoroutine(IObserver<ProgramSchedule[]> observer)
        {
            var url = "https://live2.nicovideo.jp/unama/tool/v1/program_schedules";

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;

                var dto = JsonUtility.FromJson<ApiResponseDto<ProgramScheduleDto[]>>(json);
                var schedules = dto.data.Select(x => x.ToProgramSchedule()).ToArray();

                observer.OnNext(schedules);
                observer.OnCompleted();
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
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/operator_comment", NicoliveProgramId);

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
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
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
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/operator_comment", NicoliveProgramId);

            using (var www = UnityWebRequest.Delete(url))
            {
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
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
        public IObservable<Unit> StartProgramAsync()
        {
            return Observable.FromCoroutine<Unit>(o => SegmentCoroutine(o, "on_air")).Kick();
        }

        /// <summary>
        /// 番組を終了する
        /// </summary>
        /// <returns></returns>
        public IObservable<Unit> EndProgramAsync()
        {
            return Observable.FromCoroutine<Unit>(o => SegmentCoroutine(o, "end")).Kick();
        }

        private IEnumerator SegmentCoroutine(IObserver<Unit> observer, string state)
        {
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/segment", NicoliveProgramId);

            var json = "{\"state\":\"" + state + "\"}";


            using (var www = UnityWebRequest.Put(url, json))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                observer.OnNext(Unit.Default);


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
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/extension", NicoliveProgramId);

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);
#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif
#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
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
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/extension", NicoliveProgramId);

            using (var www = UnityWebRequest.Post(url, "POST"))
            {
                var data = Encoding.UTF8.GetBytes("{\"minutes\":" + minutes + "}");
                www.uploadHandler = new UploadHandlerRaw(data);
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;
                var extendDto = JsonUtility.FromJson<ApiResponseDto<ExtendDto>>(json);
                observer.OnNext(new ExtendResult(extendDto.data.end_time));
                observer.OnCompleted();
            }
        }

        #endregion

        #region 番組情報取得

        /// <summary>
        /// 番組情報を取得する
        /// </summary>
        public IObservable<ProgramInfo> GetProgramInfoAsync(string programId)
        {
            return Observable.FromCoroutine<ProgramInfo>(o => GetProgramInfo(o, programId)).Kick();
        }

        /// <summary>
        /// 番組情報を取得する
        /// SetNicoliveProgramId()で設定された番組IDを対象とする
        /// </summary>
        public IObservable<ProgramInfo> GetProgramInfoAsync()
        {
            return Observable.FromCoroutine<ProgramInfo>(o => GetProgramInfo(o, null)).Kick();
        }

        private IEnumerator GetProgramInfo(IObserver<ProgramInfo> observer, string programId)
        {
            var lv = string.IsNullOrEmpty(programId) ? NicoliveProgramId : programId;
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/programinfo", lv);

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;
                try
                {
                    var dto = JsonUtility.FromJson<ApiResponseDto<ProgramInfoDto>>(json);
                    var programInfo = dto.data.ToProgramInfo();

                    observer.OnNext(programInfo);
                    observer.OnCompleted();
                }
                catch
                {
                    observer.OnError(new Exception("番組情報が取得できませんでした"));
                }
            }
        }

        #endregion

        #region 番組統計情報取得

        /// <summary>
        /// 番組統計情報を取得する
        /// </summary>
        public IObservable<StatisticsResult> GetProgramStatisticsAsync()
        {
            return Observable.FromCoroutine<StatisticsResult>(GetProgramStatistics).Kick();
        }

        private IEnumerator GetProgramStatistics(IObserver<StatisticsResult> observer)
        {
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/statistics", NicoliveProgramId);

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;

                var dto = JsonUtility.FromJson<ApiResponseDto<StatisticsResultDto>>(json);
                var statistics = dto.data.ToStatisticsResult();

                observer.OnNext(statistics);
                observer.OnCompleted();
            }
        }

        #endregion

        #region アンケート

        /// <summary>
        /// アンケートを開始する
        /// </summary>
        /// <param name="title">アンケートタイトル</param>
        /// <param name="questions">設問</param>
        /// <returns></returns>
        public IObservable<Unit> StartEnqueteAsync(string title, IEnumerable<string> questions)
        {
            var enqueteObservable = Observable.FromCoroutine<Unit>(o => StartEnqueteCoroutine(o, title, questions));

            if (string.IsNullOrEmpty(_pgProgramId))
            {
                return GetPgProgramIdAsync()
                    .Do(pg => _pgProgramId = pg) //pgがない場合は取得する
                    .SelectMany(_ => enqueteObservable).Kick();
            }

            return enqueteObservable.Kick();
        }

        private IEnumerator StartEnqueteCoroutine(IObserver<Unit> observer, string title, IEnumerable<string> questions)
        {
            var url = string.Format("https://live2.nicovideo.jp/unama/api/v1/programs/{0}/enquete", _pgProgramId);

            var items = questions as string[] ?? questions.ToArray();

            if (items.Length < 2)
            {
                observer.OnError(new NicoliveApiClientException("アンケートの実行には回答が2つ以上必要です"));
                yield break;
            }

            var json = JsonUtility.ToJson(new EnqueteRequest
            {
                programId = _pgProgramId,
                question = title,
                items = items
            });

            using (var www = UnityWebRequest.Post(url, "POST"))
            {
                var data = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(data);
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
        }


        /// <summary>
        /// アンケートの結果を表示する
        /// </summary>
        /// <returns></returns>
        public IObservable<EnqueteResult> ShowResultEnqueteAsync()
        {
            var enqueteObservable = Observable.FromCoroutine<EnqueteResult>(ShowResultEnqueteCoroutine);

            if (string.IsNullOrEmpty(_pgProgramId))
            {
                return GetPgProgramIdAsync()
                    .Do(pg => _pgProgramId = pg) //pgがない場合は取得する
                    .SelectMany(_ => enqueteObservable).Kick();
            }

            return enqueteObservable.Kick();
        }

        private IEnumerator ShowResultEnqueteCoroutine(IObserver<EnqueteResult> observer)
        {
            var url = string.Format("https://live2.nicovideo.jp/unama/api/v1/programs/{0}/enquete/show_result", _pgProgramId);

            var json = "{ \"programId\":" + _pgProgramId + " }";

            using (var www = UnityWebRequest.Post(url, "POST"))
            {
                var data = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(data);
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var result = www.downloadHandler.text;
                var dto = JsonUtility.FromJson<ApiResponseDto<EnqueteResultDto>>(result);

                observer.OnNext(dto.data.ToEnqueteResult());
                observer.OnCompleted();
            }
        }


        /// <summary>
        /// アンケートの結果を表示する
        /// </summary>
        /// <returns></returns>
        public IObservable<Unit> FinishEnqueteAsync()
        {
            var enqueteObservable = Observable.FromCoroutine<Unit>(FinishEnqueteCoroutine);

            if (string.IsNullOrEmpty(_pgProgramId))
            {
                return GetPgProgramIdAsync()
                    .Do(pg => _pgProgramId = pg) //pgがない場合は取得する
                    .SelectMany(_ => enqueteObservable).Kick();
            }

            return enqueteObservable.Kick();
        }

        private IEnumerator FinishEnqueteCoroutine(IObserver<Unit> observer)
        {
            var url = string.Format("https://live2.nicovideo.jp/unama/api/v1/programs/{0}/enquete/end", _pgProgramId);

            var json = "{ \"programId\":" + _pgProgramId + " }";

            using (var www = UnityWebRequest.Post(url, "POST"))
            {
                var data = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(data);
                www.SetRequestHeader("Content-type", "application/json");
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
        }

        #endregion

        #region pg取得

        /// <summary>
        /// pgから始まる方の番組IDを取得する
        /// </summary>
        public IObservable<string> GetPgProgramIdAsync()
        {
            return Observable.FromCoroutine<string>(GetPgProgramIdCoroutine).Kick();
        }

        private IEnumerator GetPgProgramIdCoroutine(IObserver<string> observer)
        {
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/player", NicoliveProgramId);

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
                www.SetRequestHeader("User-Agent", _userAgent);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicoliveApiClientException(www.downloadHandler.text));
                    yield break;
                }

                var json = www.downloadHandler.text;
                var programId = Regex.Match(json, "programId\":\"(.*?)\"").Groups[1].Value;
                observer.OnNext(programId);
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
