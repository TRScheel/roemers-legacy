
using System;
using Godot;

namespace RoemersLegacy.Scripts.CelestialBody
{
    public partial class CelestialBody : MeshInstance3D
    {
        private CelestialBodyDetails _details = CelestialBodyDetails.Empty;
        public CelestialBodyDetails Details
        {
            get => _details;
            set
            {
                _details = value;
                CalculateSphereMeshProperties();
            }
        }

        private SphereMesh? _sphereMesh;
        public SphereMesh SphereMesh
        {
            get
            {
                _sphereMesh ??= GetNode<SphereMesh>("SphereMesh");

                return _sphereMesh;
            }
        }

        public override void _Ready()
        {

        }

        public void CalculateSphereMeshProperties()
        {
            SphereMesh.Radius = (float)Math.Log10(Details.EquatorialRadius);
            SphereMesh.Height = (float)Math.Log10(Details.PolarRadius * 2);
        }
    }
}