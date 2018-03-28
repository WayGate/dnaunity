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

using System.Runtime.InteropServices;

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif    

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tCLIFile 
    {
        // The filename
        public /*char**/byte *assemblyName;
        // True if this is a mono assembly
        public int isMonoAssembly;
        // Pointer to metadata for this assembly
        public tMetaData* pMetaData;

        // The following fields will be unused (0 or null) if this is a mono assembly.

        // The gchandle for the bytes for the file
        public PTR gcHandle;
        // The RVA sections of this file
        public tRVA *pRVA;
        // null-terminated UTF8 string of file version
        public byte *pVersion;
        // The entry point token if this is executable, 0 if it isn't
        public /*IDX_TABLE*/uint entryPoint;
    };

    public unsafe static class CLIFile
    {
        // Is this exe/dll file for the .NET virtual machine?
        const int DOT_NET_MACHINE = 0x14c;

        private unsafe struct tFilesLoaded 
        {
        	public tCLIFile *pCLIFile;
        	public tFilesLoaded *pNext;
        };

        // In .NET Core, the core libraries are split over numerous assemblies. For simplicity,
        // the DNA corlib just puts them in one assembly
        static /*char**/byte** monoAssemblies = null;

        // Assembly modules mapped to UnityEngine.dll
        static /*char**/byte** unityModuleAssemblies = null;

        // In .NET Core, the core libraries are split over numerous assemblies. For simplicity,
        // the DNA corlib just puts them in one assembly
        static /*char**/byte** dnaCorlibAssemblies = null;

        // Paths to search for assemblies
        static /*char**/byte** assemblySearchPaths = null;
        static int assemblySearchPathsCount = 0;


        // Keep track of all the files currently loaded
        static tFilesLoaded *pFilesLoaded = null;

        static byte* /*char**/ scCorLib; 

        public static void Init(string[] searchPaths)
        {
            dnaCorlibAssemblies = S.buildArray(
                "mscorlib",
                "System",
                "System.Core",
                null
            );

            #if UNITY_5 || UNITY_2017 || UNITY_2018
            monoAssemblies = S.buildArray(
                "UnityEngine",
                "UnityEngine.UI",
                "UnityEngine.CoreModule",
                "UnityEngine.AudioModule",
                "UnityEngine.AnimationModule",
                "UnityEngine.InputModule",
                "UnityEngine.ParticleSystemModule",
                "UnityEngine.PhysicsModule",
                "UnityEngine.Physics2DModule",
                null
            );
            unityModuleAssemblies = S.buildArray(
                "UnityEngine.CoreModule",
                "UnityEngine.AudioModule",
                "UnityEngine.AnimationModule",
                "UnityEngine.InputModule",
                "UnityEngine.ParticleSystemModule",
                "UnityEngine.PhysicsModule",
                "UnityEngine.Physics2DModule",
                null
            );
            #else
            monoAssemblies = S.buildArray(
                null
            );
            unityModuleAssemblies = S.buildArray(
                null
            );
            #endif

            assemblySearchPaths = S.buildArray(searchPaths);
            assemblySearchPathsCount = searchPaths.Length;

            scCorLib = new S("corlib");

            pFilesLoaded = null;
        }

        public static void Clear()
        {
            // Release metadata images
            tFilesLoaded* pFiles = pFilesLoaded;
            while (pFiles != null) {
                if (pFiles->pCLIFile->gcHandle != 0) {
                    GCHandle handle = GCHandle.FromIntPtr((System.IntPtr)pFiles->pCLIFile->gcHandle);
                    handle.Free();
                }
                pFiles = pFiles->pNext;
            }
            pFilesLoaded = null;
            monoAssemblies = null;
            unityModuleAssemblies = null;
            dnaCorlibAssemblies = null;
            assemblySearchPaths = null;
            assemblySearchPathsCount = 0;
            scCorLib = null;
        }

        public static tMetaData* GetMetaDataForLoadedAssembly(byte *pLoadedAssemblyName) 
        {
        	tFilesLoaded *pFiles = pFilesLoaded;

        	while (pFiles != null) {
        		tCLIFile *pCLIFile = pFiles->pCLIFile;
                tMD_Assembly *pThisAssembly = (tMD_Assembly*)MetaData.GetTableRow(pCLIFile->pMetaData, MetaData.MAKE_TABLE_INDEX(0x20, 1));
        		if (S.strcmp(pLoadedAssemblyName, pThisAssembly->name) == 0) {
        			// Found the correct assembly, so return its meta-data
        			return pCLIFile->pMetaData;
        		}
        		pFiles = pFiles->pNext;
        	}

            Sys.Crash("Assembly %s is not loaded\n", (PTR)pLoadedAssemblyName);
            return null;
        }

        public static tMetaData* GetMetaDataForAssembly(byte *pAssemblyName) 
        {
        	tFilesLoaded *pFiles;
            int monoAssembly = 0;
            tCLIFile* pCLIFile = null;
            tMD_Assembly* pThisAssembly = null;
            tMetaData** ppChildMetaData = null;
            int i, j, childCount;

            // Check corlib assemblies
            i = 0;
            while (dnaCorlibAssemblies[i] != null) {
                if (S.strcmp(pAssemblyName, dnaCorlibAssemblies[i]) == 0) {
                    pAssemblyName = scCorLib;
        			break;
        		}
                i++;
        	}

         	// Look in already-loaded files first
        	pFiles = pFilesLoaded;
        	while (pFiles != null) {
        		pCLIFile = pFiles->pCLIFile;
        		if (S.strcmp(pAssemblyName, pCLIFile->assemblyName) == 0) {
        			// Found the correct assembly, so return its meta-data
        			return pCLIFile->pMetaData;
        		}
        		pFiles = pFiles->pNext;
        	}

           // Mono/Unity assemblies only load metadata, no code
            if (monoAssemblies != null) {
                i = 0;
                while (monoAssemblies[i] != null) {
                    if (S.strcmp(pAssemblyName, monoAssemblies[i]) == 0) {
                        if (i == 0) {
                            // Handle "UnityEngine" assemblies
                            j = 0;
                            childCount = 0;
                            while (unityModuleAssemblies[j] != null) {
                                childCount++;
                                j++;
                            }
                            ppChildMetaData = (tMetaData**)Mem.malloc((SIZE_T)((childCount + 1) * sizeof(tMetaData*)));
                            Mem.memset(ppChildMetaData, 0, (SIZE_T)((childCount + 1) * sizeof(tMetaData*)));
                            j = 0;
                            while (unityModuleAssemblies[j] != null) {
                                ppChildMetaData[j] = GetMetaDataForAssembly(unityModuleAssemblies[j]);
                                j++;
                            }
                        }
                        monoAssembly = 1;
            		    break;
        		    }
                    i++;
        	    }
            }

            // Assembly not loaded, so load it if possible
            if (monoAssembly != 0) {
                pCLIFile = CLIFile.WrapMonoAssembly(pAssemblyName);
                if (pCLIFile == null)
                    Sys.Crash("Cannot load required mono assembly file: %s.dll", (PTR)pAssemblyName);
            } else {
                byte* fileName = stackalloc byte[256];
                S.snprintf(fileName, 256, "%s.dll", (PTR)pAssemblyName);
                pCLIFile = CLIFile.LoadAssembly(fileName);
                if (pCLIFile == null)
                    Sys.Crash("Cannot load required assembly file: %s.dll", (PTR)pAssemblyName);
            }

            pCLIFile->pMetaData->ppChildMetaData = ppChildMetaData;

            return pCLIFile->pMetaData;
        }

        public static tMD_TypeDef* FindTypeInAllLoadedAssemblies(/*STRING*/byte* nameSpace, /*STRING*/byte* name) 
        {
        	tFilesLoaded *pFiles = pFilesLoaded;
        	while (pFiles != null) {
        		tCLIFile *pCLIFile = pFiles->pCLIFile;

        		tMD_TypeDef* typeDef = MetaData.GetTypeDefFromName(pCLIFile->pMetaData, nameSpace, name, null, /* assertExists */ 0);
        		if (typeDef != null) {
        			return typeDef;
        		}

        		pFiles = pFiles->pNext;
        	}

            Sys.Crash("CLIFile_FindTypeInAllLoadedAssemblies(): Cannot find type %s.%s", (PTR)nameSpace, (PTR)name);
        	return null;
        }

        private static byte[] LoadFileFromDisk(byte* fileName) 
        {
            string fileNameStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)fileName);
            if (fileNameStr.StartsWith("${RESOURCES}")) {
                #if UNITY_5 || UNITY_2017 || UNITY_2018
                fileNameStr = fileNameStr.Substring(13);
                UnityEngine.TextAsset bindata = UnityEngine.Resources.Load<UnityEngine.TextAsset>(fileNameStr);
                return bindata != null ? bindata.bytes : null;
                #else
                return null;
                #endif
            } else {
                if (!System.IO.File.Exists(fileNameStr))
                    return null;
                return System.IO.File.ReadAllBytes(fileNameStr);
            }
        }

        private static tCLIFile* LoadPEFile(byte[] image) 
        {
            tCLIFile *pRet = ((tCLIFile*)Mem.malloc((SIZE_T)sizeof(tCLIFile)));

            System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.Alloc(image, GCHandleType.Pinned);
            byte* pData = (byte*)handle.AddrOfPinnedObject();

        	byte *pMSDOSHeader = (byte*)&(((byte*)pData)[0]);
        	byte *pPEHeader;
        	byte *pPEOptionalHeader;
        	byte *pPESectionHeaders;
        	byte *pCLIHeader;
        	byte *pRawMetaData;

        	int i;
        	uint lfanew;
        	ushort machine;
        	int numSections;
        	//uint imageBase;
        	//int fileAlignment;
            uint cliHeaderRVA;
            //uint cliHeaderSize;
            uint metaDataRVA;
            //uint metaDataSize;
        	tMetaData *pMetaData;

        	pRet->pRVA = RVA.New();
            pRet->gcHandle = (PTR)(System.IntPtr)handle;
        	pRet->pMetaData = pMetaData = MetaData.New();

        	lfanew = *(uint*)&(pMSDOSHeader[0x3c]);
        	pPEHeader = pMSDOSHeader + lfanew + 4;
        	pPEOptionalHeader = pPEHeader + 20;
        	pPESectionHeaders = pPEOptionalHeader + 224;

        	machine = *(ushort*)&(pPEHeader[0]);
        	if (machine != DOT_NET_MACHINE) {
        		return null;
        	}
        	numSections = *(ushort*)&(pPEHeader[2]);

        	//imageBase = *(uint*)&(pPEOptionalHeader[28]);
        	//fileAlignment = *(int*)&(pPEOptionalHeader[36]);

        	for (i=0; i<numSections; i++) {
        		byte *pSection = pPESectionHeaders + i * 40;
        		RVA.Create(pRet->pRVA, pData, pSection);
        	}

        	cliHeaderRVA = *(uint*)&(pPEOptionalHeader[208]);
        	//cliHeaderSize = *(uint*)&(pPEOptionalHeader[212]);

            pCLIHeader = (byte*)RVA.FindData(pRet->pRVA, cliHeaderRVA);

        	metaDataRVA = *(uint*)&(pCLIHeader[8]);
        	//metaDataSize = *(uint*)&(pCLIHeader[12]);
        	pRet->entryPoint = *(uint*)&(pCLIHeader[20]);
            pRawMetaData = (byte*)RVA.FindData(pRet->pRVA, metaDataRVA);

        	// Load all metadata
        	{
        		uint versionLen = *(uint*)&(pRawMetaData[12]);
        		uint ofs, numberOfStreams;
        		void *pTableStream = null;
        		uint tableStreamSize = 0;
        		pRet->pVersion = &(pRawMetaData[16]);
                Sys.log_f(1, "CLI version: %s\n", (PTR)pRet->pVersion);
        		ofs = 16 + versionLen;
        		numberOfStreams = *(ushort*)&(pRawMetaData[ofs + 2]);
        		ofs += 4;

        		for (i=0; i<(int)numberOfStreams; i++) {
        			uint streamOffset = *(uint*)&pRawMetaData[ofs];
        			uint streamSize = *(uint*)&pRawMetaData[ofs+4];
        			byte *pStreamName = &pRawMetaData[ofs+8];
        			void *pStream = pRawMetaData + streamOffset;
        			ofs += (uint)((S.strlen(pStreamName)+4) & (~0x3)) + 8;
                    if (S.strcasecmp(pStreamName, "#Strings") == 0) {
        				MetaData.LoadStrings(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, "#US") == 0) {
        				MetaData.LoadUserStrings(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, "#Blob") == 0) {
        				MetaData.LoadBlobs(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, "#GUID") == 0) {
        				MetaData.LoadGUIDs(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, "#~") == 0) {
        				pTableStream = pStream;
        				tableStreamSize = streamSize;
        			}
        		}
        		// Must load tables last
        		if (pTableStream != null) {
                    MetaData.LoadTables(pMetaData, pRet->pRVA, pTableStream, (uint)tableStreamSize);
        		}
        	}

        	// Mark all generic definition type and methods as such
            for (i=(int)pMetaData->tables.numRows[MetaDataTable.MD_TABLE_GENERICPARAM]; i>0; i--) {
        		tMD_GenericParam *pGenericParam;
        		/*IDX_TABLE*/uint ownerIdx;

        		pGenericParam = (tMD_GenericParam*)MetaData.GetTableRow
                    (pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_GENERICPARAM, (uint)i));
        		ownerIdx = pGenericParam->owner;
        		switch (MetaData.TABLE_ID(ownerIdx)) {
        			case MetaDataTable.MD_TABLE_TYPEDEF:
        				{
        					tMD_TypeDef *pTypeDef = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, ownerIdx);
        					pTypeDef->isGenericDefinition = 1;
        				}
        				break;
        			case MetaDataTable.MD_TABLE_METHODDEF:
        				{
        					tMD_MethodDef *pMethodDef = (tMD_MethodDef*)MetaData.GetTableRow(pMetaData, ownerIdx);
        					pMethodDef->isGenericDefinition = 1;
        				}
        				break;
        			default:
        				Sys.Crash("Wrong generic parameter owner: 0x%08x", ownerIdx);
                        break;
        		}
        	}

        	// Mark all nested classes as such
            for (i=(int)pMetaData->tables.numRows[MetaDataTable.MD_TABLE_NESTEDCLASS]; i>0; i--) {
        		tMD_NestedClass *pNested;
                tMD_TypeDef *pParent;
                tMD_TypeDef *pChild;

                pNested = (tMD_NestedClass*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_NESTEDCLASS, (uint)i));
        		pParent = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, pNested->enclosingClass);
        		pChild = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, pNested->nestedClass);
        		pChild->pNestedIn = pParent;
        	}

        	return pRet;
        }

        public static tCLIFile* LoadAssembly(/*char**/byte *pFileName) 
        {
            byte[] rawData;
        	tCLIFile *pRet;
        	tFilesLoaded *pNewFile;
            byte* filepath = stackalloc byte[512];
            tMD_Assembly* pThisAssembly = null;

            rawData = null;
            for (int i = 0; i < assemblySearchPathsCount; i++)
            {
                S.snprintf(filepath, 512, "%s/%s", (PTR)assemblySearchPaths[i], (PTR)pFileName);
                rawData = LoadFileFromDisk(filepath);
                if (rawData != null)
                    break;
            }
            if (rawData == null)
                Sys.Crash("Unable to load assembly file %s", (PTR)pFileName);

            Sys.log_f(1, "\nLoading file: %s\n", (PTR)pFileName);

            pRet = LoadPEFile(rawData);

            // Get the assembly info - there is only ever one of these in the each file's metadata
            pThisAssembly = (tMD_Assembly*)MetaData.GetTableRow(pRet->pMetaData, MetaData.MAKE_TABLE_INDEX(0x20, 1));
            int nameLen = S.strlen(pThisAssembly->name) + 1;
            pRet->assemblyName = (byte*)Mem.mallocForever((uint)nameLen);
            S.strncpy(pRet->assemblyName, pThisAssembly->name, nameLen);

        	// Record that we've loaded this file
            pNewFile = ((tFilesLoaded*)Mem.mallocForever((SIZE_T)sizeof(tFilesLoaded)));
        	pNewFile->pCLIFile = pRet;
        	pNewFile->pNext = pFilesLoaded;
        	pFilesLoaded = pNewFile;

        	return pRet;
        }

        public static tCLIFile* WrapMonoAssembly(/*char**/byte* pAssemblyName)
        {
            tCLIFile* pRet;
            tFilesLoaded* pNewFile;
            tMetaData* pMetaData;
            System.Reflection.Assembly assembly = null;

            string assemblyName = Marshal.PtrToStringAnsi((System.IntPtr)pAssemblyName);
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++) {
                System.Reflection.Assembly assem = assemblies[i];
                if (assem.GetName().Name == assemblyName) {
                    assembly = assem;
                    break;
                }
            }

            if (assembly == null) {
                Sys.Crash("Unable to load assembly file %s", (PTR)pAssemblyName);
            }

            pRet = ((tCLIFile*)Mem.malloc((SIZE_T)sizeof(tCLIFile)));
            Mem.memset(pRet, 0, (SIZE_T)sizeof(tCLIFile));
            
            pRet->pMetaData = pMetaData = MetaData.New();
            MetaData.WrapMonoAssembly(pMetaData, assembly);

            string codeBase = assembly.CodeBase;
            System.UriBuilder uri = new System.UriBuilder(codeBase);
            string path = System.Uri.UnescapeDataString(uri.Path);
            string assmName = System.IO.Path.GetFileNameWithoutExtension(path);
            pRet->assemblyName = new S(assmName);

            // Record that we've loaded this file
            pNewFile = ((tFilesLoaded*)Mem.mallocForever((SIZE_T)sizeof(tFilesLoaded)));
            pNewFile->pCLIFile = pRet;
            pNewFile->pNext = pFilesLoaded;
            pFilesLoaded = pNewFile;

            return pRet;
        }

        public static int Execute(tCLIFile *pThis, string[] args) 
        {
        	tThread *pThread;
        	/*HEAP_PTR*/byte* pArgs;
        	int i;

            // Create a string array for the program arguments
            // Don't include the argument that is the program name.
            pArgs = System_Array.NewVector(Type.types[Type.TYPE_SYSTEM_ARRAY_STRING], (uint)(args.Length - 1));
        	Heap.MakeUndeletable(pArgs);
        	for (i = 1; i < args.Length; i++) {
        		tSystemString* pArgStr = System_String.FromMonoString(args[i]);
        		System_Array.StoreElement(pArgs, (uint)(i - 1), (byte*)&pArgStr);
        	}

        	// Create the main application thread
        	pThread = Thread.New();
        	Thread.SetEntryPoint(pThread, pThis->pMetaData, pThis->entryPoint, (byte*)&pArgs, (uint)sizeof(void*));

        	return Thread.Execute();
        }

        public static void GetHeapRoots(tHeapRoots *pHeapRoots) 
        {
        	tFilesLoaded *pFile;

        	pFile = pFilesLoaded;
        	while (pFile != null) {
        		MetaData.GetHeapRoots(pHeapRoots, pFile->pCLIFile->pMetaData);
        		pFile = pFile->pNext;
        	}
        }
    }
}
