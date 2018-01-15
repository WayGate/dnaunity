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
    #if UNITY_WEBGL || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif 

    public unsafe static class Type
    {

        public const int ELEMENT_TYPE_VOID          = 0x01;
        public const int ELEMENT_TYPE_BOOLEAN       = 0x02;
        public const int ELEMENT_TYPE_CHAR          = 0x03;
        public const int ELEMENT_TYPE_I1            = 0x04;
        public const int ELEMENT_TYPE_U1            = 0x05;
        public const int ELEMENT_TYPE_I2            = 0x06;
        public const int ELEMENT_TYPE_U2            = 0x07;
        public const int ELEMENT_TYPE_I4            = 0x08;
        public const int ELEMENT_TYPE_U4            = 0x09;
        public const int ELEMENT_TYPE_I8            = 0x0a;
        public const int ELEMENT_TYPE_U8            = 0x0b;
        public const int ELEMENT_TYPE_R4            = 0x0c;
        public const int ELEMENT_TYPE_R8            = 0x0d;
        public const int ELEMENT_TYPE_STRING        = 0x0e;
        public const int ELEMENT_TYPE_PTR           = 0x0f;
        public const int ELEMENT_TYPE_BYREF         = 0x10;
        public const int ELEMENT_TYPE_VALUETYPE     = 0x11;
        public const int ELEMENT_TYPE_CLASS         = 0x12;
        public const int ELEMENT_TYPE_VAR           = 0x13; // Generic argument type

        public const int ELEMENT_TYPE_GENERICINST   = 0x15;

        public const int ELEMENT_TYPE_INTPTR        = 0x18;
        public const int ELEMENT_TYPE_UINTPTR       = 0x19;

        public const int ELEMENT_TYPE_OBJECT        = 0x1c;
        public const int ELEMENT_TYPE_SZARRAY       = 0x1d;
        public const int ELEMENT_TYPE_MVAR          = 0x1e;


        public const int TYPE_SYSTEM_OBJECT                             = 0;
        public const int TYPE_SYSTEM_ARRAY_NO_TYPE                      = 1;
        public const int TYPE_SYSTEM_VOID                               = 2;
        public const int TYPE_SYSTEM_BOOLEAN                            = 3;
        public const int TYPE_SYSTEM_BYTE                               = 4;
        public const int TYPE_SYSTEM_SBYTE                              = 5;
        public const int TYPE_SYSTEM_CHAR                               = 6;
        public const int TYPE_SYSTEM_INT16                              = 7;
        public const int TYPE_SYSTEM_INT32                              = 8;
        public const int TYPE_SYSTEM_STRING                             = 9;
        public const int TYPE_SYSTEM_INTPTR                             = 10;
        public const int TYPE_SYSTEM_RUNTIMEFIELDHANDLE                 = 11;
        public const int TYPE_SYSTEM_INVALIDCASTEXCEPTION               = 12;
        public const int TYPE_SYSTEM_UINT32                             = 13;
        public const int TYPE_SYSTEM_UINT16                             = 14;
        public const int TYPE_SYSTEM_ARRAY_CHAR                         = 15;
        public const int TYPE_SYSTEM_ARRAY_OBJECT                       = 16;
        public const int TYPE_SYSTEM_COLLECTIONS_GENERIC_IENUMERABLE_T  = 17;
        public const int TYPE_SYSTEM_COLLECTIONS_GENERIC_ICOLLECTION_T  = 18;
        public const int TYPE_SYSTEM_COLLECTIONS_GENERIC_ILIST_T        = 19;
        public const int TYPE_SYSTEM_MULTICASTDELEGATE                  = 20;
        public const int TYPE_SYSTEM_NULLREFERENCEEXCEPTION             = 21;
        public const int TYPE_SYSTEM_SINGLE                             = 22;
        public const int TYPE_SYSTEM_DOUBLE                             = 23;
        public const int TYPE_SYSTEM_INT64                              = 24;
        public const int TYPE_SYSTEM_UINT64                             = 25;
        public const int TYPE_SYSTEM_RUNTIMETYPE                        = 26;
        public const int TYPE_SYSTEM_TYPE                               = 27;
        public const int TYPE_SYSTEM_RUNTIMETYPEHANDLE                  = 28;
        public const int TYPE_SYSTEM_RUNTIMEMETHODHANDLE                = 29;
        public const int TYPE_SYSTEM_ENUM                               = 30;
        public const int TYPE_SYSTEM_ARRAY_STRING                       = 31;
        public const int TYPE_SYSTEM_ARRAY_INT32                        = 32;
        public const int TYPE_SYSTEM_THREADING_THREAD                   = 33;
        public const int TYPE_SYSTEM_THREADING_THREADSTART              = 34;
        public const int TYPE_SYSTEM_THREADING_PARAMETERIZEDTHREADSTART = 35;
        public const int TYPE_SYSTEM_WEAKREFERENCE                      = 36;
        public const int TYPE_SYSTEM_IO_FILEMODE                        = 37;
        public const int TYPE_SYSTEM_IO_FILEACCESS                      = 38;
        public const int TYPE_SYSTEM_IO_FILESHARE                       = 39;
        public const int TYPE_SYSTEM_ARRAY_BYTE                         = 40;
        public const int TYPE_SYSTEM_GLOBALIZATION_UNICODECATEGORY      = 41;
        public const int TYPE_SYSTEM_OVERFLOWEXCEPTION                  = 42;
        public const int TYPE_SYSTEM_PLATFORMID                         = 43;
        public const int TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES            = 44;
        public const int TYPE_SYSTEM_UINTPTR                            = 45;
        public const int TYPE_SYSTEM_NULLABLE                           = 46;
        public const int TYPE_SYSTEM_ARRAY_TYPE                         = 47;
        public const int TYPE_SYSTEM_REFLECTION_PROPERTYINFO            = 48;
        public const int TYPE_SYSTEM_REFLECTION_METHODINFO              = 49;
        public const int TYPE_SYSTEM_REFLECTION_METHODBASE              = 50;

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct tArrayTypeDefs 
        {
        	public tMD_TypeDef *pArrayType;
            public tMD_TypeDef *pElementType;

            public tArrayTypeDefs *pNext;
        };

        static tArrayTypeDefs *pArrays;

        const int GENERICARRAYMETHODS_NUM = 13;
        static byte genericArrayMethodsInited = 0;
        static tMD_MethodDef*[] ppGenericArrayMethods = new tMD_MethodDef*[GENERICARRAYMETHODS_NUM];

        const int GENERICARRAYMETHODS_Internal_GetGenericEnumerator     = 0;
        const int GENERICARRAYMETHODS_get_Length                        = 1;
        const int GENERICARRAYMETHODS_get_IsReadOnly                    = 2;
        const int GENERICARRAYMETHODS_Internal_GenericAdd               = 3;
        const int GENERICARRAYMETHODS_Internal_GenericClear             = 4;
        const int GENERICARRAYMETHODS_Internal_GenericContains          = 5;
        const int GENERICARRAYMETHODS_Internal_GenericCopyTo            = 6;
        const int GENERICARRAYMETHODS_Internal_GenericRemove            = 7;
        const int GENERICARRAYMETHODS_Internal_GenericIndexOf           = 8;
        const int GENERICARRAYMETHODS_Internal_GenericInsert            = 9;
        const int GENERICARRAYMETHODS_Internal_GenericRemoveAt          = 10;
        const int GENERICARRAYMETHODS_Internal_GenericGetItem           = 11;
        const int GENERICARRAYMETHODS_Internal_GenericSetItem           = 12;
        static /*char**/ byte*[] pGenericArrayMethodsInit = new byte*[] {
            new S("Internal_GetGenericEnumerator"),
            new S("get_Length"),
            new S("Internal_GenericIsReadOnly"),
            new S("Internal_GenericAdd"),
            new S("Internal_GenericClear"),
            new S("Internal_GenericContains"),
            new S("Internal_GenericCopyTo"),
            new S("Internal_GenericRemove"),
            new S("Internal_GenericIndexOf"),
            new S("Internal_GenericInsert"),
            new S("Internal_GenericRemoveAt"),
            new S("Internal_GenericGetItem"),
            new S("Internal_GenericSetItem"),
        };

        static void GetMethodDefs() 
        {
        	/*IDX_TABLE*/uint token, last;
        	tMetaData *pMetaData;

        	pMetaData = types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE]->pMetaData;
        	last = types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE]->isLast != 0?
        		MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_METHODDEF, pMetaData->tables.numRows[MetaDataTable.MD_TABLE_METHODDEF]):
        		(types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE][1].methodList - 1);
        	token = types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE]->methodList;
        	for (; token <= last; token++) {
        		tMD_MethodDef *pMethod;
        		uint i;

        		pMethod = (tMD_MethodDef*)MetaData.GetTableRow(pMetaData, token);
        		for (i=0; i<GENERICARRAYMETHODS_NUM; i++) {
        			if (S.strcmp(pMethod->name, pGenericArrayMethodsInit[i]) == 0) {
        				ppGenericArrayMethods[i] = pMethod;
        				break;
        			}
        		}

        	}
        	genericArrayMethodsInited = 1;
        }

        static void CreateNewArrayType(tMD_TypeDef *pNewArrayType, tMD_TypeDef *pElementType, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
            throw new System.NotImplementedException();
            #if NO
        	MetaData.Fill_TypeDef(types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE], null, null);

            Mem.memcpy(pNewArrayType, types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE], (SIZE_T)sizeof(tMD_TypeDef));
        	pNewArrayType->pArrayElementType = pElementType;
        	pNewArrayType->isFilled = 1;

        	// Auto-generate the generic interfaces IEnumerable<T>, ICollection<T> and IList<T> for this array
        	{
                tInterfaceMap *pInterfaceMap;
                tInterfaceMap *pAllIMs;
        		tMD_TypeDef *pInterfaceT;
        		tMD_MethodDef *pMethod;
        		uint orgNumInterfaces;

        		if (genericArrayMethodsInited == 0) {
        			GetMethodDefs();
        		}

        		orgNumInterfaces = pNewArrayType->numInterfaces;
        		pNewArrayType->numInterfaces += 3;
        		pAllIMs = (tInterfaceMap*)Mem.mallocForever(pNewArrayType->numInterfaces * sizeof(tInterfaceMap));
        		Mem.memcpy(pAllIMs, pNewArrayType->pInterfaceMaps, orgNumInterfaces * sizeof(tInterfaceMap));
        		pNewArrayType->pInterfaceMaps = pAllIMs;

        		// Get the IEnumerable<T> interface
        		pInterfaceMap = &pAllIMs[orgNumInterfaces + 0];
        		pInterfaceT = Generics_GetGenericTypeFromCoreType(types[Type.TYPE_SYSTEM_COLLECTIONS_GENERIC_IENUMERABLE_T], 1, &pElementType);
        		pInterfaceMap->pInterface = pInterfaceT;
        		pInterfaceMap->pVTableLookup = null;
        		pInterfaceMap->ppMethodVLookup = Mem.mallocForever(pInterfaceT->numVirtualMethods * sizeof(tMD_MethodDef*));
        		pMethod = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GetGenericEnumerator], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[0] = pMethod;

        		// Get the ICollection<T> interface
        		pInterfaceMap = &pAllIMs[orgNumInterfaces + 1];
        		pInterfaceT = Generics_GetGenericTypeFromCoreType(types[Type.TYPE_SYSTEM_COLLECTIONS_GENERIC_ICOLLECTION_T], 1, &pElementType);
        		pInterfaceMap->pInterface = pInterfaceT;
        		pInterfaceMap->pVTableLookup = null;
        		pInterfaceMap->ppMethodVLookup = Mem.mallocForever(pInterfaceT->numVirtualMethods * sizeof(tMD_MethodDef*));
        		pInterfaceMap->ppMethodVLookup[0] = ppGenericArrayMethods[GENERICARRAYMETHODS_get_Length];
        		pInterfaceMap->ppMethodVLookup[1] = ppGenericArrayMethods[GENERICARRAYMETHODS_get_IsReadOnly];
        		pInterfaceMap->ppMethodVLookup[2] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericAdd], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[3] = ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericClear];
        		pInterfaceMap->ppMethodVLookup[4] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericContains], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[5] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericCopyTo], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[6] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericRemove], pNewArrayType, 1, &pElementType);

        		// Get the IList<T> interface
        		pInterfaceMap = &pAllIMs[orgNumInterfaces + 2];
        		pInterfaceT = Generics_GetGenericTypeFromCoreType(types[Type.TYPE_SYSTEM_COLLECTIONS_GENERIC_ILIST_T], 1, &pElementType); //, ppClassTypeArgs, ppMethodTypeArgs);
        		pInterfaceMap->pInterface = pInterfaceT;
        		pInterfaceMap->pVTableLookup = null;
        		pInterfaceMap->ppMethodVLookup = Mem.mallocForever(pInterfaceT->numVirtualMethods * sizeof(tMD_MethodDef*));
        		pInterfaceMap->ppMethodVLookup[0] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericIndexOf], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[1] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericInsert], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[2] = ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericRemoveAt];
        		pInterfaceMap->ppMethodVLookup[3] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericGetItem], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[4] = Generics_GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericSetItem], pNewArrayType, 1, &pElementType);
        	}

        	Sys.log_f(2, "Array: Array[%s.%s]\n", pElementType->nameSpace, pElementType->name);
            #endif
        }

        // Returns a TypeDef for an array to the given element type
        public static tMD_TypeDef* GetArrayTypeDef(tMD_TypeDef *pElementType, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	tArrayTypeDefs *pIterArrays;

        	if (pElementType == null) {
        		return types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE];
        	}
        	
        	pIterArrays = pArrays;
        	while (pIterArrays != null) {
        		if (pIterArrays->pElementType == pElementType) {
        			return pIterArrays->pArrayType;
        		}
        		pIterArrays = pIterArrays->pNext;
        	}

        	// Must have this new array type in the linked-list of array type before it is initialised
        	// (otherwise it can get stuck in an infinite loop)
            pIterArrays = ((tArrayTypeDefs*)Mem.mallocForever((SIZE_T)sizeof(tArrayTypeDefs)));
        	pIterArrays->pElementType = pElementType;
        	pIterArrays->pNext = pArrays;
        	pArrays = pIterArrays;
            pIterArrays->pArrayType = ((tMD_TypeDef*)Mem.malloc((SIZE_T)sizeof(tMD_TypeDef)));

        	CreateNewArrayType(pIterArrays->pArrayType, pElementType, ppClassTypeArgs, ppMethodTypeArgs);
        	return pIterArrays->pArrayType;
        }

        static byte* scSystem, scValueType, scObject;

        public static uint IsValueType(tMD_TypeDef *pTypeDef) 
        {
        	// If this type is an interface, then return 0
        	if (MetaData.TYPE_ISINTERFACE(pTypeDef)) {
        		return 0;
        	}
        	// If this type is Object or ValueType then return an answer
            if (S.strcmp(pTypeDef->nameSpace, new S(ref scSystem, "System")) == 0) {
                if (S.strcmp(pTypeDef->name, new S(ref scValueType, "ValueType")) == 0) {
        			return 1;
        		}
                if (S.strcmp(pTypeDef->name, new S(ref scObject, "Object")) == 0) {
        			return 0;
        		}
        	}
        	// Return the isValueType determined by parent type
        	pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pTypeDef->pMetaData, pTypeDef->extends, null, null);
        	MetaData.Fill_TypeDef(pTypeDef, null, null);
        	return pTypeDef->isValueType;
        }

        // Get the TypeDef from the type signature
        // Also get the size of a field from the signature
        // This is needed to avoid recursive sizing of type like System.Boolean,
        // that has a field of type System.Boolean
        public static tMD_TypeDef* GetTypeFromSig(tMetaData *pMetaData, /*SIG*/byte* *pSig, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	uint entry;

        	entry = MetaData.DecodeSigEntry(pSig);
        	switch (entry) {
        		case Type.ELEMENT_TYPE_VOID:
        			return null;

        		case Type.ELEMENT_TYPE_BOOLEAN:
        			return types[Type.TYPE_SYSTEM_BOOLEAN];

        		case Type.ELEMENT_TYPE_CHAR:
        			return types[Type.TYPE_SYSTEM_CHAR];

        		case Type.ELEMENT_TYPE_I1:
        			return types[Type.TYPE_SYSTEM_SBYTE];

        		case Type.ELEMENT_TYPE_U1:
        			return types[Type.TYPE_SYSTEM_BYTE];

        		case Type.ELEMENT_TYPE_I2:
        			return types[Type.TYPE_SYSTEM_INT16];

        		case Type.ELEMENT_TYPE_U2:
        			return types[Type.TYPE_SYSTEM_UINT16];

        		case Type.ELEMENT_TYPE_I4:
        			return types[Type.TYPE_SYSTEM_INT32];

        		case Type.ELEMENT_TYPE_I8:
        			return types[Type.TYPE_SYSTEM_INT64];

        		case Type.ELEMENT_TYPE_U8:
        			return types[Type.TYPE_SYSTEM_UINT64];

        		case Type.ELEMENT_TYPE_U4:
        			return types[Type.TYPE_SYSTEM_UINT32];

        		case Type.ELEMENT_TYPE_R4:
        			return types[Type.TYPE_SYSTEM_SINGLE];

        		case Type.ELEMENT_TYPE_R8:
        			return types[Type.TYPE_SYSTEM_DOUBLE];

        		case Type.ELEMENT_TYPE_STRING:
        			return types[Type.TYPE_SYSTEM_STRING];

        		case Type.ELEMENT_TYPE_PTR:
        			return types[Type.TYPE_SYSTEM_UINTPTR];

        		case Type.ELEMENT_TYPE_BYREF:
        			{
        				tMD_TypeDef *pByRefType;

        				// type of the by-ref parameter, don't care
        				pByRefType = Type.GetTypeFromSig(pMetaData, pSig, ppClassTypeArgs, ppMethodTypeArgs);
        			}
                    return types[Type.TYPE_SYSTEM_INTPTR];

                case Type.ELEMENT_TYPE_INTPTR:
        			return types[Type.TYPE_SYSTEM_INTPTR];

        		case Type.ELEMENT_TYPE_VALUETYPE:
        		case Type.ELEMENT_TYPE_CLASS:
        			entry = MetaData.DecodeSigEntryToken(pSig);
        			return MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, entry, ppClassTypeArgs, ppMethodTypeArgs);

        		case Type.ELEMENT_TYPE_VAR:
        			entry = MetaData.DecodeSigEntry(pSig); // This is the argument number
        			if (ppClassTypeArgs == null) {
        				// Return null here as we don't yet know what the type really is.
        				// The generic instantiation code figures this out later.
        				return null;
        			} else {
        				return ppClassTypeArgs[entry];
        			}

        		case Type.ELEMENT_TYPE_GENERICINST:
        			{
        				tMD_TypeDef *pType;

        				pType = Generics.GetGenericTypeFromSig(pMetaData, pSig, ppClassTypeArgs, ppMethodTypeArgs);
        				return pType;
        			}

        		//case Type.ELEMENT_TYPE_INTPTR:
        		//	return types[Type.TYPE_SYSTEM_INTPTR];

        		case Type.ELEMENT_TYPE_UINTPTR:
        			return types[Type.TYPE_SYSTEM_UINTPTR];

        		case Type.ELEMENT_TYPE_OBJECT:
        			return types[Type.TYPE_SYSTEM_OBJECT];

        		case Type.ELEMENT_TYPE_SZARRAY:
        			{
        				tMD_TypeDef *pElementType;

        				pElementType = Type.GetTypeFromSig(pMetaData, pSig, ppClassTypeArgs, ppMethodTypeArgs);
        				return Type.GetArrayTypeDef(pElementType, ppClassTypeArgs, ppMethodTypeArgs);
        			}

        		case Type.ELEMENT_TYPE_MVAR:
        			entry = MetaData.DecodeSigEntry(pSig); // This is the argument number
        			if (ppMethodTypeArgs == null) {
        				// Can't do anything sensible, as we don't have any type args
        				return null;
        			} else {
        				return ppMethodTypeArgs[entry];
        			}

        		default:
        			Sys.Crash("Type.GetTypeFromSig(): Cannot handle signature element type: 0x%02x", entry);
                    return null;
        	}
        }

        public static tMD_TypeDef **types;
        static uint numInitTypes;

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct tTypeInit
        {
        	public /*char**/byte* assemblyName;
            public /*char**/byte* nameSpace;
            public /*char**/byte* name;
            public byte stackType;
            public byte stackSize;
            public byte arrayElementSize;
            public byte instanceMemSize;
        };

        static byte* scMscorlib;
        static byte* scSystemCollectionsGeneric;
        static byte* scSystemReflection;
        static byte* scSystemThreading;
        static byte* scSystemIO;
        static byte* scSystemGlobalization;

        #if UNITY_WEBGL || DNA_32BIT
        const int PTR_SIZE = 4;
        #else
        const int PTR_SIZE = 8;
        #endif 


        static tTypeInit[] typeInit = {
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Object"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Array"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Void"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Boolean"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 4, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Byte"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 1, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("SByte"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 1, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Char"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 2, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Int16"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 2, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Int32"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 4, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("String"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("IntPtr"), stackType = EvalStack.EVALSTACK_PTR, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeFieldHandle"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("InvalidCastException"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UInt32"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 4, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UInt16"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 4, arrayElementSize = 2, instanceMemSize = 4},
            new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_CHAR, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_OBJECT, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemCollectionsGeneric, "System.Collections.Generic"), name = new S("IEnumerable`1"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemCollectionsGeneric, "System.Collections.Generic"), name = new S("ICollection`1"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemCollectionsGeneric, "System.Collections.Generic"), name = new S("IList`1"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("MulticastDelegate"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("NullReferenceException"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Single"), stackType = EvalStack.EVALSTACK_F32, stackSize = 4, arrayElementSize = 4, instanceMemSize = 4},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Double"), stackType = EvalStack.EVALSTACK_F64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Int64"), stackType = EvalStack.EVALSTACK_INT64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UInt64"), stackType = EvalStack.EVALSTACK_INT64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeType"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tRuntimeType)},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Type"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeTypeHandle"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeMethodHandle"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Enum"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_STRING, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_INT32, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemThreading, "System.Threading"), name = new S("Thread"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tThread)},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemThreading, "System.Threading"), name = new S("ThreadStart"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemThreading, "System.Threading"), name = new S("ParameterizedThreadStart"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("WeakReference"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileMode"), stackType = EvalStack.EVALSTACK_O, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileAccess"), stackType = EvalStack.EVALSTACK_O, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileShare"), stackType = EvalStack.EVALSTACK_O, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_BYTE, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemGlobalization, "System.Globalization"), name = new S("UnicodeCategory"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("OverflowException"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("PlatformID"), stackType = EvalStack.EVALSTACK_INT32, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileAttributes"), stackType = EvalStack.EVALSTACK_O, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UIntPtr"), stackType = EvalStack.EVALSTACK_PTR, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Nullable`1"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_TYPE, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Reflection"), name = new S("PropertyInfo"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tPropertyInfo)},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Reflection"), name = new S("MethodInfo"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tMethodInfo)},
            new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Reflection"), name = new S("MethodBase"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tMethodBase)},
        };
        static int CorLibDone = 0;

        public static void Init() 
        {
        	uint i;

        	// Build all the type needed by the interpreter.
            numInitTypes = (uint)typeInit.Length;
            types = (tMD_TypeDef**)Mem.mallocForever((SIZE_T)(numInitTypes * sizeof(tMD_TypeDef*)));
        	for (i=0; i<numInitTypes; i++) {
        		if (typeInit[i].assemblyName != null) {
        			// Normal type initialisation
        			types[i] = MetaData.GetTypeDefFromFullName(typeInit[i].assemblyName, typeInit[i].nameSpace, typeInit[i].name);
        			// For the pre-defined system types, fill in the well-known memory sizes
        			types[i]->stackType = typeInit[i].stackType;
        			types[i]->stackSize = typeInit[i].stackSize;
        			types[i]->arrayElementSize = typeInit[i].arrayElementSize;
        			types[i]->instanceMemSize = typeInit[i].instanceMemSize;
        		}
        	}
        	for (i=0; i<numInitTypes; i++) {
        		if (typeInit[i].assemblyName != null) {
        			MetaData.Fill_TypeDef(types[i], null, null);
        		} else {
        			// Special initialisation for arrays of particular types.
        			types[i] = Type.GetArrayTypeDef(types[(uint)(typeInit[i].name)], null, null);
        		}
        	}
        	CorLibDone = 1;
        }

        public static uint IsMethod(tMD_MethodDef *pMethod, /*STRING*/byte* name, tMD_TypeDef *pReturnType, uint numParams, byte *pParamTypeIndexs) {
        	/*SIG*/byte* sig;
        	uint sigLen, numSigParams, i, nameLen;

        	nameLen = (uint)S.strlen(name);
        	if (name[nameLen-1] == '>') {
        		// Generic instance method
                if (S.strncmp(pMethod->name, name, (int)(nameLen - 1)) != 0) {
        			return 0;
        		}
        	} else {
        		if (S.strcmp(pMethod->name, name) != 0) {
        			return 0;
        		}
        	}

        	sig = MetaData.GetBlob(pMethod->signature, &sigLen);
        	i = MetaData.DecodeSigEntry(&sig); // Don't care about this
            if ((i & MetaData.SIG_METHODDEF_GENERIC) != 0) {
        		MetaData.DecodeSigEntry(&sig);
        	}
        	numSigParams = MetaData.DecodeSigEntry(&sig);

        	if (numParams != numSigParams) {
        		return 0;
        	}

        	if (pReturnType == types[Type.TYPE_SYSTEM_VOID]) {
        		pReturnType = null;
        	}

        	for (i=0; i<numParams + 1; i++) {
                tMD_TypeDef *pSigType;
                tMD_TypeDef *pParamType;

        		pSigType = Type.GetTypeFromSig(pMethod->pMetaData, &sig, null, null);
        		pParamType = (i == 0)?pReturnType:types[pParamTypeIndexs[i-1]];

        		if (pSigType != null && MetaData.TYPE_ISARRAY(pSigType) && pParamType == types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE]) {
        			// It's ok...
        		} else {
        			if (pSigType != pParamType) {
        				goto endBad;
        			}
        		}
        	}
        	return 1;

        endBad:
        	return 0;
        }

        public static uint IsDerivedFromOrSame(tMD_TypeDef *pBaseType, tMD_TypeDef *pTestType) {
        	while (pTestType != null) {
        		if (pTestType == pBaseType) {
        			return 1;
        		}
        		MetaData.Fill_TypeDef(pTestType, null, null);
        		pTestType = pTestType->pParent;
        	}
        	return 0;
        }

        public static uint IsImplemented(tMD_TypeDef *pInterface, tMD_TypeDef *pTestType) {
        	uint i;

        	for (i=0; i<pTestType->numInterfaces; i++) {
        		if (pTestType->pInterfaceMaps[i].pInterface == pInterface) {
        			return 1;
        		}
        	}
        	return 0;
        }

        public static uint IsAssignableFrom(tMD_TypeDef *pToType, tMD_TypeDef *pFromType) {
        	return
        		(Type.IsDerivedFromOrSame(pToType, pFromType) != 0 ||
                    (MetaData.TYPE_ISINTERFACE(pToType) && Type.IsImplemented(pToType, pFromType) != 0)) ? (uint)1 : (uint)0;
        }

        public static /*HEAP_PTR*/byte* GetTypeObject(tMD_TypeDef *pTypeDef) {
        	if (pTypeDef->typeObject == null) {
        		pTypeDef->typeObject = SystemRuntimeType.New(pTypeDef);
        	}
        	return pTypeDef->typeObject;
        }

    }
}
