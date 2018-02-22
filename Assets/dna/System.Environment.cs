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

    public static unsafe class System_Environment
    {
        
        public static tAsyncCall* get_TickCount(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            *(int*)pReturnValue = System.Environment.TickCount;
        	return null;
        }

        public static tAsyncCall* GetOSVersionString(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            *(byte**)pReturnValue = System_String.FromMonoString(System.Environment.OSVersion.Version.ToString());
        	return null;
        }

        public static tAsyncCall* get_Platform(byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
            *(uint*)pReturnValue = (uint)System.Environment.OSVersion.Platform;
        	return null;
        }
    }

}
