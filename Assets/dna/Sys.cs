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

    public unsafe static class Sys
    {
        const int METHOD_NAME_BUF_SIZE = 1024;
        static byte* /*char*/ methodNameBuf;

        public static void Init()
        {
            methodNameBuf = (byte*)Mem.malloc((SIZE_T)METHOD_NAME_BUF_SIZE);
        }

        public static void Clear()
        {
            methodNameBuf = null;
        }

        public static uint INTERNALCALL_RESULT_U32(void* r, uint val)
        {
            return *(uint*)r = (val); 
        }

        public static int INTERNALCALL_RESULT_I32(void* r, int val) 
        { 
            return *(int*)r = (val); 
        }

        public static void* INTERNALCALL_RESULT_PTR(void* r, void* val) 
        { 
            return *(void**)r = (void*)(val); 
        }

        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        public const int S_PTR = 4;
        #else
        public const int S_PTR = 8;
        #endif 
        public const int S_INT = 4;

        public static uint logLevel = 0;

        public static void log_f(uint level, string pMsg, params object[] args)
        {
            if (logLevel <= level)
                printf(pMsg, args);
        }

        public static int isCrashed;

        public static void Crash(string pMsg, params object[] args) 
        {
            isCrashed = 1;
            byte* buf = stackalloc byte[2048];
            S.snprintf(buf, 2048, pMsg, args);
            printf("%s", (PTR)buf);
            throw new System.InvalidOperationException("CRASH! - " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)buf));
        }
  
        public static /*char*/byte* GetMethodDesc(tMD_MethodDef *pMethod) 
        {
        	int i;

            byte* namePos = methodNameBuf;
            byte* nameEnd = namePos + METHOD_NAME_BUF_SIZE;

            namePos = S.scatprintf(namePos, nameEnd, "%s.%s.%s(", (PTR)pMethod->pParentType->nameSpace, (PTR)pMethod->pParentType->name, (PTR)pMethod->name);
        	for (i=MetaData.METHOD_ISSTATIC(pMethod)?0:1; i<pMethod->numberOfParameters; i++) {
        		if (i > (int)(MetaData.METHOD_ISSTATIC(pMethod)?0:1)) {
        			namePos = S.scatprintf(namePos, nameEnd, ",");
        		}
                tParameter *param = &(pMethod->pParams[i]);
                namePos = S.scatprintf(namePos, nameEnd, "%s", (PTR)param->pStackTypeDef->name);
        	}
        	S.scatprintf(namePos, nameEnd, ")");

        	return methodNameBuf;
        }

        public static ulong msTime() 
        {
            throw new System.NotImplementedException();
        }

        public static void SleepMS(uint ms) 
        {
            throw new System.NotImplementedException();
        }

        public static void printf(string format, params object[] args)
        {
            byte* buf = stackalloc byte[2048];
            S.snprintf(buf, 2048, format, args);
            string msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)buf);
            #if !UNITY_EDITOR && !UNITY_IOS && !UNITY_ANDROID && !UNITY_WEBGL && !UNITY_STANDALONE
            System.Console.WriteLine(msg);
            #else
            UnityEngine.Debug.Log(msg);
            #endif
        }
    }
}