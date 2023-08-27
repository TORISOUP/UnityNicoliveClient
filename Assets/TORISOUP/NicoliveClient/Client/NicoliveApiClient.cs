using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using TORISOUP.NicoliveClient.Request;
using TORISOUP.NicoliveClient.Response;
using TORISOUP.NicoliveClient.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace TORISOUP.NicoliveClient.Client
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
        /// 現在放送中(テスト中含む)のコミュニティ番組の番組IDを取得する。
        /// 更新頻度が遅めのAPIなので、数分待たないと最新情報が取得できない場合がある。
        /// チャンネル番組の放送中IDを取得する場合はGetScheduledProgramListAsync()を使うこと。
        /// </summary>
        public async UniTask<string[]> GetCurrentCommunityProgramIdAsync(CancellationToken ct)
        {
            var schedules = await GetScheduledProgramListAsync(ct);
            return schedules
                .Where(x => x.Status == ProgramStatus.Test || x.Status == ProgramStatus.OnAir)
                .Select(x => x.ProgramId)
                .ToArray();
        }

        #endregion

        #region 放送予定番組一覧取得

        /// <summary>
        /// 放送予定、現在放送中の番組一覧を取得する
        /// （更新頻度が遅めのAPIなので、数分待たないと最新値が取得できない場合がある）
        /// </summary>
        public async UniTask<ProgramSchedule[]> GetScheduledProgramListAsync(CancellationToken ct)
        {
            var url = "https://live2.nicovideo.jp/unama/tool/v1/program_schedules";

            using var uwr = UnityWebRequest.Get(url);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }

            var json = uwr.downloadHandler.text;
            var dto = JsonUtility.FromJson<ApiResponseDto<ProgramScheduleDto[]>>(json);
            var schedules = dto.data.Select(x => x.ToProgramSchedule()).ToArray();
            return schedules;
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
        public async UniTask PutOperatorCommentAsync(
            string name,
            string text,
            string color,
            bool isPermanent,
            CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/watch/{NicoliveProgramId}/operator_comment";

            var json = JsonUtility.ToJson(new OperatorCommentRequest
            {
                text = text,
                userName = name,
                isPermanent = isPermanent,
                color = color
            });

            using var uwr = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(json));
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }


        /// <summary>
        /// 運営コメントの表示をやめる
        /// </summary>
        public async UniTask DeleteOperatorCommentAsync(CancellationToken ct)
        {
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/operator_comment", NicoliveProgramId);

            using var uwr = UnityWebRequest.Delete(url);
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion

        #region 番組開始/終了

        /// <summary>
        /// 番組を開始する
        /// </summary>
        /// <returns></returns>
        public async UniTask StartProgramAsync(CancellationToken ct)
        {
            await SegmentAsync("on_air", ct);
        }

        /// <summary>
        /// 番組を終了する
        /// </summary>
        /// <returns></returns>
        public async UniTask EndProgramAsync(CancellationToken ct)
        {
            await SegmentAsync("end", ct);
        }

        private async UniTask SegmentAsync(string state, CancellationToken ct)
        {
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/segment", NicoliveProgramId);

            var json = "{\"state\":\"" + state + "\"}";


            using var uwr = UnityWebRequest.Put(url, json);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion

        #region 番組延長

        /// <summary>
        /// 延長手段を取得する
        /// </summary>
        public async UniTask<ExtensionMethods[]> GetExtensionAsync(CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/watch/{NicoliveProgramId}/extension";

            using var uwr = UnityWebRequest.Get(url);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var json = uwr.downloadHandler.text;
                var extensions = JsonUtility.FromJson<ApiResponseDto<MethodsDto>>(json);

                return extensions.data.methods.Select(x => x.ToExtensionMethods()).ToArray();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }


        /// <summary>
        /// 番組延長を行う
        /// </summary>
        /// <param name="minutes">分数（30分単位)</param>
        /// <returns></returns>
        public async UniTask<ExtendResult> ExtendProgramAsync(int minutes, CancellationToken ct)
        {
            var url = string.Format("https://live2.nicovideo.jp/watch/{0}/extension", NicoliveProgramId);

            using var uwr = UnityWebRequest.Post(url, "POST");
            var data = Encoding.UTF8.GetBytes("{\"minutes\":" + minutes + "}");
            uwr.uploadHandler = new UploadHandlerRaw(data);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var json = uwr.downloadHandler.text;
                var extendDto = JsonUtility.FromJson<ApiResponseDto<ExtendDto>>(json);
                return new ExtendResult(extendDto.data.end_time);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion

        #region 番組情報取得

        /// <summary>
        /// 番組情報を取得する
        /// </summary>
        public async UniTask<ProgramInfo> GetProgramInfoAsync(string programId, CancellationToken ct)
        {
            return await _GetProgramInfoAsync(programId, ct);
        }

        /// <summary>
        /// 番組情報を取得する
        /// SetNicoliveProgramId()で設定された番組IDを対象とする
        /// </summary>
        public async UniTask<ProgramInfo> GetCurrentProgramInfoAsync(CancellationToken ct)
        {
            return await _GetProgramInfoAsync(NicoliveProgramId, ct);
        }

        private async UniTask<ProgramInfo> _GetProgramInfoAsync(string programId, CancellationToken ct)
        {
            var lv = programId;
            var url = $"https://live2.nicovideo.jp/watch/{lv}/programinfo";

            using var uwr = UnityWebRequest.Get(url);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var json = uwr.downloadHandler.text;
                try
                {
                    var dto = JsonUtility.FromJson<ApiResponseDto<ProgramInfoDto>>(json);
                    var programInfo = dto.data.ToProgramInfo();
                    return programInfo;
                }
                catch (Exception)
                {
                    throw new ProgramNotFoundException($"{lv} is not found.");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion

        #region 番組統計情報取得

        /// <summary>
        /// 番組統計情報を取得する
        /// </summary>
        public async UniTask<StatisticsResult> GetProgramStatisticsAsync(CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/watch/{NicoliveProgramId}/statistics";

            using var uwr = UnityWebRequest.Get(url);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var json = uwr.downloadHandler.text;
                var dto = JsonUtility.FromJson<ApiResponseDto<StatisticsResultDto>>(json);
                return dto.data.ToStatisticsResult();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion

        #region アンケート

        /// <summary>
        /// アンケートを開始する
        /// </summary>
        /// <param name="nicoliveProgramId">lvから始まる方のID</param>
        /// <param name="title">アンケートタイトル</param>
        /// <param name="questions">設問</param>
        /// <returns></returns>
        public async UniTask StartEnqueteAsync(
            string title,
            IEnumerable<string> questions,
            CancellationToken ct)
        {
            await StartEnqueteAsync(NicoliveProgramId, title, questions, ct);
        }

        /// <summary>
        /// アンケートを開始する
        /// </summary>
        /// <param name="nicoliveProgramId">lvから始まる方のID</param>
        /// <param name="title">アンケートタイトル</param>
        /// <param name="questions">設問</param>
        /// <returns></returns>
        public async UniTask StartEnqueteAsync(string nicoliveProgramId,
            string title,
            IEnumerable<string> questions,
            CancellationToken ct)
        {
            var pg = await GetPgProgramIdAsync(nicoliveProgramId, ct);
            await _StartEnqueteAsync(pg, title, questions, ct);
        }

        private async UniTask _StartEnqueteAsync(string pg,
            string title,
            IEnumerable<string> questions,
            CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/unama/api/v1/programs/{pg}/enquete";

            var items = questions as string[] ?? questions.ToArray();

            if (items.Length < 2)
            {
                throw new InvalidEnqueteException("設問は2つ以上必要です");
            }

            var json = JsonUtility.ToJson(new EnqueteRequest
            {
                programId = _pgProgramId,
                question = title,
                items = items
            });

            using var uwr = UnityWebRequest.Post(url, "POST");
            var data = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(data);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }


        /// <summary>
        /// アンケートの結果を表示する
        /// </summary>
        /// <returns></returns>
        public async UniTask<EnqueteResult> ShowResultEnqueteAsync(CancellationToken ct)
        {
            return await ShowResultEnqueteAsync(NicoliveProgramId, ct);
        }


        /// <summary>
        /// アンケートの結果を表示する
        /// </summary>
        /// <returns></returns>
        public async UniTask<EnqueteResult> ShowResultEnqueteAsync(string nicoliveProgramId, CancellationToken ct)
        {
            var pg = await GetPgProgramIdAsync(nicoliveProgramId, ct);
            return await _ShowResultEnqueteAsync(pg, ct);
        }

        private async UniTask<EnqueteResult> _ShowResultEnqueteAsync(string pg, CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/unama/api/v1/programs/{pg}/enquete/show_result";

            var json = "{ \"programId\":" + pg + " }";

            using var uwr = UnityWebRequest.Post(url, "POST");
            var data = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(data);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var result = uwr.downloadHandler.text;
                var dto = JsonUtility.FromJson<ApiResponseDto<EnqueteResultDto>>(result);
                return dto.data.ToEnqueteResult();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        /// <summary>
        /// アンケートの結果を表示する
        /// </summary>
        /// <returns></returns>
        public async UniTask FinishEnqueteAsync(CancellationToken ct)
        {
            await FinishEnqueteAsync(NicoliveProgramId, ct);
        }
        
        /// <summary>
        /// アンケートの結果を表示する
        /// </summary>
        /// <returns></returns>
        public async UniTask FinishEnqueteAsync(string nicoliveProgramId, CancellationToken ct)
        {
            var pg = await GetPgProgramIdAsync(nicoliveProgramId, ct);
            await _FinishEnqueteCoroutine(ct);
        }

        private async UniTask _FinishEnqueteCoroutine(CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/unama/api/v1/programs/{_pgProgramId}/enquete/end";

            var json = "{ \"programId\":" + _pgProgramId + " }";

            using var uwr = UnityWebRequest.Post(url, "POST");
            var data = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(data);
            uwr.SetRequestHeader("Content-type", "application/json");
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion

        #region pg取得

        /// <summary>
        /// 現在の操作対象のpgから始まる方の番組IDを取得する
        /// </summary>
        public async UniTask<string> GetPgProgramIdAsync(CancellationToken ct)
        {
            return await GetPgProgramIdAsync(NicoliveProgramId, ct);
        }

        /// <summary>
        /// pgから始まる方の番組IDを取得する
        /// </summary>
        public async UniTask<string> GetPgProgramIdAsync(string lv, CancellationToken ct)
        {
            var url = $"https://live2.nicovideo.jp/watch/{lv}/player";

            using var uwr = UnityWebRequest.Get(url);
            uwr.SetRequestHeader("Cookie", "user_session=" + _niconicoUser.UserSession);
            uwr.SetRequestHeader("User-Agent", _userAgent);

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var json = uwr.downloadHandler.text;
                var programId = Regex.Match(json, "programId\":\"(.*?)\"").Groups[1].Value;
                return programId;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicoliveApiClientException(uwr.downloadHandler.text, ex);
            }
        }

        #endregion
    }


    public class NicoliveApiClientException : Exception
    {
        public NicoliveApiClientException(string message) : base(message)
        {
        }

        public NicoliveApiClientException(string message, Exception ex) : base(message, ex)
        {
        }
    }

    public class ProgramNotFoundException : NicoliveApiClientException
    {
        public ProgramNotFoundException(string message) : base(message)
        {
        }
    }

    public class InvalidEnqueteException : NicoliveApiClientException
    {
        public InvalidEnqueteException(string message) : base(message)
        {
        }
    }
}