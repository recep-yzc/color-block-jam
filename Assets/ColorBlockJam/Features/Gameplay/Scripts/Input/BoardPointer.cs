using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The one pointer the board listens to (mouse or first touch), read through the Input System.
    /// </summary>
    public sealed class BoardPointer : IInitializable, IDisposable
    {
        private readonly InputAction position = new("Point", InputActionType.Value, "<Pointer>/position");
        private readonly InputAction press = new("Press", InputActionType.Button, "<Pointer>/press");

        public Vector2 ScreenPosition => position.ReadValue<Vector2>();
        public bool WasPressedThisFrame => press.WasPressedThisFrame();
        public bool IsPressed => press.IsPressed();
        public bool WasReleasedThisFrame => press.WasReleasedThisFrame();

        /// <summary>True when the pointer is over UI, which then gets the touch instead of the board.</summary>
        public bool IsOverUi => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        public void Initialize()
        {
            position.Enable();
            press.Enable();
        }

        public void Dispose()
        {
            position.Dispose();
            press.Dispose();
        }
    }
}
