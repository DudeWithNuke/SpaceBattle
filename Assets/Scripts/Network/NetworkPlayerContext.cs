using PlaceableObject;
using UnityEngine;

namespace Network
{
    public sealed class NetworkPlayerContext : MonoBehaviour
    {
        public event System.Action<int> OnLocalPlayerAssigned;

        public int LocalPlayerIndex { get; private set; }
        public bool HasAssignedPlayer => LocalPlayerIndex > 0;

        public void SetLocalPlayerIndex(int playerIndex)
        {
            if (LocalPlayerIndex == playerIndex)
                return;

            LocalPlayerIndex = playerIndex;
            Debug.Log($"[NetworkPlayerContext] Local player assigned: Player{playerIndex}.");
            OnLocalPlayerAssigned?.Invoke(playerIndex);
        }

        public bool IsLocalPlayer(int ownerPlayerIndex)
        {
            return HasAssignedPlayer && ownerPlayerIndex == LocalPlayerIndex;
        }

        public PlaceableObjectDeploymentSide GetLocalSide(int ownerPlayerIndex)
        {
            return IsLocalPlayer(ownerPlayerIndex)
                ? PlaceableObjectDeploymentSide.OwnField
                : PlaceableObjectDeploymentSide.EnemyField;
        }

        public bool IsPlayerObject(int ownerPlayerIndex)
        {
            return GetLocalSide(ownerPlayerIndex) == PlaceableObjectDeploymentSide.OwnField;
        }

        public Vector3Int ToLocalViewCell(int ownerPlayerIndex, Vector3Int authoritativeCell, Vector3Int gridSize, bool rotateOpponentField)
        {
            if (IsLocalPlayer(ownerPlayerIndex) || !rotateOpponentField)
                return authoritativeCell;

            return new Vector3Int(
                gridSize.x - 1 - authoritativeCell.x,
                authoritativeCell.y,
                gridSize.z - 1 - authoritativeCell.z);
        }
    }
}
