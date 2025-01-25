using GameBoard;
using ModestTree;
using Selector;
using UnityEngine;
using UnityEngine.Serialization;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private Vector3Int gridSize;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private SelectorManager selectorManager;
    
    private void Start()
    {
        cellGrid.Initialize();
        cursorPlane.Initialize(cellGrid);
        selectorManager.Initialize(cellGrid, cursorPlane);
    }
}