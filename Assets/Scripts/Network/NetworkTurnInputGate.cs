using PlaceableObjectManipulation;
using UnityEngine;

namespace Network
{
    public sealed class NetworkTurnInputGate : MonoBehaviour
    {
        [SerializeField] private NetworkMatchController matchController;
        [SerializeField] private NetworkPlayerContext playerContext;
        [SerializeField] private Selection selection;
        [SerializeField] private FleetPanelController fleetPanelController;

        private bool _hasMatchState;
        private int _activePlayerIndex;

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
            _hasMatchState = state.status is "Started" or "Submitted" or "TimerExpired";
            _activePlayerIndex = state.activePlayerIndex;

            if (state.status == "TimerExpired" &&
                playerContext &&
                playerContext.LocalPlayerIndex == previousActivePlayerIndex &&
                previousActivePlayerIndex != state.activePlayerIndex)
                selection?.ForceDestroyCurrentPickedObject();

            ApplyInteractionState();
        }

        private void HandleLocalPlayerAssigned(int _)
        {
            ApplyInteractionState();
        }

        private void ApplyInteractionState()
        {
            var canInteract = _hasMatchState &&
                              playerContext &&
                              playerContext.HasAssignedPlayer &&
                              playerContext.LocalPlayerIndex == _activePlayerIndex;

            selection?.SetInteractionEnabled(canInteract);
            fleetPanelController?.SetTurnInteractionEnabled(canInteract);
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
        }
    }
}
