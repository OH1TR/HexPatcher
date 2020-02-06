using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexPatcher
{
    public class Options
    {
        [Option('i', "in", Required = true, HelpText = "Input file")]
        public string Input { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file")]
        public string Output { get; set; }

        [Option('p', "patch", Required = true, HelpText = "Patch file")]
        public string Patch { get; set; }

        [Option('t', "test", HelpText = "Test only")]
        public bool Test { get; set; }
    }

}
