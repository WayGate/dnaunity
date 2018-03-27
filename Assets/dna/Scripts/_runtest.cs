using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if VS_TESTING && RUNTEST

namespace DnaUnity
{
    static class RunTest
    {
        public static int Main(string[] args)
        {
            string cwd = Directory.GetCurrentDirectory();
            if (cwd.EndsWith("\\bin\\Debug") ||
                cwd.EndsWith("\\bin\\Release")) {
                Directory.SetCurrentDirectory(Path.GetFullPath(cwd + "\\..\\.."));
            }
            string[] assemblySearchPaths = new string[] {
                System.IO.Path.GetFullPath("../corlib/bin/Debug/netstandard1.3").Replace("\\", "/"),
                System.IO.Path.GetFullPath("./bin").Replace("\\", "/")
            };
            Dna.Init(10000000, assemblySearchPaths);
            try {
                return Dna.Run(args);
            } catch (Exception e) {
                Console.WriteLine(e.Message + "\n" + e.StackTrace.ToString());
                return 1;
            }
        }
    }
}

#endif
