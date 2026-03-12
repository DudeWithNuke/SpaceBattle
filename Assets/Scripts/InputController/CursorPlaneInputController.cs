using GameBoard;
using Reflex.Attributes;
using UnityEngine;

namespace InputController
{
    public class CursorPlaneInputController : MonoBehaviour
    {
        [Inject] private CursorPlane _cursorPlane;

        private void Update()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel") * CursorPlane.ScrollSensitivity;
            if (Mathf.Abs(scroll) <= CursorPlane.ScrollThreshold)
                return;

            if (scroll > 0f)
                _cursorPlane.MoveUp();
            else
                _cursorPlane.MoveDown();
        }
    }
}
