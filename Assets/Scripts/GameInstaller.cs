using GameBoard;
using InputController;
using PlaceableObject;
using PlayerCamera;
using Reflex.Core;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private ObjectSelection objectSelection;
    [SerializeField] private ObjectMoving objectMoving;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraInputController cameraInputController;
    [SerializeField] private BattlefieldInputController battlefieldInputController;
    [SerializeField] private CursorPlaneInputController cursorPlaneInputController;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(cellGrid);
        builder.RegisterValue(cursorPlane);
        builder.RegisterValue(objectSelection);
        builder.RegisterValue(objectMoving);
        
        builder.RegisterValue(cameraMovement);
        builder.RegisterValue(cameraInputController);
        builder.RegisterValue(battlefieldInputController);
        builder.RegisterValue(cursorPlaneInputController);
    }
}
