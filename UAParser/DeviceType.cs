namespace UAParser
{
    public sealed class DeviceType
    {
        public DeviceType(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public override string ToString() { return Name; }
    }
}