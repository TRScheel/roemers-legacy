using System;
using Godot;

namespace RoemersLegacy.Scripts.CelestialBody
{
	public partial class CelestialBody : Node3D
	{
		private CelestialBodyDetails _details = new(CelestialBodyDetails.Empty);
		public CelestialBodyDetails Details
		{
			get => _details;
			set
			{
				_details = value;
				RecalculateProperties();
			}
		}

		private SphereMesh? _sphereMesh;
		public SphereMesh SphereMesh
		{
			get
			{
				_sphereMesh ??= (SphereMesh)CelestialBodyMesh.Mesh;

				return _sphereMesh;
			}
		}

		private MeshInstance3D? _celestialBodyMesh;
		public MeshInstance3D CelestialBodyMesh
		{
			get
			{
				_celestialBodyMesh ??= (MeshInstance3D)GetNode("CelestialBodyMesh");

				return _celestialBodyMesh;
			}
		}

		private OrbitPath? _orbitPath;
		public OrbitPath OrbitPath
		{
			get
			{
				_orbitPath ??= (OrbitPath)GetNode("OrbitPath");

				return _orbitPath;
			}
		}

		private bool IsTheSun => string.Equals(Details.Id, "sun", StringComparison.InvariantCultureIgnoreCase);

		public override void _Ready()
		{
			SetupSun();
		}

		private void SetupSun()
		{
			if (!IsTheSun)
			{
				return;
			}

			// Load the shader and texture
			var shaderMaterial = new ShaderMaterial();
			shaderMaterial.Shader = ResourceLoader.Load<Shader>("res://Shaders/DynamicSun.gdshader");
			Texture2D texture = ResourceLoader.Load<Texture2D>("res://Textures/CelestialBody/8k_sun.jpg");

			// Assign the texture to the shader material
			shaderMaterial.SetShaderParameter("sun_texture", texture);

			// Assign the shader material to the mesh
			CelestialBodyMesh.MaterialOverride = shaderMaterial;

			var coronaMesh = new MeshInstance3D();
			coronaMesh.Mesh = new SphereMesh()
			{
				Radius = SphereMesh.Radius * 1.006f, // Slightly larger than the sun
				Height = SphereMesh.Height * 1.006f,
			};
			AddChild(coronaMesh);// Create the shader material for the corona
			var coronaShaderMaterial = new ShaderMaterial();
			coronaShaderMaterial.Shader = ResourceLoader.Load<Shader>("res://Shaders/Corona.gdshader");
			coronaMesh.MaterialOverride = coronaShaderMaterial;
			coronaMesh.Layers = 1 << 1;
			coronaMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		}

		private void TryGPUParticles()
		{
			var particleEffects = new GpuParticles3D();
			particleEffects.Amount = 200;
			particleEffects.Lifetime = 5;
			particleEffects.Preprocess = 1;

			var curve = new Curve();
			// Add points to the curve with their respective tangents
			curve.AddPoint(new Vector2(0.0f, 0.0f), 0.0f, 0.5f);   // Starting point
			curve.AddPoint(new Vector2(0.2f, 0.8f), 0.5f, 0.5f);   // Rise with gentle slope
			curve.AddPoint(new Vector2(0.5f, 1.0f), 0.5f, 0.0f);   // Peak with zero slope
			curve.AddPoint(new Vector2(0.7f, 0.7f), -0.5f, -0.5f); // Fall with gentle slope
			curve.AddPoint(new Vector2(1.0f, 0.0f), -0.5f, 0.0f);  // Ending point back to base

			// Optionally, you can bake the curve for better performance if sampling frequently
			curve.BakeResolution = 100; // Define the resolution of baked samples
			curve.Bake();

			particleEffects.DrawPass1 = new TubeTrailMesh
			{
				Radius = 5,
				RadialSteps = 8,
				Material = new StandardMaterial3D()
				{
					VertexColorUseAsAlbedo = true,
					UseParticleTrails = true,
					EmissionEnabled = true,
					Emission = new Color(1, 0.5f, 0, 0.5f)
				},
				SectionLength = 15,
				Sections = 10,
				SectionRings = 5,
				Curve = curve
			};
			particleEffects.Emitting = true;
			particleEffects.TrailEnabled = true;
			particleEffects.TrailLifetime = 5;

			var particlesMaterial = new ParticleProcessMaterial();
			var gradientTexture = new GradientTexture2D();
			var gradient = new Gradient()
			{
				Colors = new[]
				{
						new Color(1, 1, 1, 1),   // Start with full white
						new Color(1, 0.5f, 0, 0.5f), // Transition to orange
						new Color(1, 0, 0, 0)   // Fade to transparent red
					}
			};
			gradientTexture.Gradient = gradient;

			particlesMaterial.ColorRamp = gradientTexture;
			// particlesMaterial.ScaleMin = 0.9f;
			// particlesMaterial.ScaleMax = 1.1f;
			particlesMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.SphereSurface;
			particlesMaterial.EmissionSphereRadius = SphereMesh.Radius;
			particlesMaterial.Gravity = Vector3.Zero;
			particlesMaterial.RadialAccelMin = -10f;
			particlesMaterial.RadialAccelMax = -20f;
			particlesMaterial.RadialVelocityMin = 20f;
			particlesMaterial.RadialVelocityMax = 30;
			particlesMaterial.Spread = 45;
			// particlesMaterial.ScaleCurve
			// set Use As Albedo

			//particlesMaterial.color


			// // Set Visibility AABB manually
			// Vector3 aabbExtent = Vector3.One * SphereMesh.Radius * 1.1f; // Adjust based on your specific needs
			// particleEffects.VisibilityAabb = new Aabb(-aabbExtent, aabbExtent * 2);

			particleEffects.ProcessMaterial = particlesMaterial;
			CelestialBodyMesh.AddChild(particleEffects);
		}

		private void TryCPUParticles()
		{
			var particleEffects = new CpuParticles3D();
			particleEffects.Amount = 100;
			particleEffects.Lifetime = 5;
			particleEffects.Preprocess = 5;
			particleEffects.EmissionShape = CpuParticles3D.EmissionShapeEnum.Sphere;
			particleEffects.EmissionSphereRadius = SphereMesh.Radius;
			particleEffects.InitialVelocityMin = (float)(SphereMesh.Radius * 0.05);
			particleEffects.InitialVelocityMax = (float)(SphereMesh.Radius * 0.15);
			particleEffects.Gravity = Vector3.Zero;
			particleEffects.RadialAccelMin = 0.1f;
			particleEffects.RadialAccelMax = 0.5f;

			var particlesMaterial = new ParticleProcessMaterial();
			var gradientTexture = new GradientTexture2D();
			var gradient = new Gradient()
			{
				Colors = new[]
				{
						new Color(1, 1, 1, 1),   // Start with full white
						new Color(1, 0.5f, 0, 0.5f), // Transition to orange
						new Color(1, 0, 0, 0)   // Fade to transparent red
					}
			};
			gradientTexture.Gradient = gradient;

			particlesMaterial.ColorRamp = gradientTexture;
			particlesMaterial.ScaleMin = 5f;
			particlesMaterial.ScaleMax = 15f;
			// particlesMaterial.ScaleCurve

			//particleEffects.MaterialOverride = particlesMaterial;
			particleEffects.Emitting = true;
			AddChild(particleEffects);
		}

		private void RecalculateProperties()
		{
			Name = Details.Id;
			CalculateSphereMeshProperties();
			CalculatePosition();
			OrbitPath.RegeneratePath(Details);
			ApplyTextures();
		}

		private void CalculateSphereMeshProperties()
		{
			var radius = Details.EquatorialRadius <= 1 ? Details.PolarRadius : Details.EquatorialRadius;
			var height = (Details.PolarRadius <= 1 ? Details.EquatorialRadius : Details.PolarRadius) * 2;

			var sphere = new SphereMesh
			{
				Radius = Utilities.ScaleCelestialBody(radius),
				Height = Utilities.ScaleCelestialBody(height),
				RadialSegments = 128,
				Rings = 128
			};

			StandardMaterial3D material = new StandardMaterial3D
			{
				AlbedoColor = new Color(0.5f, 0.5f, 0.5f),
				Metallic = 0.0f,
				Roughness = 0.9f, // Increase roughness to soften shadows
				MetallicSpecular = 0.1f // Lower specular reflection for softer lighting
			};

			CelestialBodyMesh.MaterialOverride = material;
			CelestialBodyMesh.Mesh = sphere;
		}

		public void ApplyTextures()
		{
			var textureTypes = new[] { "map", "spec", "normal" };
			var material = new StandardMaterial3D();

			foreach (var textureType in textureTypes)
			{
				string texturePath = $"res://Textures/CelestialBody/{Details.Id}_{textureType}.jpg";
				if (ResourceLoader.Exists(texturePath))
				{
					Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);

					switch (textureType)
					{
						case "map":
							material.AlbedoTexture = texture;
							break;
						case "spec":
							material.MetallicTexture = texture;
							break;
						case "normal":
							material.NormalTexture = texture;
							break;
					}
				}
			}

			CelestialBodyMesh.Mesh = SphereMesh;
			CelestialBodyMesh.MaterialOverride = material;
		}

		private void CalculatePosition()
		{
			if (Details.SemimajorAxis <= 1)
			{
				return;
			}

			DateTimeOffset currentTime = GameTimeManager.Instance.CurrentTime;

			// Time since periapsis
			TimeSpan timeSincePeriapsis = currentTime - Details.CachedPeriapsisEpoch;

			// Mean anomaly
			double meanAnomaly = Details.MainAnomaly + Details.CachedMeanMotion * timeSincePeriapsis.TotalDays;

			// Solve Kepler's equation to find eccentric anomaly
			double eccentricAnomaly = SolveKeplerEquation(meanAnomaly, Details.Eccentricity);

			// Calculate true anomaly
			double trueAnomaly = 2 * Math.Atan2(
				Math.Sqrt(1 + Details.Eccentricity) * Math.Sin(eccentricAnomaly / 2),
				Math.Sqrt(1 - Details.Eccentricity) * Math.Cos(eccentricAnomaly / 2));

			// Calculate distance from the central body and scale it
			double r = Details.SemimajorAxis * (1 - Details.Eccentricity * Math.Cos(eccentricAnomaly));
			float scaledDistance = Utilities.ScaleDistance(r);

			// Position in orbital plane (scaled)
			Vector3 position = new Vector3(
				scaledDistance * (float)Math.Cos(trueAnomaly),
				0,
				scaledDistance * (float)Math.Sin(trueAnomaly)
			);

			// Rotate to 3D space
			double i = Mathf.DegToRad((float)Details.Inclination);
			double omega = Mathf.DegToRad((float)Details.ArgumentOfPeriapsis);
			double Omega = Mathf.DegToRad((float)Details.doubleitudeOfAscendingNode);

			position = RotatePoint(position, Vector3.Up, omega);
			position = RotatePoint(position, Vector3.Left, i);
			position = RotatePoint(position, Vector3.Up, Omega);

			// Set the position of the celestial body mesh
			CelestialBodyMesh.Position = position;
		}

		// Helper method to solve Kepler's equation using Newton's method
		private double SolveKeplerEquation(double meanAnomaly, double eccentricity)
		{
			double epsilon = 1e-6; // Convergence tolerance
			int maxIterations = 100; // Maximum number of iterations
			double eccentricAnomaly = meanAnomaly; // Initial guess

			for (int iteration = 0; iteration < maxIterations; iteration++)
			{
				double f = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly;
				double fPrime = 1 - eccentricity * Math.Cos(eccentricAnomaly);

				double delta = f / fPrime;
				eccentricAnomaly -= delta;

				if (Math.Abs(delta) < epsilon)
				{
					break; // Converged to a solution
				}
			}

			return eccentricAnomaly;
		}

		// Helper method to rotate a point around an axis
		private Vector3 RotatePoint(Vector3 point, Vector3 axis, double angle)
		{
			return point.Rotated(axis, (float)angle);
		}
	}
}
