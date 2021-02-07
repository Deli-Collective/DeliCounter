using System.Collections.Generic;
using Semver;

namespace Slicer.Backend
{
    public static class SampleData
    {
        public static Mod SampleMod1 = new()
        {
            Guid = "Sample.mod1",
            InstalledVersion = new SemVersion(1),
            Versions = new Dictionary<SemVersion, Mod.ModVersion>
            {
                [new SemVersion(1)] = new()
                {
                    Name = "Sample mod 1 1",
                    Description = "Description for sample mod 1 version 1",
                    ShortDescription = "Short Description for sample mod 1",
                    IconUrl = "https://avatars.githubusercontent.com/u/75508809?s=60&v=4"
                },
                [new SemVersion(2)] = new()
                {
                    Name = "Sample mod 1 2",
                    Description = "Description for sample mod 1",
                    ShortDescription = "Short Description for sample mod 1",
                    IconUrl = "https://avatars.githubusercontent.com/u/75508809?s=60&v=4"
                }
            }
        };
    }
}