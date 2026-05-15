using System;
using PlaceableObject;
using PlaceableObject.Abilities;
using PlaceableObject.Ships;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ShipActionSelector : MonoBehaviour
    {
        public event Action<ShipActionSelector, SelectedActionType> OnSelectionChanged;

        [SerializeField] private Button defaultAbilityButton;
        [FormerlySerializedAs("abilityButtonSlot1")]
        [FormerlySerializedAs("abilityToggleSlot1")]
        [SerializeField] private Button abilityButtonSlot1;
        [FormerlySerializedAs("abilityButtonSlot2")]
        [FormerlySerializedAs("abilityToggleSlot2")]
        [SerializeField] private Button abilityButtonSlot2;

        private Ship _currentShip;
        private SelectedActionType _selectedAction = SelectedActionType.None;
        private bool _isPlayerBattlefieldActive = true;
        private bool _isInteractionEnabled = true;
        private UnityAction _defaultAbilityClicked;
        private UnityAction _slot1AbilityClicked;
        private UnityAction _slot2AbilityClicked;

        public SelectedActionType SelectedAction => _selectedAction;
        public Ship CurrentShip => _currentShip;

        private void Awake()
        {
            _defaultAbilityClicked = SubmitStandardAttack;
            _slot1AbilityClicked = () => SubmitAbility(0);
            _slot2AbilityClicked = () => SubmitAbility(1);

            ConfigureButton(defaultAbilityButton, _defaultAbilityClicked);
            ConfigureButton(abilityButtonSlot1, _slot1AbilityClicked);
            ConfigureButton(abilityButtonSlot2, _slot2AbilityClicked);
        }

        public void SetShip(Ship ship)
        {
            if (_currentShip == ship)
                return;

            _currentShip = ship;
            ResetSelection();
            UpdateInteractability();
        }

        public void Clear()
        {
            _currentShip = null;
            ResetSelection();
            UpdateInteractability();
        }

        public void SetBattlefield(bool isPlayerBattlefieldActive)
        {
            _isPlayerBattlefieldActive = isPlayerBattlefieldActive;

            if (_selectedAction == SelectedActionType.AbilitySlot1 || _selectedAction == SelectedActionType.AbilitySlot2)
            {
                var slotIndex = _selectedAction == SelectedActionType.AbilitySlot1 ? 0 : 1;
                var ability = GetAbility(slotIndex);

                if (ability && !IsAbilityAllowedOnCurrentField(ability))
                    SelectStandardAttack();
            }

            UpdateInteractability();
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
            UpdateInteractability();
        }

        public void SelectStandardAttack()
        {
            SelectAction(SelectedActionType.StandardAttack);
        }

        public void SubmitStandardAttack()
        {
            SubmitAction(SelectedActionType.StandardAttack);
        }

        private void SubmitAbility(int slotIndex)
        {
            var ability = GetAbility(slotIndex);
            if (!IsAbilityAllowedOnCurrentField(ability))
                return;

            var targetAction = slotIndex == 0 ? SelectedActionType.AbilitySlot1 : SelectedActionType.AbilitySlot2;
            SubmitAction(targetAction);
        }

        public void ResetSelection()
        {
            SelectAction(SelectedActionType.None);
        }

        private void UpdateInteractability()
        {
            var canSelect = _isInteractionEnabled && _currentShip && _currentShip.State == PlaceableObjectState.Placed;

            SetButtonActive(defaultAbilityButton, canSelect);
            SetButtonActive(abilityButtonSlot1, canSelect);
            SetButtonActive(abilityButtonSlot2, canSelect);

            SetButtonInteractable(defaultAbilityButton, canSelect && IsAbilityAllowedOnCurrentField(_currentShip.DefaultAbility));
            SetButtonInteractable(abilityButtonSlot1, canSelect && IsAbilityAvailable(0));
            SetButtonInteractable(abilityButtonSlot2, canSelect && IsAbilityAvailable(1));
        }

        private void SubmitAction(SelectedActionType actionType)
        {
            if (_selectedAction == actionType)
            {
                OnSelectionChanged?.Invoke(this, actionType);
                return;
            }

            SelectAction(actionType);
        }

        private void SelectAction(SelectedActionType actionType)
        {
            if (_selectedAction == actionType)
                return;

            _selectedAction = actionType;
            OnSelectionChanged?.Invoke(this, actionType);
        }

        private void ConfigureButton(Button button, UnityAction handler)
        {
            if (!button)
                return;

            button.onClick.AddListener(handler);
            DisableNavigation(button);
        }

        private Ability GetAbility(int slotIndex)
        {
            if (!_currentShip)
                return null;

            return slotIndex switch
            {
                0 => _currentShip.UnitAbility,
                1 => _currentShip.FactionAbility,
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

            if (abilityButtonSlot1)
                abilityButtonSlot1.onClick.RemoveListener(_slot1AbilityClicked);

            if (abilityButtonSlot2)
                abilityButtonSlot2.onClick.RemoveListener(_slot2AbilityClicked);
        }
    }
}
