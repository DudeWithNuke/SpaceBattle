using System;
using PlaceableObject;
using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UnitCard
{
    public enum SelectedActionType
    {
        None,
        Default,
        Ship,
        Faction
    }

    public class UnitCardController : MonoBehaviour
    {
        public event Action<UnitCardController, Ship> OnSpawnRequested;
        public event Action<UnitCardController, Ship> OnPickRequested;
        public event Action<UnitCardController, SelectedActionType> OnActionSelectionChanged;

        [Header("Components")]
        //[SerializeField] private ShipCardView cardView;
        [SerializeField] private UnitCardActionSelector actionSelector;
        [SerializeField] private UnitCardDeploymentController deploymentController;

        [Header("Main Button")]
        [SerializeField] private Button mainButton;
        [SerializeField] private GameObject background;

        private CanvasGroup _canvasGroup;
        private bool _isAbilityInteractionEnabled = true;
        private bool _isTurnInteractionEnabled;
        private bool _isPickedShipInteractionBlocked;

        public Ship Prefab => deploymentController.ShipPrefab;
        public Ship Instance => deploymentController.ShipInstance;
        public SelectedActionType SelectedAction => actionSelector.SelectedAction;
        public bool HasInstance => deploymentController.HasInstance;

        private void Awake()
        {
            EnsureCanvasGroup();
            ApplyCanvasInteractionState();
        }

        public void Initialize(Ship prefab)
        {
            EnsureCanvasGroup();
            deploymentController.Initialize(prefab);
            //cardView.SetShip(prefab);

            SubscribeToComponents();
            if (mainButton)
            {
                mainButton.onClick.AddListener(HandleMainClick);
                DisableNavigation(mainButton);
            }

            RefreshInteractability();
        }

        public void BindInstance(Ship instance)
        {
            deploymentController.BindInstance(instance);
            actionSelector.SetShip(instance);
            RefreshInteractability();
        }

        public void ReleaseInstance()
        {
            deploymentController.ReleaseInstance();
            actionSelector.Clear();
            RefreshInteractability();
        }

        public void SetAbilityBattlefield(bool isPlayerBattlefieldActive)
        {
            actionSelector.SetBattlefield(isPlayerBattlefieldActive);
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isAbilityInteractionEnabled = isEnabled;
            actionSelector.SetInteractionEnabled(IsInteractionAllowed());
            RefreshInteractability();
        }

        public void SetTurnInteractionEnabled(bool isEnabled)
        {
            _isTurnInteractionEnabled = isEnabled;
            actionSelector.SetInteractionEnabled(IsInteractionAllowed());
            RefreshInteractability();
        }

        public void SetPickedShipInteractionBlocked(bool isBlocked)
        {
            _isPickedShipInteractionBlocked = isBlocked;
            actionSelector.SetInteractionEnabled(IsInteractionAllowed());
            RefreshInteractability();
        }

        private void SubscribeToComponents()
        {
            deploymentController.OnShipSpawnRequested += (ctrl, ship) => OnSpawnRequested?.Invoke(this, ship);
            deploymentController.OnShipPickRequested += (ctrl, ship) => OnPickRequested?.Invoke(this, ship);
            deploymentController.OnShipPlaced += HandleShipPlaced;
            deploymentController.OnShipPicked += HandleShipPicked;
            deploymentController.OnShipDestroyed += HandleShipDestroyed;

            actionSelector.OnSelectionChanged += (selector, actionType) => OnActionSelectionChanged?.Invoke(this, actionType);
        }

        private void HandleMainClick()
        {
            if (!IsInteractionAllowed())
                return;

            var state = deploymentController.CurrentState;

            if (!state.HasValue || state.Value == PlaceableObjectState.Stored)
            {
                deploymentController.RequestSpawn();
            }
        }

        private void HandleShipPlaced(UnitCardDeploymentController controller)
        {
            actionSelector.SelectDefaultAttack();
            RefreshInteractability();
        }

        private void HandleShipPicked(UnitCardDeploymentController controller)
        {
            actionSelector.ResetSelection();
            RefreshInteractability();
        }

        private void HandleShipDestroyed(UnitCardDeploymentController controller)
        {
            actionSelector.ResetSelection();
            //cardView.Clear();
            RefreshInteractability();
        }

        private void RefreshInteractability()
        {
            ApplyCanvasInteractionState();

            if (background)
                background.SetActive(!IsPlaced());

            if (!mainButton)
                return;

            mainButton.interactable = IsInteractionAllowed() && IsStored();
        }

        private bool IsInteractionAllowed()
        {
            return _isAbilityInteractionEnabled &&
                   _isTurnInteractionEnabled &&
                   !_isPickedShipInteractionBlocked;
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup)
                return;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (!_canvasGroup)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void ApplyCanvasInteractionState()
        {
            EnsureCanvasGroup();

            var isAllowed = IsInteractionAllowed();
            _canvasGroup.interactable = isAllowed;
            _canvasGroup.blocksRaycasts = isAllowed;
        }

        private bool IsStored()
        {
            var state = deploymentController.CurrentState;
            return state is null or PlaceableObjectState.Stored;
        }

        private bool IsPlaced()
        {
            return deploymentController.CurrentState == PlaceableObjectState.Placed;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private void OnDestroy()
        {
            if (mainButton)
                mainButton.onClick.RemoveListener(HandleMainClick);
        }
    }
}
