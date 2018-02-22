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
        public static tInternalCall[] internalCalls = null;

        public static byte* PTypes(byte a, byte b = 255, byte c = 255, byte d = 255, byte e = 255, byte f = 255)
        {
            int count;
            if (b == 255)
                count = 1;
            else if (c == 255)
                count = 2;
            else if (d == 255)
                count = 3;
            else if (e == 255)
                count = 4;
            else if (f == 255)
                count = 5;
            else
                count = 6;
            byte* types = (byte*)Mem.malloc((SIZE_T)(sizeof(byte*) * count));
            types[0] = a;
            if (count > 1)
                types[1] = b;
            if (count > 2)
                types[2] = c;
            if (count > 3)
                types[3] = d;
            if (count > 4)
                types[4] = e;
            if (count > 5)
                types[5] = f;
            return types;
        }

        public static void Init()
        {
            internalCalls = new tInternalCall[] {

                new tInternalCall {nameSpace = new S("System"), type = new S("Object"), method = new S("Equals"), fn = new H(System_Object.Equals), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Clone"), fn = new H(System_Object.Clone), returnType = Type.TYPE_SYSTEM_OBJECT, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT)},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetHashCode"), fn = new H(System_Object.GetHashCode), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetType"), fn = new H(System_Object.GetType), returnType = Type.TYPE_SYSTEM_TYPE, numParameters = 0},

                new tInternalCall {nameSpace = null, type = new S("String"), method = new S(".ctor"), fn = new H(System_String.ctor_CharInt32), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,     method = new S(".ctor"), fn = new H(System_String.ctor_CharAIntInt), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_CHAR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,     method = new S(".ctor"), fn = new H(System_String.ctor_StringIntInt), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("get_Chars"), fn = new H(System_String.get_Chars), returnType = Type.TYPE_SYSTEM_CHAR, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("InternalConcat"), fn = new H(System_String.InternalConcat), returnType = Type.TYPE_SYSTEM_STRING, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("InternalTrim"), fn = new H(System_String.InternalTrim), returnType = Type.TYPE_SYSTEM_STRING, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_CHAR, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("Equals"), fn = new H(System_String.Equals), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("GetHashCode"), fn = new H(System_String.GetHashCode), returnType = Type.TYPE_SYSTEM_INT32},
                new tInternalCall {nameSpace = null, type = null,     method = new S("InternalReplace"), fn = new H(System_String.InternalReplace), returnType = Type.TYPE_SYSTEM_STRING, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("InternalIndexOf"), fn = new H(System_String.InternalIndexOf), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 4, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_BOOLEAN)},
                new tInternalCall {nameSpace = null, type = null,     method = new S("InternalIndexOfAny"), fn = new H(System_String.InternalIndexOfAny), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 4, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_CHAR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_BOOLEAN)},

                new tInternalCall {nameSpace = null, type = new S("Array"), method = new S("Internal_GetValue"), fn = new H(System_Array.Internal_GetValue), returnType = Type.TYPE_SYSTEM_OBJECT, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Internal_SetValue"), fn = new H(System_Array.Internal_SetValue), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Clear"), fn = new H(System_Array.Clear), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Internal_Copy"), fn = new H(System_Array.Internal_Copy), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 5, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Resize"), fn = new H(System_Array.Resize), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Reverse"), fn = new H(System_Array.Reverse), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("CreateInstance"), fn = new H(System_Array.CreateInstance), returnType = Type.TYPE_SYSTEM_ARRAY_NO_TYPE, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_TYPE, Type.TYPE_SYSTEM_INT32)},

                new tInternalCall {nameSpace = null, type = new S("Console"), method = new S("Write"), fn = new H(System_Console.Write), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Internal_ReadKey"), fn = new H(System_Console.Internal_ReadKey), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null,    method = new S("Internal_KeyAvailable"), fn = new H(System_Console.Internal_KeyAvailable), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 0},

                /*{null, "Environment", "get_TickCount", System_Environment_get_TickCount, Type.TYPE_SYSTEM_INT32, 0},
                {null, null         , "GetOSVersionString", System_Environment_GetOSVersionString, Type.TYPE_SYSTEM_STRING, 0},
                {null, null         , "get_Platform", System_Environment_get_Platform, Type.TYPE_SYSTEM_PLATFORMID, 0}, */

                new tInternalCall {nameSpace = null, type = new S("Type"), method = new S("GetTypeFromHandle"), fn = new H(System_Type.GetTypeFromHandle), returnType = Type.TYPE_SYSTEM_TYPE, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_RUNTIMETYPEHANDLE)},
                new tInternalCall {nameSpace = null, type = null, method = new S("EnsureAssemblyLoaded"), fn = new H(System_Type.EnsureAssemblyLoaded), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetMethodInternal"), fn = new H(System_Type.GetMethod), returnType = Type.TYPE_SYSTEM_OBJECT, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetProperties"), fn = new H(System_Type.GetProperties), returnType = Type.TYPE_SYSTEM_ARRAY_NO_TYPE, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetType"), fn = new H(System_Type.GetTypeFromName), returnType = Type.TYPE_SYSTEM_TYPE, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING)},
                new tInternalCall {nameSpace = null, type = null, method = new S("get_IsValueType"), fn = new H(System_Type.get_IsValueType), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 0},
    
                new tInternalCall {nameSpace = null, type = new S("RuntimeType"), method = new S("get_Name"), fn = new H(System_RuntimeType.get_Name), returnType = Type.TYPE_SYSTEM_STRING},
                new tInternalCall {nameSpace = null, type = null, method = new S("get_Namespace"), fn = new H(System_RuntimeType.get_Namespace), returnType = Type.TYPE_SYSTEM_STRING},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetNestingParentType"), fn = new H(System_RuntimeType.GetNestingParentType), returnType = Type.TYPE_SYSTEM_RUNTIMETYPE},
                new tInternalCall {nameSpace = null, type = null, method = new S("get_BaseType"), fn = new H(System_RuntimeType.get_BaseType), returnType = Type.TYPE_SYSTEM_TYPE},
                new tInternalCall {nameSpace = null, type = null, method = new S("get_IsEnum"), fn = new H(System_RuntimeType.get_IsEnum), returnType = Type.TYPE_SYSTEM_BOOLEAN},
                new tInternalCall {nameSpace = null, type = null, method = new S("get_IsGenericType"), fn = new H(System_RuntimeType.get_IsGenericType), returnType = Type.TYPE_SYSTEM_BOOLEAN},
                new tInternalCall {nameSpace = null, type = null, method = new S("Internal_GetGenericTypeDefinition"), fn = new H(System_RuntimeType.Internal_GetGenericTypeDefinition), returnType = Type.TYPE_SYSTEM_RUNTIMETYPE},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetGenericArguments"), fn = new H(System_RuntimeType.GetGenericArguments), returnType = Type.TYPE_SYSTEM_ARRAY_TYPE},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetElementType"), fn = new H(System_RuntimeType.GetElementType), returnType = Type.TYPE_SYSTEM_TYPE},

                new tInternalCall {nameSpace = null, type = new S("Char"), method = new S("GetUnicodeCategory"), fn = new H(System_Char.GetUnicodeCategory), returnType = Type.TYPE_SYSTEM_GLOBALIZATION_UNICODECATEGORY, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR)},
                new tInternalCall {nameSpace = null, type = null, method = new S("ToLowerInvariant"), fn = new H(System_Char.ToLowerInvariant), returnType = Type.TYPE_SYSTEM_CHAR, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR)},
                new tInternalCall {nameSpace = null, type = null, method = new S("ToUpperInvariant"), fn = new H(System_Char.ToUpperInvariant), returnType = Type.TYPE_SYSTEM_CHAR, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_CHAR)},

                new tInternalCall {nameSpace = null, type = new S("GC"), method = new S("Collect"), fn = new H(System_GC.Collect), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("Internal_CollectionCount"), fn = new H(System_GC.Internal_CollectionCount), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("GetTotalMemory"), fn = new H(System_GC.GetTotalMemory), returnType = Type.TYPE_SYSTEM_INT64, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_BOOLEAN)},
                new tInternalCall {nameSpace = null, type = null, method = new S("SuppressFinalize"), fn = new H(System_GC.SuppressFinalize), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT)},

                new tInternalCall {nameSpace = null, type = new S("Enum"), method = new S("Internal_GetValue"), fn = new H(System_Enum.Internal_GetValue), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("Internal_GetInfo"), fn = new H(System_Enum.Internal_GetInfo), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_TYPE, Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INTPTR)},

                new tInternalCall {nameSpace = null, type = new S("ValueType"), method = new S("GetFields"), fn = new H(System_ValueType.GetFields), returnType = Type.TYPE_SYSTEM_ARRAY_NO_TYPE, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT, Type.TYPE_SYSTEM_OBJECT)},

                new tInternalCall {nameSpace = null, type = new S("WeakReference"), method = new S("get_Target"), fn = new H(System_WeakReference.get_Target), returnType = Type.TYPE_SYSTEM_OBJECT, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("set_Target"), fn = new H(System_WeakReference.set_Target), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT)},

                new tInternalCall {nameSpace = null, type = new S("DateTime"), method = new S("InternalUtcNow"), fn = new H(System_DateTime.InternalUtcNow), returnType = Type.TYPE_SYSTEM_INT64, numParameters = 0},

                new tInternalCall {nameSpace = null, type = new S("Math"), method = new S("Sin"), fn = new H(System_Math.Sin), returnType = Type.TYPE_SYSTEM_DOUBLE, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_DOUBLE)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Cos"), fn = new H(System_Math.Cos), returnType = Type.TYPE_SYSTEM_DOUBLE, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_DOUBLE)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Tan"), fn = new H(System_Math.Tan), returnType = Type.TYPE_SYSTEM_DOUBLE, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_DOUBLE)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Pow"), fn = new H(System_Math.Pow), returnType = Type.TYPE_SYSTEM_DOUBLE, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_DOUBLE, Type.TYPE_SYSTEM_DOUBLE)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Sqrt"), fn = new H(System_Math.Sqrt), returnType = Type.TYPE_SYSTEM_DOUBLE, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_DOUBLE)},

                new tInternalCall {nameSpace = new S("System.Threading"), type = new S("Thread"), method = new S(".ctor"), fn = new H(System_Threading_Thread.ctor), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_THREADING_THREADSTART)},
                new tInternalCall {nameSpace = null, type = null, method = new S(".ctor"), fn = new H(System_Threading_Thread.ctorParam), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_THREADING_PARAMETERIZEDTHREADSTART)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Start"), fn = new H(System_Threading_Thread.Start), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 0},
                new tInternalCall {nameSpace = null, type = null, method = new S("Sleep"), fn = new H(System_Threading_Thread.Sleep), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null, method = new S("get_CurrentThread"), fn = new H(System_Threading_Thread.get_CurrentThread), returnType = Type.TYPE_SYSTEM_THREADING_THREAD, numParameters = 0},

                new tInternalCall {nameSpace = null, type = new S("Monitor"), method = new S("Internal_TryEnter"), fn = new H(System_Threading_Monitor.Internal_TryEnter), returnType = Type.TYPE_SYSTEM_BOOLEAN, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Internal_Exit"), fn = new H(System_Threading_Monitor.Internal_Exit), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_OBJECT)},

                new tInternalCall {nameSpace = null, type = new S("Interlocked"), method = new S("CompareExchange"), fn = new H(System_Threading_Interlocked.CompareExchange_Int32), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 3, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Increment"), fn = new H(System_Threading_Interlocked.Increment_Int32), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Decrement"), fn = new H(System_Threading_Interlocked.Decrement_Int32), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 1, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Add"), fn = new H(System_Threading_Interlocked.Add_Int32), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32)},
                new tInternalCall {nameSpace = null, type = null, method = new S("Exchange"), fn = new H(System_Threading_Interlocked.Exchange_Int32), returnType = Type.TYPE_SYSTEM_INT32, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INT32)},

                /*
                {"System.IO", "FileInternal", "Open", System_IO_FileInternal_Open, Type.TYPE_SYSTEM_INTPTR, 5, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_IO_FILEMODE, Type.TYPE_SYSTEM_IO_FILEACCESS, Type.TYPE_SYSTEM_IO_FILESHARE, Type.TYPE_SYSTEM_INTPTR}},
                {null,        null,           "Read", System_IO_FileInternal_Read, Type.TYPE_SYSTEM_INT32, 5, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_ARRAY_BYTE, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INT32, Type.TYPE_SYSTEM_INTPTR}},
                {null,        null,           "Close", System_IO_FileInternal_Close, Type.TYPE_SYSTEM_VOID, 2, {Type.TYPE_SYSTEM_INTPTR, Type.TYPE_SYSTEM_INTPTR}},
                {null,        null,           "GetCurrentDirectory", System_IO_FileInternal_GetCurrentDirectory, Type.TYPE_SYSTEM_STRING, 1, {Type.TYPE_SYSTEM_INTPTR}},
                {null,        null,           "GetFileAttributes", System_IO_FileInternal_GetFileAttributes, Type.TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES, 2, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_INTPTR}},
                {null,        null,           "GetFileSystemEntries", System_IO_FileInternal_GetFileSystemEntries, Type.TYPE_SYSTEM_ARRAY_STRING, 5, {Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_STRING, Type.TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES, Type.TYPE_SYSTEM_IO_FILESYSTEMATTRIBUTES, Type.TYPE_SYSTEM_INTPTR}},
                */

                new tInternalCall {nameSpace = new S("System.Runtime.CompilerServices"), type = new S("RuntimeHelpers"), method = new S("InitializeArray"), fn = new H(System_Runtime_CompilerServices.InitializeArray), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 2, parameterTypes = PTypes(Type.TYPE_SYSTEM_ARRAY_NO_TYPE, Type.TYPE_SYSTEM_RUNTIMEFIELDHANDLE)}, 

                new tInternalCall {nameSpace = new S("System.Diagnostics"), type = new S("Debugger"), method = new S("Break"), fn = new H(System_Diagnostics_Debugger.Break), returnType = Type.TYPE_SYSTEM_VOID, numParameters = 0},

            };
        }

        static void* /*fnInternalCall*/ Map_Delegate(tMD_MethodDef *pMethod)
        {
//            // Note that it is not neccessary to check argument type here, as delegates are very tightly controlled
//            if (S.strcmp(pMethod->name, ".ctor") == 0) {
//                return ctor;
//            }

            return null;
        }

        public static /*fnInternalCall*/void* Map(tMD_MethodDef *pMethod) 
        {
        	if (pMethod->pParentType->pParent == Type.types[Type.TYPE_SYSTEM_MULTICASTDELEGATE]) {
        		// Special case to handle delegates
                /*fnInternalCall*/ void* fn = Map_Delegate(pMethod);
        		if (fn != null) {
        			return fn;
        		}
        	} else {

                /*STRING*/ byte* curNameSpace = null;
                /*STRING*/ byte* curType = null;
                for (int i = 0; i < internalCalls.Length; i++) {
                    fixed (tInternalCall *pCall = &internalCalls[i]) {
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
