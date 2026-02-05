using UnityEngine;
using UnityEngine.InputSystem;

namespace LWT.UnityWorkbench.InputHandling
{
    /// <summary>
    /// Input utility functions.
    /// </summary>
    public static class InputUtility
    {
        /// <summary>
        /// Gets the mouse position in world space on a plane.
        /// </summary>
        public static Vector3 GetMouseWorldPosition(Camera camera = null, float planeHeight = 0f)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return Vector3.zero;

            Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Ray ray = camera.ScreenPointToRay(mousePos);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Gets the direction from an object to the mouse position.
        /// </summary>
        public static Vector3 GetDirectionToMouse(Vector3 fromPosition, Camera camera = null, float planeHeight = 0f)
        {
            Vector3 mouseWorld = GetMouseWorldPosition(camera, planeHeight);
            Vector3 direction = mouseWorld - fromPosition;
            direction.y = 0f;
            return direction.normalized;
        }

        /// <summary>
        /// Checks if the mouse is over a UI element.
        /// </summary>
        public static bool IsMouseOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Gets the current mouse delta (movement).
        /// </summary>
        public static Vector2 GetMouseDelta()
        {
            return Mouse.current?.delta.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// Gets the mouse scroll delta.
        /// </summary>
        public static float GetScrollDelta()
        {
            return Mouse.current?.scroll.ReadValue().y ?? 0f;
        }

        /// <summary>
        /// Checks if any key is pressed.
        /// </summary>
        public static bool AnyKeyPressed()
        {
            return Keyboard.current?.anyKey.wasPressedThisFrame ?? false;
        }

        /// <summary>
        /// Checks if a specific key is pressed.
        /// </summary>
        public static bool IsKeyPressed(Key key)
        {
            return Keyboard.current?[key].wasPressedThisFrame ?? false;
        }

        /// <summary>
        /// Checks if a specific key is held.
        /// </summary>
        public static bool IsKeyHeld(Key key)
        {
            return Keyboard.current?[key].isPressed ?? false;
        }

        /// <summary>
        /// Checks if a mouse button is pressed.
        /// </summary>
        public static bool IsMouseButtonPressed(int button)
        {
            if (Mouse.current == null) return false;

            return button switch
            {
                0 => Mouse.current.leftButton.wasPressedThisFrame,
                1 => Mouse.current.rightButton.wasPressedThisFrame,
                2 => Mouse.current.middleButton.wasPressedThisFrame,
                _ => false
            };
        }

        /// <summary>
        /// Checks if a mouse button is held.
        /// </summary>
        public static bool IsMouseButtonHeld(int button)
        {
            if (Mouse.current == null) return false;

            return button switch
            {
                0 => Mouse.current.leftButton.isPressed,
                1 => Mouse.current.rightButton.isPressed,
                2 => Mouse.current.middleButton.isPressed,
                _ => false
            };
        }

        /// <summary>
        /// Gets the current gamepad left stick value.
        /// </summary>
        public static Vector2 GetLeftStick()
        {
            return Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// Gets the current gamepad right stick value.
        /// </summary>
        public static Vector2 GetRightStick()
        {
            return Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// Gets the left trigger value.
        /// </summary>
        public static float GetLeftTrigger()
        {
            return Gamepad.current?.leftTrigger.ReadValue() ?? 0f;
        }

        /// <summary>
        /// Gets the right trigger value.
        /// </summary>
        public static float GetRightTrigger()
        {
            return Gamepad.current?.rightTrigger.ReadValue() ?? 0f;
        }

        /// <summary>
        /// Checks if a gamepad is connected.
        /// </summary>
        public static bool IsGamepadConnected()
        {
            return Gamepad.current != null;
        }

        /// <summary>
        /// Checks if a keyboard is connected.
        /// </summary>
        public static bool IsKeyboardConnected()
        {
            return Keyboard.current != null;
        }

        /// <summary>
        /// Checks if touch is available.
        /// </summary>
        public static bool IsTouchAvailable()
        {
            return Touchscreen.current != null;
        }

        /// <summary>
        /// Gets the primary touch position.
        /// </summary>
        public static Vector2 GetTouchPosition()
        {
            if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0)
                return Vector2.zero;

            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        /// <summary>
        /// Gets the number of active touches.
        /// </summary>
        public static int GetTouchCount()
        {
            return Touchscreen.current?.touches.Count ?? 0;
        }

        /// <summary>
        /// Applies a deadzone to a Vector2 input.
        /// </summary>
        public static Vector2 ApplyDeadzone(Vector2 input, float deadzone)
        {
            float magnitude = input.magnitude;
            if (magnitude < deadzone) return Vector2.zero;

            // Rescale input to 0-1 range after deadzone
            float normalizedMagnitude = (magnitude - deadzone) / (1f - deadzone);
            return input.normalized * normalizedMagnitude;
        }

        /// <summary>
        /// Applies a radial deadzone.
        /// </summary>
        public static Vector2 ApplyRadialDeadzone(Vector2 input, float innerDeadzone, float outerDeadzone = 1f)
        {
            float magnitude = input.magnitude;

            if (magnitude < innerDeadzone) return Vector2.zero;
            if (magnitude > outerDeadzone) return input.normalized;

            float normalizedMagnitude = (magnitude - innerDeadzone) / (outerDeadzone - innerDeadzone);
            return input.normalized * normalizedMagnitude;
        }

        /// <summary>
        /// Applies an axis deadzone (per-axis).
        /// </summary>
        public static Vector2 ApplyAxisDeadzone(Vector2 input, float deadzone)
        {
            return new Vector2(
                Mathf.Abs(input.x) < deadzone ? 0f : input.x,
                Mathf.Abs(input.y) < deadzone ? 0f : input.y
            );
        }

        /// <summary>
        /// Smooths input over time.
        /// </summary>
        public static Vector2 SmoothInput(Vector2 current, Vector2 target, float smoothTime, ref Vector2 velocity)
        {
            return new Vector2(
                Mathf.SmoothDamp(current.x, target.x, ref velocity.x, smoothTime),
                Mathf.SmoothDamp(current.y, target.y, ref velocity.y, smoothTime)
            );
        }
    }
}
