using System;
using System.Collections.Generic;
using System.Text;
using GameBoard;
using PlaceableObject;
using Reflex.Extensions;
using Reflex.Injectors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ShipButton : MonoBehaviour
    {
        public static event Action<ShipButton, PlaceableObject.PlaceableObject> OnObjectSpawned;
        public static event Action<ShipButton, PlaceableObject.PlaceableObject, string> OnActionRequested;

        private PlaceableObject.PlaceableObject _placeableObjectPrefab;
        private PlaceableObject.PlaceableObject _placeableObjectInstance;
        private Button _button;
        private TMP_Text _label;
        private PlaceableObjectUiProfile _uiProfile;
        private readonly List<Button> _actionButtons = new();

        [Header("Card UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text statsLabel;
        [SerializeField] private Transform actionButtonsRoot;
        [SerializeField] private Button actionButtonPrefab;
        [SerializeField] private Button primaryActionButton;

        public void Initialize(PlaceableObject.PlaceableObject prefab)
        {
            _placeableObjectPrefab = prefab;
            _button = primaryActionButton != null ? primaryActionButton : GetComponent<Button>();
            _label = GetComponentInChildren<TMP_Text>(true);
            _uiProfile = _placeableObjectPrefab ? _placeableObjectPrefab.GetComponent<PlaceableObjectUiProfile>() : null;

            name = _placeableObjectPrefab.name + " Button";
            if (_label != null)
                _label.text = GetDisplayName(_placeableObjectPrefab, _uiProfile);

            SetupIcon();
            SetupStats();
            BuildActionButtons();

            _button.onClick.AddListener(HandleClick);
            DisableKeyboard();
            UpdateActionButtonsState();
        }

        private static string GetDisplayName(PlaceableObject.PlaceableObject placeableObject, PlaceableObjectUiProfile uiProfile)
        {
            if (uiProfile != null && !string.IsNullOrWhiteSpace(uiProfile.DisplayName))
                return uiProfile.DisplayName;

            if (!placeableObject)
                return "Unknown";

            return string.IsNullOrWhiteSpace(placeableObject.name)
                ? placeableObject.GetType().Name
                : placeableObject.name;
        }

        private void SetupIcon()
        {
            if (iconImage == null)
                return;

            if (_uiProfile == null || _uiProfile.Icon == null)
            {
                iconImage.enabled = false;
                return;
            }

            iconImage.enabled = true;
            iconImage.sprite = _uiProfile.Icon;
        }

        private void SetupStats()
        {
            if (statsLabel == null)
                return;

            if (_uiProfile == null || _uiProfile.Stats.Count == 0)
            {
                statsLabel.text = string.Empty;
                return;
            }

            var builder = new StringBuilder();
            foreach (var stat in _uiProfile.Stats)
            {
                if (string.IsNullOrWhiteSpace(stat.Label) && string.IsNullOrWhiteSpace(stat.Value))
                    continue;

                if (builder.Length > 0)
                    builder.AppendLine();

                if (string.IsNullOrWhiteSpace(stat.Label))
                    builder.Append(stat.Value);
                else if (string.IsNullOrWhiteSpace(stat.Value))
                    builder.Append(stat.Label);
                else
                    builder.Append(stat.Label).Append(": ").Append(stat.Value);
            }

            statsLabel.text = builder.ToString();
        }

        private void BuildActionButtons()
        {
            ClearActionButtons();
            if (_uiProfile == null || actionButtonsRoot == null || actionButtonPrefab == null)
                return;

            foreach (var action in _uiProfile.Actions)
            {
                if (string.IsNullOrWhiteSpace(action.ActionId))
                    continue;

                var actionButton = Instantiate(actionButtonPrefab, actionButtonsRoot);
                var actionId = action.ActionId;
                var buttonLabel = actionButton.GetComponentInChildren<TMP_Text>(true);
                if (buttonLabel != null)
                    buttonLabel.text = string.IsNullOrWhiteSpace(action.DisplayName) ? actionId : action.DisplayName;

                actionButton.onClick.AddListener(() => HandleActionClick(actionId));
                _actionButtons.Add(actionButton);
            }
        }

        public void DisableInteraction()
        {
            _button.interactable = false;
            UpdateActionButtonsState();
        }

        public void EnableInteraction()
        {
            if (_placeableObjectInstance)
                return;

            _button.interactable = true;
            UpdateActionButtonsState();
        }

        private void DisableKeyboard()
        {
            var navigation = _button.navigation;
            navigation.mode = Navigation.Mode.None;
            _button.navigation = navigation;

            foreach (var actionButton in _actionButtons)
            {
                var actionNavigation = actionButton.navigation;
                actionNavigation.mode = Navigation.Mode.None;
                actionButton.navigation = actionNavigation;
            }
        }

        private void HandleClick()
        {
            if (!_placeableObjectInstance)
            {
                SpawnPrefab();
                return;
            }

            if (_placeableObjectInstance.State == PlaceableObjectState.Placed)
            {
                _placeableObjectInstance.TryPick();
                DisableInteraction();
            }

            UpdateActionButtonsState();
        }

        private void HandleActionClick(string actionId)
        {
            if (!_placeableObjectInstance || _placeableObjectInstance.State != PlaceableObjectState.Placed)
                return;

            OnActionRequested?.Invoke(this, _placeableObjectInstance, actionId);
        }

        private void SpawnPrefab()
        {
            _placeableObjectInstance = Instantiate(_placeableObjectPrefab, GetSpawnPositionUnderCursor(), Quaternion.identity);
            if (!_placeableObjectInstance)
                return;

            var sceneContainer = gameObject.scene.GetSceneContainer();
            GameObjectInjector.InjectObject(_placeableObjectInstance.gameObject, sceneContainer);

            _placeableObjectInstance.OnPlaced += HandlePlaced;
            _placeableObjectInstance.OnPicked += HandlePicked;
            _placeableObjectInstance.OnDestroyed += HandleDestroyed;

            DisableInteraction();
            OnObjectSpawned?.Invoke(this, _placeableObjectInstance);
            UpdateActionButtonsState();
        }

        private static Vector3 GetSpawnPositionUnderCursor()
        {
            var camera = Camera.main;
            var cursorPlane = FindFirstObjectByType<CursorPlane>();
            if (!camera || cursorPlane == null)
                return Vector3.zero;

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            return cursorPlane.Plane.Raycast(ray, out var distance) 
                ? ray.GetPoint(distance) 
                : new Vector3(0f, cursorPlane.currentLayer, 0f);
        }

        private void HandlePlaced(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject == _placeableObjectInstance)
                DisableInteraction();

            UpdateActionButtonsState();
        }

        private void HandlePicked(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject == _placeableObjectInstance)
                DisableInteraction();

            UpdateActionButtonsState();
        }

        private void HandleDestroyed(PlaceableObject.PlaceableObject placeableObject)
        {
            if (placeableObject != _placeableObjectInstance)
                return;

            UnsubscribeFromInstance();
            _placeableObjectInstance = null;
            EnableInteraction();
            UpdateActionButtonsState();
        }

        private void UnsubscribeFromInstance()
        {
            if (!_placeableObjectInstance)
                return;

            _placeableObjectInstance.OnPlaced -= HandlePlaced;
            _placeableObjectInstance.OnPicked -= HandlePicked;
            _placeableObjectInstance.OnDestroyed -= HandleDestroyed;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);

            ClearActionButtons();
            UnsubscribeFromInstance();
        }

        private void UpdateActionButtonsState()
        {
            var canUseActions = _placeableObjectInstance && _placeableObjectInstance.State == PlaceableObjectState.Placed;
            foreach (var actionButton in _actionButtons)
                actionButton.interactable = canUseActions;
        }

        private void ClearActionButtons()
        {
            foreach (var actionButton in _actionButtons)
            {
                if (!actionButton)
                    continue;

                actionButton.onClick.RemoveAllListeners();
                Destroy(actionButton.gameObject);
            }

            _actionButtons.Clear();
        }
    }
}
