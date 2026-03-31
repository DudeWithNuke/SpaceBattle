using PlaceableObject;
using PlaceableObjectManipulation;
using Reflex.Attributes;
using UnityEngine;

namespace InputController
{
    public class BattlefieldInputController : MonoBehaviour
    {
        private Camera _camera;
        [Inject] private Selection _selection;

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

            if (_selection)
                _selection.DestroyCurrentPickedObject();
        }

        private void HandleLeftClick()
        {
            if (!Input.GetMouseButtonUp(0))
                return;

            if (!_selection)
                return;

            if (_selection.CurrentPickedObject)
            {
                _selection.TryPlaceCurrent();
                return;
            }

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            _selection.TryPickClosest(ray);
        }
    }
}
