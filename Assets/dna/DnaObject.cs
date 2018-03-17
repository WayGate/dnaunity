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
            if (dnaObjects != null) { 
                foreach (KeyValuePair<PTR,System.WeakReference> pair in dnaObjects) {
                    DnaObject obj = pair.Value.Target as DnaObject;
                    if (obj != null) {
                        obj.dnaPtr = null;
                    }
                }
            }
            dnaObjects = null;
        }

        public static DnaObject CreateInstance(tMD_TypeDef* pTypeDef, object monoBaseObject = null)
        {
            Mem.heapcheck();
            byte* pPtr = null;
            if (monoBaseObject != null)
                pPtr = Heap.AllocMonoObject(pTypeDef, monoBaseObject);
            else
                pPtr = Heap.AllocType(pTypeDef);
            Mem.heapcheck();
            return WrapObject(pPtr);
        }

        public static DnaObject WrapObject(byte* pPtr)
        {
            if (pPtr == null)
                return null;

            System.WeakReference weak;
            DnaObject obj = null;
            if (dnaObjects.TryGetValue((PTR)pPtr, out weak)) {
                obj = weak.Target as DnaObject;
                if (obj != null)
                    return obj;
            }

            obj = new DnaObject(pPtr);
            dnaObjects.Add((PTR)pPtr, new System.WeakReference(obj));

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

            if (deadRefs != null) {
                for (int i = 0; i < deadRefs.Count; i++) {
                    dnaObjects.Remove(deadRefs[i]);
                }
            }
        }

        public DnaObject(byte* ptr)
        {
            Heap.MakeUndeletable(ptr);
            dnaPtr = ptr;
        }

        /// <summary>
        /// Gets the type def for this object.
        /// </summary>
        /// <returns></returns>
        ulong GetTypeDef()
        {
            if (dnaPtr == null)
                throw new System.NullReferenceException();
            return (ulong)Heap.GetType(dnaPtr);
        }

        /// <summary>
        /// Returns a method def given method name and argument types for this object.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        public ulong FindMethod(string methodName, System.Type[] argTypes)
        {
            if (dnaPtr == null)
                throw new System.NullReferenceException();
            tMD_TypeDef* pTypeDef = Heap.GetType(dnaPtr);
            return Dna.FindMethod((ulong)pTypeDef, methodName, argTypes);
        }

        /// <summary>
        /// Call to a DNA method given it's method def.
        /// </summary>
        /// <param name="methodDef">The method def</param>
        /// <param name="args">The arguments to pass (or null for no arguments)</param>
        /// <returns>The value returned by the method.</returns>
        public object Call(ulong methodDef, object[] args = null)
        {
            if (dnaPtr == null)
                throw new System.NullReferenceException();
            tMD_TypeDef* pTypeDef = Heap.GetType(dnaPtr);
            return Dna.Call(methodDef, this, args);
        }

        /// <summary>
        /// Call a method on a DNA type with a typedef and method name.
        /// </summary>
        /// <param name="typeDef">Pointer to the type def</param>
        /// <param name="methodName">The name of the method</param>
        /// <param name="argTypes">The argument types</param>
        /// <param name="args">The arguments to pass (or null for no arguments)</param>
        /// <returns>The value returned by the method.</returns>
        public object Call(string methodName, System.Type[] argTypes = null, object[] args = null)
        {
            if (dnaPtr == null)
                throw new System.NullReferenceException();
            tMD_TypeDef* pTypeDef = Heap.GetType(dnaPtr);
            return Dna.Call((ulong)pTypeDef, methodName, argTypes, this, args);
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