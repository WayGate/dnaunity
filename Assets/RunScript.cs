using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DnaUnity;

namespace DnaUnity
{

    public class RunScript : MonoBehaviour 
    {
        public int heapSize = 10000000;
        public string[] assemblySearchPaths = new string[] {
            "${UNITY_DIR}/Mono/Lib/mono/unity",
            "${UNITY_DIR}/Managed",
            "${PROJECT_DIR}/Library/ScriptAssemblies"
        };

        public string filename;

        public string arguments;


    	// Use this for initialization
    	void Start()
        {
    	}
    	
    	// Update is called once per frame
    	void Update()
        {
    	}

        public void InitDna()
        {
            Dna.Reset();
            Dna.Init(heapSize, assemblySearchPaths); 
        }

        public void Run()
        {
            List<string> argsList = new List<string>();
            argsList.Add(filename);
            argsList.AddRange(arguments.Split(' '));
            InitDna();
            Dna.Run(argsList.ToArray());
        }

        public void RunAllTests()
        {
            RunTest1();
        }

        public void RunTest1()
        {
            Debug.Log("Running Test1");
            InitDna();
            Dna.Load("Test1.dll");
            Dna.Call("Test1.Test");
            Debug.Log("## Test1 Complete!");
        }
    }

}