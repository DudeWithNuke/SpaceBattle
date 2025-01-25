using GameBoard;
using UnityEngine;

namespace Selector
{
    public class SelectorManager : MonoBehaviour
    {
        [SerializeField] private PlaceableObject.PlaceableObject placeableObject;

        private LayerSelector _layerSelector;
        private ObjectMoving _objectMoving;
        
        private CellSelector _cellSelector;

        public void Initialize(CellGrid cellGrid, CursorPlane cursorPlane)
        {
            enabled = false;
            
            _layerSelector = new LayerSelector(cellGrid.outputCells, cursorPlane);
            _objectMoving = new ObjectMoving(cellGrid.outputCells, cursorPlane, placeableObject);
            //_cellSelector = new CellSelector(cellGrid.outputCells, cursorPlane, placeableObject);
            
            enabled = true;
        }

        private void Update()
        {
            _layerSelector.Run();
            _objectMoving.Run();
            //_cellSelector.Run();
        }
    }
}