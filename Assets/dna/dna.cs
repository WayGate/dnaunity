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

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif           

    /// <summary>
    /// Root class for interfacing with the DNA scripting engine.
    /// </summary>
    public unsafe static class Dna
    {
        static bool _isInitialized;

        public const int DEFAULT_MEM_SIZE = 256 * 1024;

        #if UNITY_EDITOR
        public static string[] defaultAssemblySearchPaths = new string[] {
            "${UNITY_DIR}/Mono/Lib/mono/unity",
            "${UNITY_DIR}/Managed",
            "${PROJECT_DIR}/Library/ScriptAssemblies"
        };
        #else
        // Resources path
        public static string[] defaultAssemblySearchPaths = new string[] {
            "UnityDna"
        };
        #endif

        /// <summary>
        /// Initializes the DNA script engine.
        /// </summary>
        /// <param name="memsize">The heap memory size to use (note: can not be expanded)</param>
        /// <param name="assemblySearchPaths">Array of assembly search paths to use when loading assemblies</param>
        public static void Init(int memsize = DEFAULT_MEM_SIZE, string[] assemblySearchPaths = null)
        {
            if (_isInitialized)
                throw new System.InvalidOperationException("Dna has already been initialized.  Use Dna.Reset() to reset the interpreter");

            if (assemblySearchPaths == null)
                assemblySearchPaths = defaultAssemblySearchPaths;
            #if UNITY_EDITOR
            string[] finalAssemblySearchPaths = new string[assemblySearchPaths.Length];
            string unityDir = UnityEditor.EditorApplication.applicationContentsPath;
            string projectDir = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
            for (int i = 0; i < assemblySearchPaths.Length; i++)
            {
                finalAssemblySearchPaths[i] = assemblySearchPaths[i]
                    .Replace("${UNITY_DIR}", unityDir)
                    .Replace("${PROJECT_DIR}", projectDir);
            }
            #else
            string[] finalAssemblySearchPaths = assemblySearchPaths;
            #endif

            Mem.Init(memsize);
            H.Init();
            Sys.Init();
            JIT.Init();
            JIT_Execute.Init();
            DnaObject.Init();
            MetaData.Init();
            MonoType.Init();
            Generics.Init();
            Heap.Init();
            Finalizer.Init();
            InternalCall.Init();
            CLIFile.Init(finalAssemblySearchPaths);
            Type.Init();

            _isInitialized = true;
        }

        /// <summary>
        /// Resets entire DNA environment to it's initial state, clearing all DnaObject references to null.
        /// </summary>
        public static void Reset()
        {
            Type.Clear();
            CLIFile.Clear();
            InternalCall.Clear();
            Finalizer.Clear();
            Heap.Clear();
            Generics.Clear();
            MonoType.Clear();
            MetaData.Clear();
            DnaObject.Clear();
            JIT_Execute.Clear();
            JIT.Clear();
            Sys.Clear();
            H.Clear();
            Mem.Clear();

            _isInitialized = false;
        }

        /// <summary>
        /// True if the DNA scripting engine is currently initialized.
        /// </summary>
        public static bool isInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        static int InternalLoadAndRun(bool tryRun, string[] args) 
        {
            if (!_isInitialized)
                Init();

            /*char**/byte *pFileName = new S(args[0]);
            int argc = args.Length;
            /*char**/byte** argv = S.buildArray(args);
            tCLIFile *pCLIFile;
        	int retValue;
        #if DIAG_TOTAL_TIME
        	ulong startTime;
        #endif

        #if DIAG_OPCODE_TIMES
        	Mem.memset(opcodeTimes, 0, sizeof(opcodeTimes));
        #endif

        #if DIAG_OPCODE_USE
        	Mem.memset(opcodeNumUses, 0, sizeof(opcodeNumUses));
        #endif

            pCLIFile = CLIFile.LoadAssembly(pFileName);

        #if DIAG_TOTAL_TIME
        	startTime = microTime();
        #endif

            if (tryRun) {
            	if (pCLIFile->entryPoint != 0) {
                    retValue = CLIFile.Execute(pCLIFile, argc, argv);
            	} else {
                    Sys.printf("File %s has no entry point, skipping execution\n", (PTR)pFileName);
            		retValue = 0;
            	}
            } else {
                retValue = 0;
            }

        #if DIAG_TOTAL_TIME
        	printf("Total execution time = %d ms\n", (int)((microTime() - startTime) / 1000));
        #endif

        #if DIAG_GC
        	printf("Total GC time = %d ms\n", (int)(Heap.gcTotalTime / 1000));
        #endif

        #if DIAG_METHOD_CALLS
        	{
        		uint numMethods, i;
        		int howMany = 25;
        		tMetaData *pCorLib;
        		// Report on most-used methods
        		pCorLib = CLIFile_GetMetaDataForAssembly("mscorlib");
        		numMethods = pCorLib->tables.numRows[MetaDataTable.MD_TABLE_METHODDEF];
        		printf("\nCorLib method usage:\n");
        		for (; howMany > 0; howMany--) {
        			tMD_MethodDef *pMethod;
        			uint maxCount = 0, maxIndex = 0;
        			for (i=1; i<=numMethods; i++) {
        				pMethod = (tMD_MethodDef*)MetaData.GetTableRow(pCorLib, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODDEF, i));
        				if (pMethod->callCount > maxCount) {
        					maxCount = pMethod->callCount;
        					maxIndex = i;
        				}
        			}
        			pMethod = (tMD_MethodDef*)MetaData.GetTableRow(pCorLib, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODDEF, maxIndex));
        			printf("%d: %s (%d)\n", (int)pMethod->callCount, Sys_GetMethodDesc(pMethod), (int)(pMethod->totalTime/1000));
        			pMethod->callCount = 0;
        		}
        		printf("\n");
        	}
        	{
        		uint numMethods, i;
        		int howMany = 25;
        		tMetaData *pCorLib;
        		// Report on most-used methods
        		pCorLib = CLIFile_GetMetaDataForAssembly("mscorlib");
        		numMethods = pCorLib->tables.numRows[MetaDataTable.MD_TABLE_METHODDEF];
        		printf("\nCorLib method execution time:\n");
        		for (; howMany > 0; howMany--) {
        			tMD_MethodDef *pMethod;
        			ulong maxTime = 0;
        			uint maxIndex = 0;
        			for (i=1; i<=numMethods; i++) {
        				pMethod = (tMD_MethodDef*)MetaData.GetTableRow(pCorLib, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODDEF, i));
        				if (pMethod->totalTime > maxTime) {
        					maxTime = pMethod->totalTime;
        					maxIndex = i;
        				}
        			}
        			pMethod = (tMD_MethodDef*)MetaData.GetTableRow(pCorLib, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODDEF, maxIndex));
        			printf("%d: %s (%d)\n", (int)pMethod->callCount, Sys_GetMethodDesc(pMethod), (int)(pMethod->totalTime/1000));
        			pMethod->totalTime = 0;
        		}
        		printf("\n");
        	}
        #endif
        #if DIAG_OPCODE_TIMES
        	{
        		int howMany = 25;
        		uint i;
        		printf("\nOpCodes execution time:\n");
        		for (; howMany > 0; howMany--) {
        			ulong maxTime = 0;
        			uint maxIndex = 0;
        			for (i=0; i<JitOps.JIT_OPCODE_MAXNUM; i++) {
        				if (opcodeTimes[i] > maxTime) {
        					maxTime = opcodeTimes[i];
        					maxIndex = i;
        				}
        			}
        			printf("0x%03x: %dms (used %d times) (ave = %d)\n",
        				maxIndex, (int)(maxTime / 1000), (int)opcodeNumUses[maxIndex], (int)(maxTime / opcodeNumUses[maxIndex]));
        			opcodeTimes[maxIndex] = 0;
        		}
        	}
        #endif
        #if DIAG_OPCODE_USE
        	{
        		int howMany = 25;
        		uint i, j;
        		printf("\nOpcode use:\n");
        		for (j=1; howMany>0; howMany--, j++) {
        			uint maxUse = 0;
        			uint maxIndex = 0;
        			for (i=0; i<JitOps.JIT_OPCODE_MAXNUM; i++) {
        				if (opcodeNumUses[i] > maxUse) {
        					maxUse = opcodeNumUses[i];
        					maxIndex = i;
        				}
        			}
        			printf("%02d 0x%03x: %d\n", j, maxIndex, maxUse);
        			opcodeNumUses[maxIndex] = 0;
        		}
        	}
        #endif

        	//Sys.Crash("FINISHED!!!");

        	return retValue;
        }

        /// <summary>
        /// Load a .NET DLL or EXE assembly.
        /// </summary>
        /// <param name="filename">The filename of the assembly to load.</param>
        public static void Load(string filename)
        {
            InternalLoadAndRun(false, new string[] { filename });
        }

        /// <summary>
        /// Runs the "main" entrypoint function in the loaded EXE or DLL.
        /// </summary>
        /// <param name="args">The arguments to pass to the "main" entrypoint function</param>
        /// <returns>The integer return value (0 = no error, non zero = error code)</returns>
        public static int Run(string[] args)
        {
            return InternalLoadAndRun(true, args);
        }

        /// <summary>
        /// Returns a DNA TypeDef for a given type.
        /// </summary>
        /// <param name="type">The fully qualified name of the type (namespace and type name)</param>
        /// <returns>The TypeDef or 0 if no type by that name was found</returns>
        public static ulong FindType(string type)
        {
            byte* className = stackalloc byte[128];
            byte* nameSpace = stackalloc byte[128];

            if (!_isInitialized)
                Init();

            string[] split = type.Split('.');

            if (split.Length < 1)
                throw new System.ArgumentException("Type must have at least a type name");

            if (split.Length > 1)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < split.Length - 1; i++)
                {
                    if (i > 0)
                        sb.Append('.');
                    sb.Append(split[i]);
                }
                S.strncpy(nameSpace, sb.ToString(), 128);
            }
            else
            {
                nameSpace[0] = 0;
            }
            S.strncpy(className, split[split.Length - 1], 128);

            // Find any overload of the named method; assume it's the right one.
            // Specifying it exactly (type generic args, method generic args, arguments themselves, picking the
            // inherited methods if needed), is complex and not required at the moment.
            tMD_TypeDef* pTypeDef = CLIFile.FindTypeInAllLoadedAssemblies(nameSpace, className);
            if (pTypeDef->fillState < Type.TYPE_FILL_ALL) {
                MetaData.Fill_TypeDef(pTypeDef, null, null);
            }

            return (ulong)pTypeDef;
        }

        /// <summary>
        /// Creates an instance of the given DNA object that also potentially wraps an existing mono base object.
        /// </summary>
        /// <param name="typeDef">The type to create.</param>
        /// <returns>The DNA wrapper object.</returns>
        public static DnaObject CreateInstance(ulong typeDef, object monoBaseObject = null)
        {
            return DnaObject.CreateInstance((tMD_TypeDef*)typeDef, monoBaseObject);
        }

        /// <summary>
        /// Returns the DNA methodDef for a method given a typeDef, method name, and argument types.
        /// </summary>
        /// <param name="typeDef">The DNA typeDef for a type</param>
        /// <param name="methodName">The method name</param>
        /// <param name="argTypes">The types of the method's arguments or null if no arguments</param>
        /// <returns>The methodDef found or 0 if no method matches</returns>
        public static ulong FindMethod(ulong typeDef, string methodName, System.Type[] argTypes = null)
        {
            if (!_isInitialized)
                Init();

            int numArgs = argTypes != null ? argTypes.Length : 0;

            byte* mName = stackalloc byte[128];
            S.strncpy(mName, methodName, 128);

            // Find any overload of the named method; assume it's the right one.
            // Specifying it exactly (type generic args, method generic args, arguments themselves, picking the
            // inherited methods if needed), is complex and not required at the moment.
            tMD_TypeDef* pTypeDef = (tMD_TypeDef*)(PTR)typeDef;
            tMD_MethodDef* pMethodDef = null;
            bool matches = false;
            while (pTypeDef != null) {
                if (pTypeDef->fillState < Type.TYPE_FILL_ALL) {
                    MetaData.Fill_TypeDef(pTypeDef, null, null);
                }
                for (int i = 0; i < pTypeDef->numMethods; i++) {
                    tMD_MethodDef* pCheckMethodDef = pTypeDef->ppMethods[i];
                    int start = 0;
                    if (!MetaData.METHOD_ISSTATIC(pCheckMethodDef))
                        start = 1;
                    matches = false;
                    if (S.strcmp(pCheckMethodDef->name, mName) == 0 && 
                        (pCheckMethodDef->numberOfParameters - start == numArgs)) {
                        matches = true;
                        for (int j = start; j < pCheckMethodDef->numberOfParameters; j++) {
                            tMD_TypeDef* pArgType = MonoType.GetTypeForMonoType(argTypes[j - start], null, null);
                            if (pArgType != pCheckMethodDef->pParams[j].pStackTypeDef) {
                                matches = false;
                                break;
                            }
                        }
                    }
                    if (matches) {
                        pMethodDef = pCheckMethodDef;
                        break;
                    }
                }
                if (matches)
                    break;
                pTypeDef = pTypeDef->pParent;
            }

            return (ulong)pMethodDef;
        }

        static object[] noArgs = new object[0];

        /// <summary>
        /// Call to a DNA method given it's method def (with a byref this).
        /// </summary>
        /// <param name="methodDef">The method def</param>
        /// <param name="_this">The this object (or null if method is static)</param>
        /// <param name="args">The arguments to pass (or null for no arguments)</param>
        /// <returns>The value returned by the method.</returns>
        public static object CallRefThis(ulong methodDef, ref object _this, object[] args = null)
        {
            tMD_MethodDef* pMethodDef = (tMD_MethodDef*)methodDef;
            uint start = 0;
            uint byRefSize = 0;
            bool byRefThis = false;
            bool byRefArgs = false;
            uint paramsSize = 0;
            uint retValSize = 0;
            byte* pByRefPtr;
            byte* pStackPtr;
            object ret = null;

            byte* pBuf = stackalloc byte[256];
            uint bufSize = 256;

            tMD_TypeDef* pThisType = null;
            tMD_TypeDef* pMethodThisType = null;

            if (args == null)
                args = noArgs;

            // First pass.. check types and get by ref buf size..

            if (!MetaData.METHOD_ISSTATIC(pMethodDef)) {
                if (_this == null)
                    throw new System.NullReferenceException("Call to member method can not have null this");
                pThisType = MonoType.GetTypeForMonoObject(_this, null, null);
                pMethodThisType = MetaData.PARAM_ACTUAL_TYPE(&pMethodDef->pParams[0]);
                if (!MetaData.TYPE_ISASSIGNABLE_TO(pThisType, pMethodThisType))
                    throw new System.InvalidOperationException("This type is not compatible with method this type");
                if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[0])) {
                    byRefSize += pMethodThisType->stackSize;
                    paramsSize += (uint)sizeof(byte*);
                    byRefThis = true;
                }
                else {
                    paramsSize += pMethodDef->pParams[0].pStackTypeDef->stackSize;
                }
                start = 1;
            }

            if (args.Length != pMethodDef->numberOfParameters - start) {
                throw new System.InvalidOperationException("Wrong number of parameters for this method");
            }

            for (uint i = start; i < args.Length; i++) {
                tMD_TypeDef* pParamType = MetaData.PARAM_ACTUAL_TYPE(&pMethodDef->pParams[i]);
                tMD_TypeDef* pArgType = args[i - start] != null ? MonoType.GetTypeForMonoObject(args[i - start], null, null) : null;
                if (pArgType == null && pParamType->isValueType == 0)
                    pArgType = pParamType;
                if (!MetaData.TYPE_ISASSIGNABLE_TO(pArgType, pParamType))
                    throw new System.InvalidOperationException("Argument types are not compatible");
                if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[i])) {
                    byRefSize += pParamType->stackSize;
                    paramsSize += (uint)sizeof(byte*);
                    byRefArgs = true;
                }
                else {
                    paramsSize += pMethodDef->pParams[i].pStackTypeDef->stackSize;
                }
            }

            if (pMethodDef->pReturnType != null) {
                retValSize = pMethodDef->pReturnType->stackSize;
            }

            if (byRefSize + paramsSize + retValSize > bufSize) {
                throw new System.InvalidOperationException("Buffer overflow for parameter stack");
            }

            // Now actually marshal params to the param buffer

            pByRefPtr = pBuf;
            pStackPtr = pBuf + byRefSize;

            if (!MetaData.METHOD_ISSTATIC(pMethodDef)) {
                if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[0])) {
                    MonoType.MarshalFromObject(pByRefPtr, ref _this);
                    *(byte**)pStackPtr = pByRefPtr;
                    pByRefPtr += pMethodDef->pParams[0].pByRefTypeDef->stackSize;
                    pStackPtr += sizeof(byte*);
                }
                else {
                    MonoType.MarshalFromObject(pStackPtr, ref _this);
                    pStackPtr += pMethodDef->pParams[0].pStackTypeDef->stackSize;
                }
            }

            for (uint i = start; i < args.Length; i++) {
                if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[i])) {
                    MonoType.MarshalFromObject(pByRefPtr, ref args[i - start]);
                    *(byte**)pStackPtr = pByRefPtr;
                    pByRefPtr += pMethodDef->pParams[i].pByRefTypeDef->stackSize;
                    pStackPtr += sizeof(byte*);
                }
                else {
                    MonoType.MarshalFromObject(pStackPtr, ref args[i - start]);
                    pStackPtr += pMethodDef->pParams[i].pStackTypeDef->stackSize;
                }
            }

            // Call the method (usuing reusable call thread)

            int status = Thread.Call(pMethodDef, pBuf + byRefSize, pBuf + byRefSize + paramsSize);
            if (status != Thread.THREAD_STATUS_EXIT) {
                throw new System.InvalidOperationException("Thread blocked in call to method");
            }

            // Marshal back any byref values

            if (byRefArgs || byRefThis) {
                pByRefPtr = pBuf;
                if (byRefThis) {
                    object refThis;
                    MonoType.MarshalToObject(pByRefPtr, out refThis);
                    _this = refThis;
                    pByRefPtr += pMethodDef->pParams[0].pByRefTypeDef->stackSize;
                }
                if (byRefArgs) {
                    for (uint i = start; i < args.Length; i++) {
                        if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[i])) {
                            object refArg;
                            MonoType.MarshalToObject(pByRefPtr, out refArg);
                            args[i - start] = refArg;
                            pByRefPtr += pMethodDef->pParams[i].pByRefTypeDef->stackSize;
                        }
                    }
                }
            }
    
            // Marshal the return value

            if (pMethodDef->pReturnType != null) {
                MonoType.MarshalToObject(pBuf + byRefSize + paramsSize, out ret);
            }

            return ret;
        }

        /// <summary>
        /// Call to a DNA method given it's method def (with a byref this).
        /// </summary>
        /// <param name="methodDef">The method def</param>
        /// <param name="_this">The this object (or null if method is static)</param>
        /// <param name="args">The arguments to pass (or null for no arguments)</param>
        /// <returns>The value returned by the method.</returns>
        public static object Call(ulong methodDef, object _this = null, object[] args = null)
        {
            return CallRefThis(methodDef, ref _this, args);
        }


        /// <summary>
        /// Call a method on a DNA type with a typedef and method name.
        /// </summary>
        /// <param name="typeDef">Pointer to the type def</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="argTypes">The argument types</param>
        /// <param name="_this">The this object (or null if method is static)</param>
        /// <param name="args">The arguments to pass (or null for no arguments)</param>
        /// <returns>The value returned by the method.</returns>
        public static object Call(ulong typeDef, string methodName, System.Type[] argTypes = null, object _this = null, object[] args = null)
        {
            ulong methodDef = FindMethod(typeDef, methodName, argTypes);
            if (methodDef == 0)
                throw new System.InvalidOperationException("Unable to find method " + methodName);
            return Call(methodDef, _this, args);
        }

        /// <summary>
        /// Call a method on a DNA type with type name and method name.
        /// </summary>
        /// <param name="typeName">The name of the type (including namespace)</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="argTypes">The argument types</param>
        /// <param name="_this">The this object (or null if method is static)</param>
        /// <param name="args">The arguments to pass (or null for no arguments)</param>
        /// <returns>The value returned by the method.</returns>
        public static object Call(string typeName, string methodName, System.Type[] argTypes = null, object _this = null, object[] args = null)
        {
            ulong typeDef = FindType(typeName);
            if (typeDef == 0)
                throw new System.InvalidOperationException("Unable to find type " + typeName);
            ulong methodDef = FindMethod(typeDef, methodName, argTypes);
            if (methodDef == 0)
                throw new System.InvalidOperationException("Unable to find method " + typeName + "." + methodName);
            return Call(methodDef, _this, args);
        }
    }
}

