using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UAParser
{
    public sealed class Parser
    {
        readonly Func<string, OS> _osParser;
        readonly Func<string, Device> _deviceParser;
        readonly Func<string, UserAgent> _userAgentParser;
        readonly Func<string, DeviceType> _deviceTypeParser;

        Parser(MinimalYamlParser yamlParser)
        {
            const string other = "Other";
            var defaultDevice = new Device(other, false);

            _userAgentParser = CreateParser(Read(yamlParser.ReadMapping("user_agent_parsers"), Config.UserAgent), new UserAgent(other, null, null, null));
            _osParser = CreateParser(Read(yamlParser.ReadMapping("os_parsers"), Config.OS), new OS(other, null, null, null, null));
            _deviceParser = CreateParser(Read(yamlParser.ReadMapping("device_parsers"), Config.Device), defaultDevice.Family, f => defaultDevice.Family == f ? defaultDevice : new Device(f, "Spider".Equals(f, StringComparison.InvariantCultureIgnoreCase)));

            _deviceTypeParser = CreateParser(Read(yamlParser.ReadMapping("device_type_parsers"), Config.DeviceType), other, f => other == f ? new DeviceType(other): new DeviceType(f));
        }

        static IEnumerable<T> Read<T>(IEnumerable<Dictionary<string, string>> entries, Func<Func<string, string>, T> selector)
        {
            return from cm in entries select selector(cm.Find);
        }

        public static Parser FromYaml(string yaml) { return new Parser(new MinimalYamlParser(yaml)); }
        public static Parser FromYamlFile(string path) { return new Parser(new MinimalYamlParser(File.ReadAllText(path))); }

        public static Parser GetDefault()
        {
            using (var stream = typeof(Parser).Assembly.GetManifestResourceStream("UAParser.regexes.yaml"))
                // ReSharper disable once AssignNullToNotNullAttribute
            using (var reader = new StreamReader(stream))
                return new Parser(new MinimalYamlParser(reader.ReadToEnd()));
        }

        public ClientInfo Parse(string uaString)
        {
            var os     = ParseOS(uaString);
            var device = ParseDevice(uaString);
            var ua     = ParseUserAgent(uaString);
            var deviceType = ParseDeviceType(uaString);
            return new ClientInfo(os, device, ua, deviceType);
        }

        public OS ParseOS(string uaString) { return _osParser(uaString); }
        public Device ParseDevice(string uaString) { return _deviceParser(uaString); }
        public UserAgent ParseUserAgent(string uaString) { return _userAgentParser(uaString); }

        public DeviceType ParseDeviceType(string uaString) { return _deviceTypeParser(uaString); }

        static Func<string, T> CreateParser<T>(IEnumerable<Func<string, T>> parsers, T defaultValue) where T : class
        {
            return CreateParser(parsers, defaultValue, t => t);
        }

        static Func<string, TResult> CreateParser<T, TResult>(IEnumerable<Func<string, T>> parsers, T defaultValue, Func<T, TResult> selector) where T : class
        {
            parsers = parsers != null ? parsers.ToArray() : Enumerable.Empty<Func<string, T>>();
            return ua => selector(parsers.Select(p => p(ua)).FirstOrDefault(m => m != null) ?? defaultValue);
        }

        static class Config
        {
            // ReSharper disable once InconsistentNaming
            public static Func<string, OS> OS(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "OS");
                var os = indexer("os_replacement");
                var v1 = indexer("os_v1_replacement");
                var v2 = indexer("os_v2_replacement");
                return Parsers.OS(regex, os, v1, v2);
            }

            public static Func<string, UserAgent> UserAgent(Func<string, string> indexer)
            {
                var regex = Regex(indexer, "User agent");
                var family = indexer("family_replacement");
                var v1 = indexer("v1_replacement");
                var v2 = indexer("v2_replacement");
                return Parsers.UserAgent(regex, family, v1, v2);
            }

            public static Func<string, string> Device(Func<string, string> indexer)
            {
                return Parsers.Device(Regex(indexer, "Device"), indexer("device_replacement"));
            }

            public static Func<string, string> DeviceType(Func<string, string> indexer)
            {
                return Parsers.DeviceType(Regex(indexer, "DeviceType"), indexer("device_type_replacement"));
            }

            static Regex Regex(Func<string, string> indexer, string key)
            {
                var pattern = indexer("regex");
                if (pattern == null)
                    throw new Exception(String.Format("{0} is missing regular expression specification.", key));

                // Some expressions in the regex.yaml file causes parsing errors 
                // in .NET such as the \_ token so need to alter them before 
                // proceeding.

                if (pattern.IndexOf(@"\_", StringComparison.Ordinal) >= 0)
                    pattern = pattern.Replace(@"\_", "_");

                // TODO: potentially allow parser to specify e.g. to use 
                // compiled regular expressions which are faster but increase 
                // startup time
                
                return new Regex(pattern);
            }

           
        }
        
        static class Parsers
        {
            // ReSharper disable once InconsistentNaming
            public static Func<string, OS> OS(Regex regex, string osReplacement, string v1Replacement, string v2Replacement)
            {
                return Create(regex, from family in Replace(osReplacement, "$1")
                    from v1 in Replace(v1Replacement)
                    from v2 in Replace(v2Replacement)
                    from v3 in Select(v => v)
                    from v4 in Select(v => v)
                    select new OS(family, v1, v2, v3, v4));
            }

            public static Func<string, string> Device(Regex regex, string familyReplacement)
            {
                return Create(regex, Replace(familyReplacement, "$1"));
            }

            public static Func<string, string> DeviceType(Regex regex, string familyReplacement)
            {
                return Create(regex, Replace(familyReplacement, "$1"));
            }

            public static Func<string, UserAgent> UserAgent(Regex regex, string familyReplacement, string majorReplacement, string minorReplacement)
            {
                return Create(regex, from family in Replace(familyReplacement, "$1")
                    from v1 in Replace(majorReplacement)
                    from v2 in Replace(minorReplacement)
                    from v3 in Select()
                    select new UserAgent(family, v1, v2, v3));
            }

            static Func<Match, IEnumerator<int>, string> Replace(string replacement)
            {
                return replacement != null ? Select(_ => replacement) : Select();
            }

            static Func<Match, IEnumerator<int>, string> Replace(
                string replacement, string token)
            {
                return replacement != null && replacement.Contains(token)
                    ? Select(s => s != null ? replacement.ReplaceFirstOccurence(token, s) : replacement)
                    : Replace(replacement);
            }

            static Func<Match, IEnumerator<int>, string> Select() { return Select(v => v); }

            static Func<Match, IEnumerator<int>, T> Select<T>(Func<string, T> selector)
            {
                return (m, num) =>
                {
                    if (!num.MoveNext()) throw new InvalidOperationException();
                    var groups = m.Groups; Group group;
                    return selector(num.Current <= groups.Count && (group = groups[num.Current]).Success
                        ? group.Value : null);
                };
            }

            static Func<string, T> Create<T>(Regex regex, Func<Match, IEnumerator<int>, T> binder)
            {
                return input =>
                {
                    var m = regex.Match(input);
                    var num = Generate(1, n => n + 1);
                    return m.Success ? binder(m, num) : default(T);
                };
            }

            static IEnumerator<T> Generate<T>(T initial, Func<T, T> next)
            {
                for (var state = initial; ; state = next(state))
                    yield return state;
                // ReSharper disable once FunctionNeverReturns
            }
        }
    }
}