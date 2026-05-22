using PlaceableObjectManipulation;
using PlayerCamera;
using UnityEngine;

namespace Network
{
    public sealed class NetworkTurnInputGate : MonoBehaviour
    {
        [SerializeField] private NetworkMatchController matchController;
        [SerializeField] private NetworkPlayerContext playerContext;
        [SerializeField] private Selection selection;
        [SerializeField] private FleetPanelController fleetPanelController;
        [SerializeField] private CameraMovement cameraMovement;

        private bool _hasMatchState;
        private int _activePlayerIndex;
        private MatchPhase _phase;

        private void Awake()
        {
            ResolveDependencies();
            ApplyInteractionState();
        }

        private void OnEnable()
        {
            ResolveDependencies();

            if (matchController)
                matchController.OnMatchStateChanged += HandleMatchStateChanged;
            if (playerContext)
                playerContext.OnLocalPlayerAssigned += HandleLocalPlayerAssigned;

            ApplyInteractionState();
        }

        private void OnDisable()
        {
            if (matchController)
                matchController.OnMatchStateChanged -= HandleMatchStateChanged;
            if (playerContext)
                playerContext.OnLocalPlayerAssigned -= HandleLocalPlayerAssigned;
        }

        private void HandleMatchStateChanged(MatchStateDto state)
        {
            var previousActivePlayerIndex = _activePlayerIndex;
            var previousPhase = _phase;
            _hasMatchState = state.phase is MatchPhase.Deployment or MatchPhase.Battle;
            _phase = state.phase;
            _activePlayerIndex = state.activePlayerIndex;

            if (ShouldForceReturnPickedObject(state, previousPhase, previousActivePlayerIndex))
                selection?.ForceDestroyCurrentPickedObject();

            ApplyInteractionState();
        }

        private void HandleLocalPlayerAssigned(int _)
        {
            ApplyInteractionState();
        }

        private void ApplyInteractionState()
        {
            var isDeploymentPhase = _phase == MatchPhase.Deployment;
            var isBattlePhase = _phase == MatchPhase.Battle;
            var canInteract = _hasMatchState &&
                              playerContext &&
                              playerContext.HasAssignedPlayer &&
                              (isDeploymentPhase || playerContext.LocalPlayerIndex == _activePlayerIndex);

            selection?.SetInteractionEnabled(canInteract);
            selection?.SetCanPickPlacedObjects(isDeploymentPhase);
            selection?.SetCanPickPlacedAbilities(isBattlePhase);
            fleetPanelController?.SetTurnInteractionEnabled(canInteract);
            fleetPanelController?.SetBattlePhase(isBattlePhase);
            cameraMovement?.SetBattlefieldSwitchEnabled(isBattlePhase);
        }

        private bool ShouldForceReturnPickedObject(MatchStateDto state, MatchPhase previousPhase, int previousActivePlayerIndex)
        {
            if (!playerContext)
                return false;

            if (previousPhase == MatchPhase.Deployment &&
                state.phase == MatchPhase.Battle)
                return true;

            return state.status == MatchStatus.TimerExpired &&
                   playerContext.LocalPlayerIndex == previousActivePlayerIndex &&
                   previousActivePlayerIndex != state.activePlayerIndex;
        }

        private void ResolveDependencies()
        {
            if (!matchController)
                matchController = GetComponent<NetworkMatchController>() ?? FindFirstObjectByType<NetworkMatchController>();
            if (!playerContext)
                playerContext = GetComponent<NetworkPlayerContext>() ?? FindFirstObjectByType<NetworkPlayerContext>();
            if (!selection)
                selection = FindFirstObjectByType<Selection>();
            if (!fleetPanelController)
                fleetPanelController = FindFirstObjectByType<FleetPanelController>();
            if (!cameraMovement)
                cameraMovement = FindFirstObjectByType<CameraMovement>();
        }
    }
}
