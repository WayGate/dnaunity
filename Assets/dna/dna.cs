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

    public unsafe static class Dna
    {
        static bool isInitialized;

        public static void Init(int memsize = 256 * 1024)
        {
            if (!isInitialized)
            {
                Mem.Init(memsize);
                JIT.Init();
                JIT_Execute.Init();
                MetaData.Init();
                Generics.Init();
                Type.Init();
                Heap.Init();
                Finalizer.Init();
                InternalCall.Init();
                CLIFile.Init();
                isInitialized = true;
            }
        }

        public static void Reset()
        {
            if (isInitialized)
            {
                Mem.Clear();
                isInitialized = false;
            }
        }

        static int InternalLoadAndRun(bool tryRun, string[] args) 
        {
            if (!isInitialized)
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

            pCLIFile = CLIFile.Load(pFileName);

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

        public static int Load(string filename)
        {
            return InternalLoadAndRun(false, new string[] { filename });
        }

        public static int Run(string[] args)
        {
            return InternalLoadAndRun(true, args);
        }

        public static int Call(string method) 
        {
            if (!isInitialized)
                Init();

            string[] split = method.Split('.');

            byte* nameSpace = new S(split[0]);
            byte* className = new S(split[1]);
            byte* methodName = new S(split[2]);
            // TODO: Can't we reuse threads? Need to reset their state somehow.
            tThread *pThread = Thread.New();

            // Find any overload of the named method; assume it's the right one.
            // Specifying it exactly (type generic args, method generic args, arguments themselves, picking the
            // inherited methods if needed), is complex and not required at the moment.
            tMD_TypeDef *pTypeDef = CLIFile.FindTypeInAllLoadedAssemblies(nameSpace, className);
            if (pTypeDef == null)
                return 1;
            MetaData.Fill_TypeDef(pTypeDef, null, null);
            for (int i=0; i<pTypeDef->numMethods; i++) {
                if (S.strcmp(pTypeDef->ppMethods[i]->name, methodName) == 0) {
                    tMD_MethodDef *pMethodDef = pTypeDef->ppMethods[i];

                    // We found the method - now call it
                    Thread.SetEntryPoint(pThread, pTypeDef->pMetaData, pMethodDef->tableIndex, null, 0);
                    int result = Thread.Execute();

                    return result;
                }
            }

            return 1;
        }

    }
}

