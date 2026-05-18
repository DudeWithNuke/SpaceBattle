using System;
using PlaceableObject;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.UnitCard
{
    public class UnitCardActionSelector : MonoBehaviour
    {
        public event Action<UnitCardActionSelector, SelectedActionType> OnSelectionChanged;

        [SerializeField] private Button defaultAbilityButton;
        [SerializeField] private Button shipAbilityButton;
        [SerializeField] private Button factionAbilityButton;

        private bool _isPlayerBattlefieldActive = true;
        private bool _isInteractionEnabled = true;
        private UnityAction _defaultAbilityClicked;
        private UnityAction _shipAbilityClicked;
        private UnityAction _factionAbilityClicked;

        public SelectedActionType SelectedAction { get; private set; } = SelectedActionType.None;

        public Ship CurrentShip { get; private set; }

        private void Awake()
        {
            _defaultAbilityClicked = SubmitDefaultAttack;
            _shipAbilityClicked = () => SubmitAbility(0);
            _factionAbilityClicked = () => SubmitAbility(1);

            ConfigureButton(defaultAbilityButton, _defaultAbilityClicked);
            ConfigureButton(shipAbilityButton, _shipAbilityClicked);
            ConfigureButton(factionAbilityButton, _factionAbilityClicked);
        }

        public void SetShip(Ship ship)
        {
            if (CurrentShip == ship)
                return;

            CurrentShip = ship;
            ResetSelection();
            UpdateInteractability();
        }

        public void Clear()
        {
            CurrentShip = null;
            ResetSelection();
            UpdateInteractability();
        }

        public void SetBattlefield(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;

            if (SelectedAction == SelectedActionType.Ship || SelectedAction == SelectedActionType.Faction)
            {
                var slotIndex = SelectedAction == SelectedActionType.Ship ? 0 : 1;
                var ability = GetAbility(slotIndex);

                if (ability && !IsAbilityAllowedOnCurrentField(ability))
                    SelectDefaultAttack();
            }

            UpdateInteractability();
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
            UpdateInteractability();
        }

        public void SelectDefaultAttack()
        {
            SelectAction(SelectedActionType.Default);
        }

        public void SubmitDefaultAttack()
        {
            SubmitAction(SelectedActionType.Default);
        }

        private void SubmitAbility(int slotIndex)
        {
            var ability = GetAbility(slotIndex);
            if (!IsAbilityAllowedOnCurrentField(ability))
                return;

            var targetAction = slotIndex == 0 ? SelectedActionType.Ship : SelectedActionType.Faction;
            SubmitAction(targetAction);
        }

        public void ResetSelection()
        {
            SelectAction(SelectedActionType.None);
        }

        private void UpdateInteractability()
        {
            var canSelect = _isInteractionEnabled && CurrentShip && CurrentShip.State == PlaceableObjectState.Placed;

            SetButtonActive(defaultAbilityButton, canSelect);
            SetButtonActive(shipAbilityButton, canSelect);
            SetButtonActive(factionAbilityButton, canSelect);

            SetButtonInteractable(defaultAbilityButton, canSelect && IsAbilityAllowedOnCurrentField(CurrentShip.DefaultAbility));
            SetButtonInteractable(shipAbilityButton, canSelect && IsAbilityAvailable(0));
            SetButtonInteractable(factionAbilityButton, canSelect && IsAbilityAvailable(1));
        }

        private void SubmitAction(SelectedActionType actionType)
        {
            if (SelectedAction == actionType)
            {
                OnSelectionChanged?.Invoke(this, actionType);
                return;
            }

            SelectAction(actionType);
        }

        private void SelectAction(SelectedActionType actionType)
        {
            if (SelectedAction == actionType)
                return;

            SelectedAction = actionType;
            OnSelectionChanged?.Invoke(this, actionType);
        }

        private static void ConfigureButton(Button button, UnityAction handler)
        {
            if (!button)
                return;

            button.onClick.AddListener(handler);
            DisableNavigation(button);
        }

        private Ability GetAbility(int slotIndex)
        {
            if (!CurrentShip)
                return null;

            return slotIndex switch
            {
                0 => CurrentShip.UnitAbility,
                1 => CurrentShip.FactionAbility,
                _ => null
            };
        }

        private bool IsAbilityAvailable(int slotIndex)
        {
            return IsAbilityAllowedOnCurrentField(GetAbility(slotIndex));
        }

        private bool IsAbilityAllowedOnCurrentField(PlaceableObject.PlaceableObject ability)
        {
            if (!ability)
                return false;

            return _isPlayerBattlefieldActive
                ? ability.AllowedDeploymentSide == PlaceableObjectDeploymentSide.OwnField
                : ability.AllowedDeploymentSide == PlaceableObjectDeploymentSide.EnemyField;
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (!button)
                return;

            button.interactable = interactable;
        }

        private static void SetButtonActive(Button button, bool isActive)
        {
            if (!button)
                return;

            button.enabled = isActive;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private void OnDestroy()
        {
            if (defaultAbilityButton)
                defaultAbilityButton.onClick.RemoveListener(_defaultAbilityClicked);

            if (shipAbilityButton)
                shipAbilityButton.onClick.RemoveListener(_shipAbilityClicked);

            if (factionAbilityButton)
                factionAbilityButton.onClick.RemoveListener(_factionAbilityClicked);
        }
    }
}
