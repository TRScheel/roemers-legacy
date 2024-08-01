using Godot;
using System;

public partial class CelestialBodyComponent : Node
{
	private float semiMajorAxisMillionKilometers = 1.0f;
	private float eccentricity = 0.0f;
	private float inclinationDegrees = 0.0f;
	private float longitudeOfAscendingNodeDegrees = 0.0f;
	private float argumentOfPeriapsisDegrees = 0.0f;
	private Vector3 rotationAxis = new Vector3(0.0f, 1.0f, 0.0f);
	private float axialTiltDegrees = 0.0f;
	private Vector3 initialRotationDegrees = Vector3.Zero;
	private float rotationPeriodHours = 24.0f;

	[Export]
	public float SemiMajorAxisMillionKilometers
	{
		get => semiMajorAxisMillionKilometers;
		set
		{
			semiMajorAxisMillionKilometers = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public float Eccentricity
	{
		get => eccentricity;
		set
		{
			eccentricity = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public float InclinationDegrees
	{
		get => inclinationDegrees;
		set
		{
			inclinationDegrees = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public float LongitudeOfAscendingNodeDegrees
	{
		get => longitudeOfAscendingNodeDegrees;
		set
		{
			longitudeOfAscendingNodeDegrees = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public float ArgumentOfPeriapsisDegrees
	{
		get => argumentOfPeriapsisDegrees;
		set
		{
			argumentOfPeriapsisDegrees = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public Vector3 RotationAxis
	{
		get => rotationAxis;
		set
		{
			rotationAxis = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public float AxialTiltDegrees
	{
		get => axialTiltDegrees;
		set
		{
			axialTiltDegrees = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public Vector3 InitialRotationDegrees
	{
		get => initialRotationDegrees;
		set
		{
			initialRotationDegrees = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public float RotationPeriodHours
	{
		get => rotationPeriodHours;
		set
		{
			rotationPeriodHours = value;
			UpdateCachedValues();
		}
	}

	[Export]
	public Node3D? CentralBody { get; set; } // The body around which this object orbits

	private Node3D? celestialObject;

	// Cached values for repeated calculations
	private double semiMajorAxisMeters;
	private long rotationPeriodTicks;
	private long orbitalPeriodTicks;
	private double inclinationRadians;
	private double longitudeOfAscendingNodeRadians;
	private double argumentOfPeriapsisRadians;
	private double axialTiltRadians;

	private long lastElapsedTimeTicks;

	private Vector3 position = new Vector3();

	public override void _Ready()
	{
		celestialObject = (Node3D)GetParent();

		// Apply initial rotation
		celestialObject.RotateX(Mathf.DegToRad(initialRotationDegrees.X));
		celestialObject.RotateY(Mathf.DegToRad(initialRotationDegrees.Y));
		celestialObject.RotateZ(Mathf.DegToRad(initialRotationDegrees.Z));

		// Cache repeated calculations
		UpdateCachedValues();

		lastElapsedTimeTicks = GameTimeManager.Instance.GetElapsedTime();
	}

	public override void _Process(double delta)
	{
		if (CentralBody is null)
		{
			GD.PrintErr("CentralBody is not set for ", celestialObject.Name);
			return;
		}

		long elapsedTimeTicks = GameTimeManager.Instance.GetElapsedTime();

		// Only update if the game time has changed
		if (elapsedTimeTicks != lastElapsedTimeTicks)
		{
			UpdateOrbit(elapsedTimeTicks);
			UpdateRotation(elapsedTimeTicks);
			lastElapsedTimeTicks = elapsedTimeTicks;
		}

		celestialObject.Transform = CentralBody.Transform.Translated(position);
	}

	private void UpdateCachedValues()
	{
		semiMajorAxisMeters = semiMajorAxisMillionKilometers * 1_000_000;
		rotationPeriodTicks = (long)(rotationPeriodHours * 3600 * TimeSpan.TicksPerSecond);
		inclinationRadians = Mathf.DegToRad(inclinationDegrees);
		longitudeOfAscendingNodeRadians = Mathf.DegToRad(longitudeOfAscendingNodeDegrees);
		argumentOfPeriapsisRadians = Mathf.DegToRad(argumentOfPeriapsisDegrees);
		axialTiltRadians = Mathf.DegToRad(axialTiltDegrees);

		// Assuming Kepler's third law for orbital period: T^2 = a^3 (T in years, a in AU)
		// Convert semiMajorAxis from meters to astronomical units (AU) and compute period in ticks
		double semiMajorAxisAU = semiMajorAxisMeters / 1.496e+11;
		double orbitalPeriodYears = Math.Sqrt(semiMajorAxisAU * semiMajorAxisAU * semiMajorAxisAU);
		double orbitalPeriodSeconds = orbitalPeriodYears * 365.25 * 24 * 3600;
		orbitalPeriodTicks = (long)(orbitalPeriodSeconds * TimeSpan.TicksPerSecond);
	}

	private void UpdateOrbit(long elapsedTimeTicks)
	{
		// Normalize elapsed time to the orbital period
		double meanAnomaly = (elapsedTimeTicks % orbitalPeriodTicks) / (double)orbitalPeriodTicks * Math.Tau;

		// Calculate the Eccentric Anomaly (E) from the Mean Anomaly (M) using Newton's method
		double E = meanAnomaly;
		for (int i = 0; i < 5; i++) // Iterative approximation
		{
			E = meanAnomaly + Eccentricity * Math.Sin(E);
		}

		// Calculate the True Anomaly (ν) from the Eccentric Anomaly (E)
		double trueAnomaly = 2.0 * Math.Atan2(
			Math.Sqrt(1.0 + Eccentricity) * Math.Sin(E / 2.0),
			Math.Sqrt(1.0 - Eccentricity) * Math.Cos(E / 2.0)
		);

		// Calculate the distance (r) from the central body in meters
		double distance = semiMajorAxisMeters * (1 - Eccentricity * Math.Cos(E));

		// Position in the orbital plane
		double x = distance * Math.Cos(trueAnomaly);
		double y = distance * Math.Sin(trueAnomaly);

		// Convert to 3D space using orbital elements
		position = new Vector3((float)x, (float)y, 0);
		position = ApplyRotation(position, longitudeOfAscendingNodeRadians, new Vector3(0, 1, 0)); // Rotate by Ω around Y-axis
		position = ApplyRotation(position, inclinationRadians, new Vector3(1, 0, 0)); // Rotate by i around X-axis
		position = ApplyRotation(position, argumentOfPeriapsisRadians, new Vector3(0, 0, 1)); // Rotate by ω around Z-axis
	}

	private void UpdateRotation(long elapsedTimeTicks)
	{
		// Normalize elapsed time to the rotation period
		double rotationFraction = (elapsedTimeTicks % rotationPeriodTicks) / (double)rotationPeriodTicks;
		double angle = rotationFraction * Math.Tau; // Tau is 2*Pi

		// Apply axial tilt
		var tiltedAxis = new Vector3(
			RotationAxis.X * (float)Math.Cos(axialTiltRadians),
			RotationAxis.Y,
			RotationAxis.Z * (float)Math.Sin(axialTiltRadians)
		).Normalized();

		celestialObject.GlobalTransform.Rotated(tiltedAxis, (float)angle);
	}

	private Vector3 ApplyRotation(Vector3 position, double angleRadians, Vector3 axis)
	{
		Quaternion rotation = new Quaternion(axis, (float)angleRadians);
		return rotation * position;
	}
}
