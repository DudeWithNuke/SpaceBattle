using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace PlaceableObject
{
    public class ObjectSelection : MonoBehaviour
    { 
        public event Action<PlaceableObject> OnStateChanged;
        [SerializeField] public List<PlaceableObject> placeableObjects;
        private PlaceableObject _currentSelectedPlaceableObject;
        
        [SerializeField] public Transform buttonPanel;
        [SerializeField] public ShipButton buttonPrefab;

        private Dictionary<PlaceableObject, ShipButton> _shipButtons;
        
        private void Awake()
        {
            CreateButtons();
            ShipButton.OnObjectSpawned += OnObjectSpawned;
        }

        private void OnObjectSpawned(PlaceableObject placeableObject)
        {
            OnPicked(placeableObject);
            placeableObject.OnPicked += OnPicked;
            placeableObject.OnPlaced += OnPlaced;
        }

        private void OnPicked(PlaceableObject placeableObject)
        {
            _currentSelectedPlaceableObject = placeableObject;
            OnStateChanged?.Invoke(placeableObject);
        }
        
        private void OnPlaced(PlaceableObject placeableObject)
        {
            _currentSelectedPlaceableObject = null;
            OnStateChanged?.Invoke(_currentSelectedPlaceableObject);
        }
        
        private void DisableOtherButtons(PlaceableObject excludedPlaceableObject)
        {
            var buttons = _shipButtons
                .Where(kvp => kvp.Key != excludedPlaceableObject)
                .Select(kvp => kvp.Value)
                .ToList();

            foreach (var button in buttons)
                button.DisableInteraction();
        }

        private void CreateButtons()
        {
            foreach (var placeableObject in placeableObjects)
            {
                var shipButton = Instantiate(buttonPrefab, buttonPanel);
                shipButton.Initialize(placeableObject);
            }
        }
    }
}