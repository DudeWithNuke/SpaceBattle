using PlaceableObject;
using PlaceableObject.Manipulation;
using Reflex.Attributes;
using UnityEngine;

namespace InputController
{
    public class BattlefieldInputController : MonoBehaviour
    {
        private Camera _camera;
        [Inject] private ObjectSelection _objectSelection;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            HandleRemovePickedObject();
            HandleLeftClick();
        }

        private void HandleRemovePickedObject()
        {
            if (!Input.GetKeyUp(KeyCode.X))
                return;

            if (_objectSelection)
                _objectSelection.DestroyCurrentPickedObject();
        }

        private void HandleLeftClick()
        {
            if (!Input.GetMouseButtonUp(0))
                return;

            if (!_objectSelection)
                return;

            if (_objectSelection.CurrentPickedObject)
            {
                _objectSelection.TryPlaceCurrent();
                return;
            }

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            _objectSelection.TryPickClosest(ray);
        }
    }
}
