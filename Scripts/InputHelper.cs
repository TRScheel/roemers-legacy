using Godot;

namespace RoemersLegacy.Scripts
{
    public static class InputHelper
    {
        public static bool IsActionPressedOrHeld(string action)
        {
            return Input.IsActionJustPressed(action) || Input.IsActionPressed(action);
        }
    }
}