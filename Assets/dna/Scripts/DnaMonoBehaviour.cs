// Copyright (c) 2012 DotNetAnywhere
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if UNITY_5 || UNITY_2017 || UNITY_2018

using UnityEngine;

namespace DnaUnity
{

    public unsafe class DnaMonoBehaviour : MonoBehaviour
    {
        public string dnaComponentName;

        protected static DnaScript dnaScript;

        DnaObject dnaObject;

        private ulong dnaTypeDef;
        private ulong updateDef;
        private ulong lateUpdateDef;
        private ulong onDestroyDef;

        // Flags to avoid infinite recursion when calling DNA methods
        uint isCalling;
        const uint CALLING_AWAKE = 0x1;
        const uint CALLING_START = 0x2;
        const uint CALLING_UPDATE = 0x4;
        const uint CALLING_LATE_UPDATE = 0x8;
        const uint CALLING_ON_DESTROY = 0x10;

        void Awake()
        {
            if ((isCalling & CALLING_AWAKE) == 0)
            {
                isCalling |= CALLING_AWAKE;

                // Make sure DNA is initialized and scripts are loaded
                if (dnaScript == null)
                {
                    dnaScript = UnityEngine.Object.FindObjectOfType<DnaScript>();
                    if (dnaScript != null)
                    {
                        dnaScript.Initialize();
                        dnaScript.LoadScripts();
                    }
                    else
                    {
                        throw new UnityException("No DnaScript component found in scene!");
                    }
                }

                dnaTypeDef = Dna.FindType(dnaComponentName);
                if (dnaTypeDef != 0)
                {
                    dnaObject = Dna.CreateInstance(dnaTypeDef, this);
                    ulong awakeDef = Dna.FindMethod(dnaTypeDef, "Awake");
                    if (awakeDef != 0)
                        Dna.Call(awakeDef, dnaObject);
                }
                isCalling &= CALLING_AWAKE;
            }
        }

        void Start()
        {
            if ((isCalling & CALLING_START) == 0)
            {
                isCalling |= CALLING_START;
                if (dnaTypeDef != 0)
                {
                    ulong startDef = Dna.FindMethod(dnaTypeDef, "Start");
                    if (startDef != 0)
                        Dna.Call(startDef, dnaObject);
                    updateDef = Dna.FindMethod(dnaTypeDef, "Update");
                    lateUpdateDef = Dna.FindMethod(dnaTypeDef, "LateUpdate");
                    onDestroyDef = Dna.FindMethod(dnaTypeDef, "OnDestroy");
                }
                isCalling &= CALLING_START;
            }
        }

        private void Update()
        {
            if ((isCalling & CALLING_UPDATE) == 0)
            {
                isCalling |= CALLING_UPDATE;
                if (updateDef != 0)
                    Dna.Call(updateDef, dnaObject);
                isCalling &= ~CALLING_UPDATE;
            }
        }

        private void OnDestroy()
        {
            if ((isCalling & CALLING_ON_DESTROY) == 0)
            {
                isCalling |= CALLING_ON_DESTROY;
                if (onDestroyDef != 0)
                    Dna.Call(onDestroyDef, dnaObject);
                isCalling &= ~CALLING_ON_DESTROY;
            }
        }


    }
}

#endif