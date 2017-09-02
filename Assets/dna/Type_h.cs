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

const int ELEMENT_TYPE_VOID		0x01
const int ELEMENT_TYPE_BOOLEAN	0x02
const int ELEMENT_TYPE_CHAR		0x03
const int ELEMENT_TYPE_I1			0x04
const int ELEMENT_TYPE_U1			0x05
const int ELEMENT_TYPE_I2			0x06
const int ELEMENT_TYPE_U2			0x07
const int ELEMENT_TYPE_I4			0x08
const int ELEMENT_TYPE_U4			0x09
const int ELEMENT_TYPE_I8			0x0a
const int ELEMENT_TYPE_U8			0x0b
const int ELEMENT_TYPE_R4			0x0c
const int ELEMENT_TYPE_R8			0x0d
const int ELEMENT_TYPE_STRING		0x0e
const int ELEMENT_TYPE_PTR		0x0f
const int ELEMENT_TYPE_BYREF		0x10
const int ELEMENT_TYPE_VALUETYPE	0x11
const int ELEMENT_TYPE_CLASS		0x12
const int ELEMENT_TYPE_VAR		0x13 // Generic argument type

const int ELEMENT_TYPE_GENERICINST 0x15

const int ELEMENT_TYPE_INTPTR		0x18
const int ELEMENT_TYPE_UINTPTR	0x19

const int ELEMENT_TYPE_OBJECT		0x1c
const int ELEMENT_TYPE_SZARRAY	0x1d
const int ELEMENT_TYPE_MVAR		0x1e


extern tMD_TypeDef **types;
const int TYPE_SYSTEM_OBJECT 0
const int TYPE_SYSTEM_ARRAY_NO_TYPE 1
const int TYPE_SYSTEM_VOID 2
const int TYPE_SYSTEM_BOOLEAN 3
const int TYPE_SYSTEM_BYTE 4
const int TYPE_SYSTEM_SBYTE 5
const int TYPE_SYSTEM_CHAR 6
const int TYPE_SYSTEM_INT16 7
const int TYPE_SYSTEM_INT32 8
const int TYPE_SYSTEM_STRING 9
const int TYPE_SYSTEM_INTPTR 10
const int TYPE_SYSTEM_RUNTIMEFIELDHANDLE 11
const int TYPE_SYSTEM_INVALIDCASTEXCEPTION 12
const int TYPE_SYSTEM_UINT32 13
const int TYPE_SYSTEM_UINT16 14
const int TYPE_SYSTEM_ARRAY_CHAR 15
const int TYPE_SYSTEM_ARRAY_OBJECT 16
const int TYPE_SYSTEM_COLLECTIONS_GENERIC_IENUMERABLE_T 17
const int TYPE_SYSTEM_COLLECTIONS_GENERIC_ICOLLECTION_T 18
const int TYPE_SYSTEM_COLLECTIONS_GENERIC_ILIST_T 19
const int TYPE_SYSTEM_MULTICASTDELEGATE 20
const int TYPE_SYSTEM_NULLREFERENCEEXCEPTION 21
const int TYPE_SYSTEM_SINGLE 22
const int TYPE_SYSTEM_DOUBLE 23
const int TYPE_SYSTEM_INT64 24
const int TYPE_SYSTEM_UINT64 25
const int TYPE_SYSTEM_RUNTIMETYPE 26
const int TYPE_SYSTEM_TYPE 27
const int TYPE_SYSTEM_RUNTIMETYPEHANDLE 28
const int TYPE_SYSTEM_RUNTIMEMETHODHANDLE 29
const int TYPE_SYSTEM_ENUM 30
const int TYPE_SYSTEM_ARRAY_STRING 31
const int TYPE_SYSTEM_ARRAY_INT32 32
const int TYPE_SYSTEM_THREADING_THREAD 33
const int TYPE_SYSTEM_THREADING_THREADSTART 34
const int TYPE_SYSTEM_THREADING_PARAMETERIZEDTHREADSTART 35
const int TYPE_SYSTEM_WEAKREFERENCE 36
const int TYPE_SYSTEM_IO_FILEMODE 37
const int TYPE_SYSTEM_IO_FILEACCESS 38
const int TYPE_SYSTEM_IO_FILESHARE 39
const int TYPE_SYSTEM_ARRAY_BYTE 40
const int TYPE_SYSTEM_GLOBALIZATION_UNICODECATEGORY 41
const int TYPE_SYSTEM_OVERFLOWEXCEPTION 42
const int TYPE_SYSTEM_PLATFORMID 43
const int TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES 44
const int TYPE_SYSTEM_UINTPTR 45
const int TYPE_SYSTEM_NULLABLE 46
const int TYPE_SYSTEM_ARRAY_TYPE 47
const int TYPE_SYSTEM_REFLECTION_PROPERTYINFO 48
const int TYPE_SYSTEM_REFLECTION_METHODINFO 49
const int TYPE_SYSTEM_REFLECTION_METHODBASE 50

//U32 Type_IsMethod(tMD_MethodDef *pMethod, STRING name, tMD_TypeDef *pReturnType, U32 numParams, ...);
U32 Type_IsMethod(tMD_MethodDef *pMethod, STRING name, tMD_TypeDef *pReturnType, U32 numParams, U8 *pParamTypeIndexs);

void Type_Init();

U32 Type_IsValueType(tMD_TypeDef *pTypeDef);

tMD_TypeDef* Type_GetTypeFromSig(tMetaData *pMetaData, SIG *pSig, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs);

// Is TestType derived from BaseType or the same as BaseType?
U32 Type_IsDerivedFromOrSame(tMD_TypeDef *pBaseType, tMD_TypeDef *pTestType);
// Does TestType implement pInterface?
U32 Type_IsImplemented(tMD_TypeDef *pInterface, tMD_TypeDef *pTestType);
// Can a variable of FromType be assigend to ToType?
U32 Type_IsAssignableFrom(tMD_TypeDef *pToType, tMD_TypeDef *pFromType);

tMD_TypeDef* Type_GetArrayTypeDef(tMD_TypeDef *pElementType, tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs);

HEAP_PTR Type_GetTypeObject(tMD_TypeDef *pTypeDef);

#endif