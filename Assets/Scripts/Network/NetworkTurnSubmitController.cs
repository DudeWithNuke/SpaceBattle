using UnityEngine;
using PlaceableObjectManipulation;

namespace Network
{
    public sealed class NetworkTurnSubmitController : MonoBehaviour
    {
        [SerializeField] private NetworkMatchController matchController;
        [SerializeField] private NetworkPlacementController placementController;
        [SerializeField] private Selection selection;
        [SerializeField] private KeyCode debugSubmitKey = KeyCode.Return;

        private void Awake()
        {
            ResolveDependencies();
        }

        private void Update()
        {
            if (debugSubmitKey == KeyCode.None)
                return;
            if (!Input.GetKeyUp(debugSubmitKey))
                return;

            SubmitTurn();
        }

        public void SubmitTurn()
        {
            ResolveDependencies();

            if (!matchController || matchController.CurrentPhase != MatchPhase.Battle)
                return;

            if (selection && selection.CurrentPickedObject)
            {
                Debug.LogWarning("[NetworkTurnSubmitController] Submit rejected. A picked object must be placed or cancelled first.");
                return;
            }

            if (placementController)
                placementController.SubmitLocalFieldSnapshot(matchController.CurrentPhase, matchController.CurrentTurnNumber);

            matchController.RequestEndTurn();
        }

        private void ResolveDependencies()
        {
            if (!matchController)
                matchController = GetComponent<NetworkMatchController>() ?? FindFirstObjectByType<NetworkMatchController>();
            if (!placementController)
                placementController = GetComponent<NetworkPlacementController>() ?? FindFirstObjectByType<NetworkPlacementController>();
            if (!selection)
                selection = FindFirstObjectByType<Selection>();
        }
    }
}
