using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if VS_TESTING

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
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.AnimationModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.AudioModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.InputModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.ParticleSystemModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.Physics2DModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.PhysicsModule.dll");
            System.Reflection.Assembly.LoadFile("c:/Program Files/Unity/Editor/Data/Managed/UnityEngine/UnityEngine.UIModule.dll");

            Dna.Init(10000000, assemblySearchPaths);
            Dna.Load("SpinCube.dll");
            ulong typeDef = Dna.FindType("SpinCubeComponent");
            DnaObject obj = Dna.CreateInstance(typeDef, null);
            Dna.Call("Testing", "Test");
        }
    }
}

#endif
