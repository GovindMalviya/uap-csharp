namespace UAParser
{
    public class ClientInfo
    {
        // ReSharper disable once InconsistentNaming
        public OS OS { get; private set; }
        public Device Device { get; private set; }
        public UserAgent UserAgent { get; private set; }

        public DeviceType DeviceType { get; private set; }

        public ClientInfo(OS os, Device device, UserAgent userAgent,DeviceType deviceType)
        {
            OS = os;
            Device = device;
            UserAgent = userAgent;
            DeviceType = deviceType;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", OS, Device, UserAgent);
        }
    }
}