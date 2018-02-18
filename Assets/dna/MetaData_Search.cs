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

    public unsafe static partial class MetaData
    {

        public static uint CompareNameAndSig(/*STRING*/byte* name, /*BLOB_*/byte* sigBlob, tMetaData *pSigMetaData, 
            tMD_TypeDef **ppSigClassTypeArgs, tMD_TypeDef **ppSigMethodTypeArgs, tMD_MethodDef *pMethod, tMD_TypeDef **ppMethodClassTypeArgs, 
            tMD_TypeDef **ppMethodMethodTypeArgs) 
        {
        	if (S.strcmp(name, pMethod->name) == 0) {
        		/*SIG*/byte* sig, thisSig;
        		uint e, thisE, paramCount, i;

        		sig = MetaData.GetBlob(sigBlob, null);
        		thisSig = MetaData.GetBlob(pMethod->signature, null);

        		e = MetaData.DecodeSigEntry(&sig);
        		thisE = MetaData.DecodeSigEntry(&thisSig);
        		// Check method call type (static, etc...)
        		if (e != thisE) {
        			return 0;
        		}

        		// If method has generic arguments, check the generic type argument count
                if ((e & SIG_METHODDEF_GENERIC) != 0) {
        			e = MetaData.DecodeSigEntry(&sig);
        			thisE = MetaData.DecodeSigEntry(&thisSig);
        			// Generic argument count
        			if (e != thisE) {
        				return 0;
        			}
        		}

        		e = MetaData.DecodeSigEntry(&sig);
        		thisE = MetaData.DecodeSigEntry(&thisSig);
        		// check parameter count
        		if (e != thisE) {
        			return 0;
        		}
        		paramCount = e + 1; // +1 to include the return type

        		// check all parameters
        		for (i=0; i<paramCount; i++) {
                    tMD_TypeDef *pParamType;
                    tMD_TypeDef *pThisParamType;

        			pParamType = Type.GetTypeFromSig(pSigMetaData, &sig, ppSigClassTypeArgs, ppSigMethodTypeArgs);
        			pThisParamType = Type.GetTypeFromSig(pMethod->pMetaData, &thisSig, ppMethodClassTypeArgs, ppMethodMethodTypeArgs);
        			if (pParamType != pThisParamType) {
        				return 0;
        			}
        		}
        		// All parameters the same, so found the right method
        		return 1;
        	}
        	return 0;
        }

        const int MSG_BUF_SIZE = 2048;

        static tMD_MethodDef* FindMethodInType(tMD_TypeDef *pTypeDef, /*STRING*/byte* name, tMetaData *pSigMetaData, 
            /*BLOB_*/byte* sigBlob, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	uint i;
        	tMD_TypeDef *pLookInType = pTypeDef;

        	do {
        		for (i=0; i<pLookInType->numMethods; i++) {
        			if (MetaData.CompareNameAndSig(name, sigBlob, pSigMetaData, ppClassTypeArgs, ppMethodTypeArgs, 
                        pLookInType->ppMethods[i], pLookInType->ppClassTypeArgs, null) != 0) {
        				return pLookInType->ppMethods[i];
        			}
        		}
        		pLookInType = pLookInType->pParent;
        	} while (pLookInType != null);

        	{
        		// Error reporting!!
        		uint entry, numParams, j;
        		/*SIG*/byte* sig;
                /*char**/byte* pMsg, pMsgPos, pMsgEnd;
        		tMD_TypeDef* pParamTypeDef;

                pMsgPos = pMsg = (byte*)Mem.malloc(MSG_BUF_SIZE);
                pMsgEnd = pMsg + MSG_BUF_SIZE;
        		*pMsg = 0;
        		sig = MetaData.GetBlob(sigBlob, &j);
        		entry = MetaData.DecodeSigEntry(&sig);
        		if ((entry & SIG_METHODDEF_HASTHIS) == 0) {
                    pMsgPos = S.scatprintf(pMsgPos, pMsgEnd, "static ");
        		}
                if ((entry & SIG_METHODDEF_GENERIC) != 0) {
        			// read number of generic type args - don't care what it is
        			MetaData.DecodeSigEntry(&sig);
        		}
        		numParams = MetaData.DecodeSigEntry(&sig);
        		pParamTypeDef = Type.GetTypeFromSig(pSigMetaData, &sig, ppClassTypeArgs, ppMethodTypeArgs); // return type
        		if (pParamTypeDef != null) {
                    pMsgPos = S.scatprintf(pMsgPos, pMsgEnd, "%s ", (PTR)pParamTypeDef->name);
        		}
                pMsgPos = S.scatprintf(pMsgPos, pMsgEnd, "%s.%s.%s(", (PTR)pTypeDef->nameSpace, (PTR)pTypeDef->name, (PTR)name);
        		for (j=0; j<numParams; j++) {
        			pParamTypeDef = Type.GetTypeFromSig(pSigMetaData, &sig, ppClassTypeArgs, ppMethodTypeArgs);
        			if (j > 0) {
                        pMsgPos = S.scatprintf(pMsgPos, pMsgEnd, ",");
        			}
        			if (pParamTypeDef != null) {
                        pMsgPos = S.scatprintf(pMsgPos, pMsgEnd, "%s", (PTR)pParamTypeDef->name);
        			} else {
                        pMsgPos = S.scatprintf(pMsgPos, pMsgEnd, "???");
        			}
        		}
                Sys.Crash("FindMethodInType(): Cannot find method %s)", (PTR)pMsg);
        	}
        	return null;
        }

        static tMD_FieldDef* FindFieldInType(tMD_TypeDef *pTypeDef, /*STRING*/byte* name) 
        {
        	uint i;

        	MetaData.Fill_TypeDef(pTypeDef, null, null);

        	for (i=0; i<pTypeDef->numFields; i++) {
        		if (S.strcmp(pTypeDef->ppFields[i]->name, name) == 0) {
        			return pTypeDef->ppFields[i];
        		}
        	}

            Sys.Crash("FindFieldInType(): Cannot find field '%s' in type %s.%s", (PTR)name, (PTR)pTypeDef->nameSpace, (PTR)pTypeDef->name);
        	return null;
        }

        public static tMetaData* GetResolutionScopeMetaData(tMetaData *pMetaData, /*IDX_TABLE*/uint resolutionScopeToken, 
            tMD_TypeDef **ppInNestedType) 
        {
        	switch (MetaData.TABLE_ID(resolutionScopeToken)) {
        		case MetaDataTable.MD_TABLE_ASSEMBLYREF:
        			{
        				tMD_AssemblyRef *pAssemblyRef;

        				pAssemblyRef = (tMD_AssemblyRef*)MetaData.GetTableRow(pMetaData, resolutionScopeToken);
        				*ppInNestedType = null;
        				return CLIFile.GetMetaDataForAssembly(pAssemblyRef->name);
        			}
        		case MetaDataTable.MD_TABLE_TYPEREF:
        			{
        				tMD_TypeDef *pTypeDef;

        				pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, resolutionScopeToken, null, null);
        				*ppInNestedType = pTypeDef;
        				return pTypeDef->pMetaData;
        			}
        		default:
        			Sys.Crash("MetaData.GetResolutionScopeMetaData(): Cannot resolve token: 0x%08x", resolutionScopeToken);
        			return null;
        	}
        }

        public static tMD_TypeDef* GetTypeDefFromName(tMetaData *pMetaData, /*STRING*/byte* nameSpace, /*STRING*/byte* name, 
            tMD_TypeDef *pInNestedClass, byte assertExists) 
        {
        	uint i;

        	for (i=1; i<=pMetaData->tables.numRows[MetaDataTable.MD_TABLE_TYPEDEF]; i++) {
        		tMD_TypeDef *pTypeDef;

        		pTypeDef = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_TYPEDEF, i));
        		if (pInNestedClass == pTypeDef->pNestedIn &&
        			S.strcmp(name, pTypeDef->name) == 0 &&
        			(pInNestedClass != null || S.strcmp(nameSpace, pTypeDef->nameSpace) == 0)) {
        			return pTypeDef;
        		}
        	}

        	if (assertExists != 0) {
                Sys.Crash("MetaData.GetTypeDefFromName(): Cannot find type %s.%s", (PTR)nameSpace, (PTR)name);
        		return null;
        	} else {
        		return null;
        	}
        }

        public static tMD_TypeDef* GetTypeDefFromFullName(/*STRING*/byte* assemblyName, /*STRING*/byte* nameSpace, /*STRING*/byte* name) 
        {
        	tMetaData *pTypeMetaData;

        	pTypeMetaData = CLIFile.GetMetaDataForAssembly(assemblyName);

        	// Note that this cannot get a nested class, as this final parameter is always null
        	return MetaData.GetTypeDefFromName(pTypeMetaData, nameSpace, name, null, /* assertExists */ 1);
        }

        public static tMD_TypeDef* GetTypeDefFromDefRefOrSpec(tMetaData *pMetaData, /*IDX_TABLE*/uint token, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	void *pTableEntry;

        	pTableEntry = MetaData.GetTableRow(pMetaData, token);
        	if (pTableEntry == null) {
        		return null;
        	}
        	if (((tMDC_ToTypeDef*)pTableEntry)->pTypeDef != null) {
        		return ((tMDC_ToTypeDef*)pTableEntry)->pTypeDef;
        	}

        	switch (MetaData.TABLE_ID(token)) {
        		case MetaDataTable.MD_TABLE_TYPEDEF:
        			((tMDC_ToTypeDef*)pTableEntry)->pTypeDef = (tMD_TypeDef*)pTableEntry;
        			return (tMD_TypeDef*)pTableEntry;
        		case MetaDataTable.MD_TABLE_TYPEREF:
        			{
        				tMetaData *pTypeDefMetaData;
        				tMD_TypeRef *pTypeRef;
        				tMD_TypeDef *pTypeDef;
        				tMD_TypeDef *pInNestedClass;

        				pTypeRef = (tMD_TypeRef*)pTableEntry;
        				pTypeDefMetaData = MetaData.GetResolutionScopeMetaData(pMetaData, pTypeRef->resolutionScope, &pInNestedClass);
        				pTypeDef = MetaData.GetTypeDefFromName(pTypeDefMetaData, pTypeRef->nameSpace, pTypeRef->name, pInNestedClass, /* assertExists */ 1);
        				pTypeRef->pTypeDef = pTypeDef;
        				return pTypeDef;
        			}
        		case MetaDataTable.MD_TABLE_TYPESPEC:
        			{
        				tMD_TypeSpec *pTypeSpec;
        				tMD_TypeDef *pTypeDef;
        				/*SIG*/byte* sig;

        				pTypeSpec = (tMD_TypeSpec*)pTableEntry;
        				sig = MetaData.GetBlob(pTypeSpec->signature, null);
        				pTypeDef = Type.GetTypeFromSig(pTypeSpec->pMetaData, &sig, ppClassTypeArgs, ppMethodTypeArgs);
        				// Note: Cannot cache the TypeDef for this TypeSpec because it
        				// can change depending on class arguemnts given.

        				return pTypeDef;
        			}
        		default:
        			Sys.Crash("MetaData.GetTypeDefFromDefRefOrSpec(): Cannot handle token: 0x%08x", token);
        			return null;
        	}
        }

        public static tMD_TypeDef* GetTypeDefFromMethodDef(tMD_MethodDef *pMethodDef) 
        {
        	tMetaData *pMetaData;
        	uint i;

        	pMetaData = pMethodDef->pMetaData;
        	for (i=pMetaData->tables.numRows[MetaDataTable.MD_TABLE_TYPEDEF]; i>0; i--) {
        		tMD_TypeDef *pTypeDef;

        		pTypeDef = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_TYPEDEF, i));
        		if (pTypeDef->methodList <= pMethodDef->tableIndex) {
        			return pTypeDef;
        		}
        	}

            Sys.Crash("MetaData.GetTypeDefFromMethodDef(): Cannot find type for method: %s", (PTR)pMethodDef->name);
        	return null;
        }

        public static tMD_TypeDef* GetTypeDefFromFieldDef(tMD_FieldDef *pFieldDef) 
        {
        	tMetaData *pMetaData;
        	uint i;

        	pMetaData = pFieldDef->pMetaData;
        	for (i=pMetaData->tables.numRows[MetaDataTable.MD_TABLE_TYPEDEF]; i>0; i--) {
        		tMD_TypeDef *pTypeDef;

        		pTypeDef = (tMD_TypeDef*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_TYPEDEF, i));
        		if (pTypeDef->fieldList <= pFieldDef->tableIndex) {
        			return pTypeDef;
        		}
        	}

            Sys.Crash("MetaData.GetTypeDefFromFieldDef(): Cannot find type for field: %s", (PTR)pFieldDef->name);
        	return null;
        }

        public static tMD_MethodDef* GetMethodDefFromDefRefOrSpec(tMetaData *pMetaData, /*IDX_TABLE*/uint token, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	void *pTableEntry;

        	pTableEntry = MetaData.GetTableRow(pMetaData, token);
        	if (((tMDC_ToMethodDef*)pTableEntry)->pMethodDef != null) {
        		return ((tMDC_ToMethodDef*)pTableEntry)->pMethodDef;
        	}

        	switch (MetaData.TABLE_ID(token)) {
        		case MetaDataTable.MD_TABLE_METHODDEF:
        			((tMDC_ToMethodDef*)pTableEntry)->pMethodDef = (tMD_MethodDef*)pTableEntry;
        			return (tMD_MethodDef*)pTableEntry;
        		case MetaDataTable.MD_TABLE_MEMBERREF:
        			{
        				tMD_MemberRef *pMemberRef;

        				pMemberRef = (tMD_MemberRef*)pTableEntry;
        				switch (MetaData.TABLE_ID(pMemberRef->class_))
        				{
        				case MetaDataTable.MD_TABLE_TYPEREF:
        				case MetaDataTable.MD_TABLE_TYPESPEC:
        					{
        						tMD_TypeDef *pTypeDef;
        						tMD_MethodDef *pMethodDef;

        						pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, pMemberRef->class_, ppClassTypeArgs, ppMethodTypeArgs);
        						MetaData.Fill_TypeDef(pTypeDef, null, null);
        						pMethodDef = FindMethodInType(pTypeDef, pMemberRef->name, pMetaData, pMemberRef->signature, pTypeDef->ppClassTypeArgs, ppMethodTypeArgs);
        						//pMethodDef->pMethodDef = pMethodDef;
        						return pMethodDef;
        					}
        				default:
        					Sys.Crash("MetaData.GetMethodDefFromMethodDefOrRef(): Cannot handle pMemberRef->class_=0x%08x", pMemberRef->class_);
                            return null;
        				}
        			}
        		case MetaDataTable.MD_TABLE_METHODSPEC:
        			{
        				tMD_MethodSpec *pMethodSpec;
        				tMD_MethodDef *pMethodDef;

        				pMethodSpec = (tMD_MethodSpec*)pTableEntry;
        				pMethodDef = Generics.GetMethodDefFromSpec(pMethodSpec, ppClassTypeArgs, ppMethodTypeArgs);

        				// Note: Cannot cache the MethodDef from the MethodSpec, as class generic arguments
        				// may be different.
        				
        				return pMethodDef;
        			}
        	}

        	Sys.Crash("MetaData.GetMethodDefFromMethodDefOrRef(): Cannot handle token: 0x%08x", token);
        	return null;
        }

        public static tMD_FieldDef* GetFieldDefFromDefOrRef(tMetaData *pMetaData, /*IDX_TABLE*/uint token, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	void *pTableEntry;

        	pTableEntry = MetaData.GetTableRow(pMetaData, token);
        	if (((tMDC_ToFieldDef*)pTableEntry)->pFieldDef != null) {
        		return ((tMDC_ToFieldDef*)pTableEntry)->pFieldDef;
        	}

        	switch (MetaData.TABLE_ID(token)) {
        		case MetaDataTable.MD_TABLE_FIELDDEF:
        			((tMDC_ToFieldDef*)pTableEntry)->pFieldDef = (tMD_FieldDef*)pTableEntry;
        			return (tMD_FieldDef*)pTableEntry;
        		case MetaDataTable.MD_TABLE_MEMBERREF:
        			{
        				tMD_MemberRef *pMemberRef;

        				pMemberRef = (tMD_MemberRef*)pTableEntry;
        				switch (MetaData.TABLE_ID(pMemberRef->class_))
        				{
        				case MetaDataTable.MD_TABLE_TYPEREF:
        				case MetaDataTable.MD_TABLE_TYPESPEC:
        					{
        						tMD_TypeDef *pTypeDef;
        						tMD_FieldDef *pFieldDef;

        						pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, pMemberRef->class_, ppClassTypeArgs, ppMethodTypeArgs);
        						pFieldDef = FindFieldInType(pTypeDef, pMemberRef->name);
        						if (MetaData.TABLE_ID(pMemberRef->class_) == MetaDataTable.MD_TABLE_TYPEREF) {
        							// Can't do this for TypeSpec because the resulting TypeDef will change
        							// depending on what the class type arguments are.
        							((tMDC_ToFieldDef*)pTableEntry)->pFieldDef = pFieldDef;
        						}
        						return pFieldDef;
        					}
        				default:
        					Sys.Crash("MetaData.GetMethodDefFromMethodDefOrRef(): Cannot handle pMemberRef->class_=0x%08x", pMemberRef->class_);
                            break;
        				}
                        return null;
        			}
        	}

        	Sys.Crash("MetaData.GetFieldDefFromDefOrRef(): Cannot handle token: 0x%08x", token);
        	return null;
        }

        // Return pointer to the relevant Def structure.
        // pObjectType returns:
        // 0 - tMD_TypeDef
        // 1 - tMD_MethodDef
        // 2 - tMD_FieldDef
        // (These link up with the JitOps.JIT_LOADTOKEN_* opcodes)
        public static byte* GetTypeMethodField(tMetaData *pMetaData, /*IDX_TABLE*/uint token, uint *pObjectType, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
        	switch (MetaData.TABLE_ID(token)) {
        		case MetaDataTable.MD_TABLE_TYPEDEF:
        		case MetaDataTable.MD_TABLE_TYPEREF:
        		case MetaDataTable.MD_TABLE_TYPESPEC:
        			{
        				tMD_TypeDef *pTypeDef;

        				pTypeDef = MetaData.GetTypeDefFromDefRefOrSpec(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);
        				MetaData.Fill_TypeDef(pTypeDef, null, null);
        				*pObjectType = 0;
        				return (byte*)pTypeDef;
        			}
        		case MetaDataTable.MD_TABLE_METHODDEF:
        method:
        			{
        				tMD_MethodDef *pMethodDef;

        				pMethodDef = MetaData.GetMethodDefFromDefRefOrSpec(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);
        				if (pMethodDef->isFilled == 0) {
        					tMD_TypeDef *pTypeDef;

        					pTypeDef = MetaData.GetTypeDefFromMethodDef(pMethodDef);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        				}
        				*pObjectType = 1;
        				return (byte*)pMethodDef;
        			}
        		case MetaDataTable.MD_TABLE_FIELDDEF:
        field:
        			{
        				tMD_FieldDef *pFieldDef;

        				pFieldDef = MetaData.GetFieldDefFromDefOrRef(pMetaData, token, ppClassTypeArgs, ppMethodTypeArgs);
        				if (pFieldDef->pParentType == null) {
        					tMD_TypeDef *pTypeDef;

        					pTypeDef = MetaData.GetTypeDefFromFieldDef(pFieldDef);
        					MetaData.Fill_TypeDef(pTypeDef, null, null);
        				}
        				*pObjectType = 2;
        				return (byte*)pFieldDef;
        			}
        		case MetaDataTable.MD_TABLE_MEMBERREF:
        			{
        				tMD_MemberRef *pMemberRef;
        				/*SIG*/byte* sig;

        				pMemberRef = (tMD_MemberRef*)MetaData.GetTableRow(pMetaData, token);
        				sig = MetaData.GetBlob(pMemberRef->signature, null);
        				if (*(byte*)sig == 0x06) {
        					// Field
        					goto field;
        				} else {
        					// Method
        					goto method;
        				}
        			}
        	}

        	Sys.Crash("MetaData.GetTypeMethodField(): Cannot handle token: 0x%08x", token);
        	return null;
        }

        public static tMD_ImplMap* GetImplMap(tMetaData *pMetaData, /*IDX_TABLE*/uint memberForwardedToken) 
        {
        	uint i;

        	for (i=pMetaData->tables.numRows[MetaDataTable.MD_TABLE_IMPLMAP]; i >= 1; i--) {
        		tMD_ImplMap *pImplMap = (tMD_ImplMap*)MetaData.GetTableRow(pMetaData, MetaData.MAKE_TABLE_INDEX(MetaDataTable.MD_TABLE_IMPLMAP, i));
        		if (pImplMap->memberForwarded == memberForwardedToken) {
        			return pImplMap;
        		}
        	}

        	Sys.Crash("MetaData.GetImplMap() Cannot find mapping for token: 0x%08x", memberForwardedToken);
        	return null;
        }

        public static /*STRING*/byte* GetModuleRefName(tMetaData *pMetaData, /*IDX_TABLE*/uint memberRefToken) 
        {
        	tMD_ModuleRef *pModRef = (tMD_ModuleRef*)MetaData.GetTableRow(pMetaData, memberRefToken);
        	return pModRef->name;
        }

    }
}