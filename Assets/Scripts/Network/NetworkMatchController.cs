using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public sealed class NetworkMatchController : MonoBehaviour
    {
        private const int MaxPlayers = 2;
        private const float DeploymentDurationSeconds = 60f;
        private const float BattleTurnDurationSeconds = 30f;

        public event System.Action<PlayerAssignedEventDto> OnPlayerAssigned;
        public event System.Action<MatchStateDto> OnMatchStateChanged;
        public event System.Action<FieldSnapshotDto> OnFieldSnapshotReceived;

        private readonly Dictionary<ulong, PlayerConnectionInfo> _playersByClientId = new();
        private readonly Dictionary<int, FieldSnapshotDto> _pendingFieldSnapshotsByPlayer = new();
        private int _turnNumber;
        private int _activePlayerIndex;
        private float _turnRemainingSeconds;
        private MatchPhase _phase;
        private bool _matchStarted;
        private bool _callbacksRegistered;
        private bool _messageHandlersRegistered;

        private NetworkManager _networkManager;
        private NetworkPlayerContext _playerContext;
        private NetworkPlacementController _placementController;
        private bool _timerSnapshotSubmitted;

        public MatchPhase CurrentPhase => _phase;
        public int CurrentTurnNumber => _turnNumber;

        private void Awake()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            RegisterCallbacks();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void Update()
        {
            if (!_networkManager || !_matchStarted)
                return;

            _turnRemainingSeconds -= Time.deltaTime;
            TrySubmitSnapshotBeforeClientTimerExpires();

            if (!_networkManager.IsServer)
                return;

            if (_turnRemainingSeconds > 0f)
                return;

            if (_phase == MatchPhase.Deployment)
                CompleteDeployment(MatchStatus.DeploymentTimerExpired);
            else
                CompleteActiveTurn(MatchStatus.TimerExpired);
        }

        public void SubmitFleetPreset(FleetPresetDto preset)
        {
            if (IsHost())
            {
                HandleFleetPreset(NetworkManager.ServerClientId, preset);
                return;
            }

            SendToServer(NetworkMessageNames.SubmitFleetPreset, ToJson(preset));
        }

        public void RequestEndTurn()
        {
            var request = new EndTurnRequestDto { turnNumber = _turnNumber };
            if (IsHost())
            {
                HandleEndTurnRequest(NetworkManager.ServerClientId, request);
                return;
            }

            SendToServer(NetworkMessageNames.EndTurnRequest, ToJson(request));
        }

        public void SubmitFieldSnapshot(FieldSnapshotDto snapshot)
        {
            if (snapshot.ownerPlayerIndex <= 0)
                return;

            if (IsHost())
            {
                HandleFieldSnapshot(NetworkManager.ServerClientId, snapshot);
                return;
            }

            SendToServer(NetworkMessageNames.SubmitFieldSnapshot, ToJson(snapshot));
        }

        public void InitializeAfterNetworkStart()
        {
            RegisterCallbacks();

            if (_networkManager && _networkManager.IsServer)
                EnsureServerPlayerRegistered(NetworkManager.ServerClientId);
        }

        private void RegisterCallbacks()
        {
            ResolveDependencies();
            if (!_networkManager)
                return;

            if (!_callbacksRegistered)
            {
                _networkManager.OnClientConnectedCallback += HandleClientConnected;
                _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
                _callbacksRegistered = true;
            }

            if (_networkManager.CustomMessagingManager == null)
                return;
            if (_messageHandlersRegistered)
                return;

            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.SubmitFleetPreset,
                ReceiveFleetPreset);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.EndTurnRequest,
                ReceiveEndTurnRequest);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.MatchState,
                ReceiveMatchState);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.PlayerAssigned,
                ReceivePlayerAssigned);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.SubmitFieldSnapshot,
                ReceiveFieldSnapshot);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.FieldSnapshot,
                ReceiveFieldSnapshotBroadcast);
            _messageHandlersRegistered = true;
            Debug.Log("[NetworkMatchController] Network message handlers registered.");
        }

        private void UnregisterCallbacks()
        {
            if (!_networkManager)
                return;

            if (_callbacksRegistered)
            {
                _networkManager.OnClientConnectedCallback -= HandleClientConnected;
                _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
                _callbacksRegistered = false;
            }

            if (_networkManager.CustomMessagingManager == null || !_messageHandlersRegistered)
                return;

            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.SubmitFleetPreset);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.EndTurnRequest);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.MatchState);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.PlayerAssigned);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.SubmitFieldSnapshot);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.FieldSnapshot);
            _messageHandlersRegistered = false;
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!_networkManager.IsServer)
                return;

            EnsureServerPlayerRegistered(clientId);
        }

        private void EnsureServerPlayerRegistered(ulong clientId)
        {
            if (_playersByClientId.ContainsKey(clientId))
                return;

            if (_playersByClientId.Count >= MaxPlayers)
            {
                Debug.LogWarning($"[NetworkMatchController] Rejecting extra client {clientId}. Match already has two players.");
                _networkManager.DisconnectClient(clientId);
                return;
            }

            var playerIndex = _playersByClientId.Count + 1;
            _playersByClientId[clientId] = new PlayerConnectionInfo(
                clientId,
                playerIndex,
                clientId == NetworkManager.ServerClientId);

            Debug.Log($"[NetworkMatchController] Player{playerIndex} connected. ClientId={clientId}.");
            SendPlayerAssignment(clientId, playerIndex);
            TryStartMatch();
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!_networkManager.IsServer)
                return;

            if (!_playersByClientId.Remove(clientId))
                return;

            Debug.Log($"[NetworkMatchController] Client {clientId} removed from match.");
        }

        private void ReceiveFleetPreset(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            HandleFleetPreset(senderClientId, FromJson<FleetPresetDto>(json));
        }

        private void ReceiveEndTurnRequest(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            HandleEndTurnRequest(senderClientId, FromJson<EndTurnRequestDto>(json));
        }

        private void ReceiveMatchState(ulong senderClientId, FastBufferReader reader)
        {
            if (_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            var state = FromJson<MatchStateDto>(json);
            _phase = state.phase;
            _turnNumber = state.turnNumber;
            _activePlayerIndex = state.activePlayerIndex;
            _turnRemainingSeconds = state.remainingSeconds;
            _matchStarted = state.phase is MatchPhase.Deployment or MatchPhase.Battle;
            _timerSnapshotSubmitted = false;
            OnMatchStateChanged?.Invoke(state);
            Debug.Log($"[NetworkMatchController] Active player from network state: Player{state.activePlayerIndex}.");
            Debug.Log($"[NetworkMatchController] Match state received. Status={state.status}, Turn={state.turnNumber}, ActivePlayer={state.activePlayerIndex}.");
        }

        private void ReceivePlayerAssigned(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string json);
            var evt = FromJson<PlayerAssignedEventDto>(json);
            ApplyPlayerAssignment(evt);
        }

        private void ReceiveFieldSnapshot(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            HandleFieldSnapshot(senderClientId, FromJson<FieldSnapshotDto>(json));
        }

        private void ReceiveFieldSnapshotBroadcast(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string json);
            OnFieldSnapshotReceived?.Invoke(FromJson<FieldSnapshotDto>(json));
        }

        private void HandleFleetPreset(ulong clientId, FleetPresetDto preset)
        {
            if (!_playersByClientId.TryGetValue(clientId, out var player))
            {
                Debug.LogWarning($"[NetworkMatchController] Fleet rejected. Unknown client {clientId}.");
                return;
            }

            if (!ValidateFleetPreset(preset))
            {
                Debug.LogWarning($"[NetworkMatchController] Fleet rejected for Player{player.PlayerIndex}.");
                return;
            }

            player.FleetPreset = preset;
            player.HasSubmittedFleet = true;
            Debug.Log($"[NetworkMatchController] Fleet accepted for Player{player.PlayerIndex}.");

            TryStartMatch();
        }

        private void HandleEndTurnRequest(ulong clientId, EndTurnRequestDto request)
        {
            if (!_matchStarted)
            {
                Debug.LogWarning("[NetworkMatchController] End turn rejected. Match is not started.");
                return;
            }

            if (_phase == MatchPhase.Deployment)
            {
                Debug.LogWarning($"[NetworkMatchController] End turn ignored. Deployment ends by timer only. Client={clientId}.");
                return;
            }

            if (!IsActivePlayer(clientId))
            {
                Debug.LogWarning($"[NetworkMatchController] End turn rejected. Client {clientId} is not active player.");
                return;
            }

            CompleteActiveTurn(MatchStatus.Submitted);
        }

        private void HandleFieldSnapshot(ulong clientId, FieldSnapshotDto snapshot)
        {
            if (!_playersByClientId.TryGetValue(clientId, out var player))
            {
                Debug.LogWarning($"[NetworkMatchController] Field snapshot ignored. Unknown client {clientId}.");
                return;
            }

            if (_phase == MatchPhase.Battle && player.PlayerIndex != _activePlayerIndex)
            {
                Debug.LogWarning($"[NetworkMatchController] Field snapshot ignored. Player{player.PlayerIndex} is not active.");
                return;
            }

            snapshot.ownerPlayerIndex = player.PlayerIndex;
            snapshot.phase = _phase;
            snapshot.turnNumber = _turnNumber;
            _pendingFieldSnapshotsByPlayer[player.PlayerIndex] = snapshot;
            Debug.Log($"[NetworkMatchController] Field snapshot buffered for Player{player.PlayerIndex}. Objects={snapshot.objects?.Length ?? 0}.");
        }

        private void TryStartMatch()
        {
            if (_matchStarted)
                return;
            if (_playersByClientId.Count != MaxPlayers)
                return;

            _matchStarted = true;
            _phase = MatchPhase.Deployment;
            _turnNumber = 0;
            _activePlayerIndex = 0;
            _turnRemainingSeconds = DeploymentDurationSeconds;
            Debug.Log("[NetworkMatchController] Deployment phase started.");
            BroadcastMatchState(MatchStatus.DeploymentStarted);
        }

        private void CompleteDeployment(MatchStatus reason)
        {
            CaptureLocalSnapshotForTimer();
            FlushPendingPlacements();
            _phase = MatchPhase.Battle;
            _turnNumber = 1;
            _activePlayerIndex = Random.Range(1, MaxPlayers + 1);
            _turnRemainingSeconds = BattleTurnDurationSeconds;
            _timerSnapshotSubmitted = false;
            Debug.Log($"[NetworkMatchController] Deployment completed by {reason}. Battle starts. Active player: Player{_activePlayerIndex}.");
            BroadcastMatchState(reason);
        }

        private void CompleteActiveTurn(MatchStatus reason)
        {
            CaptureLocalSnapshotForTimer();
            FlushPendingPlacements();
            _turnNumber++;
            _activePlayerIndex = _activePlayerIndex == 1 ? 2 : 1;
            _turnRemainingSeconds = BattleTurnDurationSeconds;
            _timerSnapshotSubmitted = false;
            Debug.Log($"[NetworkMatchController] Turn completed by {reason}. Active player: Player{_activePlayerIndex}. Turn={_turnNumber}.");
            BroadcastMatchState(reason);
        }

        private void FlushPendingPlacements()
        {
            foreach (var snapshot in _pendingFieldSnapshotsByPlayer.Values)
            {
                OnFieldSnapshotReceived?.Invoke(snapshot);
                Broadcast(NetworkMessageNames.FieldSnapshot, ToJson(snapshot));
            }

            _pendingFieldSnapshotsByPlayer.Clear();
        }

        private void CaptureLocalSnapshotForTimer()
        {
            ResolveDependencies();

            if (!_placementController || !_playerContext || !_playerContext.HasAssignedPlayer)
                return;
            if (_phase == MatchPhase.Battle && _playerContext.LocalPlayerIndex != _activePlayerIndex)
                return;

            var snapshot = _placementController.CreateLocalFieldSnapshot(_phase, _turnNumber);
            if (snapshot.ownerPlayerIndex > 0)
                HandleFieldSnapshot(NetworkManager.ServerClientId, snapshot);
        }

        private void TrySubmitSnapshotBeforeClientTimerExpires()
        {
            if (!_networkManager || _networkManager.IsServer)
                return;
            if (_timerSnapshotSubmitted || _turnRemainingSeconds > 0.25f)
                return;
            if (!_playerContext || !_playerContext.HasAssignedPlayer)
                return;
            if (_phase == MatchPhase.Battle && _playerContext.LocalPlayerIndex != _activePlayerIndex)
                return;

            _placementController?.SubmitLocalFieldSnapshot(_phase, _turnNumber);
            _timerSnapshotSubmitted = true;
        }

        private void BroadcastMatchState(MatchStatus status)
        {
            var state = new MatchStateDto
            {
                phase = _phase,
                turnNumber = _turnNumber,
                activePlayerIndex = _activePlayerIndex,
                remainingSeconds = _turnRemainingSeconds,
                status = status
            };

            OnMatchStateChanged?.Invoke(state);
            Broadcast(NetworkMessageNames.MatchState, ToJson(state));
        }

        private static bool ValidateFleetPreset(FleetPresetDto preset)
        {
            return preset.ships != null && preset.ships.Length > 0;
        }

        private bool IsHost()
        {
            return _networkManager && _networkManager.IsServer;
        }

        private bool IsActivePlayer(ulong clientId)
        {
            return _playersByClientId.TryGetValue(clientId, out var player) &&
                   player.PlayerIndex == _activePlayerIndex;
        }

        private void SendToServer(string messageName, string json)
        {
            if (!_networkManager || !_networkManager.IsClient)
                return;

            Debug.Log($"[NetworkMatchController] Sending to server: {messageName}.");
            Send(messageName, NetworkManager.ServerClientId, json);
        }

        private void Broadcast(string messageName, string json)
        {
            if (!_networkManager || !_networkManager.IsServer)
                return;

            foreach (var clientId in _networkManager.ConnectedClientsIds)
            {
                if (clientId == NetworkManager.ServerClientId)
                    continue;

                Debug.Log($"[NetworkMatchController] Broadcasting {messageName} to client {clientId}.");
                Send(messageName, clientId, json);
            }
        }

        private void SendPlayerAssignment(ulong clientId, int playerIndex)
        {
            var evt = new PlayerAssignedEventDto { playerIndex = playerIndex };

            if (clientId == NetworkManager.ServerClientId)
            {
                ApplyPlayerAssignment(evt);
                return;
            }

            Send(NetworkMessageNames.PlayerAssigned, clientId, ToJson(evt));
        }

        private void ApplyPlayerAssignment(PlayerAssignedEventDto evt)
        {
            ResolveDependencies();
            _playerContext?.SetLocalPlayerIndex(evt.playerIndex);
            OnPlayerAssigned?.Invoke(evt);
        }

        private void ResolveDependencies()
        {
            _networkManager = NetworkManager.Singleton;
            if (!_playerContext)
                _playerContext = GetComponent<NetworkPlayerContext>() ?? FindFirstObjectByType<NetworkPlayerContext>();
            if (!_placementController)
                _placementController = GetComponent<NetworkPlacementController>() ?? FindFirstObjectByType<NetworkPlacementController>();
        }

        private void Send(string messageName, ulong clientId, string json)
        {
            if (_networkManager.CustomMessagingManager == null)
            {
                Debug.LogWarning($"[NetworkMatchController] Cannot send {messageName}. CustomMessagingManager is not ready.");
                return;
            }

            var writerCapacity = Mathf.Max(
                Encoding.UTF8.GetByteCount(json),
                json.Length * sizeof(char)) + 1024;
            using var writer = new FastBufferWriter(writerCapacity, Allocator.Temp);
            writer.WriteValueSafe(json);
            _networkManager.CustomMessagingManager.SendNamedMessage(
                messageName,
                clientId,
                writer,
                NetworkDelivery.ReliableFragmentedSequenced);
        }

        private static string ToJson<T>(T payload)
        {
            return JsonUtility.ToJson(new NetworkEnvelope<T> { payload = payload });
        }

        private static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<NetworkEnvelope<T>>(json).payload;
        }
    }
}
