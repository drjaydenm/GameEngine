using System.Collections.Generic;
using Veldrid;

namespace GameEngine.Core.Input.Veldrid
{
    public class VeldridKeyboard : IKeyboard
    {
        private InputSnapshot currentSnapshot;
        private InputSnapshot previousSnapshot;
        private HashSet<Key> currentlyPressedKeys;
        private HashSet<Key> previouslyPressedKeys;

        public VeldridKeyboard()
        {
            currentlyPressedKeys = new HashSet<Key>();
            previouslyPressedKeys = new HashSet<Key>();
        }

        public void Update(InputSnapshot inputSnapshot)
        {
            previousSnapshot = currentSnapshot;
            currentSnapshot = inputSnapshot;

            previouslyPressedKeys = new HashSet<Key>(currentlyPressedKeys);

            for (var i = 0; i < currentSnapshot.KeyEvents.Count; i++)
            {
                var keyEvent = currentSnapshot.KeyEvents[i];
                if (keyEvent.Down && !currentlyPressedKeys.Contains(keyEvent.Key))
                {
                    currentlyPressedKeys.Add(keyEvent.Key);
                }
                else if (!keyEvent.Down && currentlyPressedKeys.Contains(keyEvent.Key))
                {
                    currentlyPressedKeys.Remove(keyEvent.Key);
                }
            }
        }

        public bool IsKeyDown(Keys key)
        {
            return currentlyPressedKeys.Contains(EngineKeysToVeldrid(key));
        }

        public bool IsKeyUp(Keys key)
        {
            return !IsKeyDown(key);
        }

        public bool WasKeyDown(Keys key)
        {
            return previouslyPressedKeys.Contains(EngineKeysToVeldrid(key));
        }

        public bool WasKeyUp(Keys key)
        {
            return !WasKeyDown(key);
        }

        public bool WasKeyHeld(Keys key)
        {
            return IsKeyDown(key) && WasKeyDown(key);
        }

        public bool WasKeyPressed(Keys key)
        {
            return IsKeyDown(key) && WasKeyUp(key);
        }

        public bool WasKeyReleased(Keys key)
        {
            return IsKeyUp(key) && WasKeyDown(key);
        }

        private static Key EngineKeysToVeldrid(Keys key)
        {
            return key switch
            {
                Keys.Back => Key.Back,
                Keys.Tab => Key.Tab,
                Keys.Enter => Key.Enter,
                Keys.Pause => Key.Pause,
                Keys.CapsLock => Key.CapsLock,
                Keys.Escape => Key.Escape,
                Keys.Space => Key.Space,
                Keys.PageUp => Key.PageUp,
                Keys.PageDown => Key.PageDown,
                Keys.End => Key.End,
                Keys.Home => Key.Home,
                Keys.Left => Key.Left,
                Keys.Up => Key.Up,
                Keys.Right => Key.Right,
                Keys.Down => Key.Down,
                Keys.PrintScreen => Key.PrintScreen,
                Keys.Insert => Key.Insert,
                Keys.Delete => Key.Delete,
                Keys.A => Key.A,
                Keys.B => Key.B,
                Keys.C => Key.C,
                Keys.D => Key.D,
                Keys.E => Key.E,
                Keys.F => Key.F,
                Keys.G => Key.G,
                Keys.H => Key.H,
                Keys.I => Key.I,
                Keys.J => Key.J,
                Keys.K => Key.K,
                Keys.L => Key.L,
                Keys.M => Key.M,
                Keys.N => Key.N,
                Keys.O => Key.O,
                Keys.P => Key.P,
                Keys.Q => Key.Q,
                Keys.R => Key.R,
                Keys.S => Key.S,
                Keys.T => Key.T,
                Keys.U => Key.U,
                Keys.V => Key.V,
                Keys.W => Key.W,
                Keys.X => Key.X,
                Keys.Y => Key.Y,
                Keys.Z => Key.Z,
                Keys.LeftWindows => Key.LWin,
                Keys.RightWindows => Key.RWin,
                Keys.Sleep => Key.Sleep,
                Keys.NumPad0 => Key.Keypad0,
                Keys.NumPad1 => Key.Keypad1,
                Keys.NumPad2 => Key.Keypad2,
                Keys.NumPad3 => Key.Keypad3,
                Keys.NumPad4 => Key.Keypad4,
                Keys.NumPad5 => Key.Keypad5,
                Keys.NumPad6 => Key.Keypad6,
                Keys.NumPad7 => Key.Keypad7,
                Keys.NumPad8 => Key.Keypad8,
                Keys.NumPad9 => Key.Keypad9,
                Keys.Multiply => Key.KeypadMultiply,
                Keys.Add => Key.KeypadAdd,
                Keys.Subtract => Key.KeypadSubtract,
                Keys.Decimal => Key.KeypadDecimal,
                Keys.Divide => Key.KeypadDivide,
                Keys.F1 => Key.F1,
                Keys.F2 => Key.F2,
                Keys.F3 => Key.F3,
                Keys.F4 => Key.F4,
                Keys.F5 => Key.F5,
                Keys.F6 => Key.F6,
                Keys.F7 => Key.F7,
                Keys.F8 => Key.F8,
                Keys.F9 => Key.F9,
                Keys.F10 => Key.F10,
                Keys.F11 => Key.F11,
                Keys.F12 => Key.F12,
                Keys.F13 => Key.F13,
                Keys.F14 => Key.F14,
                Keys.F15 => Key.F15,
                Keys.NumLock => Key.NumLock,
                Keys.Scroll => Key.ScrollLock,
                Keys.LeftShift => Key.LShift,
                Keys.RightShift => Key.RShift,
                Keys.LeftControl => Key.LControl,
                Keys.RightControl => Key.RControl,
                Keys.LeftAlt => Key.LAlt,
                Keys.RightAlt => Key.RAlt,
                Keys.Semicolon => Key.Semicolon,
                Keys.Plus => Key.Plus,
                Keys.Comma => Key.Comma,
                Keys.Minus => Key.Minus,
                Keys.Period => Key.Period,
                Keys.Tilde => Key.Tilde,
                Keys.OpenBrackets => Key.BracketLeft,
                Keys.CloseBrackets => Key.BracketRight,
                Keys.Quotes => Key.Quote,
                Keys.Backslash => Key.BackSlash,
                Keys.Slash => Key.Slash,
                _ => throw new System.NotImplementedException("Unrecognized key was requested")
            };
        }
    }
}
