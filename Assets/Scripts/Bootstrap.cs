using GameBoard;
using PlaceableObject;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private CellGrid _cellGrid;
    private CursorPlane _cursorPlane;
    private ObjectSelection _objectSelection;
    private ObjectMoving _objectMoving;
    
    private void Start()
    {
        _cellGrid.Initialize();
        _cursorPlane.Initialize();
        _objectSelection.Initialize();
        _objectMoving.Initialize();
    }

    private void OnValidate()
    {
        _cellGrid = GetComponentInChildren<CellGrid>();
        _cursorPlane = GetComponentInChildren<CursorPlane>();
        _objectSelection = GetComponentInChildren<ObjectSelection>();
        _objectMoving = GetComponentInChildren<ObjectMoving>();
    }
}