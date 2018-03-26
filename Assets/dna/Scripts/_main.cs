using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if VS_TESTING && TEST_UNITY

namespace DnaUnity
{
    static class Startup
    {
        public static string[] assemblySearchPaths = new string[] {
            "C:/Users/bcool/projects/dnaunity/corlib/bin/Debug/netstandard1.3",
            "C:/Users/bcool/projects/dnaunity/Library/ScriptAssemblies"
        };

        const bool LOAD_UNITY_DLLS = true;

        const string UNITY_DLL_PATH = "c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/";

        static string[] dllsToLoad = {
            "UnityEngine.dll",
            "UnityEngine.AnimationModule.dll",
            "UnityEngine.AudioModule.dll",
            "UnityEngine.CoreModule.dll",
            "UnityEngine.InputModule.dll",
            "UnityEngine.ParticleSystemModule.dll",
            "UnityEngine.Physics2DModule.dll",
            "UnityEngine.PhysicsModule.dll",
            "UnityEngine.UIModule.dll"
        };

        public static void Main()
        {
            if (LOAD_UNITY_DLLS) {
                foreach (string dllName in dllsToLoad) {
                    System.Reflection.Assembly.LoadFile(UNITY_DLL_PATH + dllName);
                }
            }

            Dna.Init(10000000, assemblySearchPaths);
            Dna.Load("SpinCube.dll");
            ulong typeDef = Dna.FindType("SpinCubeComponent");
            DnaObject obj = Dna.CreateInstance(typeDef, null);
            Dna.Call("Testing", "Test");
        }
    }
}

#endif
