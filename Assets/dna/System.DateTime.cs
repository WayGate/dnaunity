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

const int TicksPerSecond 10000000L
const int TicksPerMicroSecond 10L
const int TicksAtUnixEpoch 621355968000000000L
const int TicksAtFileTimeEpoch 504911232000000000L

tAsyncCall* System_DateTime_InternalUtcNow(byte* pThis_, byte* pParams, byte* pReturnValue) {

#if WIN32

	FILETIME ft;

	GetSystemTimeAsFileTime(&ft);

	*(ulong*)pReturnValue = ((ulong)ft.dwHighDateTime) * 0x100000000L + ((ulong)ft.dwLowDateTime) + TicksAtFileTimeEpoch;

#else

	struct timeval tp;

	gettimeofday(&tp, null);

	*(ulong*)pReturnValue = ((ulong)tp.tv_sec) * TicksPerSecond + ((ulong)tp.tv_usec) * TicksPerMicroSecond
		+ TicksAtUnixEpoch;

#endif

	return null;
}

#endif
