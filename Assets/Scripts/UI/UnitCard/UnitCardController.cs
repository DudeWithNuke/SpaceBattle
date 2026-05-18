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

        private bool _isInteractionEnabled = true;
        private bool _isPickedShipInteractionBlocked;

        public Ship Prefab => deploymentController.ShipPrefab;
        public Ship Instance => deploymentController.ShipInstance;
        public SelectedActionType SelectedAction => actionSelector.SelectedAction;
        public bool HasInstance => deploymentController.HasInstance;

        public void Initialize(Ship prefab)
        {
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
            _isInteractionEnabled = isEnabled;
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
            if (background)
                background.SetActive(!IsPlaced());

            if (!mainButton)
                return;

            mainButton.interactable = IsInteractionAllowed() && IsStored();
        }

        private bool IsInteractionAllowed()
        {
            return _isInteractionEnabled && !_isPickedShipInteractionBlocked;
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
