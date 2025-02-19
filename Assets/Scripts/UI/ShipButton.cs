using System;
using PlaceableObject;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ShipButton : MonoBehaviour
    {
        public static event Action<PlaceableObject.PlaceableObject> OnObjectSpawned;

        private PlaceableObject.PlaceableObject _placeableObjectPrefab;
        private PlaceableObject.PlaceableObject _placeableObjectInstance;
        private Button _button;

        public void Initialize(PlaceableObject.PlaceableObject prefab)
        {
            _placeableObjectPrefab = prefab;
            _button = GetComponent<Button>();

            name = _placeableObjectPrefab.name + " Button";


            _button.onClick.AddListener(HandleClick);
            
            DisableKeyboard();
        }

        public void DisableInteraction()
        {
            _button.interactable = false;
        }

        public void EnableInteraction()
        {
            _button.interactable = true;
        }

        private void DisableKeyboard()
        {
            var navigation = _button.navigation;
            navigation.mode = Navigation.Mode.None;
            _button.navigation = navigation;
        }

        private void HandleClick()
        {
            SpawnPrefab();
        }

        private void SpawnPrefab()
        {
            if (_placeableObjectInstance != null && _placeableObjectInstance.State == PlaceableObjectState.Picked)
            {
                Destroy(_placeableObjectInstance.gameObject);
                return;
            }
            
            _placeableObjectInstance = Instantiate(_placeableObjectPrefab, new Vector3Int(0, 0, 0), Quaternion.identity);
            var placeableObject = _placeableObjectInstance.GetComponent<PlaceableObject.PlaceableObject>();

            if (placeableObject == null)
            {
                Debug.LogError("Could not find PlaceableObject component on instantiated object.");
                Destroy(_placeableObjectInstance);
                return;
            }
            
            OnObjectSpawned?.Invoke(placeableObject);
        }
        
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }
    }
}