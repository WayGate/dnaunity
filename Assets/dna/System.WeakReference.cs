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
    public unsafe struct tWeakRef
    {
        // The target of this weak-ref
        public /*HEAP_PTR*/byte* target;
        // Does this weak-ref track resurrection?
        public uint trackRes;
        // Link to the next weak-ref that points to the same target
        public tWeakRef *pNextWeakRef;
    }

    public unsafe static class System_WeakReference
    {

        public static tAsyncCall* get_Target(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tWeakRef *pThis = (tWeakRef*)pThis_;
        	*(/*HEAP_PTR*/byte**)pReturnValue = pThis->target;
        	return null;
        }

        public static tAsyncCall* set_Target(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue) 
        {
        	tWeakRef *pThis = (tWeakRef*)pThis_;
        	/*HEAP_PTR*/byte* target = ((/*HEAP_PTR*/byte**)pParams)[0];

        	if (pThis->target != null) {
        		tWeakRef **ppWeakRef = (tWeakRef**)Heap.GetWeakRefAddress(pThis->target);
        		while (*ppWeakRef != null) {
        			tWeakRef *pWeakRef = *ppWeakRef;
        			if (pWeakRef == pThis) {
        				*ppWeakRef = pWeakRef->pNextWeakRef;
        				Heap.RemovedWeakRefTarget(pWeakRef->target);
        				goto foundOK;
        			}
        			ppWeakRef = &(pWeakRef->pNextWeakRef);
        		}
        		Sys.Crash("WeakRef.set_Target() Error: cannot find weak-ref target for removal");
        foundOK:;
        	}
        	pThis->target = target;
        	if (target != null) {
        		pThis->pNextWeakRef = (tWeakRef*)Heap.SetWeakRefTarget(target, (/*HEAP_PTR*/byte*)pThis);
        	}

        	return null;
        }

        public static void TargetGone(/*HEAP_PTR*/byte* *ppWeakRef_, uint removeLongRefs) 
        {
        	tWeakRef **ppWeakRef = (tWeakRef**)ppWeakRef_;
        	tWeakRef *pWeakRef = *ppWeakRef;
        	while (pWeakRef != null) {
        		if (removeLongRefs != 0 || pWeakRef->trackRes == 0) {
        			// Really remove it
        			pWeakRef->target = null;
        		} else {
        			// Long ref, so keep it
        			*ppWeakRef = pWeakRef;
        			ppWeakRef = &(pWeakRef->pNextWeakRef);
        		}
        		pWeakRef = pWeakRef->pNextWeakRef;
        	}
        	*ppWeakRef = null;
        }

    }
}