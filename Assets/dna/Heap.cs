// Copyright (c) 2012 DotNetAnywhere
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:Heap.Alloc
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

    // Memory roots are:
    // All threads, all MethodStates - the ParamLocals memory and the evaluation stack
    // All static fields of all Type.types
    // Note that the evaluation stack is not typed, so every 4-byte entry is treated as a pointer

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tSync 
    {
        // The thread that holds this sync block
        public tThread *pThread;
        // The number of times this thread has entered the sync block
        public uint count;

        // Link to the first weak-ref that targets this object.
        // This allows the tracking of all weak-refs that target this object.
        public /*HEAP_PTR*/byte* weakRef;
    }

    // The memory is kept track of using a balanced binary search tree (ordered by memory address)
    // See http://www.eternallyconfuzzled.com/tuts/datastructures/jsw_tut_andersson.aspx for details.

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tHeapRootEntry
    {
        public uint numPointers; // The number of pointers within this memory area
        public void **pMem;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct tHeapRoots {
        public uint capacity;
        public uint num;
        public tHeapRootEntry *pHeapEntries;
    }

    public unsafe static class Heap
    {

        private unsafe struct tHeapEntry
        {
            // Left/right links in the heap binary tree
            public fixed /*tHeapEntry*/ PTR pLink[2];
            // The 'level' of this node. Leaf nodes have lowest level
            public byte level;
            // Used to mark that this node is still in use.
            // If this is set to 0xff, then this heap entry is undeletable.
            public byte marked;
            // Set to 1 if the Finalizer needs to be run.
            // Set to 2 if this has been added to the Finalizer queue
            // Set to 0 when the Finalizer has been run (or there is no Finalizer in the first place)
            // Only set on type that have a Finalizer
            public byte needToFinalize;

            // unused
            public byte padding;

            // The type in this heap entry
            public tMD_TypeDef *pTypeDef;

            // Used for locking sync, and tracking WeakReference that point to this object
            public tSync *pSync;

            // The user memory
            public fixed byte memory[1];
        }

        // Get the tHeapEntry pointer when given a /*HEAP_PTR*/byte* object
        static tHeapEntry* GET_HEAPENTRY(byte* heapObj) 
        {
            return ((tHeapEntry*)(heapObj - sizeof(tHeapEntry)));
        }

        static tHeapEntry *pHeapTreeRoot;
        static tHeapEntry *nil;
        const int MAX_TREE_DEPTH = 40;

        // The total heap memory currently allocated
        static SIZE_T trackHeapSize;
        // The max heap size allowed before a garbage collection is triggered
        static SIZE_T heapSizeMax;
        // The number of allocated memory nodes
        static uint numNodes = 0;
        // The number of collections done
        static uint numCollections = 0;

        #if DIAG_GC
        // Track how much time GC's are taking
        static ulong gcTotalTime = 0;
        #endif

        const int MIN_HEAP_SIZE = 50000;
        const int MAX_HEAP_EXCESS = 200000;

        public static void Heap_Init() 
        {
        	// Initialise vars
        	trackHeapSize = 0;
        	heapSizeMax = MIN_HEAP_SIZE;
        	// Create nil node - for leaf termination
            nil = ((tHeapEntry*)Mem.mallocForever((SIZE_T)sizeof(tHeapEntry)));
            Mem.memset(nil, 0, (SIZE_T)sizeof(tHeapEntry));
            nil->pLink[0] = nil->pLink[1] = (PTR)nil;
        	// Set the heap tree as empty
        	pHeapTreeRoot = nil;
        }

        // Get the size of a heap entry, NOT including the header
        // This works by returning the size of the type, unless the type is an array or a string,
        // which are the only two type that can have variable sizes
        static uint GetSize(tHeapEntry *pHeapEntry) 
        {
        	tMD_TypeDef *pType = pHeapEntry->pTypeDef;
        	if (pType == Type.types[Type.TYPE_SYSTEM_STRING]) {
        		// If it's a string, return the string length in bytes
        		return SystemString.GetNumBytes((/*HEAP_PTR*/byte*)(pHeapEntry + 1));
        	}
        	if (MetaData.TYPE_ISARRAY(pType)) {
        		// If it's an array, return the array length * array element size
        		return SystemArray.GetNumBytes((/*HEAP_PTR*/byte*)(pHeapEntry + 1), pType->pArrayElementType);
        	}
        	// If it's not string or array, just return the instance memory size
        	return pType->instanceMemSize;
        }

        static tHeapEntry* TreeSkew(tHeapEntry *pRoot) 
        {
            if (((tHeapEntry*)pRoot->pLink[0])->level == pRoot->level && pRoot->level != 0) {
                tHeapEntry *pSave = (tHeapEntry*)pRoot->pLink[0];
        		pRoot->pLink[0] = pSave->pLink[1];
                pSave->pLink[1] = (PTR)pRoot;
        		pRoot = pSave;
        	}
        	return pRoot;
        }

        static tHeapEntry* TreeSplit(tHeapEntry *pRoot) 
        {
            if (((tHeapEntry*)((tHeapEntry*)pRoot->pLink[1])->pLink[1])->level == pRoot->level && pRoot->level != 0) {
                tHeapEntry *pSave = (tHeapEntry*)pRoot->pLink[1];
        		pRoot->pLink[1] = pSave->pLink[0];
                pSave->pLink[0] = (PTR)pRoot;
        		pRoot = pSave;
        		pRoot->level++;
        	}
        	return pRoot;
        }

        static tHeapEntry* TreeInsert(tHeapEntry *pRoot, tHeapEntry *pEntry) 
        {
        	if (pRoot == nil) {
        		pRoot = pEntry;
        		pRoot->level = 1;
                pRoot->pLink[0] = pRoot->pLink[1] = (PTR)nil;
        		pRoot->marked = 0;
        	} else {
        		tHeapEntry* pNode = pHeapTreeRoot;
                tHeapEntry*[] pUp = new tHeapEntry*[MAX_TREE_DEPTH];
        		int top = 0, dir;
        		// Find leaf position to insert into tree. This first step is unbalanced
        		for (;;) {
        			pUp[top++] = pNode;
                    dir = ((PTR)pNode < (PTR)pEntry) ? 1 : 0; // 0 for left, 1 for right
                    if (pNode->pLink[dir] == (PTR)nil) {
        				break;
        			}
                    pNode = (tHeapEntry*)pNode->pLink[dir];
        		}
        		// Create new node
                pNode->pLink[dir] = (PTR)pEntry;
        		pEntry->level = 1;
                pEntry->pLink[0] = pEntry->pLink[1] = (PTR)nil;
        		pEntry->marked = 0;
        		// Balance the tree
        		while (--top >= 0) {
        			if (top != 0) {
                        dir = (pUp[top-1]->pLink[1] == (PTR)pUp[top]) ? 1 : 0;
        			}
        			pUp[top] = TreeSkew(pUp[top]);
        			pUp[top] = TreeSplit(pUp[top]);
        			if (top != 0) {
                        pUp[top-1]->pLink[dir] = (PTR)pUp[top];
        			} else {
        				pRoot = pUp[0];
        			}
        		}
        	}
        	return pRoot;
        }

        static tHeapEntry* TreeRemove(tHeapEntry *pRoot, tHeapEntry *pDelete) 
        {
        	if (pRoot != nil) {
        		if (pRoot == pDelete) {
                    if (pRoot->pLink[0] != (PTR)nil && pRoot->pLink[1] != (PTR)nil) {
        				tHeapEntry *pL0;
        				byte l;
                        tHeapEntry *pHeir = (tHeapEntry*)pRoot->pLink[0];
                        tHeapEntry **ppHeirLink = (tHeapEntry **)&pHeir->pLink[0];
                        while (pHeir->pLink[1] != (PTR)nil) {
                            ppHeirLink = (tHeapEntry **)&pHeir->pLink[1];
                            pHeir = (tHeapEntry *)pHeir->pLink[1];
        				}
        				// Swap the two nodes
                        pL0 = (tHeapEntry *)pHeir->pLink[0];
        				l = pHeir->level;
        				// Bring heir to replace root
        				pHeir->pLink[0] = pRoot->pLink[0];
        				pHeir->pLink[1] = pRoot->pLink[1];
        				pHeir->level = pRoot->level;
        				// Send root to replace heir
        				*ppHeirLink = pRoot;
                        pRoot->pLink[0] = (PTR)pL0;
                        pRoot->pLink[1] = (PTR)nil;
        				pRoot->level = l;
        				// Set correct return value
        				pL0 = pRoot;
        				pRoot = pHeir;
        				// Delete the node that's been sent down
                        pRoot->pLink[0] = (PTR)TreeRemove((tHeapEntry*)pRoot->pLink[0], pL0);
        			} else {
                        pRoot = (tHeapEntry*)pRoot->pLink[pRoot->pLink[0] == (PTR)nil ? 1 : 0];
        			}
        		} else {
                    int dir = (PTR)pRoot < (PTR)pDelete ? 1 : 0;
                    pRoot->pLink[dir] = (PTR)TreeRemove((tHeapEntry*)pRoot->pLink[dir], pDelete);
        		}
        	}

            if (((tHeapEntry*)pRoot->pLink[0])->level < pRoot->level-1 || ((tHeapEntry*)pRoot->pLink[1])->level < pRoot->level-1) {
                if (((tHeapEntry*)pRoot->pLink[1])->level > --pRoot->level) {
                    ((tHeapEntry*)pRoot->pLink[1])->level = pRoot->level;
        		}
        		pRoot = TreeSkew(pRoot);
                pRoot->pLink[1] = (PTR)TreeSkew((tHeapEntry*)pRoot->pLink[1]);
                ((tHeapEntry*)pRoot->pLink[1])->pLink[1] = (PTR)TreeSkew((tHeapEntry*)((tHeapEntry*)pRoot->pLink[1])->pLink[1]);
        		pRoot = TreeSplit(pRoot);
                pRoot->pLink[1] = (PTR)TreeSplit((tHeapEntry*)pRoot->pLink[1]);
        	}

        	return pRoot;
        }

        public static void GarbageCollect() 
        {
        	tHeapRoots heapRoots;
        	tHeapEntry* pNode;
            tHeapEntry*[] pUp = new tHeapEntry*[MAX_TREE_DEPTH * 2];
        	int top;
        	tHeapEntry *pToDelete = null;
        	SIZE_T orgHeapSize = trackHeapSize;
        	uint orgNumNodes = numNodes;
        #if DIAG_GC
        	ulong startTime;
        #endif

        	numCollections++;

        #if DIAG_GC
        	startTime = microTime();
        #endif

        	heapRoots.capacity = 64;
        	heapRoots.num = 0;
            heapRoots.pHeapEntries = (tHeapRootEntry*)Mem.malloc(heapRoots.capacity * (SIZE_T)sizeof(tHeapRootEntry));

        	Thread.GetHeapRoots(&heapRoots);
        	CLIFile.GetHeapRoots(&heapRoots);

        	// Mark phase
        	while (heapRoots.num > 0) {
        		tHeapRootEntry *pRootsEntry;
        		uint i;
        		uint moreRootsAdded = 0;
        		uint rootsEntryNumPointers;
        		void **pRootsEntryMem;

        		// Get a piece of memory off the list of heap memory roots.
        		pRootsEntry = &heapRoots.pHeapEntries[heapRoots.num - 1];
        		rootsEntryNumPointers = pRootsEntry->numPointers;
        		pRootsEntryMem = pRootsEntry->pMem;
        		// Mark this entry as done
        		pRootsEntry->numPointers = 0;
        		pRootsEntry->pMem = null;
        		// Iterate through all pointers in it
        		for (i=0; i<rootsEntryNumPointers; i++) {
        			void *pMemRef = pRootsEntryMem[i];
        			// Quick escape for known non-memory 
        			if (pMemRef == null) {
        				continue;
        			}
        			// Find this piece of heap memory in the tracking tree.
        			// Note that the 2nd memory address comparison MUST be >, not >= as might be expected,
        			// to allow for a zero-sized memory to be detected (and not garbage collected) properly.
        			// E.g. The object class has zero memory.
        			pNode = pHeapTreeRoot;
        			while (pNode != nil) {
        				if (pMemRef < (void*)pNode) {
                            pNode = (tHeapEntry*)pNode->pLink[0];
        				} else if ((byte*)pMemRef > ((byte*)pNode) + GetSize(pNode) + sizeof(tHeapEntry)) {
                            pNode = (tHeapEntry*)pNode->pLink[1];
        				} else {
        					// Found memory. See if it's already been marked.
        					// If it's already marked, then don't do anything.
        					// It it's not marked, then add all of its memory to the roots, and mark it.
        					if (pNode->marked == 0) {
        						tMD_TypeDef *pType = pNode->pTypeDef;

        						// Not yet marked, so mark it, and add it to heap roots.
        						pNode->marked = 1;
        	
        						// Don't look at the contents of strings, arrays of primitive Type.types, or WeakReferences
        						if (pType->stackType == EvalStack.EVALSTACK_O ||
        							pType->stackType == EvalStack.EVALSTACK_VALUETYPE ||
        							pType->stackType == EvalStack.EVALSTACK_PTR) {

        							if (pType != Type.types[Type.TYPE_SYSTEM_STRING] &&
        								(!MetaData.TYPE_ISARRAY(pType) ||
        								pType->pArrayElementType->stackType == EvalStack.EVALSTACK_O ||
        								pType->pArrayElementType->stackType == EvalStack.EVALSTACK_VALUETYPE ||
        								pType->pArrayElementType->stackType == EvalStack.EVALSTACK_PTR)) {

        								if (pType != Type.types[Type.TYPE_SYSTEM_WEAKREFERENCE]) {
        									Heap.SetRoots(&heapRoots,pNode->memory, GetSize(pNode));
        									moreRootsAdded = 1;
        								}
        							}
        						}
        					}
        					break;
        				}
        			}
        		}
        		if (moreRootsAdded == 0) {
        			heapRoots.num--;
        		}
        	}

        	Mem.free(heapRoots.pHeapEntries);

        	// Sweep phase
        	// Traverse nodes
        	pUp[0] = pHeapTreeRoot;
        	top = 1;
        	while (top != 0) {
        		// Get this node
        		pNode = pUp[--top];
        		// Act on this node
        		if (pNode->marked != 0) {
        			if (pNode->marked != 0xff) {
        				// Still in use (but not marked undeletable), so unmark
        				pNode->marked = 0;
        			}
        		} else {
        			// Not in use any more, so put in deletion queue if it does not need Finalizing
        			// If it does need Finalizing, then don't garbage collect, and put in Finalization queue.
        			if (pNode->needToFinalize != 0) {
        				if (pNode->needToFinalize == 1) {
        					Finalizer.AddFinalizer((/*HEAP_PTR*/byte*)pNode + sizeof(tHeapEntry));
        					// Mark it has having been placed in the finalization queue.
        					// When it has been finalized, then this will be set to 0
        					pNode->needToFinalize = 2;
        					// If this object is being targetted by weak-ref(s), handle it
        					if (pNode->pSync != null) {
        						RemoveWeakRefTarget(pNode, 0);
        						Mem.free(pNode->pSync);
        					}
        				}
        			} else {
        				// If this object is being targetted by weak-ref(s), handle it
        				if (pNode->pSync != null) {
        					RemoveWeakRefTarget(pNode, 1);
        					Mem.free(pNode->pSync);
        				}
        				// Use pSync to point to next entry in this linked-list.
                        pNode->pSync = (tSync*)pToDelete;
        				pToDelete = pNode;
        			}
        		}
        		// Get next node(s)
                if (pNode->pLink[1] != (PTR)nil) {
                    pUp[top++] = (tHeapEntry*)pNode->pLink[1];
        		}
                if (pNode->pLink[0] != (PTR)nil) {
                    pUp[top++] = (tHeapEntry*)pNode->pLink[0];
        		}
        	}

        	// Delete all unused memory nodes.
        	while (pToDelete != null) {
        		tHeapEntry *pThis = pToDelete;
        		pToDelete = (tHeapEntry*)(pToDelete->pSync);
        		pHeapTreeRoot = TreeRemove(pHeapTreeRoot, pThis);
        		numNodes--;
                trackHeapSize -= GetSize(pThis) + (uint)sizeof(tHeapEntry);
        		Mem.free(pThis);
        	}

        #if DIAG_GC
        	gcTotalTime += microTime() - startTime;
        #endif

        	Sys.log_f(1, "--- GARBAGE --- [Size: %d -> %d] [Nodes: %d -> %d]\n",
        		orgHeapSize, trackHeapSize, orgNumNodes, numNodes);

        #if DIAG_GC
        	Sys.log_f(1, "GC time = %d ms\n", gcTotalTime / 1000);
        #endif
        }

        public static void UnmarkFinalizer(/*HEAP_PTR*/byte* heapPtr) 
        {
        	((tHeapEntry*)(heapPtr - sizeof(tHeapEntry)))->needToFinalize = 0;
        }

        public static uint NumCollections() 
        {
        	return numCollections;
        }

        public static SIZE_T GetTotalMemory() 
        {
        	return trackHeapSize;
        }

        public static void SetRoots(tHeapRoots *pHeapRoots, void *pRoots, uint sizeInBytes) 
        {
        	tHeapRootEntry *pRootEntry;

            UnityEngine.Assertions.Assert.IsTrue((sizeInBytes & 0x3) == 0);
        	if (pHeapRoots->num >= pHeapRoots->capacity) {
        		pHeapRoots->capacity <<= 1;
                pHeapRoots->pHeapEntries = (tHeapRootEntry*)Mem.realloc(pHeapRoots->pHeapEntries, (SIZE_T)(pHeapRoots->capacity * sizeof(tHeapRootEntry)));
        	}
        	pRootEntry = &pHeapRoots->pHeapEntries[pHeapRoots->num++];
        	pRootEntry->numPointers = sizeInBytes >> 2;
            pRootEntry->pMem = (void**)pRoots;
        }

        public static /*HEAP_PTR*/byte* Alloc(tMD_TypeDef *pTypeDef, uint size) 
        {
        	tHeapEntry *pHeapEntry;
        	uint totalSize;

            totalSize = (uint)sizeof(tHeapEntry) + size;

        	// Trigger garbage collection if required.
        	if (trackHeapSize >= heapSizeMax) {
        		GarbageCollect();
        		heapSizeMax = (trackHeapSize + totalSize) << 1;
        		if (heapSizeMax < trackHeapSize + totalSize + MIN_HEAP_SIZE) {
        			// Make sure there is always MIN_HEAP_SIZE available to allocate on the heap
        			heapSizeMax = trackHeapSize + totalSize + MIN_HEAP_SIZE;
        		}
        		if (heapSizeMax > trackHeapSize + totalSize + MAX_HEAP_EXCESS) {
        			// Make sure there is never more that MAX_HEAP_EXCESS space on the heap
        			heapSizeMax = trackHeapSize + totalSize + MAX_HEAP_EXCESS;
        		}
        	}

        	pHeapEntry = (tHeapEntry*)Mem.malloc(totalSize);
        	pHeapEntry->pTypeDef = pTypeDef;
        	pHeapEntry->pSync = null;
            pHeapEntry->needToFinalize = (byte)((pTypeDef->pFinalizer != null) ? 1 : 0);
        	Mem.memset(&pHeapEntry->memory[0], 0, size);
        	trackHeapSize += totalSize;

        	pHeapTreeRoot = TreeInsert(pHeapTreeRoot, pHeapEntry);
        	numNodes++;

        	return pHeapEntry->memory;
        }

        public static /*HEAP_PTR*/byte* AllocType(tMD_TypeDef *pTypeDef) 
        {
        	//printf("Heap.AllocType('%s')\n", pTypeDef->name);
        	return Alloc(pTypeDef, pTypeDef->instanceMemSize);
        }

        public static tMD_TypeDef* GetType(/*HEAP_PTR*/byte* heapEntry) 
        {
        	tHeapEntry *pHeapEntry = GET_HEAPENTRY(heapEntry);
        	return pHeapEntry->pTypeDef;
        }

        public static void MakeUndeletable(/*HEAP_PTR*/byte* heapEntry) 
        {
        	tHeapEntry *pHeapEntry = GET_HEAPENTRY(heapEntry);
        	pHeapEntry->marked = 0xff;
        }

        public static void MakeDeletable(/*HEAP_PTR*/byte* heapEntry) 
        {
        	tHeapEntry *pHeapEntry = GET_HEAPENTRY(heapEntry);
        	pHeapEntry->marked = 0;
        }

        public static /*HEAP_PTR*/byte* Box(tMD_TypeDef *pType, byte* pMem) 
        {
        	/*HEAP_PTR*/byte* boxed;

        	boxed = AllocType(pType);
            Mem.memcpy((void*)boxed, (void*)pMem, pType->instanceMemSize);

        	return boxed;
        }

        public static /*HEAP_PTR*/byte* Clone(/*HEAP_PTR*/byte* obj) 
        {
        	tHeapEntry *pObj = GET_HEAPENTRY(obj);
        	/*HEAP_PTR*/byte* clone;
        	uint size = GetSize(pObj);

        	clone = Alloc(pObj->pTypeDef, size);
            Mem.memcpy((void*)clone, (void*)pObj->memory, size);

        	return clone;
        }

        static tSync* EnsureSync(tHeapEntry *pHeapEntry) 
        {
        	if (pHeapEntry->pSync == null) {
                tSync *pSync = ((tSync*)Mem.malloc((SIZE_T)sizeof(tSync)));
                Mem.memset(pSync, 0, (SIZE_T)sizeof(tSync));
        		pHeapEntry->pSync = pSync;
        	}
        	return pHeapEntry->pSync;
        }

        static void DeleteSync(tHeapEntry *pHeapEntry) 
        {
        	if (pHeapEntry->pSync != null) {
        		if (pHeapEntry->pSync->count == 0 && pHeapEntry->pSync->weakRef == null) {
        			Mem.free(pHeapEntry->pSync);
        			pHeapEntry->pSync = null;
        		}
        	}
        }

        // Return 1 if lock succesfully got
        // Return 0 if couldn't get the lock this time
        public static uint SyncTryEnter(/*HEAP_PTR*/byte* obj) 
        {
        	tHeapEntry *pHeapEntry = GET_HEAPENTRY(obj);
        	tThread *pThread = Thread.GetCurrent();
        	tSync *pSync;

        	pSync = EnsureSync(pHeapEntry);
        	if (pSync->pThread == null) {
        		pSync->pThread = pThread;
        		pSync->count = 1;
        		return 1;
        	}
        	if (pSync->pThread == pThread) {
        		pSync->count++;
        		return 1;
        	}
        	return 0;
        }

        // Returns 1 if all is OK
        // Returns 0 if the wrong thread is releasing the sync, or if no thread hold the sync
        public static uint SyncExit(/*HEAP_PTR*/byte* obj) {
        	tHeapEntry *pHeapEntry = GET_HEAPENTRY(obj);
        	tThread *pThread = Thread.GetCurrent();
        	if (pHeapEntry->pSync == null) {
        		return 0;
        	}
        	if (pHeapEntry->pSync->pThread != pThread) {
        		return 0;
        	}
        	if (--pHeapEntry->pSync->count == 0) {
        		DeleteSync(pHeapEntry);
        	}
        	return 1;
        }

        static void RemoveWeakRefTarget(tHeapEntry *pTarget, uint removeLongRefs) {
        	SystemWeakReference.TargetGone(&pTarget->pSync->weakRef, removeLongRefs);
        }

        // Returns the previous first weak-ref in target targetted by weakref
        public static /*HEAP_PTR*/byte* SetWeakRefTarget(/*HEAP_PTR*/byte* target, /*HEAP_PTR*/byte* weakRef) {
        	tHeapEntry *pTarget = GET_HEAPENTRY(target);
        	tSync *pSync;
        	/*HEAP_PTR*/byte* prevWeakRef;

        	pSync = EnsureSync(pTarget);
        	prevWeakRef = pSync->weakRef;
        	pSync->weakRef = weakRef;
        	return prevWeakRef;
        }

        public static /*HEAP_PTR*/byte** GetWeakRefAddress(/*HEAP_PTR*/byte* target) {
        	tHeapEntry *pTarget = GET_HEAPENTRY(target);
        	return &pTarget->pSync->weakRef;
        }

        public static void RemovedWeakRefTarget(/*HEAP_PTR*/byte* target) {
        	tHeapEntry *pTarget = GET_HEAPENTRY(target);
        	DeleteSync(pTarget);
        }

    }

}
