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
    using _SIZE_T = System.UInt32;
    using _PTR = System.UInt32;
    #else
    using _SIZE_T = System.UInt64;
    using _PTR = System.UInt64;
    #endif   

#if NO

// Indexes into the user-string heap
typedef uint /*IDX_USERSTRINGS*/uint;

// Index into a table. most significant byte stores which table, other 3 bytes store index
typedef uint /*IDX_TABLE*/uint;

// Flag Type.types
typedef uint /*FLAGS32*/uint;
typedef ushort /*FLAGS16*/ushort;

// Pointers
typedef byte* /*HEAP_PTR*/byte*;
typedef byte* byte*;
typedef byte* /*SIG*/byte*;
typedef byte* /*STRING*/byte*; // UTF8/ASCII string
typedef ushort* /*STRING2*/ushort*; // UTF16 string
typedef byte* /*BLOB_*/byte*;
typedef byte* /*GUID_*/byte*;

// Int Type.types
typedef long long long;
typedef unsigned long long ulong;

//#if WIN32

typedef int int;
typedef uint uint;
typedef short short;
typedef ushort ushort;
typedef char sbyte;
typedef byte byte;

//#endif // WIN32



#endif

    [StructLayout(LayoutKind.Explicit)]
    public struct uConvDouble 
    {
        [FieldOffset(0)]
        public double d;
        [FieldOffset(0)]
        public ulong u64;
        [FieldOffset(0)]
        public uint u32a;
        [FieldOffset(4)]
        public uint u32b;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct uConvFloat
    {
        [FieldOffset(0)]
        public float f;
        [FieldOffset(0)]
        public uint u32;
    };

    // Native function call
    public unsafe delegate tAsyncCall* fnInternalCall(tJITCallNative* pMethodDef, byte* pThis_, byte* pParams, byte* pReturnValue);
    // Native function call check routine for blocking IO
    public unsafe delegate uint fnInternalCallCheck(tJITCallNative* pMethodDef, byte* pThis_, byte* pParams, byte* pReturnValue, tAsyncCall *pAsync);
    // Field getter setter
    public unsafe delegate void fnFieldGetterSetter(tMD_FieldDef* pFieldDef, byte* _this, byte* pInValue);


    public unsafe struct tAsyncCall 
    {
    	// If this is a sleep call, then put the sleep time in ms here.
    	// -1 means it's not a sleep call. Inifite timeouts are not allowed.
    	public int sleepTime;
    	// If this is a blocking IO call, then this is the function to poll to see if the result is available.
        public /*fnInternalCallCheck*/void* checkFn;
    	// A state pointer for general use for blocking IO calls.
    	public byte* state;
    	// Not for most functions to use. Record the start time of this async call
    	public ulong startTime;
    };

}
