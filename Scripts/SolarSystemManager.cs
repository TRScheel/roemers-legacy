using Godot;
using System;

public partial class SolarSystemManager : Node3D
{
	private MeshInstance3D sun;
	private MeshInstance3D planet;

	public override void _Ready()
	{
		// Create and configure the sun
		sun = new MeshInstance3D();
		sun.Name = "Sun";
		AddChild(sun);

		// Add a sphere mesh to the sun
		var sunMesh = new SphereMesh();
		sunMesh.Radius = 20.0f; // Size of the sun
		sunMesh.Height = 40.0f; // Size of the sun
		sunMesh.Material = new StandardMaterial3D { AlbedoColor = new Color(1.0f, 1.0f, 0.0f) }; // Yellow color
		sun.Mesh = sunMesh;

		// Create and configure the planet
		planet = new MeshInstance3D();
		planet.Name = "Planet";
		sun.AddChild(planet);

		// Add a sphere mesh to the planet
		var planetMesh = new SphereMesh();
		planetMesh.Radius = 5.0f; // Size of the planet
		planetMesh.Height = 10.0f; // Size of the sun
		planetMesh.Material = new StandardMaterial3D { AlbedoColor = new Color(0.0f, 0.0f, 1.0f) }; // Blue color
		planet.Mesh = planetMesh;

		// Add CelestialBodyComponent to the planet
		var planetComponent = new CelestialBodyComponent
		{
			SemiMajorAxisMillionKilometers = 1f, // 1 million kilometers
			Eccentricity = 0.1f, // Slightly elliptical orbit
			InclinationDegrees = 0.0f, // No inclination
			LongitudeOfAscendingNodeDegrees = 0.0f,
			ArgumentOfPeriapsisDegrees = 0.0f,
			RotationAxis = new Vector3(0.0f, 1.0f, 0.0f), // Y-axis rotation
			AxialTiltDegrees = 23.5f, // Earth-like axial tilt
			InitialRotationDegrees = Vector3.Zero,
			RotationPeriodHours = 24.0f, // 24 hours rotation period
			CentralBody = sun // The Sun as the central body
		};
		planet.AddChild(planetComponent);

		// Start the game time
		GameTimeManager.Instance.ResumeTime();
	}

	public override void _Process(double delta)
	{
		// Advance the game time by delta
		GameTimeManager.Instance.AdvanceTime(TimeSpan.FromSeconds(delta / 100));
	}
}
