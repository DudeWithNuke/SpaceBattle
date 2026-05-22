using System;
using System.Collections;
using System.Text;
using GameBoard;
using Network.Match;
using Network.Placement;
using Network.Player;
using Network.Turn;
using PlaceableObjectManipulation;
using PlayerCamera;
using Reflex.Attributes;
using UI.FleetPanel;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Network
{
    public sealed class NetworkRuntime : MonoBehaviour
    {
        public event Action<PlayerAssignedEventDto> OnPlayerAssigned;
        public event Action<MatchStateDto> OnMatchStateChanged;
        public event Action<FieldSnapshotDto> OnFieldSnapshotReceived;

        private readonly MatchSession _session = new();
        private readonly PrefabRegistry _prefabRegistry = new();

        private NetworkManager _networkManager;
        private PlacementSynchronizer _placementSynchronizer;
        private TurnInputGateService _turnInputGateService;
        private TurnSubmitService _turnSubmitService;

        [Inject] private PlayerContext _playerContext;
        [Inject] private Spawning _spawning;
        [Inject] private SpawnedObjectLifecycleTracker _lifecycleTracker;
        [Inject] private CellGrid _cellGrid;
        [Inject] private ShipRoster _shipRoster;
        [Inject] private Selection _selection;
        [Inject] private FleetPanelController _fleetPanelController;
        [Inject] private CameraMovement _cameraMovement;

        [SerializeField] private bool rotateOpponentFieldCoordinates;
        [SerializeField] private bool rotateOpponentObjects;
        [SerializeField] private KeyCode debugSubmitKey = KeyCode.Return;

        private bool _callbacksRegistered;
        private bool _messageHandlersRegistered;
        private bool _timerSnapshotSubmitted;

        public MatchPhase CurrentPhase => _session.CurrentPhase;
        public int CurrentTurnNumber => _session.CurrentTurnNumber;

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
            HandleDebugSubmitKey();

            if (!_networkManager || !_session.IsMatchStarted)
                return;

            var timerExpired = _session.AdvanceTime(Time.deltaTime);
            TrySubmitSnapshotBeforeClientTimerExpires();

            if (!_networkManager.IsServer || !timerExpired)
                return;

            CaptureLocalSnapshotForTimer();
            HandleSessionResult(_session.CompleteExpiredTurn());
        }

        public void Configure(NetworkManager networkManager)
        {
            UnregisterCallbacks();

            _networkManager = networkManager;

            _prefabRegistry.Initialize(_shipRoster);
            _placementSynchronizer = new PlacementSynchronizer(_playerContext, _prefabRegistry);
            _turnInputGateService = new TurnInputGateService(_playerContext);
            _turnSubmitService = new TurnSubmitService(
                () => CurrentPhase,
                () => CurrentTurnNumber,
                () => _selection && _selection.CurrentPickedObject,
                SubmitLocalFieldSnapshot,
                RequestEndTurn);

            _playerContext.OnLocalPlayerAssigned -= HandleLocalPlayerAssigned;
            _playerContext.OnLocalPlayerAssigned += HandleLocalPlayerAssigned;

            RegisterCallbacks();
            ApplyInteractionState();
        }

        public void InitializeAfterNetworkStart()
        {
            RegisterCallbacks();

            if (_networkManager && _networkManager.IsServer)
                EnsureServerPlayerRegistered(NetworkManager.ServerClientId);
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
            var request = new EndTurnRequestDto { turnNumber = _session.CurrentTurnNumber };
            if (IsHost())
            {
                HandleEndTurnRequest(NetworkManager.ServerClientId);
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

        public void SubmitTurn()
        {
            _turnSubmitService?.SubmitTurn();
        }

        private void HandleDebugSubmitKey()
        {
            if (debugSubmitKey == KeyCode.None)
                return;
            if (!Input.GetKeyUp(debugSubmitKey))
                return;

            SubmitTurn();
        }

        private void RegisterCallbacks()
        {
            if (!_networkManager)
                _networkManager = NetworkManager.Singleton;
            if (!_networkManager)
                return;

            if (!_callbacksRegistered)
            {
                _networkManager.OnClientConnectedCallback += HandleClientConnected;
                _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
                _callbacksRegistered = true;
            }

            if (_networkManager.CustomMessagingManager == null || _messageHandlersRegistered)
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
            Log.Info("[NetworkRuntime] Network message handlers registered.");
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
            if (!_session.CanAcceptPlayer(clientId))
            {
                Log.Warn($"[NetworkRuntime] Rejecting extra client {clientId}. Match already has two players.");
                _networkManager.DisconnectClient(clientId);
                return;
            }

            HandleSessionResult(
                _session.RegisterPlayer(clientId, clientId == NetworkManager.ServerClientId),
                clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (!_networkManager.IsServer)
                return;

            if (_session.RemovePlayer(clientId))
                Log.Info($"[NetworkRuntime] Client {clientId} removed from match.");
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

            reader.ReadValueSafe(out string _);
            HandleEndTurnRequest(senderClientId);
        }

        private void ReceiveMatchState(ulong senderClientId, FastBufferReader reader)
        {
            if (_networkManager.IsServer)
                return;

            reader.ReadValueSafe(out string json);
            var state = FromJson<MatchStateDto>(json);
            _session.ApplyRemoteMatchState(state);
            _timerSnapshotSubmitted = false;
            OnMatchStateChanged?.Invoke(state);
            ApplyInteractionState(_turnInputGateService.ApplyMatchState(state));
            Log.Info($"[NetworkRuntime] Match state received. Status={state.status}, Turn={state.turnNumber}, ActivePlayer={state.activePlayerIndex}.");
        }

        private void ReceivePlayerAssigned(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out string json);
            ApplyPlayerAssignment(FromJson<PlayerAssignedEventDto>(json));
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
            HandleFieldSnapshotReceived(FromJson<FieldSnapshotDto>(json));
        }

        private void HandleFleetPreset(ulong clientId, FleetPresetDto preset)
        {
            HandleSessionResult(_session.SubmitFleetPreset(clientId, preset));
        }

        private void HandleEndTurnRequest(ulong clientId)
        {
            if (!_session.CanSubmitEndTurn(clientId))
                return;

            CaptureLocalSnapshotForTimer();
            HandleSessionResult(_session.CompleteSubmittedTurn());
        }

        private void HandleFieldSnapshot(ulong clientId, FieldSnapshotDto snapshot)
        {
            _session.SubmitFieldSnapshot(clientId, snapshot);
        }

        private FieldSnapshotDto CreateLocalFieldSnapshot(MatchPhase phase, int turnNumber)
        {
            return _placementSynchronizer.CreateLocalFieldSnapshot(_lifecycleTracker.TrackedObjects, phase, turnNumber);
        }

        private void SubmitLocalFieldSnapshot(MatchPhase phase, int turnNumber)
        {
            var snapshot = CreateLocalFieldSnapshot(phase, turnNumber);
            if (snapshot.ownerPlayerIndex <= 0)
                return;

            Log.Info($"[NetworkRuntime] Submitting field snapshot. Player{snapshot.ownerPlayerIndex}, Objects={snapshot.objects?.Length ?? 0}.");
            SubmitFieldSnapshot(snapshot);
        }

        private void HandleFieldSnapshotReceived(FieldSnapshotDto snapshot)
        {
            if (snapshot.objects == null)
                return;

            DestroyStaleSnapshotObjects(snapshot);
            foreach (var fieldObject in snapshot.objects)
                HandleFieldObjectState(fieldObject);

            OnFieldSnapshotReceived?.Invoke(snapshot);
        }

        private void HandleFieldObjectState(FieldObjectPlacedEventDto evt)
        {
            if (!_placementSynchronizer.TryCreateRemotePlacement(evt, out var placement))
                return;

            Log.Info($"[NetworkRuntime] Applying field object snapshot: objectOwner=Player{evt.objectOwnerPlayerIndex}, fieldOwner=Player{evt.fieldOwnerPlayerIndex}, prefab={evt.prefabId}, cell={evt.originCell}.");
            StartCoroutine(ReplaceConfirmedObject(placement));
        }

        private IEnumerator ReplaceConfirmedObject(NetworkRemotePlacement placement)
        {
            if (placement.ExistingObject)
            {
                Destroy(placement.ExistingObject.gameObject);
                yield return null;
            }

            yield return SpawnConfirmedObject(placement.Event, placement.Prefab);
        }

        private IEnumerator SpawnConfirmedObject(
            FieldObjectPlacedEventDto evt,
            PlaceableObject.PlaceableObject prefab)
        {
            var isLocalPlayerObject = _playerContext.IsLocalPlayer(evt.objectOwnerPlayerIndex);
            var localCell = _playerContext.ToLocalViewCell(
                evt.fieldOwnerPlayerIndex,
                evt.originCell,
                _cellGrid.GridSize,
                rotateOpponentFieldCoordinates);
            var isPlayerObject = _playerContext.IsPlayerObject(evt.fieldOwnerPlayerIndex);
            var instance = _spawning.SpawnAtCell(prefab, localCell, isPlayerObject);
            if (!instance)
                yield break;

            instance.SetCellStateVisualizationEnabled(isLocalPlayerObject);

            if (!isLocalPlayerObject && rotateOpponentObjects)
                instance.ApplyLocalViewRotation180();

            instance.CurrentPosition = localCell;
            instance.SetNetworkIdentity(evt.objectId, evt.prefabId, evt.objectOwnerPlayerIndex, true);
            instance.SetNetworkFieldOwner(evt.fieldOwnerPlayerIndex);
            _placementSynchronizer.RegisterRemoteObject(evt.objectId, instance);
            _lifecycleTracker.Register(instance);

            yield return null;

            instance.ApplyConfirmedPlacement(localCell);
        }

        private void DestroyStaleSnapshotObjects(FieldSnapshotDto snapshot)
        {
            foreach (var staleObject in _placementSynchronizer.GetStaleObjects(snapshot))
                Destroy(staleObject.gameObject);
        }

        private void FlushPendingPlacements()
        {
            foreach (var snapshot in _session.FlushPendingSnapshots())
            {
                HandleFieldSnapshotReceived(snapshot);
                Broadcast(NetworkMessageNames.FieldSnapshot, ToJson(snapshot));
            }
        }

        private void CaptureLocalSnapshotForTimer()
        {
            if (_placementSynchronizer == null || _playerContext == null || !_playerContext.HasAssignedPlayer)
                return;
            if (_session.CurrentPhase == MatchPhase.Battle &&
                _playerContext.LocalPlayerIndex != _session.ActivePlayerIndex)
                return;

            var snapshot = CreateLocalFieldSnapshot(_session.CurrentPhase, _session.CurrentTurnNumber);
            if (snapshot.ownerPlayerIndex > 0)
                HandleFieldSnapshot(NetworkManager.ServerClientId, snapshot);
        }

        private void TrySubmitSnapshotBeforeClientTimerExpires()
        {
            if (!_networkManager || _networkManager.IsServer)
                return;
            if (_timerSnapshotSubmitted || _session.TurnRemainingSeconds > 0.25f)
                return;
            if (_playerContext == null || !_playerContext.HasAssignedPlayer)
                return;
            if (_session.CurrentPhase == MatchPhase.Battle &&
                _playerContext.LocalPlayerIndex != _session.ActivePlayerIndex)
                return;

            SubmitLocalFieldSnapshot(_session.CurrentPhase, _session.CurrentTurnNumber);
            _timerSnapshotSubmitted = true;
        }

        private void HandleSessionResult(MatchSessionResult result, ulong targetClientId = 0)
        {
            while (true)
            {
                switch (result.Action)
                {
                    case MatchSessionAction.PlayerAssigned:
                        SendPlayerAssignment(targetClientId, result.PlayerAssigned);
                        result = _session.TryStartMatch();
                        targetClientId = 0;
                        continue;
                    case MatchSessionAction.MatchStateChanged:
                        FlushPendingPlacements();
                        _timerSnapshotSubmitted = false;
                        OnMatchStateChanged?.Invoke(result.MatchState);
                        ApplyInteractionState(_turnInputGateService.ApplyMatchState(result.MatchState));
                        Broadcast(NetworkMessageNames.MatchState, ToJson(result.MatchState));
                        break;
                    case MatchSessionAction.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            }
        }

        private void SendPlayerAssignment(ulong clientId, PlayerAssignedEventDto evt)
        {
            if (clientId == NetworkManager.ServerClientId)
            {
                ApplyPlayerAssignment(evt);
                return;
            }

            Send(NetworkMessageNames.PlayerAssigned, clientId, ToJson(evt));
        }

        private void ApplyPlayerAssignment(PlayerAssignedEventDto evt)
        {
            _playerContext.SetLocalPlayerIndex(evt.playerIndex);
            OnPlayerAssigned?.Invoke(evt);
        }

        private void HandleLocalPlayerAssigned(int _)
        {
            ApplyInteractionState();
        }

        private void ApplyInteractionState()
        {
            if (_turnInputGateService == null)
                return;

            ApplyInteractionState(_turnInputGateService.CreateCurrentState());
        }

        private void ApplyInteractionState(NetworkTurnInteractionState state)
        {
            if (state.ShouldForceReturnPickedObject)
                _selection?.ForceDestroyCurrentPickedObject();

            _selection?.SetInteractionEnabled(state.CanInteract);
            _selection?.SetCanPickPlacedObjects(state.IsDeploymentPhase);
            _selection?.SetCanPickPlacedAbilities(state.IsBattlePhase);
            _fleetPanelController?.SetTurnInteractionEnabled(state.CanInteract);
            _fleetPanelController?.SetBattlePhase(state.IsBattlePhase);
            _cameraMovement?.SetBattlefieldSwitchEnabled(state.IsBattlePhase);
        }

        private bool IsHost()
        {
            return _networkManager && _networkManager.IsServer;
        }

        private void SendToServer(string messageName, string json)
        {
            if (!_networkManager || !_networkManager.IsClient)
                return;

            Log.Info($"[NetworkRuntime] Sending to server: {messageName}.");
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

                Log.Info($"[NetworkRuntime] Broadcasting {messageName} to client {clientId}.");
                Send(messageName, clientId, json);
            }
        }

        private void Send(string messageName, ulong clientId, string json)
        {
            if (_networkManager.CustomMessagingManager == null)
            {
                Log.Warn($"[NetworkRuntime] Cannot send {messageName}. CustomMessagingManager is not ready.");
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
