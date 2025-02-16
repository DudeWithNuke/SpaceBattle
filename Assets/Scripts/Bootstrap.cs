using GameBoard;
using PlaceableObject;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private ObjectSelection objectSelection;
    [SerializeField] private ObjectMoving objectMoving;
    
    private void Start()
    {
        cellGrid.Initialize();
        cursorPlane.Initialize();
        
        objectSelection.Initialize();
        objectMoving.Initialize();
    }
}