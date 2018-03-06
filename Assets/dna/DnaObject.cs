using System.Collections;
using System.Collections.Generic;

namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif

    public unsafe class DnaObject : System.IDisposable
    {
        private static Dictionary<PTR, System.WeakReference> dnaObjects;

        public byte* dnaPtr;

        // Flag: Has Dispose already been called?
        bool disposed = false;

        private static int _collectionCount;

        public static void Init()
        {
            dnaObjects = new Dictionary<SIZE_T, System.WeakReference>();
            _collectionCount = System.GC.CollectionCount(0);
        }

        public static void Clear()
        {
            // Make sure we clear every single reference to a DNA object that Mono runtime may have.
            foreach (KeyValuePair<PTR,System.WeakReference> pair in dnaObjects) {
                DnaObject obj = pair.Value.Target as DnaObject;
                if (obj != null) {
                    obj.dnaPtr = null;
                }
            }
            dnaObjects = null;
        }

        public static DnaObject MakeDnaObject(byte* ptr)
        {
            if (ptr == null)
                return null;

            System.WeakReference weak;
            DnaObject obj = null;
            if (dnaObjects.TryGetValue((PTR)ptr, out weak)) {
                obj = weak.Target as DnaObject;
                if (obj != null)
                    return obj;
            }

            obj = new DnaObject(ptr);
            dnaObjects.Add((PTR)ptr, new System.WeakReference(obj));

            // If there is a collection - clear dead references
            if (System.GC.CollectionCount(0) != _collectionCount) {
                ClearDeadReferences();
            }

            return obj;
        }

        private static void ClearDeadReferences()
        {
            List<PTR> deadRefs = null;
            foreach (KeyValuePair<PTR, System.WeakReference> pair in dnaObjects) {
                DnaObject obj = pair.Value.Target as DnaObject;
                if (obj == null) {
                    if (deadRefs == null)
                        deadRefs = new List<PTR>();
                    deadRefs.Add(pair.Key);
                }
            }

            for (int i = 0; i < deadRefs.Count; i++) {
                dnaObjects.Remove(deadRefs[i]);
            }
        }

        public DnaObject(byte* ptr)
        {
            Heap.MakeUndeletable(ptr);
            dnaPtr = ptr;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        { 
            Dispose(true);
            System.GC.SuppressFinalize(this);           
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return; 

            if (disposing) {
                Heap.MakeDeletable(dnaPtr);
            }

            // Free any unmanaged objects here.
            disposed = true;
        }

        ~DnaObject()
        {
            Dispose(false);
        }
    }

}