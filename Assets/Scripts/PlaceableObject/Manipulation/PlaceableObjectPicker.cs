using UnityEngine;

namespace PlaceableObject.Manipulation
{
    public class PlaceableObjectPicker : MonoBehaviour
    {
        private readonly RaycastHit[] _raycastResults = new RaycastHit[16];

        public bool TryPickClosest(Ray ray)
        {
            var hitCount = Physics.RaycastNonAlloc(ray, _raycastResults, Mathf.Infinity);
            if (hitCount == 0)
                return false;

            PlaceableObject closestObject = null;
            var closestDistance = float.PositiveInfinity;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _raycastResults[i];
                if (hit.distance >= closestDistance)
                    continue;
                if (!hit.collider.TryGetComponent(out PlaceableObject placeableObject))
                    continue;
                if (placeableObject.State != PlaceableObjectState.Placed)
                    continue;

                closestDistance = hit.distance;
                closestObject = placeableObject;
            }

            return closestObject && closestObject.TryPick();
        }
    }
}
