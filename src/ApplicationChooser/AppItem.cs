using System.Diagnostics;

namespace ApplicationChooser
{
    [DebuggerDisplay("{Name}")]
    public class AppItem
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
        public bool IsRequired { get; set; }
    }
}