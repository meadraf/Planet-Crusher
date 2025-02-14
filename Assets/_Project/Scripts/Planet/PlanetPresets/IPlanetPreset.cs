namespace _Project.Scripts.Planet.PlanetPresets
{
    public interface IPlanetPreset 
    {
        public float Radius { get; set; }
        public float SmallSphereRadius { get; set; }
        public int Subdivisions { get; set; } 
        public int NumberOfPatches { get; set; } 
    }
}