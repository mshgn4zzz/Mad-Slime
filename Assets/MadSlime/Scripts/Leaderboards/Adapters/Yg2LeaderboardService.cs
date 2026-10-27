using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;
using YG.Utils.LB;

namespace Leaderboards
{
    public class Yg2LeaderboardService : MonoBehaviour, ILeaderboardService
    {
        private const int EntriesTimeoutSeconds = 8;
        private const int MaxTopCount = 20;

        private Dictionary<int, UniTaskCompletionSource<LeaderboardResponse>> _requests =
            new Dictionary<int, UniTaskCompletionSource<LeaderboardResponse>>();
        private int _nextRequestId;

        public bool IsAuthorized => YG2.player.auth;

        public string PlayerId => YG2.player.id;

#if UNITY_WEBGL && UNITY_EDITOR == false
        [DllImport("__Internal")]
        private static extern void MadSlimeGetLeaderboard(string target, int requestId, string leaderboardName,
            int topCount, bool currentPlayer);
#endif

        public void SetScore(string leaderboardName, int score)
        {
            YG2.SetLeaderboard(leaderboardName, score);
        }

        public void OpenAuthDialog()
        {
            YG2.OpenAuthDialog();
        }

        public async UniTask<LeaderboardData> RequestEntriesAsync(string leaderboardName, int topCount,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(leaderboardName) || topCount < 1 || topCount > MaxTopCount)
            {
                throw new ArgumentException("Assign a leaderboard name and a top count between 1 and 20.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (YG2.infoYG.Leaderboards.enable == false)
            {
                throw new InvalidOperationException("Yandex leaderboards are disabled.");
            }

            LeaderboardResponse topResponse = await RequestAsync(leaderboardName, topCount, false, cancellationToken);
            LeaderboardEntryData[] top = MapPlayers(topResponse.players);
            LeaderboardEntryData? currentPlayer = null;

            if (IsAuthorized == false)
            {
                return new LeaderboardData(top, null);
            }

            string playerId = PlayerId;

            for (int i = 0; i < top.Length; i++)
            {
                if (top[i].Id == playerId)
                {
                    return new LeaderboardData(top, top[i]);
                }
            }

            bool hasPlayerError = false;

            try
            {
                LeaderboardResponse playerResponse =
                    await RequestAsync(leaderboardName, topCount, true, cancellationToken);
                LeaderboardEntryData[] players = MapPlayers(playerResponse.players);

                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].Id == playerId)
                    {
                        currentPlayer = players[i];
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                hasPlayerError = true;
            }

            return new LeaderboardData(top, currentPlayer, hasPlayerError);
        }

        private async UniTask<LeaderboardResponse> RequestAsync(
            string leaderboardName, int topCount, bool currentPlayer, CancellationToken cancellationToken)
        {
            using (CancellationTokenSource requestSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, this.GetCancellationTokenOnDestroy()))
            {
                _nextRequestId = checked(_nextRequestId + 1);

                int requestId = _nextRequestId;
                UniTaskCompletionSource<LeaderboardResponse> completion =
                    new UniTaskCompletionSource<LeaderboardResponse>();

                _requests.Add(requestId, completion);

                try
                {
#if UNITY_WEBGL && UNITY_EDITOR == false
                    MadSlimeGetLeaderboard(gameObject.name, requestId, leaderboardName, topCount, currentPlayer);
#elif UNITY_EDITOR
                    completion.TrySetResult(Simulate(leaderboardName, topCount, currentPlayer));
#else
                    completion.TrySetException(new PlatformNotSupportedException("Leaderboards require Yandex WebGL."));
#endif

                    LeaderboardResponse response = await completion.Task
                        .Timeout(TimeSpan.FromSeconds(EntriesTimeoutSeconds), DelayType.Realtime)
                        .AttachExternalCancellation(requestSource.Token);

                    if (string.IsNullOrEmpty(response.error) == false)
                    {
                        throw new InvalidOperationException(response.error);
                    }

                    return response;
                }
                finally
                {
                    _requests.Remove(requestId);
                    completion.TrySetCanceled();
                }
            }
        }

        public void OnLeaderboardResponse(string json)
        {
            LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);

            if (response == null || _requests.TryGetValue(response.requestId,
                out UniTaskCompletionSource<LeaderboardResponse> completion) == false)
            {
                return;
            }

            completion.TrySetResult(response);
        }

        private LeaderboardEntryData[] MapPlayers(LeaderboardPlayerResponse[] source)
        {
            if (source == null)
            {
                return Array.Empty<LeaderboardEntryData>();
            }

            List<LeaderboardEntryData> entries = new List<LeaderboardEntryData>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                LeaderboardPlayerResponse player = source[i];

                if (player == null || player.rank <= 0)
                {
                    continue;
                }

                entries.Add(new LeaderboardEntryData
                {
                    Id = player.id,
                    Name = player.name,
                    Rank = player.rank,
                    Score = player.score
                });
            }

            return entries.ToArray();
        }

#if UNITY_EDITOR
        private LeaderboardResponse Simulate(string leaderboardName, int topCount, bool currentPlayer)
        {
            LBData[] leaderboards = YG2.infoYG.Leaderboards.listLBSim;

            for (int i = 0; i < leaderboards.Length; i++)
            {
                LBData leaderboard = leaderboards[i];

                if (leaderboard.technoName != leaderboardName)
                {
                    continue;
                }

                List<LeaderboardPlayerResponse> players = new List<LeaderboardPlayerResponse>();

                if (leaderboard.players != null)
                {
                    for (int playerIndex = 0; playerIndex < leaderboard.players.Length; playerIndex++)
                    {
                        LBPlayerData player = leaderboard.players[playerIndex];

                        if (player == null || player.name == InfoYG.NO_DATA)
                        {
                            continue;
                        }

                        players.Add(new LeaderboardPlayerResponse
                        {
                            id = player.uniqueID,
                            name = player.name,
                            score = player.score
                        });
                    }
                }

                players.Sort(CompareScores);

                if (leaderboard.isInvertSortOrder == true)
                {
                    players.Reverse();
                }

                for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
                {
                    LeaderboardPlayerResponse player = players[playerIndex];

                    player.rank = playerIndex + 1;

                    if (currentPlayer == true && player.id == PlayerId)
                    {
                        return new LeaderboardResponse { players = new LeaderboardPlayerResponse[] { player } };
                    }
                }

                if (currentPlayer == true)
                {
                    return new LeaderboardResponse { players = Array.Empty<LeaderboardPlayerResponse>() };
                }

                if (players.Count > topCount)
                {
                    players.RemoveRange(topCount, players.Count - topCount);
                }

                return new LeaderboardResponse { players = players.ToArray() };
            }

            return new LeaderboardResponse { error = "Leaderboard simulation is missing: " + leaderboardName };
        }

        private int CompareScores(LeaderboardPlayerResponse first, LeaderboardPlayerResponse second)
        {
            return second.score.CompareTo(first.score);
        }
#endif
    }
}
