namespace DnaUnity
{
    #if UNITY_WEBGL || DNA_32BIT
    using SIZE_T = System.UInt32;
    using PTR = System.UInt32;
    #else
    using SIZE_T = System.UInt64;
    using PTR = System.UInt64;
    #endif

    public unsafe struct S
    {
        public byte* _s;

        public S(string s)
        {
            _s = (byte*)Mem.malloc((SIZE_T)s.Length + 1);
            for (int i = 0; i < s.Length; i++)
            {
                *_s = (byte)s[i];
                _s++;
            }
            *_s = 0;
        }

        public S(ref byte* p, string s)
        {
            if (p != null)
            {
                _s = p;
            }
            else
            {
                _s = (byte*)Mem.malloc((SIZE_T)s.Length + 1);
                p = _s;
            }
            for (int i = 0; i < s.Length; i++)
            {
                *_s = (byte)s[i];
                _s++;
            }
            *_s = 0;
        }

        public static implicit operator byte*(S s)  // explicit byte to digit conversion operator
        {
            throw new System.NotImplementedException();
        }

        public static int strlen(byte* s)
        {
            throw new System.NotImplementedException();
        }

        public static int strcmp(byte* a, byte* b)
        {
            throw new System.NotImplementedException();
        }

        public static int strncmp(byte* a, byte* b, int len)
        {
            throw new System.NotImplementedException();
        }

        public static int strcasecmp(byte* a, byte* b)
        {
            throw new System.NotImplementedException();
        }

        public static int strcpy(byte* a, byte* b)
        {
            throw new System.NotImplementedException();
        }

        public static int strcat(byte* a, byte* b)
        {
            throw new System.NotImplementedException();
        }

        public static byte* sprintf(byte* bfr, string fmt, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public static byte* strchr(byte* s, int ch)
        {
            throw new System.NotImplementedException();
        }
    }
}

