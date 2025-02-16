using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PlaceableObject
{
    public class ObjectSelection : CustomMonoBehaviour<ObjectSelection>
    {
        public event Action<PlaceableObject> OnObjectPicked;
        
        [SerializeField] public List<PlaceableObject> placeableObjects;
        [SerializeField] public PlaceableObject currentObject;
        
        [SerializeField] public Transform buttonPanel;
        [SerializeField] public GameObject buttonPrefab;
        [SerializeField] public Transform spawnPoint;
        
        protected override void SetUp()
        {
            if (!StartCheck())
                return;
            
            foreach (var placeableObject in placeableObjects)
            {
                var buttonObject = Instantiate(buttonPrefab, buttonPanel);
                var button = buttonObject.GetComponent<Button>();

                if (button == null)
                {
                    Log.Error("Could not find button prefab");
                    continue;
                }

                button.onClick.AddListener(() => SpawnPrefab(placeableObject));
                buttonObject.name = placeableObject.name;
            }
        }

        private void SpawnPrefab(PlaceableObject placeableObject)
        {
            if (currentObject != null)
                Destroy(currentObject.gameObject);
            
            var newObject = Instantiate(placeableObject, spawnPoint.position, spawnPoint.rotation);
            currentObject = newObject.GetComponent<PlaceableObject>();

            if (currentObject == null) 
                Log.Error("Could not find placeable prefab");
            
            OnObjectPicked?.Invoke(currentObject);
        }

        private bool StartCheck()
        {
            var isPassed = true;

            isPassed &= CheckReference(buttonPanel);
            isPassed &= CheckReference(buttonPrefab);
            isPassed &= CheckReference(spawnPoint);

            return isPassed;

            bool CheckReference(Object reference)
            {
                if (reference != null) 
                    return true;
                
                Log.Error("Could not find reference");
                return false;
            }
        }
    }
}
