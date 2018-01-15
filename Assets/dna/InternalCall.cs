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

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tInternalCall 
    {
        public const int MAX_PARAMS = 6;

        public /*char*/byte* nameSpace;
        public /*char*/byte* type;
        public /*char*/byte* method;
        public /*fnInternalCall*/void* fn;
        public byte returnType;
        public uint numParameters;
        public byte* parameterTypes;
    }

    public unsafe static class InternalCall
    {

        public static byte* PTypes(byte a, byte b = 0, byte c = 0, byte d = 0, byte e = 0, byte f = 0)
        {
            throw new System.NotImplementedException();
        }

        public static tInternalCall[] internalCalls = new tInternalCall[] {
/*            
            {"System", "Object", "Equals", System_Object_Equals, Type.TYPE_SYSTEM_BOOLEAN, 1, {Type.TYPE_SYSTEM_OBJECT}},
            {null,      null,    "Clone", System_Object_Clone, Type.TYPE_SYSTEM_OBJECT, 1, {Type.TYPE_SYSTEM_OBJECT}},
            {null,      null,    "GetHashCode", System_Object_GetHashCode, Type.TYPE_SYSTEM_INT32, 0},
            {null,      null,    "GetType", System_Object_GetType, Type.TYPE_SYSTEM_TYPE, 0}, */

            new tInternalCall {nameSpace = null, type = new S("String"), method = new S(".ctor"), fn = new H(SystemString.ctor_CharInt32), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,     method = new S(".ctor"), fn = new H(SystemString.ctor_CharAIntInt), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_CHAR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,     method = new S(".ctor"), fn = new H(SystemString.ctor_StringIntInt), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("get_Chars"), fn = new H(SystemString.get_Chars), returnType = Type.TYPE_SYSTEM_CHAR, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("InternalConcat"), fn = new H(SystemString.InternalConcat), returnType = Type.TYPE_SYSTEM_STRING, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("InternalTrim"), fn = new H(SystemString.InternalTrim), returnType = Type.TYPE_SYSTEM_STRING, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_CHAR, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("Equals"), fn = new H(SystemString.Equals), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("GetHashCode"), fn = new H(SystemString.GetHashCode), returnType = Type.TYPE_SYSTEM_INT32},
            new tInternalCall {nameSpace = null, type = null,     method = new S("InternalReplace"), fn = new H(SystemString.InternalReplace), returnType = Type.TYPE_SYSTEM_STRING, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("InternalIndexOf"), fn = new H(SystemString.InternalIndexOf), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 4, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_BOOLEAN)},
            new tInternalCall {nameSpace = null, type = null,     method = new S("InternalIndexOfAny"), fn = new H(SystemString.InternalIndexOfAny), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 4, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_CHAR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_BOOLEAN)},

            new tInternalCall {nameSpace = null, type = new S("Array"), method = new S("Internal_GetValue"), fn = new H(SystemArray.Internal_GetValue), returnType = Type.TYPE_SYSTEM_OBJECT, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,    method = new S("Internal_SetValue"), fn = new H(SystemArray.Internal_SetValue), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,    method = new S("Clear"), fn = new H(SystemArray.Clear), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,    method = new S("Internal_Copy"), fn = new H(SystemArray.Internal_Copy), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 5, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,    method = new S("Resize"), fn = new H(SystemArray.Resize), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,    method = new S("Reverse"), fn = new H(SystemArray.Reverse), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
            new tInternalCall {nameSpace = null, type = null,    method = new S("CreateInstance"), fn = new H(SystemArray.CreateInstance), returnType = Type.TYPE_SYSTEM_ARRAY_NO_TYPE, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_TYPE, Type.TYPE_SYSTEM_INT32)},

            /*{null, "Console", "Write", System_Console_Write, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_STRING}},
            {null, null     , "Internal_ReadKey", System_Console_Internal_ReadKey, Type.TYPE_SYSTEM_INT32, 0},
            {null, null     , "Internal_KeyAvailable", System_Console_Internal_KeyAvailable, Type.TYPE_SYSTEM_BOOLEAN, 0},

            {null, "Environment", "get_TickCount", System_Environment_get_TickCount, Type.TYPE_SYSTEM_INT32, 0},
            {null, null         , "GetOSVersionString", System_Environment_GetOSVersionString, Type.TYPE_SYSTEM_STRING, 0},
            {null, null         , "get_Platform", System_Environment_get_Platform, Type.TYPE_SYSTEM_PLATFORMID, 0},

            {null, "Type", "GetTypeFromHandle", System_Type_GetTypeFromHandle, Type.TYPE_SYSTEM_TYPE, 1, {Type.TYPE_SYSTEM_RUNTIMETYPEHANDLE}},
            {null, null,   "EnsureAssemblyLoaded", System_Type_EnsureAssemblyLoaded, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_STRING}},
            {null, null,   "GetMethodInternal", System_Type_GetMethod, Type.TYPE_SYSTEM_OBJECT, 1, {Type.TYPE_SYSTEM_STRING}},
            {null, null,   "GetProperties", System_Type_GetProperties, Type.TYPE_SYSTEM_ARRAY_NO_TYPE, 0},
            {null, null,   "GetType", System_Type_GetTypeFromName, Type.TYPE_SYSTEM_TYPE, 3, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING}},
            {null, null,   "get_IsValueType", System_Type_get_IsValueType, Type.TYPE_SYSTEM_BOOLEAN, 0},
*/
            new tInternalCall {nameSpace = null, type = new S("RuntimeType"), method = new S("get_Name"), fn = new H(SystemRuntimeType.get_Name), returnType = Type.TYPE_SYSTEM_STRING},
            new tInternalCall {nameSpace = null, type = null, method = new S("get_Namespace"), fn = new H(SystemRuntimeType.get_Namespace), returnType = Type.TYPE_SYSTEM_STRING},
            new tInternalCall {nameSpace = null, type = null, method = new S("GetNestingParentType"), fn = new H(SystemRuntimeType.GetNestingParentType), returnType = Type.TYPE_SYSTEM_RUNTIMETYPE},
            new tInternalCall {nameSpace = null, type = null, method = new S("get_BaseType"), fn = new H(SystemRuntimeType.get_BaseType), returnType = Type.TYPE_SYSTEM_TYPE},
            new tInternalCall {nameSpace = null, type = null, method = new S("get_IsEnum"), fn = new H(SystemRuntimeType.get_IsEnum), returnType = Type.TYPE_SYSTEM_BOOLEAN},
            new tInternalCall {nameSpace = null, type = null, method = new S("get_IsGenericType"), fn = new H(SystemRuntimeType.get_IsGenericType), returnType = Type.TYPE_SYSTEM_BOOLEAN},
            new tInternalCall {nameSpace = null, type = null, method = new S("Internal_GetGenericTypeDefinition"), fn = new H(SystemRuntimeType.Internal_GetGenericTypeDefinition), returnType = Type.TYPE_SYSTEM_RUNTIMETYPE},
            new tInternalCall {nameSpace = null, type = null, method = new S("GetGenericArguments"), fn = new H(SystemRuntimeType.GetGenericArguments), returnType = Type.TYPE_SYSTEM_ARRAY_TYPE},
            new tInternalCall {nameSpace = null, type = null, method = new S("GetElementType"), fn = new H(SystemRuntimeType.GetElementType), returnType = Type.TYPE_SYSTEM_TYPE},

/*            {null, "Char", "GetUnicodeCategory", System_Char_GetUnicodeCategory, Type.TYPE_SYSTEM_GLOBALIZATION_UNICODECATEGORY, 1, {Type.TYPE_SYSTEM_CHAR}},
            {null, null  , "ToLowerInvariant", System_Char_ToLowerInvariant, Type.TYPE_SYSTEM_CHAR, 1, {Type.TYPE_SYSTEM_CHAR}},
            {null, null  , "ToUpperInvariant", System_Char_ToUpperInvariant, Type.TYPE_SYSTEM_CHAR, 1, {Type.TYPE_SYSTEM_CHAR}},

            {null, "GC", "Collect", System_GC_Collect, Type.TYPE_SYSTEM_VOID, 0},
            {null, null, "Internal_CollectionCount", System_GC_Internal_CollectionCount, Type.TYPE_SYSTEM_INT32, 0},
            {null, null, "GetTotalMemory", System_GC_GetTotalMemory, Type.TYPE_SYSTEM_INT64, 1, {Type.TYPE_SYSTEM_BOOLEAN}},
            {null, null, "SuppressFinalize", System_GC_SuppressFinalize, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_OBJECT}},

            {null, "Enum", "Internal_GetValue", System_Enum_Internal_GetValue, Type.TYPE_SYSTEM_INT32, 0},
            {null, null,   "Internal_GetInfo", System_Enum_Internal_GetInfo, Type.TYPE_SYSTEM_VOID, 3, {Type.TYPE_SYSTEM_TYPE, Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INTPTR}},

            {null, "ValueType", "GetFields", System_ValueType_GetFields, Type.TYPE_SYSTEM_ARRAY_NO_TYPE, 2, {Type.TYPE_SYSTEM_OBJECT, Type.TYPE_SYSTEM_OBJECT}},

            {null, "WeakReference", "get_Target", System_WeakReference_get_Target, Type.TYPE_SYSTEM_OBJECT, 0},
            {null, null,            "set_Target", System_WeakReference_set_Target, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_OBJECT}},

            {null, "DateTime", "InternalUtcNow", System_DateTime_InternalUtcNow, Type.TYPE_SYSTEM_INT64, 0},

            {null, "Math", "Sin", System_Math_Sin, Type.TYPE_SYSTEM_DOUBLE, 1, {Type.TYPE_SYSTEM_DOUBLE}},
            {null, null,   "Cos", System_Math_Cos, Type.TYPE_SYSTEM_DOUBLE, 1, {Type.TYPE_SYSTEM_DOUBLE}},
            {null, null,   "Tan", System_Math_Tan, Type.TYPE_SYSTEM_DOUBLE, 1, {Type.TYPE_SYSTEM_DOUBLE}},
            {null, null,   "Pow", System_Math_Pow, Type.TYPE_SYSTEM_DOUBLE, 2, {Type.TYPE_SYSTEM_DOUBLE, Type.TYPE_SYSTEM_DOUBLE}},
            {null, null,   "Sqrt", System_Math_Sqrt, Type.TYPE_SYSTEM_DOUBLE, 1, {Type.TYPE_SYSTEM_DOUBLE}},

            {"System.Threading", "Thread", ".ctor", System_Threading_Thread_ctor, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_THREADING_THREADSTART}},
            {null,               null,     ".ctor", System_Threading_Thread_ctorParam, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_THREADING_PARAMETERIZEDTHREADSTART}},
            {null,               null,     "Start", System_Threading_Thread_Start, Type.TYPE_SYSTEM_VOID, 0},
            {null,               null,     "Sleep", System_Threading_Thread_Sleep, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_INT32}},
            {null,               null,     "get_CurrentThread", System_Threading_Thread_get_CurrentThread, Type.TYPE_SYSTEM_THREADING_THREAD, 0},

            {null, "Monitor", "Internal_TryEnter", System_Threading_Monitor_Internal_TryEnter, Type.TYPE_SYSTEM_BOOLEAN, 2, {Type.TYPE_SYSTEM_OBJECT, Type.TYPE_SYSTEM_INT32}},
            {null, null,      "Internal_Exit", System_Threading_Monitor_Internal_Exit, Type.TYPE_SYSTEM_VOID, 1, {Type.TYPE_SYSTEM_OBJECT}},

            {null, "Interlocked", "CompareExchange", System_Threading_Interlocked_CompareExchange_Int32, Type.TYPE_SYSTEM_INT32, 3, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32}},
            {null, null,          "Increment", System_Threading_Interlocked_Increment_Int32, Type.TYPE_SYSTEM_INT32, 1, {Type.TYPE_SYSTEM_INTPTR}},
            {null, null,          "Decrement", System_Threading_Interlocked_Decrement_Int32, Type.TYPE_SYSTEM_INT32, 1, {Type.TYPE_SYSTEM_INTPTR}},
            {null, null,          "Add", System_Threading_Interlocked_Add_Int32, Type.TYPE_SYSTEM_INT32, 2, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32}},
            {null, null,          "Exchange", System_Threading_Interlocked_Exchange_Int32, Type.TYPE_SYSTEM_INT32, 2, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32}},

            {"System.IO", "FileInternal", "Open", System_IO_FileInternal_Open, Type.TYPE_SYSTEM_INTPTR, 5, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_IO_FILEMODE, Type.TYPE_SYSTEM_IO_FILEACCESS, Type.TYPE_SYSTEM_IO_FILESHARE, Type.TYPE_SYSTEM_INTPTR}},
            {null,        null,           "Read", System_IO_FileInternal_Read, Type.TYPE_SYSTEM_INT32, 5, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_ARRAY_BYTE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INTPTR}},
            {null,        null,           "Close", System_IO_FileInternal_Close, Type.TYPE_SYSTEM_VOID, 2, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INTPTR}},
            {null,        null,           "GetCurrentDirectory", System_IO_FileInternal_GetCurrentDirectory, Type.TYPE_SYSTEM_STRING, 1, {Type.TYPE_SYSTEM_INTPTR}},
            {null,        null,           "GetFileAttributes", System_IO_FileInternal_GetFileAttributes, Type.TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES, 2, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_INTPTR}},
            {null,        null,           "GetFileSystemEntries", System_IO_FileInternal_GetFileSystemEntries, Type.TYPE_SYSTEM_ARRAY_STRING, 5, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES, Type.TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES, Type.TYPE_SYSTEM_INTPTR}},

            {"System.Runtime.CompilerServices", "RuntimeHelpers", "InitializeArray", System_Runtime_CompilerServices_InitializeArray, Type.TYPE_SYSTEM_VOID, 2, {Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_RUNTIMEFIELDHANDLE}},

            {"System.Diagnostics", "Debugger", "Break", System_Diagnostics_Debugger_Break, Type.TYPE_SYSTEM_VOID, 0},

            {"System.Runtime.InteropServices",  "GCHandle",     "ToHeapRef", Framework_JSInterop_ToHeapRef, Type.TYPE_SYSTEM_INT32, 1, {Type.TYPE_SYSTEM_OBJECT}}, 
            {null,                              null,           "FromHeapRef", Framework_JSInterop_FromHeapRefImpl, Type.TYPE_SYSTEM_OBJECT, 1, {Type.TYPE_SYSTEM_INT32}}, 
*/
        };

        static void* /*fnInternalCall*/ Map_Delegate(tMD_MethodDef *pMethod)
        {
//            // Note that it is not neccessary to check argument type here, as delegates are very tightly controlled
//            if (S.strcmp(pMethod->name, ".ctor") == 0) {
//                return ctor;
//            }

            return null;
        }

        public static /*fnInternalCall*/void* InternalCall_Map(tMD_MethodDef *pMethod) 
        {
        	if (pMethod->pParentType->pParent == Type.types[Type.TYPE_SYSTEM_MULTICASTDELEGATE]) {
        		// Special case to handle delegates
                /*fnInternalCall*/ void* fn = Map_Delegate(pMethod);
        		if (fn != null) {
        			return fn;
        		}
        	} else {

                for (int i = 0; i < internalCalls.Length; i++) {
                    fixed (tInternalCall *pCall = &internalCalls[i]) {
                        /*STRING*/byte* curNameSpace = null;
                        /*STRING*/byte* curType = null;
            			if (pCall->nameSpace != null) {
            				curNameSpace = pCall->nameSpace;
            			}
            			if (pCall->type != null) {
            				curType = pCall->type;
            			}
            			if (S.strcmp(pMethod->pParentType->nameSpace, curNameSpace) == 0) {
            				if (S.strcmp(pMethod->pParentType->name, curType) == 0) {
            					if (Type.IsMethod(pMethod, pCall->method, Type.types[pCall->returnType], pCall->numParameters, pCall->parameterTypes) != 0) {
            						return pCall->fn;
            					}
            				}
            			}
                    }
        		}

        	}
            Sys.Crash("InternalCall_Map(): Cannot map [%s]%s.%s", (PTR)pMethod->pParentType->nameSpace, (PTR)pMethod->pParentType->name, (PTR)pMethod->name);
            return null;
        }

    }
}
