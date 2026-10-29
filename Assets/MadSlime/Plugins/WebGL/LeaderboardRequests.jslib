mergeInto(LibraryManager.library, {
    MadSlimeGetLeaderboard: function (targetPointer, requestId, namePointer, topCount, currentPlayer) {
        const target = UTF8ToString(targetPointer);
        const leaderboardName = UTF8ToString(namePointer);
        const complete = function (players, error) {
            SendMessage(target, "OnLeaderboardResponse", JSON.stringify({
                RequestId: requestId,
                Error: error,
                Players: players
            }));
        };
        const mapEntry = function (entry) {
            let playerName = "anonymous";
            if (entry.player.publicName &&
                (!entry.player.scopePermissions || entry.player.scopePermissions.public_name === "allow")) {
                playerName = entry.player.publicName;
            }
            return {
                Id: entry.player.uniqueID,
                Name: playerName,
                Rank: entry.rank,
                Score: entry.score
            };
        };
        Promise.resolve().then(function () {
            if (typeof ysdk === "undefined" || !ysdk || !ysdk.leaderboards) {
                throw new Error("Yandex leaderboards are unavailable");
            }
            if (currentPlayer) {
                return ysdk.isAvailableMethod("leaderboards.getPlayerEntry").then(function (available) {
                    if (!available) {
                        throw new Error("Player leaderboard entry is unavailable");
                    }
                    return ysdk.leaderboards.getPlayerEntry(leaderboardName).then(function (entry) {
                        return [mapEntry(entry)];
                    });
                });
            }
            return ysdk.leaderboards.getEntries(leaderboardName, {
                quantityTop: topCount,
                includeUser: false
            }).then(function (result) {
                return result.entries.map(mapEntry);
            });
        }).then(function (players) {
            complete(players, "");
        }).catch(function (error) {
            if (currentPlayer && error && error.code === "LEADERBOARD_PLAYER_NOT_PRESENT") {
                complete([], "");
                return;
            }
            let reason = "Leaderboard request failed";
            if (error && error.message) {
                reason = error.message;
            }
            complete([], reason);
        });
    }
});
