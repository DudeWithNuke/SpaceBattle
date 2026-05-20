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
        private bool _isBattlePhase;

        public Ship Prefab => deploymentController.ShipPrefab;
        public Ship Instance => deploymentController.ShipInstance;
        public SelectedActionType SelectedAction => actionSelector.SelectedAction;
        public bool HasInstance => deploymentController.HasInstance;

        private void Awake()
        {
            EnsureCanvasGroup();
            ApplyCanvasInteractionState(GetAvailability());
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

        public void SetAbilityInteractionEnabled(bool isEnabled)
        {
            _isAbilityInteractionEnabled = isEnabled;
            RefreshInteractability();
        }

        public void SetTurnInteractionEnabled(bool isEnabled)
        {
            _isTurnInteractionEnabled = isEnabled;
            RefreshInteractability();
        }

        public void SetPickedShipInteractionBlocked(bool isBlocked)
        {
            _isPickedShipInteractionBlocked = isBlocked;
            RefreshInteractability();
        }

        public void SetBattlePhase(bool isBattlePhase)
        {
            _isBattlePhase = isBattlePhase;
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
            var availability = GetAvailability();
            ApplyCanvasInteractionState(availability);
            actionSelector.SetInteractionEnabled(availability.CanUseActions);

            if (background)
                background.SetActive(!IsPlaced());

            if (!mainButton)
                return;

            mainButton.interactable = availability.CanUseMainButton;
        }

        private bool IsInteractionAllowed()
        {
            return _isAbilityInteractionEnabled &&
                   _isTurnInteractionEnabled &&
                   !_isPickedShipInteractionBlocked;
        }

        private UnitCardAvailability GetAvailability()
        {
            var canInteract = IsInteractionAllowed();
            var canUseMainButton = canInteract && !_isBattlePhase && IsStored();
            var canUseActions = canInteract && _isBattlePhase && IsPlaced();
            var blocksRaycasts = canUseMainButton || canUseActions;

            return new UnitCardAvailability(blocksRaycasts, canUseMainButton, canUseActions);
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup)
                return;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (!_canvasGroup)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void ApplyCanvasInteractionState(UnitCardAvailability availability)
        {
            EnsureCanvasGroup();

            _canvasGroup.interactable = availability.BlocksRaycasts;
            _canvasGroup.blocksRaycasts = availability.BlocksRaycasts;
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
