﻿namespace DnaUnity
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
            if (s != null)
            {
                byte* p = _s = (byte*)Mem.malloc((SIZE_T)s.Length + 1);
                for (int i = 0; i < s.Length; i++)
                {
                    *p = (byte)s[i];
                    p++;
                }
                *p = 0;
            }
            else
            {
                _s = null;
            }
        }

        public S(ref byte* sc, string s)
        {
            if (s != null)
            {
                if (sc != null)
                {
                    _s = sc;
                }
                else
                {
                    byte* p = sc = _s = (byte*)Mem.malloc((SIZE_T)s.Length + 1);
                    for (int i = 0; i < s.Length; i++)
                    {
                        *p++ = (byte)s[i];
                    }
                    *p = 0;
                }
            }
            else
            {
                _s = null;
            }
        }

        public static implicit operator byte*(S s)  // explicit byte to ptr conversion operator
        {
            return s._s;
        }

        public static string str(byte* p)
        {
            if (p == null)
                return null;
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)p);
        }

        // check for corrupt string
        public static int isvalidstr(byte* s)
        {
            try {
                if (s == null)
                    return 0;
                while (*s != 0)
                {
                    if (!(*s == '\r' || *s == '\n' || *s == '\t' || (*s >= ' ' && *s <= '~')))
                        return 0;
                    s++;
                }
            } catch (System.Exception) {
                return 0;
            }
            return 1;
        }

        public static byte** buildArray(params string[] args)
        {
            byte** arry = null;
            if (args != null && args.Length > 0)
            {
                arry = (byte**)Mem.malloc((SIZE_T)(sizeof(byte*) * args.Length));
                for (int i = 0; i < args.Length; i++)
                {
                    arry[i] = new S(args[i]);
                }
            }
            return arry;
        }

        public static int strlen(byte* s)
        {
            if (s == null)
                throw new System.ArgumentNullException();
            byte* p = s;
            char ch = (char)*p;
            while (ch != '\x0')
            {
                p++;
                ch = (char)*p;
            }
            return (int)(p - s);
        }

        public static int strcmp(byte* a, byte* b)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            char _a, _b;
            for (;;) 
            {
                _a = (char)*a;
                _b = (char)*b;
                if (_a < _b)
                    return -1;
                else if (_a > _b)
                    return 1;
                else if (_a == '\x0' || _b == '\x0')
                    break;
                a++;
                b++;
            } 
            return 0;
        }

        public static int strcmp(byte* a, string b)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            int i = 0;
            int len = b.Length;
            for (;;)
            {
                char _a = (char)*a;
                char _b = (i < len) ? b[i] : (char)0;
                if (_a < _b)
                    return -1;
                else if (_a > _b)
                    return 1;
                else if (_a == '\x0' || _b == '\x0')
                    break;
                a++;
                i++;
            }
            return 0;
        }

        public static int strncmp(byte* a, byte* b, int len)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            while (len > 0)
            {
                char _a = (char)*a;
                char _b = (char)*b;
                if (_a < _b)
                    return -1;
                else if (_a > _b)
                    return 1;
                else if (_a == '\x0' || _b == '\x0')
                    break;
                a++;
                b++;
                len--;
            }
            return 0;
        }

        public static int strcasecmp(byte* a, string b)
        {
            if (a == null || b == null)
                throw new System.ArgumentNullException();
            int i = 0;
            int len = b.Length;
            char _a, _b;
            for (;;)
            {
                _a = (char)*a;
                if (_a >= 'a' && _a <= 'z')
                    _a -= (char)32;
                _b = (i < len) ? b[i] : (char)0;
                if (_b >= 'a' && _b <= 'z')
                    _b -= (char)32;
                if (_a < _b)
                    return -1;
                else if (_a > _b)
                    return 1;
                else if (_a == '\x0' || _b == '\x0')
                    break;
                a++;
                i++;
            }
            return 0;
        }

        public static byte* strncpy(byte* a, byte* b, int size)
        {
            Mem.heapcheck();
            if (a == null || b == null)
                throw new System.NullReferenceException();
            byte* dst = a;
            byte* end = a + (size - 1);
            while (*b != 0)
            {
                if (a >=  end)
                    throw new System.IndexOutOfRangeException();                
                *a++ = *b++;
            }
            *a = 0;
            Mem.heapcheck();
            return dst;
        }

        public static byte* strncpy(byte* a, string b, int size)
        {
            Mem.heapcheck();
            if (a == null || b == null)
                throw new System.NullReferenceException();
            byte* dst = a;
            byte* end = a + (size - 1);
            int len = b.Length;
            for (int i = 0; i < len; i++)
            {
                if (a >=  end)
                    throw new System.IndexOutOfRangeException();                
                *a++ = (byte)b[i];
            }
            *a = 0;
            Mem.heapcheck();
            return dst;
        }

        public static byte* strncat(byte* a, byte* b, int size)
        {
            Mem.heapcheck();
            if (a == null || b == null)
                throw new System.NullReferenceException();
            byte* dst = a;
            int len = strlen(a);
            if (len + 1 >= size)
                throw new System.IndexOutOfRangeException();
            strncpy(a + len, b, size - len);
            Mem.heapcheck();
            return dst;
        }

        public static byte* strncat(byte* a, string b, int size)
        {
            Mem.heapcheck();
            if (a == null || b == null)
                throw new System.NullReferenceException();
            byte* dst = a;
            int len = strlen(a);
            if (len + 1 >= size)
                throw new System.IndexOutOfRangeException();
            strncpy(a + len, b, size - len);
            Mem.heapcheck();
            return dst;
        }

        public static byte* strdup(byte* s)
        {
            Mem.heapcheck();
            int len = strlen(s) + 1;
            byte* p = (byte*)Mem.malloc((SIZE_T)len);
            strncpy(p, s, len);
            return p;
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

        public static byte* scatprintf(byte* bfr, byte* end, string fmt, params object[] args)
        {
            if (bfr == null || fmt == null)
                throw new System.NullReferenceException();
            Mem.heapcheck();
            int curarg = 0;
            int i = 0;
            int fmtLen = fmt.Length;
            byte* b = bfr;
            byte* e = end;
            while (i < fmtLen) {
                char ch = fmt[i];
                if (ch == '%' && i + 1 < fmtLen) {
                    i++;
                    ch = fmt[i];
                    if (ch == 's') {
                        object sarg = args[curarg];
                        curarg++;
                        if (sarg == null) {
                            if (b + 4 > e)
                                throw new System.IndexOutOfRangeException();
                            *b++ = (byte)'n';
                            *b++ = (byte)'u';
                            *b++ = (byte)'l';
                            *b++ = (byte)'l';
                        } else if (sarg is string) {
                            string str = (string)sarg;
                            for (int j = 0; j < str.Length; j++) {
                                if (b >= e)
                                    throw new System.IndexOutOfRangeException();
                                *b++ = (byte)str[j];
                            }
                        } else if (sarg is PTR) {
                            byte* s = (byte*)((PTR)sarg);
                            if (s == null) {
                                if (b + 4 >= e)
                                    throw new System.IndexOutOfRangeException();
                                *b++ = (byte)'n';
                                *b++ = (byte)'u';
                                *b++ = (byte)'l';
                                *b++ = (byte)'l';
                            } else {
                                while (*s != 0) {
                                    if (b >= e)
                                        throw new System.IndexOutOfRangeException();
                                    *b++ = *s++;
                                }
                            }
                        } else {
                            throw new System.ArgumentException();
                        }
                    } else if (ch == 'x' || ch == 'X' || ch == 'd') {
                        int v;
                        object arg = args[curarg];
                        if (arg is int)
                            v = (int)arg;
                        else if (arg is uint)
                            v = (int)(uint)arg;
                        else if (arg is long)
                            v = (int)(long)arg;
                        else if (arg is ulong)
                            v = (int)(ulong)arg;
                        else
                            v = System.Convert.ToInt32(arg);
                        curarg++;
                        string vs = (ch == 'x' || ch == 'X' ? v.ToString("X") : v.ToString());
                        for (int j = 0; j < vs.Length; j++) {
                            if (b >= e)
                                throw new System.IndexOutOfRangeException();
                            *b++ = (byte)vs[j];
                        }
                    } else if (ch == 'l' && i + 2 < fmtLen && fmt[i + 1] == 'l' && (fmt[i + 2] == 'x' || fmt[i + 2] == 'X' || fmt[i + 2] == 'd')) {
                        i += 2;
                        ch = fmt[i];
                        long lv = System.Convert.ToInt64(args[curarg]);
                        curarg++;
                        string lvs = (ch == 'x' || ch == 'X' ? lv.ToString("X") : lv.ToString());
                        for (int j = 0; j < lvs.Length; j++) {
                            if (b >= e)
                                throw new System.IndexOutOfRangeException();
                            *b++ = (byte)lvs[j];
                        }
                    } else if (ch == '0' && i + 2 < fmtLen && (fmt[i + 1] >= '0' && fmt[i + 1] <= '9') && (fmt[i + 2] == 'x' || fmt[i + 2] == 'X' || fmt[i + 2] == 'd')) {
                        char l0 = fmt[i + 1];
                        i += 2;
                        ch = fmt[i];
                        int v0;
                        object arg = args[curarg];
                        if (arg is int)
                            v0 = (int)arg;
                        else if (arg is uint)
                            v0 = (int)(uint)arg;
                        else if (arg is long)
                            v0 = (int)(long)arg;
                        else if (arg is ulong)
                            v0 = (int)(ulong)arg;
                        else
                            v0 = System.Convert.ToInt32(arg);
                        curarg++;
                        string vs0 = (ch == 'x' || ch == 'X' ? v0.ToString("X" + l0) : v0.ToString("D" + l0));
                        for (int j = 0; j < vs0.Length; j++) {
                            if (b >= e)
                                throw new System.IndexOutOfRangeException();
                            *b++ = (byte)vs0[j];
                        }
                    } else {
                        if (b >= e)
                            throw new System.IndexOutOfRangeException();
                        *b++ = (byte)ch;
                    }
                } else {
                    if (b >= e)
                        throw new System.IndexOutOfRangeException();
                    *b++ = (byte)ch;
                }
                i++;
            }

            // Null terminator
            *b = 0;

            Mem.heapcheck();

            return b;
        }

        public static byte* snprintf(byte* bfr, int len, string fmt, params object[] args)
        {
            scatprintf(bfr, bfr + len, fmt, args);
            return bfr;
        }

    }
}

