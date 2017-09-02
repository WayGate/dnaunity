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

#if NO

void Crash(char *pMsg, ...) {
	va_list va;

	printf("\n\n*** CRASH ***\n");

	va_start(va, pMsg);

	vprintf(pMsg, va);

	va_end(va);

	printf("\n\n");

#if WIN32
	{
		// Cause a delibrate exception, to get into debugger
		__debugbreak();
	}
#endif

	exit(1);
}

U32 logLevel = 0;

typedef void (*LOG_CALLBACK)(char*);

LOG_CALLBACK logCallback;
char logBuf[256];

void set_log_cb(LOG_CALLBACK cb) {
    logCallback = cb;
    if (cb != NULL) {
        log_f(1, "Log callback successfully set!");
    }
}

void set_log_level(U32 level) {
    logLevel = level;
}

void log_f(U32 level, char *pMsg, ...) {
	va_list va;

	if (logLevel >= level) {
		va_start(va, pMsg);
		vprintf(pMsg, va);
		va_end(va);
        if (logCallback != NULL) {
            va_start(va, pMsg);
            vsnprintf(logBuf, 256, pMsg, va);
            va_end(va);
            logCallback(logBuf);
        }
	}
}

static char methodName[2048];
char* Sys_GetMethodDesc(tMD_MethodDef *pMethod) {
	U32 i;

	sprintf(methodName, "%s.%s.%s(", pMethod->pParentType->nameSpace, pMethod->pParentType->name, pMethod->name);
	for (i=METHOD_ISSTATIC(pMethod)?0:1; i<pMethod->numberOfParameters; i++) {
		if (i > (U32)(METHOD_ISSTATIC(pMethod)?0:1)) {
			sprintf(strchr(methodName, 0), ",");
		}
        tParameter *param = &(pMethod->pParams[i]);
		sprintf(strchr(methodName, 0), "%s", param->pTypeDef->name);
	}
	sprintf(strchr(methodName, 0), ")");
	return methodName;
}

static U32 mallocForeverSize = 0;
// malloc() some memory that will never need to be resized or freed.
void* mallocForever(U32 size) {
	mallocForeverSize += size;
log_f(3, "--- mallocForever: TotalSize %d\n", mallocForeverSize);
	return malloc(size);
}

/*
#if _DEBUG
void* mallocTrace(int s, char *pFile, int line) {
	//printf("MALLOC: %s:%d %d\n", pFile, line, s);
#undef malloc
	return malloc(s);
}
#endif
*/

U64 msTime() {
#if WIN32
	static LARGE_INTEGER freq = {0,0};
	LARGE_INTEGER time;
	if (freq.QuadPart == 0) {
		QueryPerformanceFrequency(&freq);
	}
	QueryPerformanceCounter(&time);
	return (time.QuadPart * 1000) / freq.QuadPart;
#else
	struct timeval tp;
	U64 ms;
	gettimeofday(&tp,NULL);
	ms = tp.tv_sec;
	ms *= 1000;
	ms += ((U64)tp.tv_usec)/((U64)1000);
	return ms;
#endif
}

#if DIAG_METHOD_CALLS || DIAG_OPCODE_TIMES || DIAG_GC || DIAG_TOTAL_TIME
U64 microTime() {
#if WIN32
	static LARGE_INTEGER freq = {0,0};
	LARGE_INTEGER time;
	if (freq.QuadPart == 0) {
		QueryPerformanceFrequency(&freq);
	}
	QueryPerformanceCounter(&time);
	return (time.QuadPart * 1000000) / freq.QuadPart;
#else
	struct timeval tp;
	U64 ms;
	gettimeofday(&tp,NULL);
	ms = tp.tv_sec;
	ms *= 1000000;
	ms += ((U64)tp.tv_usec);
	return ms;
#endif
}
#endif

void SleepMS(U32 ms) {
#if WIN32
	Sleep(ms);
#else
	sleep(ms / 1000);
	usleep((ms % 1000) * 1000);
#endif
}

#endif
