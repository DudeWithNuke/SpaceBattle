using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameBoard
{
    public class CellGrid : InitializeMonoBehaviour<CellGrid>
    {
        //[SerializeField] private PlaceableObject.PlaceableObject placeableObject;
        [SerializeField] private Cell prefab;
        [SerializeField] public Vector3Int gridSize;
        public List<Cell> outputCells;
        
        protected override void SetUp()
        {
            gridSize = new Vector3Int(
                (int)Mathf.Clamp(gridSize.x, 0, Mathf.Infinity),
                (int)Mathf.Clamp(gridSize.y, 0, Mathf.Infinity),
                (int)Mathf.Clamp(gridSize.z, 0, Mathf.Infinity)
            );

            Clear();
            Create();
        }

        private void Create()
        {
            outputCells = new List<Cell>();

            for (var x = 0; x < gridSize.x; x++)
            for (var y = 0; y < gridSize.y; y++)
            for (var z = 0; z < gridSize.z; z++)
            {
                var position = new Vector3Int(x, y, z) + transform.position;
                var cell = Instantiate(prefab, position, Quaternion.identity, transform);
                cell.Initialize(new Vector3Int(x, y, z));//, placeableObject);
                outputCells.Add(cell);
            }
        }

        private void Clear()
        {
            if (outputCells == null || outputCells.Count == 0)
                return;

            for (var i = 0; i < outputCells.Count;)
            {
                outputCells[i].DestroyPrefab();
                outputCells.RemoveAt(i);
            }
        }
    }
}