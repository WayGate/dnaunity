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
    // Note that care is needed to ensure the target object refered to in the delegate is not accidently
    // garbage collected.
    // Because a non-precise GC is currently used, this just happens automatically.
    // The /*HEAP_PTR*/byte* targetObj keeps the target object alive.

    public unsafe struct tDelegate 
    {
        // These must match the definition in Delegate.cs
        // The target object, null if calling a static method
        public /*HEAP_PTR*/byte* targetObj;
        // The target method
        public tMD_MethodDef *pTargetMethod;
        // The next delegate in a multicast delegate
        public tDelegate *pNext;
    }

    public unsafe static class Delegate
    {

        public static tMD_MethodDef* GetMethod(void *pThis_) 
        {
        	tDelegate *pThis = (tDelegate*)pThis_;

        	return pThis->pTargetMethod;
        }

        public static tMD_MethodDef* GetMethodAndStore(void *pThis_, /*HEAP_PTR*/byte* *pTargetObj, void **ppNextDelegate) 
        {
        	tDelegate *pThis = (tDelegate*)pThis_;

        	*pTargetObj = pThis->targetObj;
        	if (ppNextDelegate != null) {
        		*ppNextDelegate = pThis->pNext;
        	}
        	return pThis->pTargetMethod;
        }

        static tAsyncCall* ctor(byte* pThis_, byte* pParams, byte* pReturnValue) {
        	// Note that the 'this' object is already allocated because this method is not
        	// marked as 'InternalMethod' - it is marked as 'runtime'
        	tDelegate *pThis = (tDelegate*)pThis_;

        	pThis->targetObj = ((/*HEAP_PTR*/byte**)pParams)[0];
        	pThis->pTargetMethod = ((tMD_MethodDef**)pParams)[1];
        	pThis->pNext = null;

        	return null;
        }

    }
}
