using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Reflex.Attributes;

namespace GameBoard
{
    public class CursorPlane : MonoBehaviour
    {
        public const float ScrollSensitivity = 0.5f;
        public const float ScrollThreshold = 0.01f;
        private const float VerticalTransitionSpeed = 35f;

        public Plane Plane { get; private set; }
        public bool IsTransitioning { get; private set; }

        private int _layersCount;
        public int currentLayer;
        private float _slideAnimationTargetY;
        
        [Inject] private CellGrid _cellGrid;
        
        private void Start()
        {
            _layersCount = _cellGrid.GridSize.y;
            currentLayer = _layersCount / 2;
            _slideAnimationTargetY = currentLayer;
            gameObject.transform.position = GetActualPosition(_slideAnimationTargetY);
            Plane = new Plane(Vector3.up, gameObject.transform.position);
            RefreshLayerState();
        }
        
        private void Update()
        {
            UpdateVerticalTransition();
        }

        public void MoveUp()
        {
            if (currentLayer >= _layersCount - 1)
                return;

            currentLayer++;
            _slideAnimationTargetY = currentLayer;
            RefreshLayerState();
        }

        public void MoveDown()
        {
            if (currentLayer <= 0)
                return;

            currentLayer--;
            _slideAnimationTargetY = currentLayer;
            RefreshLayerState();
        }

        public void SetLayer(int layerIndex, bool immediate = false)
        {
            var clampedLayer = Mathf.Clamp(layerIndex, 0, Mathf.Max(0, _layersCount - 1));
            currentLayer = clampedLayer;
            _slideAnimationTargetY = currentLayer;
            RefreshLayerState();

            if (!immediate)
                return;

            gameObject.transform.position = GetActualPosition(_slideAnimationTargetY);
            IsTransitioning = false;
            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, gameObject.transform.position);
            Plane = newPlane;
        }

        private void UpdateVerticalTransition()
        {
            var currentPos = gameObject.transform.position;
            var nextY = Mathf.MoveTowards(currentPos.y, _slideAnimationTargetY, VerticalTransitionSpeed * Time.deltaTime);
            gameObject.transform.position = GetActualPosition(nextY);

            IsTransitioning = !Mathf.Approximately(nextY, _slideAnimationTargetY);

            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, gameObject.transform.position);
            Plane = newPlane;
        }

        private void RefreshLayerState()
        {
            SetLayerActive(currentLayer, true);
        }

        private static Vector3 GetActualPosition(float y)
        {
            return new Vector3(0, y, 0);
        }

        private void SetLayerActive(int layerIndex, bool isActive)
        {
            foreach (var cell in _cellGrid.Cells.Where(cell => !cell.IsSelected))
            {
                if (cell.Position.y == layerIndex && isActive)
                    cell.ActivateLayer();
                else
                    cell.DisableLayer();
            }
        }
    }
}
