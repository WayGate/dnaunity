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
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMethodBase
    {
        // Keep in sync with MethodBase class in .NET corlib code
        public /*HEAP_PTR*/byte* ownerType;
        public tSystemString* name;
        public tMD_MethodDef *methodDef; // Not accessed from .NET code
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tMethodInfo 
    {
        public tMethodBase methodBase;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tPropertyInfo
    {
        // Keep in sync with System.Reflection.PropertyInfo.cs
        public /*HEAP_PTR*/byte* ownerType;
        public tSystemString* name;
        public /*HEAP_PTR*/byte* propertyType;
    }

}