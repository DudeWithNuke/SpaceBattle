using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;

namespace GameBoard
{
    public class CursorPlane : MonoBehaviour
    {
        public Plane Plane {get; private set;}

        public int currentLayer;
        public int layersCount;
        
        public void Initialize(CellGrid cellGrid)
        {
            layersCount = cellGrid.gridSize.y;
            currentLayer = layersCount / 2;
            Plane = new Plane(Vector3.up, new Vector3(0, currentLayer, 0));
            Refresh();
        }
        
        public void Up()
        {
            if (currentLayer >= layersCount - 1)
                return;
            
            currentLayer++;
            Refresh();
        }

        public void Down()
        {
            if (currentLayer <= 0)
                return;
            
            currentLayer--;
            Refresh();
        }

        private void Refresh()
        {
            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, new Vector3(0, currentLayer, 0));
            Plane = newPlane;
        }
    }
}