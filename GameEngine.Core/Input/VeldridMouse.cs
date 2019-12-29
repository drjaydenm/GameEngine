﻿using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace GameEngine.Core.Input
{
    public class VeldridMouse : IMouse
    {
        public bool IsMouseLocked { get; set; }

        public Point Position => new Point((int)currentSnapshot.MousePosition.X, (int)currentSnapshot.MousePosition.Y);
        public Point PositionDelta
        {
            get
            {
                var delta = currentMousePosition - previousMousePosition;
                return new Point((int)delta.X, (int)delta.Y);
            }
        }
        public float ScrollDelta => currentSnapshot.WheelDelta;

        private InputSnapshot currentSnapshot;
        private InputSnapshot previousSnapshot;
        private Vector2 currentMousePosition;
        private Vector2 previousMousePosition;
        private HashSet<MouseButton> currentlyPressedButtons;
        private HashSet<MouseButton> previouslyPressedButtons;

        public VeldridMouse()
        {
            currentlyPressedButtons = new HashSet<MouseButton>();
            previouslyPressedButtons = new HashSet<MouseButton>();
        }

        public void Update(InputSnapshot inputSnapshot, Vector2 mousePosition)
        {
            previousSnapshot = currentSnapshot;
            currentSnapshot = inputSnapshot;

            previousMousePosition = currentMousePosition;
            currentMousePosition = mousePosition;

            previouslyPressedButtons = new HashSet<MouseButton>(currentlyPressedButtons);

            foreach (var mouseEvent in currentSnapshot.MouseEvents)
            {
                if (mouseEvent.Down && !currentlyPressedButtons.Contains(mouseEvent.MouseButton))
                {
                    currentlyPressedButtons.Add(mouseEvent.MouseButton);
                }
                else if (!mouseEvent.Down && currentlyPressedButtons.Contains(mouseEvent.MouseButton))
                {
                    currentlyPressedButtons.Remove(mouseEvent.MouseButton);
                }
            }
        }

        public bool IsButtonDown(MouseButtons button)
        {
            return currentlyPressedButtons.Contains(EngineMouseButtonToVeldrid(button));
        }

        public bool IsButtonUp(MouseButtons button)
        {
            return !IsButtonDown(button);
        }

        public bool WasButtonDown(MouseButtons button)
        {
            return previouslyPressedButtons.Contains(EngineMouseButtonToVeldrid(button));
        }

        public bool WasButtonUp(MouseButtons button)
        {
            return !WasButtonDown(button);
        }

        public bool WasButtonHeld(MouseButtons button)
        {
            return IsButtonDown(button) && WasButtonDown(button);
        }

        public bool WasButtonPressed(MouseButtons button)
        {
            return IsButtonDown(button) && WasButtonUp(button);
        }

        public bool WasButtonReleased(MouseButtons button)
        {
            return IsButtonUp(button) && WasButtonDown(button);
        }

        private static MouseButton EngineMouseButtonToVeldrid(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => MouseButton.Left,
                MouseButtons.Right => MouseButton.Right,
                MouseButtons.Middle => MouseButton.Middle,
                _ => throw new System.NotImplementedException("Unrecognized mouse button was requested")
            };
        }
    }
}
