using System;
using System.Collections.Generic;
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
        private readonly List<RaycastResult> uiHits = new();
        private PointerEventData uiPointer;

        public Vector2 ScreenPosition => position.ReadValue<Vector2>();
        public bool WasPressedThisFrame => press.WasPressedThisFrame();
        public bool IsPressed => press.IsPressed();
        public bool WasReleasedThisFrame => press.WasReleasedThisFrame();

        /// <summary>
        /// True when the pointer is over UI, which then gets the touch instead of the board. It raycasts the UI itself:
        /// on the first frame of a touch the event system has not handled the touch yet and would answer no.
        /// </summary>
        public bool IsOverUI
        {
            get
            {
                var eventSystem = EventSystem.current;
                if (eventSystem == null)
                {
                    return false;
                }

                uiPointer ??= new PointerEventData(eventSystem);
                uiPointer.position = ScreenPosition;
                uiHits.Clear();
                eventSystem.RaycastAll(uiPointer, uiHits);
                return uiHits.Count > 0;
            }
        }

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
