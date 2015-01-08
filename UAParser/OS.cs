namespace UAParser
{
    public sealed class OS
    {
        public OS(string family, string major, string minor, string patch, string patchMinor)
        {
            Family     = family;
            Major      = major;
            Minor      = minor;
            Patch      = patch;
            PatchMinor = patchMinor;
        }

        public string Family     { get; private set; }
        public string Major      { get; private set; }
        public string Minor      { get; private set; }
        public string Patch      { get; private set; }
        public string PatchMinor { get; private set; }

        public override string ToString()
        {
            var version = VersionString.Format(Major, Minor, Patch, PatchMinor);
            return Family + (!string.IsNullOrEmpty(version) ? " " + version : null);
        }
    }
}