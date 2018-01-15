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

#if NO

typedef struct tLoadedLib_ tLoadedLib;
struct tLoadedLib_ {
	// The name of the library - this is the name as specified in the .NET assembly
	/*STRING*/byte* name;
	// The library
	void *pLib;

	tLoadedLib *pNext;
};

static tLoadedLib *pLoadedLibs = null;

static tLoadedLib* GetLib(/*STRING*/byte* name) {
	// See if it's already loaded
	tLoadedLib *pLib = pLoadedLibs;
	char libName[256];
	void *pNativeLib;

	while (pLib != null) {
		if (S.strcmp(name, pLib->name) == 0) {
			return pLib;
		}
	}
	S.sprintf(libName, "%s%s", LIB_PREFIX, name);
	if (S.strlen(libName) >= 4) {
		if (S.strcmp(".dll", libName + S.strlen(libName) - 4) == 0) {
			// Cut off the ".dll" suffix if it's there
			libName[S.strlen(libName) - 4] = 0;
		}
	}
	// Not loaded, so load it
	S.sprintf(S.strchr(libName, 0), ".%s", LIB_SUFFIX);
#if WIN32
	pNativeLib = LoadLibraryA(libName);
#else
	pNativeLib = dlopen(libName, RTLD_LAZY); //DL_LAZY);
#endif
	if (pNativeLib == null) {
		// Failed to load library
		printf("Failed to load library: %s\n", libName);
#if !WIN32
		{
			/*char**/byte *pError;
			pError = dlerror();
			if (pError) {
				printf("dlopen() Error: '%s'",pError);
			}
		}
#endif
		return null;
	}
	pLib = ((tLoadedLib*)Mem.mallocForever(sizeof(tLoadedLib)));
	pLib->pNext = pLoadedLibs;
	pLoadedLibs = pLib;
	pLib->name = name;
	pLib->pLib = pNativeLib;
	return pLib;
}

#if TARGET_EMCRIPTEN
extern /*char*/byte* invokeJsFunc(/*STRING*/byte* libName, /*STRING*/byte* funcName, /*STRING*/byte* arg0);
#else
/*char*/byte* invokeJsFunc(/*STRING*/byte* libName, /*STRING*/byte* funcName, /*STRING*/byte* arg0)
{
    return 0;
}
#endif

fnPInvoke PInvoke_GetFunction(tMetaData *pMetaData, tMD_ImplMap *pImplMap) {
	tLoadedLib *pLib;
	/*STRING*/byte* libName;
	void *pProc;

	libName = MetaData.GetModuleRefName(pMetaData, pImplMap->importScope);

	return (fnPInvoke)invokeJsFunc;

	/*
	pLib = GetLib(libName);
	if (pLib == null) {
		// Library not found, so we can't find the function
		return null;
	}

#if WIN32
	pProc = GetProcAddress(pLib->pLib, pImplMap->importName);
#else
	pProc = dlsym(pLib->pLib, pImplMap->importName);
#endif
	return pProc;
	*/
}

static void* ConvertStringToANSI(/*HEAP_PTR*/byte* pHeapEntry) {
	uint strLen, i;
	/*STRING2*/ushort* str = SystemString.GetString(pHeapEntry, &strLen);
	byte *pAnsi = (byte*)Mem.malloc(strLen+1);
	for (i=0; i<strLen; i++) {
		pAnsi[i] = (byte)str[i];
	}
	pAnsi[i] = 0;
	return pAnsi;
}

// This function is needed to maintain string immutability, and to add a null-terminator
static void* ConvertStringToUnicode(/*HEAP_PTR*/byte* pHeapEntry) {
	uint strLen;
	/*STRING2*/ushort* str = SystemString.GetString(pHeapEntry, &strLen);
	ushort *pUnicode = (ushort*)Mem.malloc((strLen+1) << 1);
	Mem.memcpy(pUnicode, str, strLen << 1);
	pUnicode[strLen] = 0;
	return pUnicode;
}

typedef ulong    (STDCALL *_uCuuuuu)(uint _0, uint _1, uint _2, uint _3, uint _4);
typedef ulong    (STDCALL *_uCuuuuuu)(uint _0, uint _1, uint _2, uint _3, uint _4, uint _5);
typedef ulong    (STDCALL *_uCuuuuuuu)(uint _0, uint _1, uint _2, uint _3, uint _4, uint _5, uint _6);
typedef ulong    (STDCALL *_uCuuuuuuuu)(uint _0, uint _1, uint _2, uint _3, uint _4, uint _5, uint _6, uint _7);
typedef ulong    (STDCALL *_uCuuuuuuuuu)(uint _0, uint _1, uint _2, uint _3, uint _4, uint _5, uint _6, uint _7, uint _8);
typedef ulong    (STDCALL *_uCuuuuuuuuuu)(uint _0, uint _1, uint _2, uint _3, uint _4, uint _5, uint _6, uint _7, uint _8, uint _9);

const int CALL0(returnType) (returnType)
const int CALL1(returnType, t0) ((returnType) | ((t0)<<2))
const int CALL2(returnType, t0, t1) ((returnType) | ((t0)<<2) | ((t1)<<4))
const int CALL3(returnType, t0, t1, t2) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6))
const int CALL4(returnType, t0, t1, t2, t3) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8))
const int CALL5(returnType, t0, t1, t2, t3, t4) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8) | ((t4)<<10))
const int CALL6(returnType, t0, t1, t2, t3, t4, t5) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8) | ((t4)<<10) | ((t5)<<12))
const int CALL7(returnType, t0, t1, t2, t3, t4, t5, t6) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8) | ((t4)<<10) | ((t5)<<12) | ((t6)<<14))
const int CALL8(returnType, t0, t1, t2, t3, t4, t5, t6, t7) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8) | ((t4)<<10) | ((t5)<<12) | ((t6)<<14) | ((t7)<<16))
const int CALL9(returnType, t0, t1, t2, t3, t4, t5, t6, t7, t8) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8) | ((t4)<<10) | ((t5)<<12) | ((t6)<<14) | ((t7)<<16) | ((t8)<<18))
const int CALL10(returnType, t0, t1, t2, t3, t4, t5, t6, t7, t8, t9) ((returnType) | ((t0)<<2) | ((t1)<<4) | ((t2)<<6) | ((t3)<<8) | ((t4)<<10) | ((t5)<<12) | ((t6)<<14) | ((t7)<<16) | ((t8)<<18) | ((t9)<<20))

const int NOTHING 0
const int SINGLE 1
const int DOUBLE 2
const int DEFAULT 3

const int SET_ARG_TYPE(paramNum, type) funcParams |= (type << ((paramNum+1) << 1))

const int MAX_ARGS 16
uint PInvoke_Call(tJITCallPInvoke *pCall, byte* pParams, byte* pReturnValue, tThread *pCallingThread) {
	uint _args[MAX_ARGS];
	double _argsd[MAX_ARGS];
	void* _pTempMem[MAX_ARGS];
	uint numParams, param, paramTypeNum;
	tMD_MethodDef *pMethod = pCall->pMethod;
	tMD_TypeDef *pReturnType = pMethod->pReturnType;
	tMD_ImplMap *pImplMap = pCall->pImplMap;
	fnPInvoke pFn = pCall->fn;
	uint _argOfs = 0, _argdOfs = 0, paramOfs = 0;
	uint _tempMemOfs = 0;
	uint i;
	uint funcParams = DEFAULT;
	ulong u64Ret;
	float fRet;
	double dRet;

	// [Steve edit] Before we issue the call into JS code, we need to set the calling .NET thread's state
	// to 'suspended' so that, if the JS code makes other calls into .NET, the DNA runtime doesn't try to
	// resume this original thread automaticaly (its default behaviour in Thread.c is that, at the end of
	// each thread's execution, it tries to resume any other nonbackground thread). If we don't do this,
	// then you wouldn't be able to call back into .NET code while inside a pInvoke call because the calling
	// thread would go into an infinite loop.
	uint originalCallingThreadState = pCallingThread->state;
	pCallingThread->state |= THREADSTATE_SUSPENDED;

	if (pReturnType != null) {
		if (pReturnType == Type.types[Type.TYPE_SYSTEM_SINGLE]) {
			funcParams = SINGLE;
		} else if (pReturnType == Type.types[Type.TYPE_SYSTEM_DOUBLE]) {
			funcParams = DOUBLE;
		}
	}

	// Prepend the 'libName' and 'funcName' strings to the set of arguments
	// NOTE: These aren't currently used in js-interop.js, but they would be if I found a way
	// to pass an arbitrary set of args without declaring the C func type in advance
	_args[0] = MetaData.GetModuleRefName(pCall->pMethod->pMetaData, pCall->pImplMap->importScope);
	_args[1] = pCall->pMethod->name;
	_argOfs += 2;
	SET_ARG_TYPE(0, DEFAULT);
	SET_ARG_TYPE(1, DEFAULT);

	numParams = pMethod->numberOfParameters;
	for (param = 0, paramTypeNum = 0; param<numParams; param++, paramTypeNum++) {
		tParameter *pParam = &(pMethod->pParams[param]);
		tMD_TypeDef *pParamType = pParam->pTypeDef;
		uint paramType = DEFAULT;

		if (pParamType->stackType == EvalStack.EVALSTACK_INT32) {
			_args[_argOfs] = *(uint*)(pParams + paramOfs);
			_argOfs++;
			paramOfs += 4;
		} else if (pParamType == Type.types[Type.TYPE_SYSTEM_STRING]) {
			// Allocate a temp bit of memory for the string that's been converted.
			void *pString;
			if (MetaData.IMPLMAP_ISCHARSET_ANSI(pImplMap) || MetaData.IMPLMAP_ISCHARSET_AUTO(pImplMap) || MetaData.IMPLMAP_ISCHARSET_NOTSPEC(pImplMap)) {
				pString = ConvertStringToANSI(*(/*HEAP_PTR*/byte**)(pParams + paramOfs));
			} else if (MetaData.IMPLMAP_ISCHARSET_UNICODE(pImplMap)) {
				pString = ConvertStringToUnicode(*(/*HEAP_PTR*/byte**)(pParams + paramOfs));
			} else {
				Sys.Crash("PInvoke_Call() Cannot handle string marshalling of given type");
			}
			_pTempMem[_tempMemOfs] = pString;
			_tempMemOfs++;
			_args[_argOfs] = (uint)pString;
			_argOfs++;
			paramOfs += 4;
		} else if (pParamType == Type.types[Type.TYPE_SYSTEM_INTPTR]) {
			// Only works for 32-bit
			_args[_argOfs] = *(uint*)(pParams + paramOfs);
			_argOfs++;
			paramOfs += 4;
		} else if (pParamType == Type.types[Type.TYPE_SYSTEM_SINGLE]) {
			_argsd[_argdOfs] = *(float*)(pParams + paramOfs);
			_argdOfs++;
			paramOfs += 4;
			paramType = SINGLE;
		} else if (pParamType == Type.types[Type.TYPE_SYSTEM_DOUBLE]) {
			_argsd[_argdOfs] = *(double*)(pParams + paramOfs);
			_argdOfs++;
			paramOfs += 8;
			paramType = DOUBLE;
		} else {
			Sys.Crash("PInvoke_Call() Cannot handle parameter of type: %s", pParamType->name);
		}
		SET_ARG_TYPE(paramTypeNum + 2, paramType);
	}

	// [Steve edit] I'm hard-coding the pinvoke function pointer type here, as a workaround for
	// Emscripten's function pointer limitations.
	// See the longer comment in JIT.h for details.
	if (funcParams != 255) {
		Sys.Crash("PInvoke_Call() currently only supports calls of type 255; you tried to make a call of type %i.\n", funcParams);
	}
	int intRet = pFn(_args[0], _args[1], _args[2]);
	u64Ret = (ulong)intRet;

	/*
	switch (funcParams) {
    

	case CALL5(DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT):
		u64Ret = ((_uCuuuuu)(pFn))(_args[0], _args[1], _args[2], _args[3], _args[4]);
		break;

	case CALL6(DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT):
		u64Ret = ((_uCuuuuuu)(pFn))(_args[0], _args[1], _args[2], _args[3], _args[4], _args[5]);
		break;

	case CALL7(DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT):
		u64Ret = ((_uCuuuuuuu)(pFn))(_args[0], _args[1], _args[2], _args[3], _args[4], _args[5], _args[6]);
		break;

	case CALL8(DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT):
		u64Ret = ((_uCuuuuuuuu)(pFn))(_args[0], _args[1], _args[2], _args[3], _args[4], _args[5], _args[6], _args[7]);
		break;

	case CALL9(DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT):
		u64Ret = ((_uCuuuuuuuuu)(pFn))(_args[0], _args[1], _args[2], _args[3], _args[4], _args[5], _args[6], _args[7], _args[8]);
		break;

	case CALL10(DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT):
		u64Ret = ((_uCuuuuuuuuuu)(pFn))(_args[0], _args[1], _args[2], _args[3], _args[4], _args[5], _args[6], _args[7], _args[8], _args[9]);
		break;

	default:
		Sys.Crash("PInvoke_Call() Cannot handle the function parameters: 0x%08x", funcParams);
	}
	*/

	for (i=0; i<_tempMemOfs; i++) {
		Mem.free(_pTempMem[i]);
	}

	// [Steve edit] Restore previous thread state
	pCallingThread->state = originalCallingThreadState;

	if (pReturnType == null) {
		return 0;
	}
	if (pReturnType->stackType == EvalStack.EVALSTACK_INT32) {
		*(uint*)pReturnValue = (uint)u64Ret;
		return 4;
	}
	if (pReturnType == Type.types[Type.TYPE_SYSTEM_STRING]) {
		if (MetaData.IMPLMAP_ISCHARSET_ANSI(pImplMap) || MetaData.IMPLMAP_ISCHARSET_AUTO(pImplMap) || MetaData.IMPLMAP_ISCHARSET_NOTSPEC(pImplMap)) {
			*(/*HEAP_PTR*/byte**)pReturnValue = SystemString.FromCharPtrASCII((byte*)(uint)u64Ret);
		} else if (MetaData.IMPLMAP_ISCHARSET_UNICODE(pImplMap)) {
			*(/*HEAP_PTR*/byte**)pReturnValue = SystemString.FromCharPtrUTF16((ushort*)(uint)u64Ret);
		} else {
			Sys.Crash("PInvoke_Call() Cannot handle return string in specified format");
		}
		return sizeof(void*);
	}
	if (pReturnType == Type.types[Type.TYPE_SYSTEM_INTPTR]) {
		*(void**)pReturnValue = (void*)(uint)u64Ret;
		return sizeof(void*);
	}
	if (pReturnType == Type.types[Type.TYPE_SYSTEM_SINGLE]) {
		*(double*)pReturnValue = (double)fRet;
		return 8;
	}
	if (pReturnType == Type.types[Type.TYPE_SYSTEM_DOUBLE]) {
		*(double*)pReturnValue = dRet;
		return 8;
	}

	Sys.Crash("PInvoke_Call() Cannot handle return type: %s", pReturnType->name);
	FAKE_RETURN;
}

#endif

