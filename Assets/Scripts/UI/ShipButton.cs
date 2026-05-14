using System;
using PlaceableObject;
using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum SelectedActionType
    {
        None,
        StandardAttack,
        AbilitySlot1,
        AbilitySlot2
    }

    public class ShipButton : MonoBehaviour
    {
        public event Action<ShipButton, Ship> OnSpawnRequested;
        public event Action<ShipButton, Ship> OnPickRequested;
        public event Action<ShipButton, SelectedActionType> OnActionSelectionChanged;

        [Header("Components")]
        //[SerializeField] private ShipCardView cardView;
        [SerializeField] private ShipActionSelector actionSelector;
        [SerializeField] private ShipDeploymentController deploymentController;

        [Header("Main Button")]
        [SerializeField] private Button mainButton;

        public Ship Prefab => deploymentController.ShipPrefab;
        public Ship Instance => deploymentController.ShipInstance;
        public SelectedActionType SelectedAction => actionSelector.SelectedAction;
        public bool HasInstance => deploymentController.HasInstance;

        public void Initialize(Ship prefab)
        {
            deploymentController.Initialize(prefab);
            //cardView.SetShip(prefab);

            SubscribeToComponents();
            mainButton.onClick.AddListener(HandleMainClick);
            DisableNavigation(mainButton);
        }

        public void BindInstance(Ship instance)
        {
            deploymentController.BindInstance(instance);
            actionSelector.SetShip(instance);
        }

        public void ReleaseInstance()
        {
            deploymentController.ReleaseInstance();
            actionSelector.Clear();
        }

        public void SetAbilityBattlefield(bool isPlayerBattlefieldActive)
        {
            actionSelector.SetBattlefield(isPlayerBattlefieldActive);
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
                return;
            }

            if (state.Value == PlaceableObjectState.Placed)
            {
                actionSelector.SubmitStandardAttack();
            }
            else if (state.Value == PlaceableObjectState.Picked)
            {
                deploymentController.RequestPick();
            }
        }

        private void HandleShipPlaced(ShipDeploymentController controller)
        {
            actionSelector.SelectStandardAttack();
        }

        private void HandleShipPicked(ShipDeploymentController controller)
        {
            actionSelector.ResetSelection();
        }

        private void HandleShipDestroyed(ShipDeploymentController controller)
        {
            actionSelector.ResetSelection();
            //cardView.Clear();
        }

        private static void DisableNavigation(Selectable selectable)
        {
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private void OnDestroy()
        {
            mainButton.onClick.RemoveListener(HandleMainClick);
        }
    }
}
