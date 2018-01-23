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

namespace DnaUnity
{

    public unsafe static class System_Char
    {

        public static tAsyncCall* System_Char_GetUnicodeCategory(byte* pThis_, byte* pParams, byte* pReturnValue) {
        	uint paramCodePoint = ((uint*)pParams)[0];
        	// Do a binary search on the UC_Index array
            uint curOfs = (uint)System_Char_UC_IndexRuns.UC_Index.Length / 2;
            uint upper = (uint)System_Char_UC_IndexRuns.UC_Index.Length;
        	uint lower = 0;
        	uint indexCodePoint;
        	if (paramCodePoint == 0xffff) {
        		// Special case for 0xffff, as this will not be handled correctly by the code below
        		*(uint*)pReturnValue = 29;
        		return null;
        	}
        	for(;;) {
                indexCodePoint = System_Char_UC_IndexRuns.UC_Index[curOfs << 1];
                if (paramCodePoint >= indexCodePoint && paramCodePoint < System_Char_UC_IndexRuns.UC_Index[(curOfs+1) << 1]) {
        			// Found the correct entry...
                    uint value = System_Char_UC_IndexRuns.UC_Index[(curOfs << 1) + 1];
                    if ((value & 0x8000) != 0) {
        				// This is a run, not a direct look-up
        				value &= 0x7fff;
        				value += paramCodePoint - indexCodePoint;
                        value = System_Char_UC_IndexRuns.UC_Runs[value];
        			}
        			*(uint*)pReturnValue = value;
        			return null;
        		}
        		if (paramCodePoint < indexCodePoint) {
        			upper = curOfs;
        		} else {
        			lower = curOfs;
        		}
        		curOfs = lower + ((upper - lower) >> 1);
        	}
        }

        // Return -1 if not found
        static int SearchCaseArray(ushort[] pCaseArray, ushort find) {
        	uint lower = 0;
            uint upper = (uint)pCaseArray.Length;
            uint curOfs = (uint)pCaseArray.Length / 2;
        	ushort val;

        	if (find == 0xffff) {
        		// Hande 0xffff specially, as the search below cannot handle it.
        		return -1;
        	}

        	for(;;) {
        		val = pCaseArray[curOfs];
        		if (find >= val && find < pCaseArray[curOfs + 1]) {
        			// Found the correct entry
        			if (find == val) {
        				return (int)curOfs;
        			}
        			return -1;
        		}
        		if (find < val) {
        			upper = curOfs;
        		} else {
        			lower = curOfs;
        		}
        		if (upper == 0) {
        			return -1;
        		}
        		curOfs = lower + ((upper - lower) >> 1);
        	}
        }

        public static tAsyncCall* ToLowerInvariant(byte* pThis_, byte* pParams, byte* pReturnValue) {
        	uint paramCodePoint = ((uint*)pParams)[0];
        	int pos;

            pos = SearchCaseArray(System_Char_CaseConversion.UC_CaseUpper, (ushort)paramCodePoint);
            *(uint*)pReturnValue = (pos < 0)?paramCodePoint:System_Char_CaseConversion.UC_CaseLower[pos];

        	return null;
        }

        public static tAsyncCall* ToUpperInvariant(byte* pThis_, byte* pParams, byte* pReturnValue) {
        	uint paramCodePoint = ((uint*)pParams)[0];
        	int pos;

            pos = SearchCaseArray(System_Char_CaseConversion.UC_CaseLower, (ushort)paramCodePoint);
            *(uint*)pReturnValue = (pos < 0)?paramCodePoint:System_Char_CaseConversion.UC_CaseUpper[pos];

        	return null;
        }
    }
}