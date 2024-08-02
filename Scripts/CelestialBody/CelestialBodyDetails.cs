using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace RoemersLegacy.Scripts.CelestialBody
{
	public class CelestialBodyDetails : CelestialBodyDetailsData
	{
		// Cached values for orbital calculations
		private double _cachedMeanMotion;
		private DateTimeOffset _cachedPeriapsisEpoch;

		private bool _cacheValid = false;

		// Constructor to copy properties from an existing CelestialBodyDetails instance
		public CelestialBodyDetails(CelestialBodyDetailsData original)
		{
			// Copy all relevant properties
			base.Id = original.Id;
			base.Name = original.Name;
			base.EnglishName = original.EnglishName;
			base.IsPlanet = original.IsPlanet;
			base.Moons = original.Moons != null ? new List<MoonDetails>(original.Moons) : null;
			base.SemimajorAxis = original.SemimajorAxis;
			base.Perihelion = original.Perihelion;
			base.Aphelion = original.Aphelion;
			base.Eccentricity = original.Eccentricity;
			base.Inclination = original.Inclination;
			base.Mass = original.Mass;
			base.Volume = original.Volume;
			base.Density = original.Density;
			base.Gravity = original.Gravity;
			base.EscapeVelocity = original.EscapeVelocity;
			base.MeanRadius = original.MeanRadius;
			base.EquatorialRadius = original.EquatorialRadius;
			base.PolarRadius = original.PolarRadius;
			base.Flattening = original.Flattening;
			base.Dimension = original.Dimension;
			base.SideralOrbit = original.SideralOrbit;
			base.SideralRotation = original.SideralRotation;
			base.AroundPlanet = original.AroundPlanet;
			base.DiscoveredBy = original.DiscoveredBy;
			base.DiscoveryDate = original.DiscoveryDate;
			base.AlternativeName = original.AlternativeName;
			base.AxialTilt = original.AxialTilt;
			base.AverageTemperature = original.AverageTemperature;
			base.MainAnomaly = original.MainAnomaly;
			base.ArgumentOfPeriapsis = original.ArgumentOfPeriapsis;
			base.DoubleitudeOfAscendingNode = original.DoubleitudeOfAscendingNode;
			base.BodyType = original.BodyType;
			base.Relation = original.Relation;

			// Initialize cache based on the current game time
			RecalculateCache(GameTimeManager.Instance.CurrentTime);
		}

		// Override the properties to capture changes and invalidate cache

		public override double SemimajorAxis
		{
			get => base.SemimajorAxis;
			set
			{
				if (base.SemimajorAxis != value)
				{
					base.SemimajorAxis = value;
					InvalidateCache();
				}
			}
		}

		public override double Eccentricity
		{
			get => base.Eccentricity;
			set
			{
				if (base.Eccentricity != value)
				{
					base.Eccentricity = value;
					InvalidateCache();
				}
			}
		}

		public override double Inclination
		{
			get => base.Inclination;
			set
			{
				if (base.Inclination != value)
				{
					base.Inclination = value;
					InvalidateCache();
				}
			}
		}

		public override double ArgumentOfPeriapsis
		{
			get => base.ArgumentOfPeriapsis;
			set
			{
				if (base.ArgumentOfPeriapsis != value)
				{
					base.ArgumentOfPeriapsis = value;
					InvalidateCache();
				}
			}
		}

		public override double DoubleitudeOfAscendingNode
		{
			get => base.DoubleitudeOfAscendingNode;
			set
			{
				if (base.DoubleitudeOfAscendingNode != value)
				{
					base.DoubleitudeOfAscendingNode = value;
					InvalidateCache();
				}
			}
		}

		public override double MainAnomaly
		{
			get => base.MainAnomaly;
			set
			{
				if (base.MainAnomaly != value)
				{
					base.MainAnomaly = value;
					InvalidateCache();
				}
			}
		}

		public override double SideralOrbit
		{
			get => base.SideralOrbit;
			set
			{
				if (base.SideralOrbit != value)
				{
					base.SideralOrbit = value;
					InvalidateCache();
				}
			}
		}

		private void InvalidateCache()
		{
			_cacheValid = false;
		}

		// Method to recalculate and cache parameters
		private void RecalculateCache(DateTimeOffset currentTime)
		{
			if (_cacheValid)
				return;

			// Recompute necessary values
			_cachedMeanMotion = 2 * Math.PI / SideralOrbit; // Mean motion in radians per day
			_cachedPeriapsisEpoch = CalculatePeriapsisEpoch(currentTime);

			_cacheValid = true;
		}

		// Calculate the time of periapsis passage based on current time and orbital elements
		private DateTimeOffset CalculatePeriapsisEpoch(DateTimeOffset currentTime)
		{
			// Mean anomaly at epoch (converted from degrees to radians)
			double M0 = Mathf.DegToRad((float)MainAnomaly);

			// Sidereal orbit period in days
			double T = SideralOrbit;

			// Calculate time since periapsis passage
			double timeSincePeriapsis = (M0 / (2 * Math.PI)) * T;

			// Calculate the epoch of periapsis passage
			var days = TimeSpan.FromDays(timeSincePeriapsis);
			DateTimeOffset periapsisEpoch = currentTime - days;

			// if (IsPlanet || Id == "sun")
			// {
			// 	GD.Print($"{Id} -  M0: {M0:F2}, T: {T:F2}, timeSincePeriapsis: {timeSincePeriapsis:F2}, days: {days.TotalDays:F2}, periapsisEpoch: {periapsisEpoch}");
			// }
			return periapsisEpoch;
		}

		// Expose the cached values
		public double CachedMeanMotion
		{
			get
			{
				EnsureCacheIsValid(GameTimeManager.Instance.CurrentTime);
				return _cachedMeanMotion;
			}
		}

		public DateTimeOffset CachedPeriapsisEpoch
		{
			get
			{
				EnsureCacheIsValid(GameTimeManager.Instance.CurrentTime);
				return _cachedPeriapsisEpoch;
			}
		}

		private void EnsureCacheIsValid(DateTimeOffset currentTime)
		{
			if (!_cacheValid)
			{
				RecalculateCache(currentTime);
			}
		}
	}
}
