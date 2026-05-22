using System.Collections.Generic;
using Network.Player;
using UnityEngine;
using Utils;

namespace Network.Match
{
    public enum MatchSessionAction
    {
        None,
        PlayerAssigned,
        MatchStateChanged
    }

    public readonly struct MatchSessionResult
    {
        public readonly MatchSessionAction Action;
        public readonly PlayerAssignedEventDto PlayerAssigned;
        public readonly MatchStateDto MatchState;

        private MatchSessionResult(
            MatchSessionAction action,
            PlayerAssignedEventDto playerAssigned,
            MatchStateDto matchState)
        {
            Action = action;
            PlayerAssigned = playerAssigned;
            MatchState = matchState;
        }

        public static MatchSessionResult None()
        {
            return new MatchSessionResult(MatchSessionAction.None, default, default);
        }

        public static MatchSessionResult Assigned(int playerIndex)
        {
            return new MatchSessionResult(
                MatchSessionAction.PlayerAssigned,
                new PlayerAssignedEventDto { playerIndex = playerIndex },
                default);
        }

        public static MatchSessionResult StateChanged(MatchStateDto state)
        {
            return new MatchSessionResult(MatchSessionAction.MatchStateChanged, default, state);
        }
    }

    public sealed class MatchSession
    {
        private const int MaxPlayers = 2;
        private const float DeploymentDurationSeconds = 60f;
        private const float BattleTurnDurationSeconds = 30f;

        private readonly Dictionary<ulong, PlayerConnectionInfo> _playersByClientId = new();
        private readonly Dictionary<int, FieldSnapshotDto> _pendingFieldSnapshotsByPlayer = new();

        public MatchPhase CurrentPhase { get; private set; }

        public int CurrentTurnNumber { get; private set; }

        public int ActivePlayerIndex { get; private set; }

        public float TurnRemainingSeconds { get; private set; }

        public bool IsMatchStarted { get; private set; }

        public MatchSessionResult RegisterPlayer(ulong clientId, bool isHost)
        {
            if (_playersByClientId.ContainsKey(clientId))
                return MatchSessionResult.None();

            if (_playersByClientId.Count >= MaxPlayers)
            {
                Log.Warn($"[NetworkMatchController] Rejecting extra client {clientId}. Match already has two players.");
                return MatchSessionResult.None();
            }

            var playerIndex = _playersByClientId.Count + 1;
            _playersByClientId[clientId] = new PlayerConnectionInfo(clientId, playerIndex, isHost);
            Log.Info($"[NetworkMatchController] Player{playerIndex} connected. ClientId={clientId}.");

            return MatchSessionResult.Assigned(playerIndex);
        }

        public bool CanAcceptPlayer(ulong clientId)
        {
            return _playersByClientId.ContainsKey(clientId) || _playersByClientId.Count < MaxPlayers;
        }

        public bool RemovePlayer(ulong clientId)
        {
            return _playersByClientId.Remove(clientId);
        }

        public MatchSessionResult SubmitFleetPreset(ulong clientId, FleetPresetDto preset)
        {
            if (!_playersByClientId.TryGetValue(clientId, out var player))
            {
                Log.Warn($"[NetworkMatchController] Fleet rejected. Unknown client {clientId}.");
                return MatchSessionResult.None();
            }

            if (!ValidateFleetPreset(preset))
            {
                Log.Warn($"[NetworkMatchController] Fleet rejected for Player{player.PlayerIndex}.");
                return MatchSessionResult.None();
            }

            player.FleetPreset = preset;
            player.HasSubmittedFleet = true;
            Log.Info($"[NetworkMatchController] Fleet accepted for Player{player.PlayerIndex}.");

            return TryStartMatch();
        }

        public bool CanSubmitEndTurn(ulong clientId)
        {
            if (!IsMatchStarted)
            {
                Log.Warn("[NetworkMatchController] End turn rejected. Match is not started.");
                return false;
            }

            if (CurrentPhase == MatchPhase.Deployment)
            {
                Log.Warn($"[NetworkMatchController] End turn ignored. Deployment ends by timer only. Client={clientId}.");
                return false;
            }

            if (!IsActivePlayer(clientId))
            {
                Log.Warn($"[NetworkMatchController] End turn rejected. Client {clientId} is not active player.");
                return false;
            }

            return true;
        }

        public bool SubmitFieldSnapshot(ulong clientId, FieldSnapshotDto snapshot)
        {
            if (!_playersByClientId.TryGetValue(clientId, out var player))
            {
                Log.Warn($"[NetworkMatchController] Field snapshot ignored. Unknown client {clientId}.");
                return false;
            }

            if (CurrentPhase == MatchPhase.Battle && player.PlayerIndex != ActivePlayerIndex)
            {
                Log.Warn($"[NetworkMatchController] Field snapshot ignored. Player{player.PlayerIndex} is not active.");
                return false;
            }

            snapshot.ownerPlayerIndex = player.PlayerIndex;
            snapshot.phase = CurrentPhase;
            snapshot.turnNumber = CurrentTurnNumber;
            _pendingFieldSnapshotsByPlayer[player.PlayerIndex] = snapshot;
            Log.Info($"[NetworkMatchController] Field snapshot buffered for Player{player.PlayerIndex}. Objects={snapshot.objects?.Length ?? 0}.");
            return true;
        }

        public bool AdvanceTime(float deltaTime)
        {
            if (!IsMatchStarted)
                return false;

            TurnRemainingSeconds -= deltaTime;
            return TurnRemainingSeconds <= 0f;
        }

        public MatchSessionResult CompleteExpiredTurn()
        {
            return CurrentPhase == MatchPhase.Deployment
                ? CompleteDeployment(MatchStatus.DeploymentTimerExpired)
                : CompleteActiveTurn(MatchStatus.TimerExpired);
        }

        public MatchSessionResult CompleteSubmittedTurn()
        {
            return CompleteActiveTurn(MatchStatus.Submitted);
        }

        public void ApplyRemoteMatchState(MatchStateDto state)
        {
            CurrentPhase = state.phase;
            CurrentTurnNumber = state.turnNumber;
            ActivePlayerIndex = state.activePlayerIndex;
            TurnRemainingSeconds = state.remainingSeconds;
            IsMatchStarted = state.phase is MatchPhase.Deployment or MatchPhase.Battle;
        }

        public List<FieldSnapshotDto> FlushPendingSnapshots()
        {
            var snapshots = new List<FieldSnapshotDto>(_pendingFieldSnapshotsByPlayer.Values);
            _pendingFieldSnapshotsByPlayer.Clear();
            return snapshots;
        }

        public MatchSessionResult TryStartMatch()
        {
            if (IsMatchStarted || _playersByClientId.Count != MaxPlayers)
                return MatchSessionResult.None();

            IsMatchStarted = true;
            CurrentPhase = MatchPhase.Deployment;
            CurrentTurnNumber = 0;
            ActivePlayerIndex = 0;
            TurnRemainingSeconds = DeploymentDurationSeconds;
            Log.Info("[NetworkMatchController] Deployment phase started.");
            return MatchSessionResult.StateChanged(CreateMatchState(MatchStatus.DeploymentStarted));
        }

        private MatchSessionResult CompleteDeployment(MatchStatus reason)
        {
            CurrentPhase = MatchPhase.Battle;
            CurrentTurnNumber = 1;
            ActivePlayerIndex = Random.Range(1, MaxPlayers + 1);
            TurnRemainingSeconds = BattleTurnDurationSeconds;
            Log.Info($"[NetworkMatchController] Deployment completed by {reason}. Battle starts. Active player: Player{ActivePlayerIndex}.");
            return MatchSessionResult.StateChanged(CreateMatchState(reason));
        }

        private MatchSessionResult CompleteActiveTurn(MatchStatus reason)
        {
            CurrentTurnNumber++;
            ActivePlayerIndex = ActivePlayerIndex == 1 ? 2 : 1;
            TurnRemainingSeconds = BattleTurnDurationSeconds;
            
            Log.Info($"[NetworkMatchController] Turn completed by {reason}. Active player: Player{ActivePlayerIndex}. Turn={CurrentTurnNumber}.");
            return MatchSessionResult.StateChanged(CreateMatchState(reason));
        }

        private MatchStateDto CreateMatchState(MatchStatus status)
        {
            return new MatchStateDto
            {
                phase = CurrentPhase,
                turnNumber = CurrentTurnNumber,
                activePlayerIndex = ActivePlayerIndex,
                remainingSeconds = TurnRemainingSeconds,
                status = status
            };
        }

        private static bool ValidateFleetPreset(FleetPresetDto preset)
        {
            return preset.ships is { Length: > 0 };
        }

        private bool IsActivePlayer(ulong clientId)
        {
            return _playersByClientId.TryGetValue(clientId, out var player) &&
                   player.PlayerIndex == ActivePlayerIndex;
        }
    }
}
