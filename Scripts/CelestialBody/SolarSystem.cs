
using System;
using Godot;
using System.Text.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoemersLegacy.Scripts.CelestialBody
{
	public partial class SolarSystem : Node3D
	{
		public override void _Ready()
		{
			var file = Godot.FileAccess.Open("res://Data/SolarSystemCleaned.json", FileAccess.ModeFlags.Read);

			var celestialBodyDetails = JsonSerializer.Deserialize<List<CelestialBodyDetailsData>>(file.GetAsText()) ?? new List<CelestialBodyDetailsData>();
			var celestialBodyScene = GD.Load<PackedScene>("res://Scenes/CelestialBody.tscn");

			var majorBodies = celestialBodyDetails
					.Where(body => body.IsPlanet || body.Id == "sun");

			var bodies = majorBodies
				.Select(body =>
				{
					var node = celestialBodyScene.Instantiate<CelestialBody>();
					node.Details = new CelestialBodyDetails(body);
					return node;
				})
				.OrderByDescending(body => body.Details.Mass?.MassValue)
				.OrderByDescending(body => body.Details.Mass?.MassExponent);

			foreach (var body in bodies)
			{
				if (body.Details.Id == "sun")
				{
					// Create a new OmniLight3D instance
					OmniLight3D sunLight = new OmniLight3D();

					// Configure light properties
					sunLight.LightEnergy = 1.0f; // Set the light energy for strong sunlight
					sunLight.LightColor = new Color(1.0f, 0.95f, 0.9f); // Slightly warm color for sunlight
					sunLight.OmniRange = 100000.0f; // Set the range of the light
					sunLight.OmniAttenuation = 0f; // Set attenuation for a natural falloff effect

					// Enable shadows if needed
					sunLight.ShadowEnabled = true;
					sunLight.ShadowBias = 0.3f;
					sunLight.ShadowReverseCullFace = false;
					sunLight.LightSize = body.SphereMesh.Radius;
					body.AddChild(sunLight);
					body.CelestialBodyMesh.Layers = 1 << 1;
					AddChild(body);
				}
				else if (body.Details.AroundPlanet is null)
				{
					var sun = GetNode<CelestialBody>("sun");
					sun?.AddChild(body);
				}
				else
				{
					var parent = GetNode<CelestialBody>(body.Details.AroundPlanet.Planet);
					parent?.AddChild(body);
				}
			}
		}
	}
}
