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

        [FormerlySerializedAs("abilityButtonSlot1")]
        [SerializeField] private Toggle abilityToggleSlot1;
        [FormerlySerializedAs("abilityButtonSlot2")]
        [SerializeField] private Toggle abilityToggleSlot2;

        private Ship _currentShip;
        private SelectedActionType _selectedAction = SelectedActionType.None;
        private bool _isPlayerBattlefieldActive = true;
        private bool _isInteractionEnabled = true;
        private bool _isUpdatingToggles;
        private UnityAction<bool> _slot1ToggleChanged;
        private UnityAction<bool> _slot2ToggleChanged;

        public SelectedActionType SelectedAction => _selectedAction;
        public Ship CurrentShip => _currentShip;

        private void Awake()
        {
            _slot1ToggleChanged = isOn => HandleToggleChanged(0, isOn);
            _slot2ToggleChanged = isOn => HandleToggleChanged(1, isOn);

            ConfigureToggle(abilityToggleSlot1, _slot1ToggleChanged);
            ConfigureToggle(abilityToggleSlot2, _slot2ToggleChanged);
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
            if (_selectedAction == SelectedActionType.StandardAttack)
            {
                OnSelectionChanged?.Invoke(this, SelectedActionType.StandardAttack);
                return;
            }

            SelectStandardAttack();
        }

        private void ToggleAbility(int slotIndex)
        {
            var ability = GetAbility(slotIndex);
            if (!IsAbilityAllowedOnCurrentField(ability))
                return;

            var targetAction = slotIndex == 0 ? SelectedActionType.AbilitySlot1 : SelectedActionType.AbilitySlot2;

            if (_selectedAction == targetAction)
                SelectStandardAttack();
            else
                SelectAction(targetAction);
        }

        public void ResetSelection()
        {
            SelectAction(SelectedActionType.None);
        }

        private void UpdateInteractability()
        {
            var canSelect = _isInteractionEnabled && _currentShip && _currentShip.State == PlaceableObjectState.Placed;

            SetToggleInteractable(abilityToggleSlot1, canSelect && IsAbilityAvailable(0));
            SetToggleInteractable(abilityToggleSlot2, canSelect && IsAbilityAvailable(1));
        }

        private void SelectAction(SelectedActionType actionType)
        {
            if (_selectedAction == actionType)
                return;

            _selectedAction = actionType;
            RefreshToggleStates();
            OnSelectionChanged?.Invoke(this, actionType);
        }

        private void ConfigureToggle(Toggle toggle, UnityAction<bool> handler)
        {
            toggle.onValueChanged.AddListener(handler);

            DisableNavigation(toggle);
        }

        private void HandleToggleChanged(int slotIndex, bool isOn)
        {
            if (_isUpdatingToggles)
                return;

            if (isOn)
            {
                ToggleAbility(slotIndex);
                RefreshToggleStates();
                return;
            }

            var actionType = slotIndex == 0 ? SelectedActionType.AbilitySlot1 : SelectedActionType.AbilitySlot2;
            if (_selectedAction == actionType)
                SelectStandardAttack();
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

        private void RefreshToggleStates()
        {
            _isUpdatingToggles = true;

            abilityToggleSlot1.SetIsOnWithoutNotify(_selectedAction == SelectedActionType.AbilitySlot1);

            abilityToggleSlot2.SetIsOnWithoutNotify(_selectedAction == SelectedActionType.AbilitySlot2);

            _isUpdatingToggles = false;
        }

        private static void SetToggleInteractable(Toggle toggle, bool interactable)
        {
            toggle.interactable = interactable;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private void OnDestroy()
        {
            abilityToggleSlot1.onValueChanged.RemoveListener(_slot1ToggleChanged);

            abilityToggleSlot2.onValueChanged.RemoveListener(_slot2ToggleChanged);
        }
    }
}
