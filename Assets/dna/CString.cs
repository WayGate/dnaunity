namespace DnaUnity
{
    #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
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
            if (s == null)
                throw new System.ArgumentNullException();   
            _s = (byte*)Mem.malloc((SIZE_T)s.Length + 1);
            for (int i = 0; i < s.Length; i++)
            {
                *_s = (byte)s[i];
                _s++;
            }
            *_s = 0;
        }

        public S(ref byte* sc, string s)
        {
            if (s == null)
                throw new System.ArgumentNullException();   
            if (sc != null)
            {
                _s = sc;
            }
            else
            {
                byte* p = _s = (byte*)Mem.malloc((SIZE_T)s.Length + 1);
                sc = _s;
                for (int i = 0; i < s.Length; i++)
                {
                    *p++ = (byte)s[i];
                }
                *p = 0;
            }
        }

        public static implicit operator byte*(S s)  // explicit byte to digit conversion operator
        {
            return s._s;
        }

        public static byte** buildArray(params string[] args)
        {
            byte** arry = (byte**)Mem.malloc((SIZE_T)(sizeof(byte*) * args.Length));
            for (int i = 0; i < args.Length; i++)
            {
                arry[i] = new S(args[i]);
            }
            return arry;
        }

        public static int strlen(byte* s)
        {
            if (s == null)
                throw new System.ArgumentNullException();
            int len = 0;
            while (*s != 0)
            {
                s++;
                len++;
            }
            return len;
        }

        public static int strcmp(byte* a, byte* b)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            do 
            {
                if (*a < *b)
                    return -1;
                else if (*a > *b)
                    return 1;
            } while (*a != 0 && *b != 0);
            return 0;
        }

        public static int strncmp(byte* a, byte* b, int len)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            while (len > 0) 
            {
                if (*a < *b)
                    return -1;
                else if (*a > *b)
                    return 1;
                if (*a == 0 || *b == 0)
                    break;
                len--;
            }
            return 0;
        }

        public static int strcasecmp(byte* a, byte* b)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            do 
            {
                byte _a = *a;
                if (_a >= 'a' && _a <= 'z')
                    _a -= 32;
                byte _b = *b;
                if (_b >= 'a' && _b <= 'z')
                    _b -= 32;
                if (_a < _b)
                    return -1;
                else if (_a > _b)
                    return 1;
            } while (*a != 0 && *b != 0);
            return 0;
        }

        public static byte* strcpy(byte* a, byte* b)
        {
            if (a == null || b == null)
                throw new System.NullReferenceException();
            byte* dst = a;
            while (*b != 0)
            {
                *a++ = *b++;
            }
            *a = 0;
            return dst;
        }

        public static byte* strcat(byte* a, byte* b)
        {
            if (a == null || b == null)
                throw new System.NullReferenceException();
            byte* dst = a;
            int len = strlen(a);
            strcpy(a + len, b);
            return dst;
        }

        public static byte* strchr(byte* s, int ch)
        {
            if (s == null)
                throw new System.NullReferenceException();                 
            byte b = (byte)ch;
            while (*s != 0)
            {
                if (*s == b)
                    return s;
                s++;
            }
            return *s == b ? s : null;
        }

        public static byte* sprintf(byte* bfr, string fmt, params object[] args)
        {
            if (bfr == null || fmt == null)
                throw new System.NullReferenceException(); 
            int curarg = 0;
            int i = 0;
            int fmtLen = fmt.Length;
            byte* b = bfr;
            while (i < fmtLen)
            {
                char ch = fmt[i];
                if (ch == '%' && i + 1 < fmtLen)
                {
                    i++;
                    ch = fmt[i];
                    if (ch == 's')
                    {
                        byte* s = (byte*)(PTR)args[curarg];
                        curarg++;
                        if (s == null)
                        {
                            *b++ = (byte)'n';
                            *b++ = (byte)'u';
                            *b++ = (byte)'l';
                            *b++ = (byte)'l';
                        }
                        else
                        {
                            while (*s != 0)
                            {
                                *b++ = *s++;
                            }
                        }
                    } 
                    else if (ch == 'x' || ch == 'd')
                    {
                        int v = (int)args[curarg];
                        curarg++;
                        string vs = (ch == 'x' ? v.ToString("X") : v.ToString());
                        for (int j = 0; j < vs.Length; j++)
                        {
                            *b++ = (byte)vs[j];
                        }
                    }
                    else if (ch == 'l' && i + 2 < fmtLen && fmt[i + 1] == 'l' && (fmt[i + 2] == 'x' || fmt[i + 2] == 'd'))
                    {
                        i += 2;
                        ch = fmt[i];
                        long lv = (long)args[curarg];
                        curarg++;
                        string lvs = (ch == 'x' ? lv.ToString("X") : lv.ToString());
                        for (int j = 0; j < lvs.Length; j++)
                        {
                            *b++ = (byte)lvs[j];
                        }
                    }
                    else if (ch == '0' && i + 2 < fmtLen && (fmt[i + 1] >= '0' && fmt[i + 1] <= '9') && (fmt[i + 2] == 'x' || fmt[i + 2] == 'd'))
                    {
                        i += 2;
                        char l0 = fmt[i + 1];
                        ch = fmt[i];                        
                        int v0 = (int)args[curarg];
                        curarg++;
                        string vs0 = (ch == 'x' ? v0.ToString("X" + l0) : v0.ToString("D" + l0));
                        for (int j = 0; j < vs0.Length; j++)
                        {
                            *b++ = (byte)vs0[j];
                        }                        
                    }
                    else
                    {
                        *b++ = (byte)ch;
                    }
                }
                else
                {
                    *b++ = (byte)ch;
                }
                i++;
            }

            return bfr;
        }

    }
}

