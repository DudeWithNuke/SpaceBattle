using System;
using System.Collections.Generic;
using GameBoard;
using UnityEngine;

namespace Selector
{
    public class LayerSelector
    {
        private readonly List<Cell> _cells;
        private readonly CursorPlane _cursorPlane;
        public LayerSelector(List<Cell> cells, CursorPlane cursorPlane)
        {
           _cells = cells;
           _cursorPlane = cursorPlane;
        }
        
        public void Run()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _cursorPlane.Up();
                UpdateCellsState();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _cursorPlane.Down();
                UpdateCellsState();
            }
        }
        
        private void UpdateCellsState()
        {
            foreach (var cell in _cells)
            {
                var posY = (int)cell.transform.position.y;
                cell.UpdateState(posY == _cursorPlane.currentLayer
                    ? CellState.ActiveLayer
                    : CellState.DisabledLayer);
            }
        }
    }
}