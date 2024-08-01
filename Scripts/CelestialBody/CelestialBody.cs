
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
				CalculatePosition();
			}
		}

		private SphereMesh? _sphereMesh;
		public SphereMesh SphereMesh
		{
			get
			{
				_sphereMesh ??= (SphereMesh)Mesh;

				return _sphereMesh;
			}
		}

		public override void _Ready()
		{
		}

		public void CalculateSphereMeshProperties()
		{
			var radius = Details.EquatorialRadius <= 1 ? Details.PolarRadius : Details.EquatorialRadius;
			var height = (Details.PolarRadius <= 1 ? Details.EquatorialRadius : Details.PolarRadius) * 2;

			Mesh = new SphereMesh
			{
				Radius = ScaleRadius(radius),
				Height = ScaleRadius(height)
			};

			GD.Print($"{Details.EnglishName} has a radius of {radius} and a height of {height}");
			GD.Print($"{Details.EnglishName} sphere mesh has a radius of {SphereMesh.Radius} and a height of {SphereMesh.Height}");
		}

		public void CalculatePosition()
		{
			float semimajorAxis = Details.SemimajorAxis <= 1 ? 0 : ScaleAxis(Details.SemimajorAxis);
			Position = new Vector3(semimajorAxis, 0, 0);
			GD.Print($"{Details.EnglishName} at {Position} due to a semimajor axis of {Details.SemimajorAxis}");
		}

		float ScalePower(double value, double power)
		{
			return (float)(1.5 * Math.Pow(value, power));
		}

		float ScaleRadius(double radius)
		{
			return ScalePower(radius, 0.5);
		}

		float ScaleAxis(double axis)
		{
			return ScalePower(axis, 0.4);
		}
	}
}
