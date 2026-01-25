using GameBoard;
using PlaceableObject;
using Reflex.Core;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private ObjectSelection objectSelection;
    [SerializeField] private ObjectMoving objectMoving;

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(cellGrid);
        builder.RegisterValue(cursorPlane);
        builder.RegisterValue(objectSelection);
        builder.RegisterValue(objectMoving);
    }
}
