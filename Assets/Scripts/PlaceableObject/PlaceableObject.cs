using System;
using GameBoard;
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
        Picked,
        Placing,
        Placed
    }

    [RequireComponent(typeof(ObjectGridInteraction))]
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
        private ObjectGridInteraction _gridInteraction;

        [SerializeField] private PlaceableObjectSettings placeableObjectSettings;

        [Inject] private CellGrid _cellGrid;
        [Inject] private CursorPlane _cursorPlane;

        private void Awake()
        {
            EnsureShapeInitialized();
            _gridInteraction = GetComponent<ObjectGridInteraction>();
        }

        private void Start()
        {
            State = PlaceableObjectState.Placing;
            IsPlayerObject = DeploymentSide == PlaceableObjectDeploymentSide.OwnField;
            _gridInteraction.Initialize(Shape, UsesCellOccupancy, IsPlayerObject);
            _colorController =
                new PlaceableObjectColorController(GetComponentsInChildren<Renderer>(), placeableObjectSettings);

            var startPos = transform.position;
            CurrentPosition = WorldToCellPosition(startPos);
        }

        private void LateUpdate()
        {
            if (State == PlaceableObjectState.Picked)
                State = PlaceableObjectState.Placing;

            if (State != PlaceableObjectState.Placing)
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
            if (State != PlaceableObjectState.Placing)
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
