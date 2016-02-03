namespace ComponentDataService.Types
{
    public struct FeatureDefinition
    {
        public readonly string Name;
        public readonly FeatureType Type;
        public readonly bool IsInteger;
        public readonly int Length;		// default length for display
        public readonly bool PerChannel;

        public FeatureDefinition(FeatureType ftype, string name, bool perChannel, bool integer, int length)
        {
            Type = ftype;
            Name = name;
            PerChannel = perChannel;
            IsInteger = integer;
            Length = length;
        }
    }
}