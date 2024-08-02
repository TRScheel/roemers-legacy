using Godot;
using System;
using System.Linq;

namespace RoemersLegacy.Scripts.CelestialBody
{

	public partial class OrbitPath : Node3D
	{
		const int Points = 100;

		private Path3D? _path;
		public Path3D Path
		{
			get
			{
				_path ??= (Path3D)GetChildren().First(child => child is Path3D);

				return _path;
			}
		}

		private MeshInstance3D? _meshInstance;
		public MeshInstance3D MeshInstance
		{
			get
			{
				_meshInstance ??= (MeshInstance3D)Path.GetChildren().First(child => child is MeshInstance3D);

				return _meshInstance;
			}
		}

		private Curve3D? _curve;
		public Curve3D Curve
		{
			get
			{
				_curve ??= Path.Curve;

				return _curve;
			}

			private set
			{
				_curve = Path.Curve = value;
			}
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			MeshInstance.Visible = false;
		}

		public void RegeneratePath(CelestialBodyDetails bodyDetails)
		{
			if (bodyDetails.SemimajorAxis <= 1)
			{
				return;
			}
			CalculateOrbitPath(bodyDetails);
			CreateOrbitMesh();
		}

		private void CalculateOrbitPath(CelestialBodyDetails bodyDetails)
		{
			// Create a new Curve3D
			var curve = new Curve3D();

			// Extract and scale orbital parameters
			double a = bodyDetails.SemimajorAxis; // Scaled semi-major axis
			double e = bodyDetails.Eccentricity;  // Eccentricity
			double i = Mathf.DegToRad((float)bodyDetails.Inclination); // Inclination in radians
			double omega = Mathf.DegToRad((float)bodyDetails.ArgumentOfPeriapsis); // Argument of periapsis in radians
			double Omega = Mathf.DegToRad((float)bodyDetails.doubleitudeOfAscendingNode); // Longitude of ascending node in radians

			for (int j = 0; j <= Points; j++)
			{
				double theta = Mathf.Tau * j / Points; // True anomaly
				double r = a * (1 - e * e) / (1 + e * Mathf.Cos((float)theta)); // Scaled radius
				var scaledDistance = Utilities.ScaleDistance(r);

				// Calculate position in orbital plane
				Vector3 point = new Vector3(
					(float)(scaledDistance * Math.Cos(theta)),
					0,
					(float)(scaledDistance * Math.Sin(theta))
				);

				// Rotate by argument of periapsis (ω)
				point = RotatePoint(point, Vector3.Up, omega);

				// Rotate by inclination (i)
				point = RotatePoint(point, Vector3.Left, i);

				// Rotate by longitude of ascending node (Ω)
				point = RotatePoint(point, Vector3.Up, Omega);

				// Add point to curve
				curve.AddPoint(point);
			}

			curve.BakeInterval = 0.1f;

			// Set the curve
			Curve = curve;
		}


		// Helper method to rotate a point around an axis
		private Vector3 RotatePoint(Vector3 point, Vector3 axis, double angle)
		{
			return point.Rotated(axis, (float)angle);
		}

		private void CreateOrbitMeshLineStrip()
		{
			// Use SurfaceTool to draw a continuous line strip along the path
			var surfaceTool = new SurfaceTool();
			surfaceTool.Begin(Mesh.PrimitiveType.LineStrip);

			Color color = new Color(1, 1, 0);  // Yellow
			var bakedPoints = Curve.GetBakedPoints();

			// Iterate through each point and add it to the line strip
			foreach (Vector3 point in bakedPoints)
			{
				surfaceTool.SetColor(color);
				surfaceTool.AddVertex(point);
			}

			var shaderMaterial = new ShaderMaterial();
			shaderMaterial.Shader = ResourceLoader.Load<Shader>("res://Shaders/OrbitPath.gdshader");
			shaderMaterial.SetShaderParameter("min_width", 2.0f); // Adjust as needed
			shaderMaterial.SetShaderParameter("line_color", new Color(1, 1, 0));

			// Assign the mesh to the MeshInstance3D
			MeshInstance.Mesh = surfaceTool.Commit();
			MeshInstance.MaterialOverride = shaderMaterial;
			MeshInstance.Layers = 1 << 1;
			MeshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		}

		private void CreateOrbitMesh()
		{
			CreateOrbitMeshLineStrip();
		}
	}

}
