using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RoemersLegacy.Scripts.CelestialBody
{
    public record class CelestialBodyDetails
    {
        public static CelestialBodyDetails Empty => new() { Id = "Unknown", Name = "Unknown" };

        /// <summary>
        /// Id of the body in the API.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Body name (in French).
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// English name.
        /// </summary>
        [JsonPropertyName("englishName")]
        public string? EnglishName { get; set; }

        /// <summary>
        /// Indicates if it is a planet.
        /// </summary>
        [JsonPropertyName("isPlanet")]
        public bool IsPlanet { get; set; }

        /// <summary>
        /// Table with all moons.
        /// Contains moon name and moon API URL.
        /// </summary>
        [JsonPropertyName("moons")]
        public List<MoonDetails>? Moons { get; set; }

        /// <summary>
        /// Semimajor Axis of the body in kilometers.
        /// </summary>
        [JsonPropertyName("semimajorAxis")]
        public int SemimajorAxis { get; set; }

        /// <summary>
        /// Perihelion in kilometers.
        /// </summary>
        [JsonPropertyName("perihelion")]
        public int Perihelion { get; set; }

        /// <summary>
        /// Aphelion in kilometers.
        /// </summary>
        [JsonPropertyName("aphelion")]
        public int Aphelion { get; set; }

        /// <summary>
        /// Orbital eccentricity.
        /// </summary>
        [JsonPropertyName("eccentricity")]
        public decimal Eccentricity { get; set; }

        /// <summary>
        /// Orbital inclination in degrees.
        /// </summary>
        [JsonPropertyName("inclination")]
        public decimal Inclination { get; set; }

        /// <summary>
        /// Body mass in 10^n kg.
        /// Contains mass value and mass exponent.
        /// </summary>
        [JsonPropertyName("mass")]
        public MassDetails? Mass { get; set; }

        /// <summary>
        /// Body volume in 10^n km^3.
        /// Contains volume value and volume exponent.
        /// </summary>
        [JsonPropertyName("vol")]
        public VolumeDetails? Volume { get; set; }

        /// <summary>
        /// Body density in g/cm^3.
        /// </summary>
        [JsonPropertyName("density")]
        public decimal Density { get; set; }

        /// <summary>
        /// Surface gravity in m/s^2.
        /// </summary>
        [JsonPropertyName("gravity")]
        public decimal Gravity { get; set; }

        /// <summary>
        /// Escape speed in m/s.
        /// </summary>
        [JsonPropertyName("escape")]
        public decimal EscapeVelocity { get; set; }

        /// <summary>
        /// Mean radius in kilometers.
        /// </summary>
        [JsonPropertyName("meanRadius")]
        public int MeanRadius { get; set; }

        /// <summary>
        /// Equatorial radius in kilometers.
        /// </summary>
        [JsonPropertyName("equaRadius")]
        public int EquatorialRadius { get; set; }

        /// <summary>
        /// Polar radius in kilometers.
        /// </summary>
        [JsonPropertyName("polarRadius")]
        public int PolarRadius { get; set; }

        /// <summary>
        /// Flattening.
        /// </summary>
        [JsonPropertyName("flattening")]
        public decimal Flattening { get; set; }

        /// <summary>
        /// Body dimension on the 3 axes X, Y, and Z for non-spherical bodies.
        /// </summary>
        [JsonPropertyName("dimension")]
        public string? Dimension { get; set; }

        /// <summary>
        /// Sideral orbital time for body around another one (the Sun or a planet) in Earth days.
        /// </summary>
        [JsonPropertyName("sideralOrbit")]
        public decimal SideralOrbit { get; set; }

        /// <summary>
        /// Sideral rotation, necessary time to turn around itself, in hours.
        /// </summary>
        [JsonPropertyName("sideralRotation")]
        public decimal SideralRotation { get; set; }

        /// <summary>
        /// For a moon, the planet around which it is orbiting.
        /// Contains planet name and planet API URL.
        /// </summary>
        [JsonPropertyName("aroundPlanet")]
        public AroundPlanetDetails? AroundPlanet { get; set; }

        /// <summary>
        /// Discovery name.
        /// </summary>
        [JsonPropertyName("discoveredBy")]
        public string? DiscoveredBy { get; set; }

        /// <summary>
        /// Discovery date.
        /// </summary>
        [JsonPropertyName("discoveryDate")]
        public string? DiscoveryDate { get; set; }

        /// <summary>
        /// Temporary name.
        /// </summary>
        [JsonPropertyName("alternativeName")]
        public string? AlternativeName { get; set; }

        /// <summary>
        /// Axial tilt.
        /// </summary>
        [JsonPropertyName("axialTilt")]
        public decimal? AxialTilt { get; set; }

        /// <summary>
        /// Mean temperature in Kelvin.
        /// </summary>
        [JsonPropertyName("avgTemp")]
        public int AverageTemperature { get; set; }

        /// <summary>
        /// Mean anomaly in degrees.
        /// </summary>
        [JsonPropertyName("mainAnomaly")]
        public decimal MainAnomaly { get; set; }

        /// <summary>
        /// Argument of perihelion in degrees.
        /// </summary>
        [JsonPropertyName("argPeriapsis")]
        public decimal ArgumentOfPeriapsis { get; set; }

        /// <summary>
        /// Longitude of ascending node in degrees.
        /// </summary>
        [JsonPropertyName("longAscNode")]
        public decimal LongitudeOfAscendingNode { get; set; }

        /// <summary>
        /// The body type: Star, Planet, Dwarf Planet, Asteroid, Comet, or Moon.
        /// </summary>
        [JsonPropertyName("bodyType")]
        public string? BodyType { get; set; }

        /// <summary>
        /// The API URL for this body.
        /// </summary>
        [JsonPropertyName("rel")]
        public string? Relation { get; set; }

        public record MassDetails
        {
            /// <summary>
            /// Body mass.
            /// </summary>
            [JsonPropertyName("massValue")]
            public decimal MassValue { get; set; }

            /// <summary>
            /// Exponent of the mass.
            /// </summary>
            [JsonPropertyName("massExponent")]
            public int MassExponent { get; set; }
        }

        public record VolumeDetails
        {
            /// <summary>
            /// Body volume.
            /// </summary>
            [JsonPropertyName("volValue")]
            public decimal VolumeValue { get; set; }

            /// <summary>
            /// Exponent of the volume.
            /// </summary>
            [JsonPropertyName("volExponent")]
            public int VolumeExponent { get; set; }
        }

        public record AroundPlanetDetails
        {
            /// <summary>
            /// Planet name.
            /// </summary>
            [JsonPropertyName("planet")]
            public string? Planet { get; set; }

            /// <summary>
            /// Planet API URL.
            /// </summary>
            [JsonPropertyName("rel")]
            public string? Relation { get; set; }
        }

        public record MoonDetails
        {
            /// <summary>
            /// Moon name.
            /// </summary>
            [JsonPropertyName("moon")]
            public string? Name { get; set; }

            /// <summary>
            /// Moon API URL.
            /// </summary>
            [JsonPropertyName("rel")]
            public string? Relation { get; set; }
        }
    }

}
