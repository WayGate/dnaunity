using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DnaUnity
{

    public class RunScript : MonoBehaviour 
    {
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

        public void Run()
        {
            List<string> argsList = new List<string>();
            argsList.Add(filename);
            argsList.AddRange(arguments.Split(' '));
            DnaUnity.Dna.Run(argsList.ToArray());
        }

        public void RunAllTests()
        {
            RunTest1();
        }

        public void RunTest1()
        {
            Debug.Log("Running Test1");
            DnaUnity.Dna.Reset();
            DnaUnity.Dna.Load("Library/ScriptAssemblies/Test1.dll");
            DnaUnity.Dna.Call("Test1.Test");
            Debug.Log("## Test1 Complete!");
        }
    }

}