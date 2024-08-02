using Godot;
using System;

namespace RoemersLegacy.Scripts
{
    public static class Utilities
    {
        private static float ScalePower(double value, double power)
        {
            return (float)(1.5 * Math.Pow(value, power));
        }

        public static float ScaleCelestialBody(double radius)
        {
            return ScalePower(radius, 0.5);
        }

        public static float ScaleDistance(double axis)
        {
            return ScalePower(axis, 0.4);
        }
    }
}