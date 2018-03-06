using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DnaUnity
{

    [CustomEditor(typeof(RunScript))]
    public class RunScriptEditor : Editor 
    {
        static bool testFoldout;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RunScript runScript = (RunScript)target;

            if (GUILayout.Button("Run"))
            {
                runScript.Run();
            }

            if (GUILayout.Button("Run All Tests"))
            {
                runScript.RunAllTests();
            }

            testFoldout = EditorGUILayout.Foldout(testFoldout, "Tests");
            if (testFoldout)
            {
                if (GUILayout.Button("Run Test01_Expressions"))
                    runScript.RunTest01_Expressions();
            }

        }


    }

}