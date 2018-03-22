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
using System.Collections.Generic;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL || UNITY_STANDALONE
using UnityEngine;
#endif

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif 

    public unsafe static class Type
    {
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        const int PTR_SIZE = 4;
        #else
        const int PTR_SIZE = 8;
#endif

        public const int TYPE_FILL_NONE                                 = 0;
        public const int TYPE_FILL_PARENTS                              = 1;
        public const int TYPE_FILL_LAYOUT                               = 2;
        public const int TYPE_FILL_VTABLE                               = 3;
        public const int TYPE_FILL_MEMBERS                              = 4;
        public const int TYPE_FILL_INTERFACES                           = 5;
        public const int TYPE_FILL_ALL                                  = 6;

        public const int ELEMENT_TYPE_VOID                              = 0x01;
        public const int ELEMENT_TYPE_BOOLEAN                           = 0x02;
        public const int ELEMENT_TYPE_CHAR                              = 0x03;
        public const int ELEMENT_TYPE_I1                                = 0x04;
        public const int ELEMENT_TYPE_U1                                = 0x05;
        public const int ELEMENT_TYPE_I2                                = 0x06;
        public const int ELEMENT_TYPE_U2                                = 0x07;
        public const int ELEMENT_TYPE_I4                                = 0x08;
        public const int ELEMENT_TYPE_U4                                = 0x09;
        public const int ELEMENT_TYPE_I8                                = 0x0a;
        public const int ELEMENT_TYPE_U8                                = 0x0b;
        public const int ELEMENT_TYPE_R4                                = 0x0c;
        public const int ELEMENT_TYPE_R8                                = 0x0d;
        public const int ELEMENT_TYPE_STRING                            = 0x0e;
        public const int ELEMENT_TYPE_PTR                               = 0x0f;
        public const int ELEMENT_TYPE_BYREF                             = 0x10;
        public const int ELEMENT_TYPE_VALUETYPE                         = 0x11;
        public const int ELEMENT_TYPE_CLASS                             = 0x12;
        public const int ELEMENT_TYPE_VAR                               = 0x13; // Generic argument type

        public const int ELEMENT_TYPE_GENERICINST                       = 0x15;

        public const int ELEMENT_TYPE_INTPTR                            = 0x18;
        public const int ELEMENT_TYPE_UINTPTR                           = 0x19;

        public const int ELEMENT_TYPE_OBJECT                            = 0x1c;
        public const int ELEMENT_TYPE_SZARRAY                           = 0x1d;
        public const int ELEMENT_TYPE_MVAR                              = 0x1e;


        public const int TYPE_ID_NOT_SET                                = 0; // Not an actual type
        public const int TYPE_SYSTEM_OBJECT                             = 1;
        public const int TYPE_SYSTEM_ARRAY_NO_TYPE                      = 2;
        public const int TYPE_SYSTEM_VOID                               = 3;
        public const int TYPE_SYSTEM_BOOLEAN                            = 4;
        public const int TYPE_SYSTEM_BYTE                               = 5;
        public const int TYPE_SYSTEM_SBYTE                              = 6;
        public const int TYPE_SYSTEM_CHAR                               = 7;
        public const int TYPE_SYSTEM_INT16                              = 8;
        public const int TYPE_SYSTEM_INT32                              = 9;
        public const int TYPE_SYSTEM_STRING                             = 10;
        public const int TYPE_SYSTEM_INTPTR                             = 11;
        public const int TYPE_SYSTEM_RUNTIMEFIELDHANDLE                 = 12;
        public const int TYPE_SYSTEM_INVALIDCASTEXCEPTION               = 13;
        public const int TYPE_SYSTEM_UINT32                             = 14;
        public const int TYPE_SYSTEM_UINT16                             = 15;
        public const int TYPE_SYSTEM_ARRAY_CHAR                         = 16;
        public const int TYPE_SYSTEM_ARRAY_OBJECT                       = 17;
        public const int TYPE_SYSTEM_COLLECTIONS_GENERIC_IENUMERABLE_T  = 18;
        public const int TYPE_SYSTEM_COLLECTIONS_GENERIC_ICOLLECTION_T  = 19;
        public const int TYPE_SYSTEM_COLLECTIONS_GENERIC_ILIST_T        = 20;
        public const int TYPE_SYSTEM_MULTICASTDELEGATE                  = 21;
        public const int TYPE_SYSTEM_NULLREFERENCEEXCEPTION             = 22;
        public const int TYPE_SYSTEM_SINGLE                             = 23;
        public const int TYPE_SYSTEM_DOUBLE                             = 24;
        public const int TYPE_SYSTEM_INT64                              = 25;
        public const int TYPE_SYSTEM_UINT64                             = 26;
        public const int TYPE_SYSTEM_TYPECODE                           = 27;
        public const int TYPE_SYSTEM_RUNTIMETYPE                        = 28;
        public const int TYPE_SYSTEM_TYPE                               = 29;
        public const int TYPE_SYSTEM_RUNTIMETYPEHANDLE                  = 30;
        public const int TYPE_SYSTEM_RUNTIMEMETHODHANDLE                = 31;
        public const int TYPE_SYSTEM_ENUM                               = 32;
        public const int TYPE_SYSTEM_DATETIME                           = 33;
        public const int TYPE_SYSTEM_ARRAY_STRING                       = 34;
        public const int TYPE_SYSTEM_ARRAY_INT32                        = 35;
        public const int TYPE_SYSTEM_THREADING_THREAD                   = 36;
        public const int TYPE_SYSTEM_THREADING_THREADSTART              = 37;
        public const int TYPE_SYSTEM_THREADING_PARAMETERIZEDTHREADSTART = 38;
        public const int TYPE_SYSTEM_WEAKREFERENCE                      = 39;
        public const int TYPE_SYSTEM_IO_FILEMODE                        = 40;
        public const int TYPE_SYSTEM_IO_FILEACCESS                      = 41;
        public const int TYPE_SYSTEM_IO_FILESHARE                       = 42;
        public const int TYPE_SYSTEM_ARRAY_BYTE                         = 43;
        public const int TYPE_SYSTEM_GLOBALIZATION_UNICODECATEGORY      = 44;
        public const int TYPE_SYSTEM_OVERFLOWEXCEPTION                  = 45;
        public const int TYPE_SYSTEM_PLATFORMID                         = 46;
        public const int TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES            = 47;
        public const int TYPE_SYSTEM_UINTPTR                            = 48;
        public const int TYPE_SYSTEM_NULLABLE                           = 49;
        public const int TYPE_SYSTEM_ARRAY_TYPE                         = 50;
        public const int TYPE_SYSTEM_REFLECTION_PROPERTYINFO            = 51;
        public const int TYPE_SYSTEM_REFLECTION_METHODINFO              = 52;
        public const int TYPE_SYSTEM_REFLECTION_METHODBASE              = 53;
        public const int TYPE_SYSTEM_IFORMATPROVIDER                    = 54;
        public const int TYPE_SYSTEM_GLOBALIZATION_NUMBERSTYLES         = 55;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL || UNITY_STANDALONE

        public const int TYPE_UNITYENGINE_VECTOR2                       = 56;
        public const int TYPE_UNITYENGINE_VECTOR3                       = 57;
        public const int TYPE_UNITYENGINE_COLOR                         = 58;
        public const int TYPE_UNITYENGINE_COLOR32                       = 59;
        public const int TYPE_UNITYENGINE_VECTOR4                       = 60;
        public const int TYPE_UNITYENGINE_QUATERNION                    = 61;
        public const int TYPE_UNITYENGINE_VECTOR2INT                    = 62;
        public const int TYPE_UNITYENGINE_VECTOR3INT                    = 63;
        public const int TYPE_UNITYENGINE_RECT                          = 64;
        public const int TYPE_UNITYENGINE_RECTINT                       = 65;
        public const int TYPE_UNITYENGINE_RECTOFFSET                    = 66;
        public const int TYPE_UNITYENGINE_RAY2D                         = 67;
        public const int TYPE_UNITYENGINE_RAY                           = 68;
        public const int TYPE_UNITYENGINE_BOUNDS                        = 69;
        public const int TYPE_UNITYENGINE_PLANE                         = 70;
        public const int TYPE_UNITYENGINE_RANGEINT                      = 71;
        public const int TYPE_UNITYENGINE_MATRIX4X4                     = 72;

        public const int NUM_INIT_TYPES = 73;

#else

        public const int NUM_INIT_TYPES = 56;

#endif

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
        static tMD_MethodDef** ppGenericArrayMethods = null;

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
        static /*char**/ byte** pGenericArrayMethodsInit = null;

        public static tMD_TypeDef **types = null;

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
            public byte alignment;
            public byte hasMonoBase;
        };

        // String constant statics
        static byte* scMscorlib, scUnityEngine, scSystemCollectionsGeneric, scSystemReflection, scSystemThreading,
            scSystemIO, scSystemGlobalization, scSystem;

        static tTypeInit[] typeInit = null;
        //static int CorLibDone = 0;

        public static void Init() 
        {
            uint i;

            scMscorlib = scUnityEngine = scSystemCollectionsGeneric = scSystemReflection = scSystemThreading = 
                scSystemIO = scSystemGlobalization = scSystem = null;

            ppGenericArrayMethods = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(sizeof(tMD_MethodDef*) * GENERICARRAYMETHODS_NUM));

            pGenericArrayMethodsInit = S.buildArray(
                "Internal_GetGenericEnumerator",
                "get_Length",
                "Internal_GenericIsReadOnly",
                "Internal_GenericAdd",
                "Internal_GenericClear",
                "Internal_GenericContains",
                "Internal_GenericCopyTo",
                "Internal_GenericRemove",
                "Internal_GenericIndexOf",
                "Internal_GenericInsert",
                "Internal_GenericRemoveAt",
                "Internal_GenericGetItem",
                "Internal_GenericSetItem"
            );

            typeInit = new tTypeInit[] {
                new tTypeInit {assemblyName = null, nameSpace = null, name = null, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Object"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Array"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Void"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Boolean"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 1, instanceMemSize = 1, alignment = 1},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Byte"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 1, instanceMemSize = 1, alignment = 1},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("SByte"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 1, instanceMemSize = 1, alignment = 1},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Char"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 2, instanceMemSize = 2, alignment = 2},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Int16"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 2, instanceMemSize = 2, alignment = 2},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Int32"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("String"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE, hasMonoBase = 1},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("IntPtr"), stackType = EvalStack.EVALSTACK_PTR, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeFieldHandle"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("InvalidCastException"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UInt32"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UInt16"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 2, instanceMemSize = 2, alignment = 2},
                new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_CHAR, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_OBJECT, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemCollectionsGeneric, "System.Collections.Generic"), name = new S("IEnumerable`1"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemCollectionsGeneric, "System.Collections.Generic"), name = new S("ICollection`1"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemCollectionsGeneric, "System.Collections.Generic"), name = new S("IList`1"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("MulticastDelegate"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("NullReferenceException"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Single"), stackType = EvalStack.EVALSTACK_F32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Double"), stackType = EvalStack.EVALSTACK_F64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8, alignment = 8},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Int64"), stackType = EvalStack.EVALSTACK_INT64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8, alignment = 8},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UInt64"), stackType = EvalStack.EVALSTACK_INT64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8, alignment = 8},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("TypeCode"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeType"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tRuntimeType), alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Type"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeTypeHandle"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("RuntimeMethodHandle"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Enum"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("DateTime"), stackType = EvalStack.EVALSTACK_INT64, stackSize = 8, arrayElementSize = 8, instanceMemSize = 8, alignment = 8},
                new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_STRING, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_INT32, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemThreading, "System.Threading"), name = new S("Thread"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tThread), alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemThreading, "System.Threading"), name = new S("ThreadStart"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemThreading, "System.Threading"), name = new S("ParameterizedThreadStart"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("WeakReference"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileMode"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileAccess"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileShare"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_BYTE, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemGlobalization, "System.Globalization"), name = new S("UnicodeCategory"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("OverflowException"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("PlatformID"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemIO, "System.IO"), name = new S("FileAttributes"), stackType = EvalStack.EVALSTACK_O, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("UIntPtr"), stackType = EvalStack.EVALSTACK_PTR, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = 0, alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystem, "System"), name = new S("Nullable`1"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = null, nameSpace = null, name = (byte*)Type.TYPE_SYSTEM_TYPE, stackType = 0, stackSize = 0, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Reflection"), name = new S("PropertyInfo"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tPropertyInfo), alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Reflection"), name = new S("MethodInfo"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tMethodInfo), alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Reflection"), name = new S("MethodBase"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = PTR_SIZE, instanceMemSize = (byte)sizeof(tMethodBase), alignment = PTR_SIZE},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System"), name = new S("IFormatProvider"), stackType = EvalStack.EVALSTACK_O, stackSize = PTR_SIZE, arrayElementSize = 0, instanceMemSize = 0, alignment = 0},
                new tTypeInit {assemblyName = new S(ref scMscorlib, "mscorlib"), nameSpace = new S(ref scSystemReflection, "System.Globalization"), name = new S("NumberStyles"), stackType = EvalStack.EVALSTACK_INT32, stackSize = PTR_SIZE, arrayElementSize = 4, instanceMemSize = 4, alignment = 4},

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL || UNITY_STANDALONE
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Vector2"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Vector2), arrayElementSize = (byte)sizeof(Vector2), instanceMemSize = (byte)sizeof(Vector2), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Vector3"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Vector3), arrayElementSize = (byte)sizeof(Vector3), instanceMemSize = (byte)sizeof(Vector3), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Color"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Color), arrayElementSize = (byte)sizeof(Color), instanceMemSize = (byte)sizeof(Color), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Color32"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Color32), arrayElementSize = (byte)sizeof(Color32), instanceMemSize = (byte)sizeof(Color32), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Vector4"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Vector4), arrayElementSize = (byte)sizeof(Vector4), instanceMemSize = (byte)sizeof(Vector4), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Quaternion"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Quaternion), arrayElementSize = (byte)sizeof(Quaternion), instanceMemSize = (byte)sizeof(Quaternion), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Vector2Int"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Vector2Int), arrayElementSize = (byte)sizeof(Vector2Int), instanceMemSize = (byte)sizeof(Vector2Int), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Vector3Int"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Vector3Int), arrayElementSize = (byte)sizeof(Vector3Int), instanceMemSize = (byte)sizeof(Vector3Int), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Rect"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Rect), arrayElementSize = (byte)sizeof(Rect), instanceMemSize = (byte)sizeof(Rect), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("RectInt"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(RectInt), arrayElementSize = (byte)sizeof(RectInt), instanceMemSize = (byte)sizeof(RectInt), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("RectOffset"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)16, arrayElementSize = (byte)16, instanceMemSize = (byte)16, alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Ray2D"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Ray2D), arrayElementSize = (byte)sizeof(Ray2D), instanceMemSize = (byte)sizeof(Ray2D), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Ray"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Ray), arrayElementSize = (byte)sizeof(Ray), instanceMemSize = (byte)sizeof(Ray), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Bounds"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Bounds), arrayElementSize = (byte)sizeof(Bounds), instanceMemSize = (byte)sizeof(Bounds), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Plane"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Plane), arrayElementSize = (byte)sizeof(Plane), instanceMemSize = (byte)sizeof(Plane), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("RangeInt"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(RangeInt), arrayElementSize = (byte)sizeof(RangeInt), instanceMemSize = (byte)sizeof(RangeInt), alignment = 4},
                new tTypeInit {assemblyName = new S(ref scUnityEngine, "UnityEngine"), nameSpace = new S(ref scUnityEngine, "UnityEngine"), name = new S("Matrix4x4"), stackType = EvalStack.EVALSTACK_VALUETYPE, stackSize = (byte)sizeof(Matrix4x4), arrayElementSize =(byte) sizeof(Matrix4x4), instanceMemSize = (byte)sizeof(Matrix4x4), alignment = 4},
#endif

            };

            System.Diagnostics.Debug.Assert(typeInit.Length == NUM_INIT_TYPES);

            // Build all the types needed by the interpreter.
            types = (tMD_TypeDef**)Mem.mallocForever((SIZE_T)(NUM_INIT_TYPES * sizeof(tMD_TypeDef*)));
            for (i=0; i<NUM_INIT_TYPES; i++) {
                if (typeInit[i].assemblyName != null) {
                    // Normal type initialisation
                    types[i] = MetaData.GetTypeDefFromFullName(typeInit[i].assemblyName, typeInit[i].nameSpace, typeInit[i].name);
                    // For the pre-defined system types, fill in the well-known memory sizes
                    types[i]->typeInitId = i;
                    types[i]->stackType = typeInit[i].stackType;
                    types[i]->stackSize = typeInit[i].stackSize;
                    types[i]->arrayElementSize = typeInit[i].arrayElementSize;
                    types[i]->instanceMemSize = typeInit[i].instanceMemSize;
                    types[i]->alignment = typeInit[i].alignment;
                    types[i]->hasMonoBase = typeInit[i].hasMonoBase;
                }
            }
            if (genericArrayMethodsInited == 0) {
                GetMethodDefs();
            }
            for (i=0; i< NUM_INIT_TYPES; i++) {
                if (typeInit[i].assemblyName != null) {
                    MetaData.Fill_TypeDef(types[i], null, null);
                } else {
                    // Special initialisation for arrays of particular types.
                    uint arrayTypeId = (uint)(typeInit[i].name);
                    if (arrayTypeId != TYPE_ID_NOT_SET) {
                        if (types[(uint)(typeInit[i].name)]->fillState < Type.TYPE_FILL_ALL) {
                            Sys.Crash("Element type not filled");
                        }
                        types[i] = Type.GetArrayTypeDef(types[(uint)(typeInit[i].name)], null, null);
                    }
                }
            }
            //CorLibDone = 1;
        }

        public static void Clear()
        {
            pArrays = null;
            genericArrayMethodsInited = 0;
            ppGenericArrayMethods = null;
            pGenericArrayMethodsInit = null;
            types = null;
            scMscorlib = scSystemCollectionsGeneric = scSystemReflection = scSystemThreading =
                scSystemIO = scSystemGlobalization = scSystem = null;
            typeInit = null;
        }

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

        static void CreateNewArrayType(tMD_TypeDef *pNewArrayType, tMD_TypeDef *pElementType, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs) 
        {
            if (pNewArrayType->fillState == 1) {
                return;
            }

            Mem.heapcheck();

            if (genericArrayMethodsInited == 0) {
                GetMethodDefs();
            }

            if (types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE]->fillState < Type.TYPE_FILL_ALL) {
                MetaData.Fill_TypeDef(types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE], null, null);
            }

            Mem.memcpy(pNewArrayType, types[Type.TYPE_SYSTEM_ARRAY_NO_TYPE], (SIZE_T)sizeof(tMD_TypeDef));
        	pNewArrayType->pArrayElementType = pElementType;
            pNewArrayType->fillState = Type.TYPE_FILL_ALL;

            if (pElementType->fillState < Type.TYPE_FILL_ALL) {
                MetaData.Fill_Defer(pElementType, null, null);
            }

        	// Auto-generate the generic interfaces IEnumerable<T>, ICollection<T> and IList<T> for this array
        	{
                tInterfaceMap *pInterfaceMap;
                tInterfaceMap *pAllIMs;
        		tMD_TypeDef *pInterfaceT;
        		tMD_MethodDef *pMethod;
        		uint orgNumInterfaces;

        		orgNumInterfaces = pNewArrayType->numInterfaces;
        		pNewArrayType->numInterfaces += 3;
                pAllIMs = (tInterfaceMap*)Mem.mallocForever((SIZE_T)(pNewArrayType->numInterfaces * sizeof(tInterfaceMap)));
                Mem.memcpy(pAllIMs, pNewArrayType->pInterfaceMaps, (SIZE_T)(orgNumInterfaces * sizeof(tInterfaceMap)));
        		pNewArrayType->pInterfaceMaps = pAllIMs;

        		// Get the IEnumerable<T> interface
        		pInterfaceMap = &pAllIMs[orgNumInterfaces + 0];
        		pInterfaceT = Generics.GetGenericTypeFromCoreType(types[Type.TYPE_SYSTEM_COLLECTIONS_GENERIC_IENUMERABLE_T], 1, &pElementType);
                MetaData.Fill_TypeDef(pInterfaceT, null, null, Type.TYPE_FILL_VTABLE);
                pInterfaceMap->pInterface = pInterfaceT;
        		pInterfaceMap->pVTableLookup = null;
                pInterfaceMap->ppMethodVLookup = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pInterfaceT->numVirtualMethods * sizeof(tMD_MethodDef*)));
                tMD_MethodDef* pGenericEnumeratorMethod = ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GetGenericEnumerator];
                pMethod = Generics.GetMethodDefFromCoreMethod(pGenericEnumeratorMethod, pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[0] = pMethod;

        		// Get the ICollection<T> interface
        		pInterfaceMap = &pAllIMs[orgNumInterfaces + 1];
        		pInterfaceT = Generics.GetGenericTypeFromCoreType(types[Type.TYPE_SYSTEM_COLLECTIONS_GENERIC_ICOLLECTION_T], 1, &pElementType);
                MetaData.Fill_TypeDef(pInterfaceT, null, null, Type.TYPE_FILL_VTABLE);
                pInterfaceMap->pInterface = pInterfaceT;
        		pInterfaceMap->pVTableLookup = null;
                System.Diagnostics.Debug.Assert(pInterfaceT->numVirtualMethods >= 7);
                pInterfaceMap->ppMethodVLookup = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pInterfaceT->numVirtualMethods * sizeof(tMD_MethodDef*)));
        		pInterfaceMap->ppMethodVLookup[0] = ppGenericArrayMethods[GENERICARRAYMETHODS_get_Length];
        		pInterfaceMap->ppMethodVLookup[1] = ppGenericArrayMethods[GENERICARRAYMETHODS_get_IsReadOnly];
                pInterfaceMap->ppMethodVLookup[2] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericAdd], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[3] = ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericClear];
        		pInterfaceMap->ppMethodVLookup[4] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericContains], pNewArrayType, 1, &pElementType);
                pInterfaceMap->ppMethodVLookup[5] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericCopyTo], pNewArrayType, 1, &pElementType);
                pInterfaceMap->ppMethodVLookup[6] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericRemove], pNewArrayType, 1, &pElementType);

        		// Get the IList<T> interface
        		pInterfaceMap = &pAllIMs[orgNumInterfaces + 2];
                pInterfaceT = Generics.GetGenericTypeFromCoreType(types[Type.TYPE_SYSTEM_COLLECTIONS_GENERIC_ILIST_T], 1, &pElementType); //, ppClassTypeArgs, ppMethodTypeArgs);
                MetaData.Fill_TypeDef(pInterfaceT, null, null, Type.TYPE_FILL_VTABLE);
                pInterfaceMap->pInterface = pInterfaceT;
        		pInterfaceMap->pVTableLookup = null;
                System.Diagnostics.Debug.Assert(pInterfaceT->numVirtualMethods >= 5);
                pInterfaceMap->ppMethodVLookup = (tMD_MethodDef**)Mem.mallocForever((SIZE_T)(pInterfaceT->numVirtualMethods * sizeof(tMD_MethodDef*)));
        		pInterfaceMap->ppMethodVLookup[0] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericIndexOf], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[1] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericInsert], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[2] = ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericRemoveAt];
        		pInterfaceMap->ppMethodVLookup[3] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericGetItem], pNewArrayType, 1, &pElementType);
        		pInterfaceMap->ppMethodVLookup[4] = Generics.GetMethodDefFromCoreMethod(ppGenericArrayMethods[GENERICARRAYMETHODS_Internal_GenericSetItem], pNewArrayType, 1, &pElementType);
        	}

            Mem.heapcheck();

            Sys.log_f(2, "Array: Array[%s.%s]\n", (PTR)pElementType->nameSpace, (PTR)pElementType->name);
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

        // Get the TypeDef from the type signature
        // Also get the size of a field from the signature
        // This is needed to avoid recursive sizing of type like System.Boolean,
        // that has a field of type System.Boolean
        public static tMD_TypeDef* GetTypeFromSig(tMetaData *pMetaData, /*SIG*/byte* *pSig, 
            tMD_TypeDef **ppClassTypeArgs, tMD_TypeDef **ppMethodTypeArgs, tMD_TypeDef** ppByRefType = null) 
        {
        	uint entry;
            tMD_TypeDef* pType;

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
                        tMD_TypeDef *pByRefType = Type.GetTypeFromSig(pMetaData, pSig, ppClassTypeArgs, ppMethodTypeArgs, null);
                        if (ppByRefType != null) { 
                            *ppByRefType = pByRefType;
                        }
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
        				pType = Generics.GetGenericTypeFromSig(pMetaData, pSig, ppClassTypeArgs, ppMethodTypeArgs);
                        if (pType->fillState < Type.TYPE_FILL_ALL) {
                            MetaData.Fill_Defer(pType, ppClassTypeArgs, ppMethodTypeArgs);
                        }
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

        				pElementType = Type.GetTypeFromSig(pMetaData, pSig, ppClassTypeArgs, ppMethodTypeArgs, null);
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

            if (pMethod->signature == null) {
                Sys.Crash("Method does not have a signature");
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
        		pTypeDef->typeObject = System_RuntimeType.New(pTypeDef);
        	}
        	return pTypeDef->typeObject;
        }

    }
}
