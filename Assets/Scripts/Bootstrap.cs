using GameBoard;
using PlaceableObject;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;

    private void Start()
    {
        cellGrid.Initialize();
        cursorPlane.Initialize();
    }
}