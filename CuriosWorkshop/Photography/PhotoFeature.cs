namespace CuriosWorkshop
{
    public class PhotoFeature
    {
        public string Type { get; }
        public string Name { get; }

        public float CostOffset { get; }
        public float? CostMultiplier { get; }

        public PhotoFeature(string type, string name, float offset, float? multiplier = null)
        {
            Type = type;
            Name = name;
            CostOffset = offset;
            CostMultiplier = multiplier;
        }

    }
}
