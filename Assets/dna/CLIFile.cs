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
        public /*char**/byte *pFileName;
        // The RVA sections of this file
        public tRVA *pRVA;
        // null-terminated UTF8 string of file version
        public byte *pVersion;
        // The entry point token if this is executable, 0 if it isn't
        public /*IDX_TABLE*/uint entryPoint;

        public tMetaData *pMetaData;
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
        static /*char**/byte** assembliesMappedToDnaCorlib = null;

        const int MAPPED_ASSEMBLIES_COUNT = 15;

        // Keep track of all the files currently loaded
        static tFilesLoaded *pFilesLoaded = null;

        static byte* scCorlib;

        public static void Init()
        {
            scCorlib = null;

            assembliesMappedToDnaCorlib = S.buildArray(
                "mscorlib",
                "System.Collections",
                "System.Console",
                "System.IO",
                "System.Linq",
                "System.Net.Http",
                "System.Private.CoreLib",
                "System.Private.Uri",
                "System.Reflection",
                "System.Reflection.Extensions",
                "System.Runtime",
                "System.Runtime.Extensions",
                "System.Runtime.InteropServices",
                "System.Threading",
                "System.Threading.Tasks"
            );

            pFilesLoaded = null;
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

        	// Where applicable, redirect this assembly lookup into DNA's corlib
        	// (e.g., mscorlib, System.Runtime, etc.)
            for (int i = 0; i < MAPPED_ASSEMBLIES_COUNT; i++) {
                if (S.strcmp(pAssemblyName, assembliesMappedToDnaCorlib[i]) == 0) {
                    pAssemblyName = new S(ref scCorlib, "corlib");
        			break;
        		}
        	}

        	// Look in already-loaded files first
        	pFiles = pFilesLoaded;
        	while (pFiles != null) {
        		tCLIFile *pCLIFile;
        		tMD_Assembly *pThisAssembly;

        		pCLIFile = pFiles->pCLIFile;
        		// Get the assembly info - there is only ever one of these in the each file's metadata
                pThisAssembly = (tMD_Assembly*)MetaData.GetTableRow(pCLIFile->pMetaData, MetaData.MAKE_TABLE_INDEX(0x20, 1));
        		if (S.strcmp(pAssemblyName, pThisAssembly->name) == 0) {
        			// Found the correct assembly, so return its meta-data
        			return pCLIFile->pMetaData;
        		}
        		pFiles = pFiles->pNext;
        	}

        	// Assembly not loaded, so load it if possible
        	{
        		tCLIFile *pCLIFile;
                byte* fileName = stackalloc byte[128];
                S.sprintf(fileName, "%s.dll", (PTR)pAssemblyName);
        		pCLIFile = CLIFile.Load(fileName);
        		if (pCLIFile == null) {
                    Sys.Crash("Cannot load required assembly file: %s", (PTR)fileName);
        		}
        		return pCLIFile->pMetaData;
        	}
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

        private static void* LoadFileFromDisk(byte* fileName) 
        {
            string fileNameStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)fileName);
            if (!System.IO.File.Exists(fileNameStr))
                throw new System.InvalidOperationException("File doesn't exist");


            byte[] data = System.IO.File.ReadAllBytes(fileNameStr);
            byte* buf = (byte*)Mem.malloc((SIZE_T)data.Length);
            fixed (byte* ptr = &data[0])
            {
                Mem.memcpy(buf, ptr, (SIZE_T)data.Length);
            }

            return buf;
        }

        private static tCLIFile* LoadPEFile(void *pData) 
        {
            tCLIFile *pRet = ((tCLIFile*)Mem.malloc((SIZE_T)sizeof(tCLIFile)));

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
                    if (S.strcasecmp(pStreamName, new S("#Strings")) == 0) {
        				MetaData.LoadStrings(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, new S("#US")) == 0) {
        				MetaData.LoadUserStrings(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, new S("#Blob")) == 0) {
        				MetaData.LoadBlobs(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, new S("#GUID")) == 0) {
        				MetaData.LoadGUIDs(pMetaData, pStream, streamSize);
                    } else if (S.strcasecmp(pStreamName, new S("#~")) == 0) {
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

        public static tCLIFile* Load(/*char**/byte *pFileName) 
        {
        	void *pRawFile;
        	tCLIFile *pRet;
        	tFilesLoaded *pNewFile;

        	pRawFile = LoadFileFromDisk(pFileName);

        	if (pRawFile == null) {
                Sys.Crash("Cannot load file: %s", (PTR)pFileName);
        	}

            Sys.log_f(1, "\nLoading file: %s\n", (PTR)pFileName);

        	pRet = LoadPEFile(pRawFile);
        	pRet->pFileName = (byte*)Mem.mallocForever((uint)S.strlen(pFileName) + 1);
        	S.strcpy(pRet->pFileName, pFileName);

        	// Record that we've loaded this file
            pNewFile = ((tFilesLoaded*)Mem.mallocForever((SIZE_T)sizeof(tFilesLoaded)));
        	pNewFile->pCLIFile = pRet;
        	pNewFile->pNext = pFilesLoaded;
        	pFilesLoaded = pNewFile;

        	return pRet;
        }

        public static int Execute(tCLIFile *pThis, int argc, /*char**/byte **argp) 
        {
            throw new System.NotImplementedException();

            #if NO

        	tThread *pThread;
        	/*HEAP_PTR*/byte* args;
        	int i;

        	// Create a string array for the program arguments
        	// Don't include the argument that is the program name.
        	argc--;
        	argp++;
            args = new string[argc];
        	Heap.MakeUndeletable(args);
        	for (i = 0; i < argc; i++) {
        		/*HEAP_PTR*/byte* arg = SystemString.FromCharPtrASCII(argp[i]);
        		SystemArray.StoreElement(args, i, (byte*)&arg);
        	}

        	// Create the main application thread
        	pThread = Thread();
        	Thread.SetEntryPoint(pThread, pThis->pMetaData, pThis->entryPoint, (byte*)&args, sizeof(void*));

        	return Thread.Execute();
            #endif
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