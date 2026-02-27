using PlaceableObject;
using UnityEngine;

namespace InputController
{
    public class BattlefieldInputController : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            HandleLeftClick();
        }

        private void HandleLeftClick()
        {
            if (!Input.GetMouseButtonUp(0))
                return;

            if (PlaceableObject.PlaceableObject.CurrentPickedObject)
            {
                PlaceableObject.PlaceableObject.CurrentPickedObject.TryPlaceFromInput();
                return;
            }

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var targetObject = FindClosestPlacedObject(ray);
            if (targetObject)
                targetObject.TryPick();
        }

        private static PlaceableObject.PlaceableObject FindClosestPlacedObject(Ray ray)
        {
            var hits = Physics.RaycastAll(ray, Mathf.Infinity);
            PlaceableObject.PlaceableObject closestObject = null;
            var closestDistance = float.PositiveInfinity;

            foreach (var hit in hits)
            {
                var placeableObject = hit.collider.GetComponentInParent<PlaceableObject.PlaceableObject>();
                if (!placeableObject || placeableObject.State != PlaceableObjectState.Placed)
                    continue;

                if (!(hit.distance < closestDistance))
                    continue;

                closestDistance = hit.distance;
                closestObject = placeableObject;
            }

            return closestObject;
        }
    }
}
