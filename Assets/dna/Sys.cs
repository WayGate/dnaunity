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

    public unsafe static class Sys
    {
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

        public static uint logLevel;

        public static void log_f(uint level, string pMsg, params object[] args)
        {
            if (logLevel <= level)
                printf(pMsg, args);
        }

        public static void Crash(string pMsg, params object[] args) 
        {
            printf(pMsg, args);
            throw new System.InvalidOperationException("DnaUnity CRASH!");
        }
  
        public static /*char*/byte* GetMethodDesc(tMD_MethodDef *pMethod) 
        {
            throw new System.NotImplementedException();
            #if NO
        	uint i;

            S.sprintf(methodName, new S("%s.%s.%s("), pMethod->pParentType->nameSpace, pMethod->pParentType->name, pMethod->name);
        	for (i=MetaData.METHOD_ISSTATIC(pMethod)?0:1; i<pMethod->numberOfParameters; i++) {
        		if (i > (uint)(MetaData.MetaData.METHOD_ISSTATIC(pMethod)?0:1)) {
        			S.sprintf(S.strchr(methodName, 0), ",");
        		}
                tParameter *param = &(pMethod->pParams[i]);
                S.sprintf(S.strchr(methodName, 0), "%s", param->pTypeDef->name);
        	}
        	S.sprintf(S.strchr(methodName, 0), ")");
        	return methodName;
            #endif
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