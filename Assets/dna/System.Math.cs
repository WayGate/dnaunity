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

tAsyncCall* System_Math_Sin(byte* pThis_, byte* pParams, byte* pReturnValue) {
	*(double*)pReturnValue = sin((*((double*)(pParams + 0))));

	return null;
}

tAsyncCall* System_Math_Cos(byte* pThis_, byte* pParams, byte* pReturnValue) {
	*(double*)pReturnValue = cos((*((double*)(pParams + 0))));

	return null;
}

tAsyncCall* System_Math_Tan(byte* pThis_, byte* pParams, byte* pReturnValue) {
	*(double*)pReturnValue = tan((*((double*)(pParams + 0))));

	return null;
}

tAsyncCall* System_Math_Pow(byte* pThis_, byte* pParams, byte* pReturnValue) {
	*(double*)pReturnValue = pow((*((double*)(pParams + 0))), (*((double*)(pParams + 8))));

	return null;
}

tAsyncCall* System_Math_Sqrt(byte* pThis_, byte* pParams, byte* pReturnValue) {
	*(double*)pReturnValue = sqrt((*((double*)(pParams + 0))));

	return null;
}

#endif
