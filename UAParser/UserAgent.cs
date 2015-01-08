namespace UAParser
{
    public sealed class UserAgent
    {
        public UserAgent(string family, string major, string minor, string patch)
        {
            Family = family;
            Major  = major;
            Minor  = minor;
            Patch  = patch;
        }

        public string Family { get; private set; }
        public string Major  { get; private set; }
        public string Minor  { get; private set; }
        public string Patch  { get; private set; }

        public override string ToString()
        {
            var version = VersionString.Format(Major, Minor, Patch);
            return Family + (!string.IsNullOrEmpty(version) ? " " + version : null);
        }
    }
}