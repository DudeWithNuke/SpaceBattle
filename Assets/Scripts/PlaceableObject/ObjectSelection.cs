using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace PlaceableObject
{
    public class ObjectSelection : MonoBehaviour
    {
        public event Action<PlaceableObject> OnStateChanged;

        [SerializeField] public List<PlaceableObject> placeableObjects;
        [SerializeField] public Transform buttonPanel;
        [SerializeField] public ShipButton buttonPrefab;

        private PlaceableObject _currentSelectedPlaceableObject;
        private readonly List<ShipButton> _shipButtons = new();
        private readonly Dictionary<PlaceableObject, ShipButton> _objectToButton = new();

        private void Awake()
        {
            CreateButtons();
            ShipButton.OnObjectSpawned += OnObjectSpawned;
        }

        private void OnDestroy()
        {
            ShipButton.OnObjectSpawned -= OnObjectSpawned;

            foreach (var placeableObject in _objectToButton.Keys)
            {
                if (!placeableObject)
                    continue;

                placeableObject.OnPicked -= OnPicked;
                placeableObject.OnPlaced -= OnPlaced;
                placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;
            }
        }

        private void OnObjectSpawned(ShipButton sourceButton, PlaceableObject placeableObject)
        {
            _objectToButton[placeableObject] = sourceButton;

            placeableObject.OnPicked += OnPicked;
            placeableObject.OnPlaced += OnPlaced;
            placeableObject.OnDestroyed += OnPlaceableObjectDestroyed;

            OnPicked(placeableObject);
        }

        private void OnPicked(PlaceableObject placeableObject)
        {
            _currentSelectedPlaceableObject = placeableObject;
            DisableAllButtonsExcept(placeableObject);
            OnStateChanged?.Invoke(placeableObject);
        }

        private void OnPlaced(PlaceableObject placeableObject)
        {
            if (_currentSelectedPlaceableObject == placeableObject)
                _currentSelectedPlaceableObject = null;

            EnableAllButtons();
            OnStateChanged?.Invoke(null);
        }

        private void OnPlaceableObjectDestroyed(PlaceableObject placeableObject)
        {
            placeableObject.OnPicked -= OnPicked;
            placeableObject.OnPlaced -= OnPlaced;
            placeableObject.OnDestroyed -= OnPlaceableObjectDestroyed;

            _objectToButton.Remove(placeableObject);

            if (_currentSelectedPlaceableObject == placeableObject)
            {
                _currentSelectedPlaceableObject = null;
                EnableAllButtons();
                OnStateChanged?.Invoke(null);
            }
        }

        private void DisableAllButtonsExcept(PlaceableObject placeableObject)
        {
            foreach (var shipButton in _shipButtons)
                shipButton.DisableInteraction();

            if (_objectToButton.TryGetValue(placeableObject, out var sourceButton))
                sourceButton.DisableInteraction();
        }

        private void EnableAllButtons()
        {
            foreach (var shipButton in _shipButtons)
                shipButton.EnableInteraction();
        }

        private void CreateButtons()
        {
            foreach (var placeableObject in placeableObjects)
            {
                var shipButton = Instantiate(buttonPrefab, buttonPanel);
                shipButton.Initialize(placeableObject);
                _shipButtons.Add(shipButton);
            }
        }
    }
}
