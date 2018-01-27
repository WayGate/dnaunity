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

    public unsafe static class System_Type
    {

        public static tAsyncCall* GetTypeFromHandle(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tMD_TypeDef *pTypeDef = *(tMD_TypeDef**)pParams;

        	*(/*HEAP_PTR*/byte**)pReturnValue = Type.GetTypeObject(pTypeDef);

        	return null;
        }

        public static tAsyncCall* get_IsValueType(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	
        	*(uint*)pReturnValue = pRuntimeType->pTypeDef->isValueType;

        	return null;
        }

        public static void DotNetStringToCString(byte* buf, uint bufLength, /*STRING*/byte* dotnetString) 
        {
        	uint stringLen;
        	/*STRING2*/char* dotnetString2 = System_String.GetString(dotnetString, &stringLen);
        	if (stringLen > bufLength) {
        		Sys.Crash("String of length %i was too long for buffer of length %i\n", stringLen, bufLength);
        	}

        	int i;
        	for (i=0; i<stringLen; i++) {
        		buf[i] = (byte)dotnetString2[i];
        	}
        	buf[i] = 0;
        }

        public static tAsyncCall* EnsureAssemblyLoaded(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	byte* assemblyName = stackalloc byte[256];
        	DotNetStringToCString(assemblyName, 256, ((/*HEAP_PTR*/byte**)pParams)[0]);
        	CLIFile.GetMetaDataForAssembly(assemblyName);

        	*(/*HEAP_PTR*/byte**)pReturnValue = null;
        	return null;
        }

        public static tAsyncCall* GetTypeFromName(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            byte* namespaceName = stackalloc byte[256];
            byte* className = stackalloc byte[256];
        	tMD_TypeDef *pTypeDef;

        	DotNetStringToCString(namespaceName, 256, ((/*HEAP_PTR*/byte**)pParams)[1]);
        	DotNetStringToCString(className, 256, ((/*HEAP_PTR*/byte**)pParams)[2]);

        	if (((/*HEAP_PTR*/byte**)pParams)[0] == null) {
        		// assemblyName is null, so search all loaded assemblies
        		pTypeDef = CLIFile.FindTypeInAllLoadedAssemblies(namespaceName, className);
        	} else {
        		// assemblyName is specified
                byte* assemblyName = stackalloc byte[256];
        		DotNetStringToCString(assemblyName, 256, ((/*HEAP_PTR*/byte**)pParams)[0]);
        		tMetaData *pAssemblyMetadata = CLIFile.GetMetaDataForAssembly(assemblyName);
        		pTypeDef = MetaData.GetTypeDefFromName(pAssemblyMetadata, namespaceName, className, null, /* assertExists */ 1);
        	}

        	MetaData.Fill_TypeDef(pTypeDef, null, null);
        	*(/*HEAP_PTR*/byte**)pReturnValue = Type.GetTypeObject(pTypeDef);
        	return null;
        }

        public static tAsyncCall* GetProperties(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	tMD_TypeDef *pTypeDef = pRuntimeType->pTypeDef;
        	tMetaData *pMetaData = pTypeDef->pMetaData;

        	// First we search through the table of propertymaps to find the propertymap for the requested type
        	uint i;
        	/*IDX_TABLE*/uint firstIdx = 0, lastIdxExc = 0;
        	uint numPropertyRows = pMetaData->tables.numRows[MetaDataTable.MD_TABLE_PROPERTY];
        	uint numPropertymapRows = pMetaData->tables.numRows[MetaDataTable.MD_TABLE_PROPERTYMAP];
        	for (i=1; i <= numPropertymapRows; i++) {
        		tMD_PropertyMap *pPropertyMap = (tMD_PropertyMap*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_PROPERTYMAP, i));
        		if (pPropertyMap->parent == pTypeDef->tableIndex) {
        			firstIdx = MetaData.TABLE_OFS(pPropertyMap->propertyList);
        			if (i < numPropertymapRows) {
        				tMD_PropertyMap *pNextPropertyMap = (tMD_PropertyMap*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_PROPERTYMAP, i + 1));
        				lastIdxExc = MetaData.TABLE_OFS(pNextPropertyMap->propertyList);
        			} else {
        				lastIdxExc = numPropertyRows + 1;
        			}
        			break;
        		}
        	}

        	// Instantiate a PropertyInfo[]
        	uint numProperties = lastIdxExc - firstIdx;
        	tMD_TypeDef *pArrayType = Type.GetArrayTypeDef(Type.types[Type.TYPE_SYSTEM_REFLECTION_PROPERTYINFO], null, null);
        	/*HEAP_PTR*/byte* ret = System_Array.NewVector(pArrayType, numProperties);
        	// Allocate to return value straight away, so it cannot be GCed
        	*(/*HEAP_PTR*/byte**)pReturnValue = ret;

        	// Now fill the PropertyInfo[]
        	for (i=0; i<numProperties; i++) {
        		tMD_Property *pPropertyMetadata = (tMD_Property*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_PROPERTY, firstIdx + i));

        		// Instantiate PropertyInfo and put it in the array
        		tPropertyInfo *pPropertyInfo = (tPropertyInfo*)Heap.AllocType(Type.types[Type.TYPE_SYSTEM_REFLECTION_PROPERTYINFO]);
        		System_Array.StoreElement(ret, i, (byte*)&pPropertyInfo);

        		// Assign ownerType
        		pPropertyInfo->ownerType = pThis_;

        		// Assign name
        		pPropertyInfo->name = System_String.FromCharPtrASCII(pPropertyMetadata->name);

        		// Assign propertyType
        		uint sigLength;
        		byte* typeSig = MetaData.GetBlob(pPropertyMetadata->typeSig, &sigLength);
        		MetaData.DecodeSigEntry(&typeSig); // Ignored: prolog
        		MetaData.DecodeSigEntry(&typeSig); // Ignored: number of 'getter' parameters		
        		tMD_TypeDef *propertyTypeDef = Type.GetTypeFromSig(pMetaData, &typeSig, null, null);
        		MetaData.Fill_TypeDef(propertyTypeDef, null, null);
        		pPropertyInfo->propertyType = Type.GetTypeObject(propertyTypeDef);
        	}

        	return null;
        }

        public static tAsyncCall* GetMethod(byte* pThis_, byte* pParams, byte* pReturnValue)
        {
        	// Read param
            byte* methodName = stackalloc byte[256];
            DotNetStringToCString(methodName, (uint)256, ((/*HEAP_PTR*/byte**)pParams)[0]);

        	// Get metadata for the 'this' type
        	tRuntimeType *pRuntimeType = (tRuntimeType*)pThis_;
        	tMD_TypeDef *pTypeDef = pRuntimeType->pTypeDef;

        	// Search for the method by name
        	for (int i=0; i<pTypeDef->numMethods; i++) {
        		if (S.strcmp(pTypeDef->ppMethods[i]->name, methodName) == 0) {
        			tMD_MethodDef *pMethodDef = pTypeDef->ppMethods[i];

        			// Instantiate a MethodInfo
        			tMethodInfo *pMethodInfo = (tMethodInfo*)Heap.AllocType(Type.types[Type.TYPE_SYSTEM_REFLECTION_METHODINFO]);

        			// Assign ownerType
        			pMethodInfo->methodBase.ownerType = pThis_;

        			// Assign name
        			pMethodInfo->methodBase.name = System_String.FromCharPtrASCII(pMethodDef->name);

        			// Assign method def
        			pMethodInfo->methodBase.methodDef = pMethodDef;

        			*(/*HEAP_PTR*/byte**)pReturnValue = (/*HEAP_PTR*/byte*)pMethodInfo;
        			return null;
        		}
        	}

        	// Not found
        	*(/*HEAP_PTR*/byte**)pReturnValue = null;
        	return null;
        }

    }
}