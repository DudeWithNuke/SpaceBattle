using PlayerCamera;
using Reflex.Attributes;
using UnityEngine;

namespace InputController
{
    public class CameraInputController : MonoBehaviour
    {
        [Inject] private CameraMovement _cameraMovement;

        private void Update()
        {
            var moveInput = ReadMoveInput();
            var zoomDelta = ReadZoomDelta();
            var inputFrame = new CameraInputFrame(
                Input.GetKeyDown(KeyCode.Space),
                Input.GetMouseButtonDown(1),
                Input.GetMouseButtonUp(1),
                new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")),
                moveInput,
                zoomDelta
            );

            _cameraMovement.Tick(inputFrame, Time.deltaTime);
        }

        private static Vector2 ReadMoveInput()
        {
            var x = 0f;
            var y = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                y += 1f;

            return new Vector2(x, y);
        }

        private static float ReadZoomDelta()
        {
            var zoomDelta = 0f;

            if (Input.GetKey(KeyCode.Equals))
                zoomDelta += 1f;
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
                zoomDelta -= 1f;

            return zoomDelta;
        }
    }
}
