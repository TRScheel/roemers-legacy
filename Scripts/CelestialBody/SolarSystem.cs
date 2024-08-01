
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
			var file = Godot.FileAccess.Open("res://Data/SolarSystem.json", FileAccess.ModeFlags.Read);

			var celestialBodyDetails = JsonSerializer.Deserialize<List<CelestialBodyDetails>>(file.GetAsText()) ?? new List<CelestialBodyDetails>();
			var celestialBodyScene = GD.Load<PackedScene>("res://Scenes/CelestialBody.tscn");

			var majorBodies = celestialBodyDetails
					.Where(body => body.IsPlanet || body.Id == "soleil");

			foreach (var celestialBodyDetail in majorBodies)
			{
				var celestialBodyInstance = celestialBodyScene.Instantiate<CelestialBody>();
				celestialBodyInstance.Name = celestialBodyDetail.EnglishName;
				celestialBodyInstance.Details = celestialBodyDetail;

				AddChild(celestialBodyInstance);
			}
		}
	}
}
