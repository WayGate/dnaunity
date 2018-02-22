using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if !UNITY_EDITOR && !UNITY_IOS && !UNITY_ANDROID && !UNITY_WEBGL && !UNITY_STANDALONE

namespace DnaUnity
{
    static class Startup
    {
        public static string[] assemblySearchPaths = new string[] {
//            "C:/Program Files/Unity/Editor/Data/Mono/Lib/mono/unity",
//            "C:/Program Files/Unity/Editor/Data/Managed",
            "C:/Users/bcool/projects/dnaunity/lib",
            "C:/Users/bcool/projects/dnaunity/Library/ScriptAssemblies"
        };


        public static void Main()
        {
            Dna.Init(10000000, assemblySearchPaths);
            Dna.Load("Test1.dll");
            Dna.Call("Test.Test");

        }
    }
}

#endif
