using System;
using GameBoard;
using PlaceableObjectManipulation;
using Reflex.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace PlaceableObject
{
    public enum PlaceableObjectDeploymentSide
    {
        OwnField,
        EnemyField
    }

    public enum PlaceableObjectState
    {
        Stored,
        Picked,
        Placed
    }

    [RequireComponent(typeof(GridInteraction))]
    public abstract class PlaceableObject : MonoBehaviour
    {
        public event Action<PlaceableObject> OnPlaced;
        public event Action<PlaceableObject> OnPicked;
        public event Action<PlaceableObject> OnDestroyed;

        public PlaceableObjectState State { get; private set; }
        public PlaceableObjectShape Shape { get; private set; }
        public Vector3Int CurrentPosition { get; set; }
        public bool IsPlayerObject { get; private set; }

        public PlaceableObjectDeploymentSide AllowedDeploymentSide => DeploymentSide;
        protected abstract PlaceableObjectDeploymentSide DeploymentSide { get; }

        protected virtual bool UsesCellOccupancy => true;

        private PlaceableObjectColorController _colorController;
        private GridInteraction _gridInteraction;
        private bool _isRuntimeInitialized;
        private bool _pendingTakeFromStorage;

        [SerializeField] private PlaceableObjectSettings placeableObjectSettings;

        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private void Awake()
        {
            State = PlaceableObjectState.Stored;
            EnsureShapeInitialized();
            _gridInteraction = GetComponent<GridInteraction>();
        }

        private void Start()
        {
            IsPlayerObject = DeploymentSide == PlaceableObjectDeploymentSide.OwnField;
            _gridInteraction.Initialize(Shape, UsesCellOccupancy, IsPlayerObject);
            _colorController =
                new PlaceableObjectColorController(GetComponentsInChildren<Renderer>(), placeableObjectSettings);

            var startPos = transform.position;
            CurrentPosition = WorldToCellPosition(startPos);
            _isRuntimeInitialized = true;

            if (_pendingTakeFromStorage)
            {
                _pendingTakeFromStorage = false;
                ActivateFromStorage();
            }
        }

        private void LateUpdate()
        {
            if (State != PlaceableObjectState.Picked)
                return;

            _gridInteraction.UpdateHover(CurrentPosition);
            UpdatePlacementVisualState();
        }

        private Vector3Int WorldToCellPosition(Vector3 worldPosition)
        {
            var origin = IsPlayerObject ? _cellGrid.OwnOrigin : _cellGrid.EnemyOrigin;
            return CoordinateConverter.WorldToCellPosition(worldPosition, origin, _cursorPlane.currentLayer,
                placeableObjectSettings != null ? placeableObjectSettings.coordinateRoundingOffset : 0.5f);
        }

        public bool TryPlace()
        {
            if (State != PlaceableObjectState.Picked)
                return false;
            
            if (!_gridInteraction.CanPlace(CurrentPosition))
                return false;

            State = PlaceableObjectState.Placed;
            _gridInteraction.ApplyPlacement(CurrentPosition);
            _colorController?.SetState(PlaceableObjectVisualState.Default);
            OnPlaced?.Invoke(this);
            return true;
        }

        public bool TryPick()
        {
            if (State != PlaceableObjectState.Placed)
                return false;

            State = PlaceableObjectState.Picked;
            _gridInteraction.ApplyPick(CurrentPosition);
            UpdatePlacementVisualState();
            OnPicked?.Invoke(this);
            return true;
        }

        public bool TryTakeFromStorage()
        {
            if (State != PlaceableObjectState.Stored)
                return false;

            if (!_isRuntimeInitialized)
            {
                _pendingTakeFromStorage = true;
                return true;
            }

            ActivateFromStorage();
            return true;
        }

        private void ActivateFromStorage()
        {
            State = PlaceableObjectState.Picked;
            UpdatePlacementVisualState();
            OnPicked?.Invoke(this);
        }

        private void OnDestroy()
        {
            _gridInteraction.CleanupOnDestroy(CurrentPosition, State == PlaceableObjectState.Placed);
            OnDestroyed?.Invoke(this);
        }

        private void UpdatePlacementVisualState()
        {
            var canBePlaced = _gridInteraction.CanPlace(CurrentPosition);
            _colorController?.SetState(canBePlaced
                ? PlaceableObjectVisualState.Default
                : PlaceableObjectVisualState.InvalidPlacement);
        }

        protected abstract void DefineShape(PlaceableObjectShape shape);

        public void EnsureShapeInitialized()
        {
            if (Shape != null)
                return;

            Shape = ScriptableObject.CreateInstance<PlaceableObjectShape>();
            DefineShape(Shape);
        }
    }
}
