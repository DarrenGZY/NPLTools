using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intellisense
{
    public class PackageJson
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string Main { get; set; }

        public string Description { get; set; }

        public Author Author { get; set; }

        public Dictionary<string, string> Dependencies { get; set; }
    }

    public class Author
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }


}
