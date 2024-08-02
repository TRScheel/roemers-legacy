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

		public override void _PhysicsProcess(double delta)
		{
			// Update the position and rotation of the celestial body
			//CelestialBodyMesh.Position = CalculatePosition();
			//CelestialBodyMesh.Transform = UpdateRotation();

			TestShaderVariables();
		}

		private void TestShaderVariables()
		{
			if (_shaderMaterial is null)
			{
				return;
			}

			bool shaderVariableChanged = false;
			if (Input.IsActionJustReleased("camera_rotate_ccw"))
			{
				TimeSpeed += 0.01f;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustReleased("camera_pan_left"))
			{
				TimeSpeed -= 0.01f;
				shaderVariableChanged = true;
			}

			if (Input.IsActionJustReleased("camera_pan_in"))
			{
				UVScale += 0.1f;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustReleased("camera_pan_out"))
			{
				UVScale -= 0.1f;
				shaderVariableChanged = true;
			}

			if (Input.IsActionJustReleased("camera_pan_right"))
			{
				NoiseStrength -= 0.1f;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustReleased("camera_rotate_cw"))
			{
				NoiseStrength += 0.1f;
				shaderVariableChanged = true;
			}

			if (Input.IsActionJustReleased("camera_pitch_up"))
			{
				RedBoost += 0.1f;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustReleased("camera_pitch_down"))
			{
				RedBoost -= 0.1f;
				shaderVariableChanged = true;
			}

			if (Input.IsActionJustReleased("camera_speed_scale_increase"))
			{
				CellAmount += 5;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustReleased("camera_speed_scale_decrease"))
			{
				CellAmount -= 5;
				shaderVariableChanged = true;
			}

			if (Input.IsActionJustPressed("camera_pan_speed_increase"))
			{
				Period.X += 1;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustPressed("camera_pan_speed_decrease"))
			{
				Period.X -= 1;
				shaderVariableChanged = true;
			}

			if (Input.IsActionJustPressed("camera_rotate_speed_increase"))
			{
				Period.Y += 1;
				shaderVariableChanged = true;
			}
			else if (Input.IsActionJustPressed("camera_rotate_speed_decrease"))
			{
				Period.Y -= 1;
				shaderVariableChanged = true;
			}

			if (shaderVariableChanged)
			{
				UpdateShaderParams();
			}
		}

		private void UpdateShaderParams()
		{
			if (_shaderMaterial is null)
			{
				return;
			}

			_shaderMaterial.SetShaderParameter("time_speed", TimeSpeed);
			_shaderMaterial.SetShaderParameter("uv_scale", UVScale);
			_shaderMaterial.SetShaderParameter("red_boost", RedBoost);
			_shaderMaterial.SetShaderParameter("noise_strength", NoiseStrength);
			_shaderMaterial.SetShaderParameter("cell_amount", CellAmount);
			_shaderMaterial.SetShaderParameter("period", Period);
			GD.Print($"TimeSpeed: {TimeSpeed:F1}\tUVScale: {UVScale:F1}\tRedBoost: {RedBoost:F1}\tNoiseStrength: {NoiseStrength:F1}\tCellAmount: {CellAmount}\tPeriod: {Period}");
		}


		private Transform3D UpdateRotation()
		{
			if (Details.SideralRotation <= float.Epsilon)
			{
				return CelestialBodyMesh.Transform; // Skip bodies with no rotation
			}

			DateTimeOffset currentTime = GameTimeManager.Instance.CurrentTime;

			// Calculate the elapsed time in seconds since some reference point (e.g., game start)
			double elapsedSeconds = (currentTime - Details.CachedPeriapsisEpoch).TotalSeconds;

			// Calculate rotation angle in radians based on the sidereal rotation period (converted to seconds)
			float rotationAngle = Mathf.DegToRad((float)((360.0 / Details.SideralRotation) * (elapsedSeconds / 3600.0)));
			rotationAngle = Mathf.Wrap(rotationAngle, 0, Mathf.Pi * 2);

			// Apply axial tilt
			float axialTiltRadians = Mathf.DegToRad((float)(Details.AxialTilt ?? 0f));

			// Create a quaternion for axial tilt and rotation
			Quaternion tiltQuaternion = new Quaternion(Vector3.Right, axialTiltRadians);
			Quaternion rotationQuaternion = new Quaternion(Vector3.Up, rotationAngle);

			// Combine the rotations
			Quaternion combinedRotation = tiltQuaternion * rotationQuaternion;

			// Apply the combined rotation
			Transform3D transform = CelestialBodyMesh.Transform;
			transform.Basis = new Basis(combinedRotation);
			return transform;
		}

		private ShaderMaterial? _shaderMaterial;
		private float TimeSpeed = 0.01f;
		private float UVScale = 2.3f;
		private float RedBoost = 1f;
		private float NoiseStrength = 1.4f;
		private int CellAmount = 25;
		private Vector2 Period = new Vector2(10, 15);

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
			_shaderMaterial = CelestialBodyMesh.MaterialOverride as ShaderMaterial;
			UpdateShaderParams();

			var coronaMesh = new MeshInstance3D();
			coronaMesh.Mesh = new SphereMesh()
			{
				Radius = SphereMesh.Radius * 1.007f, // Slightly larger than the sun
				Height = SphereMesh.Height * 1.007f,
			};
			//CelestialBodyMesh.AddChild(coronaMesh);
			// Create the shader material for the corona
			var coronaShaderMaterial = new ShaderMaterial();
			coronaShaderMaterial.Shader = ResourceLoader.Load<Shader>("res://Shaders/Corona.gdshader");
			coronaMesh.MaterialOverride = coronaShaderMaterial;
			coronaMesh.Layers = 1 << 1;
			coronaMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		}

		private void RecalculateProperties()
		{
			Name = Details.Id;
			CalculateSphereMeshProperties();
			CelestialBodyMesh.Position = CalculatePosition();
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

		private Vector3 CalculatePosition(DateTimeOffset? timestamp = null)
		{
			if (Details.SemimajorAxis <= 1)
			{
				return Vector3.Zero; // Skip bodies with insignificant orbits
			}

			// Use provided timestamp or current game time
			DateTimeOffset currentTime = timestamp ?? GameTimeManager.Instance.CurrentTime;

			// Time since periapsis
			TimeSpan timeSincePeriapsis = currentTime - Details.CachedPeriapsisEpoch;

			// Mean anomaly, ensuring it's within the range 0 to 2π
			double meanAnomaly = Details.MainAnomaly + Details.CachedMeanMotion * timeSincePeriapsis.TotalDays;
			meanAnomaly = meanAnomaly % (2 * Math.PI); // Normalize to [0, 2π]

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
			double Omega = Mathf.DegToRad((float)Details.DoubleitudeOfAscendingNode);

			position = RotatePoint(position, Vector3.Up, omega);
			position = RotatePoint(position, Vector3.Left, i);
			position = RotatePoint(position, Vector3.Up, Omega);

			// if (Details.Id == "jupiter")
			// {
			// 	GD.Print($"currentTime: {currentTime}\ttimeSincePeriapsis: {timeSincePeriapsis.TotalDays:F2}\tmeanAnomaly: {meanAnomaly:F2}\teccentricAnomaly: {eccentricAnomaly:F2}\ttrueAnomaly: {trueAnomaly:F2}\tr: {r:F2}\tscaledDistance: {scaledDistance:F2}\tposition: {position}");
			// }

			return position;
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
