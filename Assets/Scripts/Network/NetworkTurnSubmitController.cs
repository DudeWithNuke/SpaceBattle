using UnityEngine;
using PlaceableObjectManipulation;

namespace Network
{
    public sealed class NetworkTurnSubmitController : MonoBehaviour
    {
        [SerializeField] private NetworkMatchController matchController;
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

            if (selection && selection.CurrentPickedObject)
            {
                Debug.LogWarning("[NetworkTurnSubmitController] Submit rejected. A picked object must be placed or cancelled first.");
                return;
            }

            matchController?.RequestEndTurn();
        }

        private void ResolveDependencies()
        {
            if (!matchController)
                matchController = GetComponent<NetworkMatchController>() ?? FindFirstObjectByType<NetworkMatchController>();
            if (!selection)
                selection = FindFirstObjectByType<Selection>();
        }
    }
}
