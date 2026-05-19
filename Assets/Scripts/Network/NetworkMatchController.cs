using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public sealed class NetworkMatchController : MonoBehaviour
    {
        private const int MaxPlayers = 2;
        private const float TurnDurationSeconds = 30f;

        public event System.Action<PlayerAssignedEventDto> OnPlayerAssigned;
        public event System.Action<MatchStateDto> OnMatchStateChanged;
        public event System.Action<ShipPlacedEventDto> OnShipPlaced;
        public event System.Action<AbilityPlacedEventDto> OnAbilityPlaced;

        private readonly Dictionary<ulong, PlayerConnectionInfo> _playersByClientId = new();
        private readonly Dictionary<int, HashSet<Vector3Int>> _occupiedShipCellsByPlayer = new();
        private readonly List<ShipPlacedEventDto> _pendingShipPlacements = new();
        private readonly List<AbilityPlacedEventDto> _pendingAbilityPlacements = new();
        private int _turnNumber;
        private int _activePlayerIndex;
        private float _turnRemainingSeconds;
        private bool _matchStarted;
        private bool _hasPlacedAbility;
        private bool _callbacksRegistered;
        private bool _messageHandlersRegistered;

        private NetworkManager _networkManager;
        private NetworkPlayerContext _playerContext;

        private void Awake()
        {
            _networkManager = NetworkManager.Singleton;
            _playerContext = GetComponent<NetworkPlayerContext>() ?? FindFirstObjectByType<NetworkPlayerContext>();
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
            if (!_networkManager || !_networkManager.IsServer || !_matchStarted)
                return;

            _turnRemainingSeconds -= Time.deltaTime;
            if (_turnRemainingSeconds > 0f)
                return;

            CompleteActiveTurn("TimerExpired");
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

        public void RequestUseAbility(UseAbilityRequestDto request)
        {
            if (IsHost())
            {
                HandleUseAbilityRequest(NetworkManager.ServerClientId, request);
                return;
            }

            SendToServer(NetworkMessageNames.UseAbilityRequest, ToJson(request));
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

        public void InitializeAfterNetworkStart()
        {
            RegisterCallbacks();

            if (_networkManager && _networkManager.IsServer)
                EnsureServerPlayerRegistered(NetworkManager.ServerClientId);
        }

        public void SubmitShipPlacement(PlaceShipRequestDto request)
        {
            if (IsHost())
            {
                HandlePlaceShipRequest(NetworkManager.ServerClientId, request);
                return;
            }

            SendToServer(NetworkMessageNames.PlaceShipRequest, ToJson(request));
        }

        public void SubmitAbilityPlacement(PlaceAbilityRequestDto request)
        {
            if (IsHost())
            {
                HandlePlaceAbilityRequest(NetworkManager.ServerClientId, request);
                return;
            }

            SendToServer(NetworkMessageNames.PlaceAbilityRequest, ToJson(request));
        }

        private void RegisterCallbacks()
        {
            _networkManager = NetworkManager.Singleton;
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
                NetworkMessageNames.UseAbilityRequest,
                ReceiveUseAbilityRequest);
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
                NetworkMessageNames.PlaceShipRequest,
                ReceivePlaceShipRequest);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.ShipPlaced,
                ReceiveShipPlaced);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.PlaceAbilityRequest,
                ReceivePlaceAbilityRequest);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(
                NetworkMessageNames.AbilityPlaced,
                ReceiveAbilityPlaced);

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
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.UseAbilityRequest);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.EndTurnRequest);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.MatchState);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.PlayerAssigned);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.PlaceShipRequest);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.ShipPlaced);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.PlaceAbilityRequest);
            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NetworkMessageNames.AbilityPlaced);
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

        private void ReceiveUseAbilityRequest(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            HandleUseAbilityRequest(senderClientId, FromJson<UseAbilityRequestDto>(json));
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
            _turnNumber = state.turnNumber;
            _activePlayerIndex = state.activePlayerIndex;
            _turnRemainingSeconds = state.remainingSeconds;
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

        private void ReceivePlaceShipRequest(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            HandlePlaceShipRequest(senderClientId, FromJson<PlaceShipRequestDto>(json));
        }

        private void ReceiveShipPlaced(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string json);
            OnShipPlaced?.Invoke(FromJson<ShipPlacedEventDto>(json));
        }

        private void ReceivePlaceAbilityRequest(ulong senderClientId, FastBufferReader reader)
        {
            if (!_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            HandlePlaceAbilityRequest(senderClientId, FromJson<PlaceAbilityRequestDto>(json));
        }

        private void ReceiveAbilityPlaced(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string json);
            OnAbilityPlaced?.Invoke(FromJson<AbilityPlacedEventDto>(json));
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

        private void HandleUseAbilityRequest(ulong clientId, UseAbilityRequestDto request)
        {
            if (!_matchStarted)
            {
                Debug.LogWarning("[NetworkMatchController] Ability request rejected. Match is not started.");
                return;
            }

            if (!IsActivePlayer(clientId))
            {
                Debug.LogWarning($"[NetworkMatchController] Ability request rejected. Client {clientId} is not active player.");
                return;
            }

            Debug.Log($"[NetworkMatchController] Ability request accepted. Ship={request.sourceShipId}, Ability={request.abilityId}, Target={request.targetCell}.");
            BroadcastMatchState("AbilityResolved");
        }

        private void HandleEndTurnRequest(ulong clientId, EndTurnRequestDto request)
        {
            if (!_matchStarted)
            {
                Debug.LogWarning("[NetworkMatchController] End turn rejected. Match is not started.");
                return;
            }

            if (!IsActivePlayer(clientId))
            {
                Debug.LogWarning($"[NetworkMatchController] End turn rejected. Client {clientId} is not active player.");
                return;
            }

            CompleteActiveTurn("Submitted");
        }

        private void HandlePlaceShipRequest(ulong clientId, PlaceShipRequestDto request)
        {
            if (!_playersByClientId.TryGetValue(clientId, out var player))
            {
                Debug.LogWarning($"[NetworkMatchController] Ship placement rejected. Unknown client {clientId}.");
                return;
            }

            if (!IsActivePlayer(clientId))
            {
                Debug.LogWarning($"[NetworkMatchController] Ship placement rejected. Client {clientId} is not active player.");
                return;
            }

            if (!ValidateShipPlacement(player.PlayerIndex, request))
            {
                Debug.LogWarning($"[NetworkMatchController] Ship placement rejected for Player{player.PlayerIndex}.");
                return;
            }

            ReserveShipCells(player.PlayerIndex, request.occupiedCells);

            var evt = new ShipPlacedEventDto
            {
                ownerPlayerIndex = player.PlayerIndex,
                objectId = request.objectId,
                prefabId = request.prefabId,
                originCell = request.originCell,
                occupiedCells = request.occupiedCells
            };

            Debug.Log($"[NetworkMatchController] Ship placed by Player{player.PlayerIndex}: {request.prefabId} at {request.originCell}.");
            _pendingShipPlacements.Add(evt);
        }

        private void HandlePlaceAbilityRequest(ulong clientId, PlaceAbilityRequestDto request)
        {
            if (!_playersByClientId.TryGetValue(clientId, out var player))
            {
                Debug.LogWarning($"[NetworkMatchController] Ability placement rejected. Unknown client {clientId}.");
                return;
            }

            if (!IsActivePlayer(clientId))
            {
                Debug.LogWarning($"[NetworkMatchController] Ability placement rejected. Client {clientId} is not active player.");
                return;
            }

            if (_hasPlacedAbility)
            {
                Debug.LogWarning("[NetworkMatchController] Ability placement rejected. Ability is already placed.");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.prefabId))
            {
                Debug.LogWarning($"[NetworkMatchController] Ability placement rejected for Player{player.PlayerIndex}. Missing prefab id.");
                return;
            }

            _hasPlacedAbility = true;

            var evt = new AbilityPlacedEventDto
            {
                ownerPlayerIndex = player.PlayerIndex,
                objectId = request.objectId,
                sourceShipId = request.sourceShipId,
                abilityId = request.abilityId,
                prefabId = request.prefabId,
                targetOwnerPlayerIndex = request.targetOwnerPlayerIndex,
                targetCell = request.targetCell
            };

            Debug.Log($"[NetworkMatchController] Ability placed by Player{player.PlayerIndex}: {request.prefabId} at {request.targetCell}.");
            _pendingAbilityPlacements.Add(evt);
        }

        private void TryStartMatch()
        {
            if (_matchStarted)
                return;
            if (_playersByClientId.Count != MaxPlayers)
                return;

            _matchStarted = true;
            _turnNumber = 1;
            _activePlayerIndex = Random.Range(1, MaxPlayers + 1);
            _turnRemainingSeconds = TurnDurationSeconds;
            Debug.Log($"[NetworkMatchController] Match started. Active player: Player{_activePlayerIndex}.");
            BroadcastMatchState("Started");
        }

        private void CompleteActiveTurn(string reason)
        {
            FlushPendingPlacements();
            _turnNumber++;
            _activePlayerIndex = _activePlayerIndex == 1 ? 2 : 1;
            _turnRemainingSeconds = TurnDurationSeconds;
            Debug.Log($"[NetworkMatchController] Turn completed by {reason}. Active player: Player{_activePlayerIndex}. Turn={_turnNumber}.");
            BroadcastMatchState(reason);
        }

        private void FlushPendingPlacements()
        {
            foreach (var evt in _pendingShipPlacements)
            {
                OnShipPlaced?.Invoke(evt);
                Broadcast(NetworkMessageNames.ShipPlaced, ToJson(evt));
            }

            foreach (var evt in _pendingAbilityPlacements)
            {
                OnAbilityPlaced?.Invoke(evt);
                Broadcast(NetworkMessageNames.AbilityPlaced, ToJson(evt));
            }

            _pendingShipPlacements.Clear();
            _pendingAbilityPlacements.Clear();
        }

        private void BroadcastMatchState(string status)
        {
            var state = new MatchStateDto
            {
                turnNumber = _turnNumber,
                activePlayerIndex = _activePlayerIndex,
                remainingSeconds = _turnRemainingSeconds,
                status = status
            };

            OnMatchStateChanged?.Invoke(state);
            Broadcast(NetworkMessageNames.MatchState, ToJson(state));
        }

        private bool IsActivePlayer(ulong clientId)
        {
            return _playersByClientId.TryGetValue(clientId, out var player) &&
                   player.PlayerIndex == _activePlayerIndex;
        }

        private static bool ValidateFleetPreset(FleetPresetDto preset)
        {
            return preset.ships != null && preset.ships.Length > 0;
        }

        private bool ValidateShipPlacement(int playerIndex, PlaceShipRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.prefabId))
                return false;
            if (request.occupiedCells == null || request.occupiedCells.Length == 0)
                return false;

            var occupiedCells = GetOccupiedShipCells(playerIndex);
            foreach (var cell in request.occupiedCells)
            {
                if (occupiedCells.Contains(cell))
                    return false;
            }

            return true;
        }

        private void ReserveShipCells(int playerIndex, IReadOnlyList<Vector3Int> occupiedCells)
        {
            var reservedCells = GetOccupiedShipCells(playerIndex);
            foreach (var cell in occupiedCells)
                reservedCells.Add(cell);
        }

        private HashSet<Vector3Int> GetOccupiedShipCells(int playerIndex)
        {
            if (_occupiedShipCellsByPlayer.TryGetValue(playerIndex, out var occupiedCells))
                return occupiedCells;

            occupiedCells = new HashSet<Vector3Int>();
            _occupiedShipCellsByPlayer[playerIndex] = occupiedCells;
            return occupiedCells;
        }

        private bool IsHost()
        {
            return _networkManager && _networkManager.IsServer;
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
            _playerContext ??= GetComponent<NetworkPlayerContext>() ?? FindFirstObjectByType<NetworkPlayerContext>();
            _playerContext?.SetLocalPlayerIndex(evt.playerIndex);
            OnPlayerAssigned?.Invoke(evt);
        }

        private void Send(string messageName, ulong clientId, string json)
        {
            if (_networkManager.CustomMessagingManager == null)
            {
                Debug.LogWarning($"[NetworkMatchController] Cannot send {messageName}. CustomMessagingManager is not ready.");
                return;
            }

            using var writer = new FastBufferWriter(4096, Allocator.Temp);
            writer.WriteValueSafe(json);
            _networkManager.CustomMessagingManager.SendNamedMessage(messageName, clientId, writer);
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
