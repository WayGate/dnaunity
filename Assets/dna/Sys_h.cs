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

const int FAKE_RETURN exit(101)

const int INTERNALCALL_PARAM(ofs, type) (*(type*)(pParams + ofs))
const int INTERNALCALL_RETURN_U32(val) *(U32*)pReturnValue = (val)
const int INTERNALCALL_RETURN_I32(val) *(I32*)pReturnValue = (val)
const int INTERNALCALL_RETURN_PTR(val) *(void**)pReturnValue = (void*)(val)

const int S_PTR PTR_SIZE
const int S_INT (4)


void Crash(char *pMsg, ...);

extern U32 logLevel;
void log_f(U32 level, char *pMsg, ...);

char* Sys_GetMethodDesc(tMD_MethodDef *pMethod);

void* mallocForever(U32 size);

U64 msTime();
void SleepMS(U32 ms);

#endif
