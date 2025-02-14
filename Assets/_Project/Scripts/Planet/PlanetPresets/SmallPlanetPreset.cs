namespace _Project.Scripts.Planet.PlanetPresets
{
    public class SmallPlanetPreset:IPlanetPreset
    {
        public float Radius { get; set; } = 4f;
        public float SmallSphereRadius { get; set; }= 0.6f;
        public int Subdivisions { get; set; } = 2;
        public int NumberOfPatches { get; set; } = 4;
    }
}