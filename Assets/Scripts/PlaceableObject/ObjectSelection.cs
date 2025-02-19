using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace PlaceableObject
{
    public class ObjectSelection : CustomMonoBehaviour<ObjectSelection>
    {
        public event Action<PlaceableObject> OnObjectPicked;

        [SerializeField] public List<PlaceableObject> placeableObjects;

        [SerializeField] public Transform buttonPanel;
        [SerializeField] public ShipButton buttonPrefab;

        private Dictionary<PlaceableObject, ShipButton> _shipButtons;

        protected override void SetUp()
        {
            CreateButtons();
            ShipButton.OnObjectSpawned += HandleObjectSpawned;
        }
        
        private void HandleObjectSpawned(PlaceableObject placeableObject)
        {
            
            
            OnObjectPicked?.Invoke(placeableObject);
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