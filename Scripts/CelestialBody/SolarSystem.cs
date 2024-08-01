
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
            var json = System.IO.File.ReadAllText("res://Data/SolarSystem.json");
            var celestialBodyDetails = JsonSerializer.Deserialize<List<CelestialBodyDetails>>(json) ?? new List<CelestialBodyDetails>();
            var celestialBodyScene = GD.Load<PackedScene>("res://Scenes/CelestialBody.tscn");

            var majorBodies = celestialBodyDetails
                    .Where(body => body.IsPlanet || body.Id == "soleil");

            foreach (var celestialBodyDetail in majorBodies)
            {
                var celestialBodyInstance = celestialBodyScene.Instantiate<CelestialBody>();
                celestialBodyInstance.Details = celestialBodyDetail;

                AddChild(celestialBodyInstance);
            }
        }
    }
}