using CommandLine;
using System.Runtime.InteropServices;

namespace BankerAlgorithm
{
    public class Arguments
    {
        [Option("fileName", Required = true, HelpText = "Test File Name")]
        public string FileName { get; set; } = "";
    }
}