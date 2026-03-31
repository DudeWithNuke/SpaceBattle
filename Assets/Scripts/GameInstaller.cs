using GameBoard;
using InputController;
using PlaceableObjectManipulation;
using PlayerCamera;
using Reflex.Core;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private Selection selection;
    [SerializeField] private Moving moving;
    [SerializeField] private StateCoordinator stateCoordinator = new();
    [SerializeField] private Spawning spawning;
    [SerializeField] private SpawnPositionResolver spawnPositionResolver = new();
    [SerializeField] private ShipRoster shipRoster;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraInputController cameraInputController;
    [SerializeField] private BattlefieldInputController battlefieldInputController;
    [SerializeField] private CursorPlaneInputController cursorPlaneInputController;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(cellGrid);
        builder.RegisterValue(cursorPlane);
        builder.RegisterValue(selection);
        builder.RegisterValue(moving);
        builder.RegisterValue(stateCoordinator);
        builder.RegisterValue(spawning);
        builder.RegisterValue(spawnPositionResolver);
        builder.RegisterValue(shipRoster);
        builder.RegisterValue(cameraMovement);
        builder.RegisterValue(cameraInputController);
        builder.RegisterValue(battlefieldInputController);
        builder.RegisterValue(cursorPlaneInputController);
    }
}
