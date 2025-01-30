using GameBoard;
using ModestTree;
using PlaceableObject;
using UnityEngine;
using UnityEngine.Serialization;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private Vector3Int gridSize;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private ObjectMoving objectMoving;
    
    private void Start()
    {
        cellGrid.Initialize();
        cursorPlane.Initialize(cellGrid);
        objectMoving.Initialize(cursorPlane);
    }
}