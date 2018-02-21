using System.Collections;
using System.Collections.Generic;

namespace DnaUnity
{
    public unsafe class DnaObject : System.IDisposable
    {
        public byte* dnaPtr;

        // Flag: Has Dispose already been called?
        bool disposed = false;

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
                // Release reference to DNA object here.
            }

            // Free any unmanaged objects here.
            disposed = true;
        }

        ~DnaObject()
        {
            Dispose(false);
        }
    }

    public struct UnityRef
    {
        public object obj;
        public int typeId;

        // Copy on write id for value type (usually the address on the stack of this obj.  In order to implement proper value
        // type semantics without unecessary copying, will make a copy on any writes where the stack address has changed)
        public uint cowId;
        #if UNITY_WEBGL     // Add dummy padding on 32 bit platforms
        public int dummy;
        #endif
    }

    public unsafe abstract class UnityType
    {
        public abstract void New(byte* parameters, byte* retValue);
        public abstract void Copy(ref UnityRef oldRef, ref UnityRef newRef, uint newCowId);
        public abstract void CallMethod(object obj, int id, uint* cowId, byte* parameters, byte* retValue);
        public abstract void GetField(object obj, int id, byte* outValue);
        public abstract void SetField(object obj, int id, uint* cowId, byte* inValue);
        public abstract void GetProperty(object obj, int id, byte* outValue);
        public abstract void SetProperty(object obj, int id, uint* cowId, byte* inValue);
    }

}