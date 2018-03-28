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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

#if UNITY_5 || UNITY_2017 || UNITY_2018
using UnityEngine;
#endif

#if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
using SIZE_T = System.UInt32;
using PTR = System.UInt32;
#else
using SIZE_T = System.UInt64;
using PTR = System.UInt64;
#endif

namespace DnaUnity
{
    public unsafe static partial class MonoType
    {
        #if (UNITY_WEBGL && !UNITY_EDITOR) || DNA_32BIT
        const int PTR_SIZE = 4;
        const uint STACK_ALIGNMENT = 4;
        #else
        const int PTR_SIZE = 8;
        const uint STACK_ALIGNMENT = 8;
#endif

        public static Dictionary<System.Type, PTR> monoTypes;

        public static void Init()
        {
            monoTypes = new Dictionary<System.Type, PTR>();
        }

        public static void Clear()
        {
            monoTypes = null;
        }

        public static void GetFieldTrampoline(tMD_FieldDef* pFieldInfo, byte* pThis, byte* pOutValue)
        {

        }

        public static void SetFieldTrampoline(tMD_FieldDef* pFieldInfo, byte* pThis, byte* pInValue)
        {

        }

#if UNITY_5 || UNITY_2017 || UNITY_2018
        public static void MarshalToVector2(byte* pPtr, out Vector2 v2)
        {
            v2 = new Vector2(*(float*)(pPtr + 0), *(float*)(pPtr + 4));
        }

        public static void MarshalToVector3(byte* pPtr, out Vector3 v3)
        {
            v3 = new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8));
        }

        public static void MarshalToColor(byte* pPtr, out Color c)
        {
            c = new Color(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8));
        }

        public static void MarshalToColor32(byte* pPtr, out Color32 c32)
        {
            c32 = new Color32(*(byte*)(pPtr + 0), *(byte*)(pPtr + 1), *(byte*)(pPtr + 2), *(byte*)(pPtr + 3));
        }

        public static void MarshalToVector4(byte* pPtr, out Vector4 v4)
        {
            v4 = new Vector4(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12));
        }

        public static void MarshalToQuaternion(byte* pPtr, out Quaternion q)
        {
            q = new Quaternion(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12));
        }

        public static void MarshalToVector2Int(byte* pPtr, out Vector2Int v2i)
        {
            v2i = new Vector2Int(*(int*)(pPtr + 0), *(int*)(pPtr + 4));
        }

        public static void MarshalToVector3Int(byte* pPtr, out Vector3Int v3i)
        {
            v3i = new Vector3Int(*(int*)(pPtr + 0), *(int*)(pPtr + 4), *(int*)(pPtr + 8));
        }

        public static void MarshalToRect(byte* pPtr, out Rect r)
        {
            r = new Rect(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12));
        }

        public static void MarshalToRectInt(byte* pPtr, out RectInt ri)
        {
            ri = new RectInt(*(int*)(pPtr + 0), *(int*)(pPtr + 4), *(int*)(pPtr + 8), *(int*)(pPtr + 12));
        }

        public static void MarshalToRectOffset(byte* pPtr, out RectOffset ro)
        {
            ro = new RectOffset(*(int*)(pPtr + 0), *(int*)(pPtr + 4), *(int*)(pPtr + 8), *(int*)(pPtr + 12));
        }

        public static void MarshalToRay2D(byte* pPtr, out Ray2D r2d)
        {
            r2d = new Ray2D(new Vector2(*(float*)(pPtr + 0), *(float*)(pPtr + 4)),
                            new Vector2(*(float*)(pPtr + 8), *(float*)(pPtr + 12)));
        }

        public static void MarshalToRay(byte* pPtr, out Ray r)
        {
            r = new Ray(new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8)),
                        new Vector3(*(float*)(pPtr + 12), *(float*)(pPtr + 16), *(float*)(pPtr + 20)));
        }

        public static void MarshalToBounds(byte* pPtr, out Bounds b)
        {
            b = new Bounds(new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8)),
                           new Vector3(*(float*)(pPtr + 12), *(float*)(pPtr + 16), *(float*)(pPtr + 20)));
        }

        public static void MarshalToPlane(byte* pPtr, out Plane p)
        {
            p = new Plane(new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8)), *(float*)(pPtr + 12));
        }

        public static void MarshalToRangeInt(byte* pPtr, out RangeInt ri)
        {
            ri = new RangeInt(*(int*)(pPtr + 0), *(int*)(pPtr + 4));
        }

        public static void MarshalToMatrix4x4(byte* pPtr, out Matrix4x4 m)
        {
            m = new Matrix4x4();
            m.SetColumn(0, new Vector4(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12)));
            m.SetColumn(1, new Vector4(*(float*)(pPtr + 16), *(float*)(pPtr + 20), *(float*)(pPtr + 24), *(float*)(pPtr + 28)));
            m.SetColumn(2, new Vector4(*(float*)(pPtr + 32), *(float*)(pPtr + 36), *(float*)(pPtr + 40), *(float*)(pPtr + 44)));
            m.SetColumn(3, new Vector4(*(float*)(pPtr + 48), *(float*)(pPtr + 52), *(float*)(pPtr + 56), *(float*)(pPtr + 60)));
        }
#endif

        public static object MarshalToMonoObj(tMD_TypeDef* pTypeDef, byte* pPtr)
        {
            if (pTypeDef->hasMonoBase != 0) {
                void* hPtr = *(void**)pPtr;
                return hPtr != null ? H.objects[(int)hPtr] : null;
            } else {
                switch (pTypeDef->typeInitId) {
                    case Type.TYPE_SYSTEM_OBJECT:
                        object o;
                        MarshalToObject(pPtr, out o);
                        return o;
                    case Type.TYPE_SYSTEM_STRING:
                        return System_String.ToMonoString(*(tSystemString**)pPtr);
                    case Type.TYPE_SYSTEM_BOOLEAN:
                        return *(bool*)pPtr;
                    case Type.TYPE_SYSTEM_BYTE:
                        return *(byte*)pPtr;
                    case Type.TYPE_SYSTEM_SBYTE:
                        return *(sbyte*)pPtr;
                    case Type.TYPE_SYSTEM_CHAR:
                        return *(char*)pPtr;
                    case Type.TYPE_SYSTEM_UINT16:
                        return *(ushort*)pPtr;
                    case Type.TYPE_SYSTEM_INT16:
                        return *(short*)pPtr;
                    case Type.TYPE_SYSTEM_UINT32:
                        return *(uint*)pPtr;
                    case Type.TYPE_SYSTEM_INT32:
                        return *(int*)pPtr;
                    case Type.TYPE_SYSTEM_UINT64:
                        return *(ulong*)pPtr;
                    case Type.TYPE_SYSTEM_INT64:
                        return *(long*)pPtr;
                    case Type.TYPE_SYSTEM_SINGLE:
                        return *(float*)pPtr;
                    case Type.TYPE_SYSTEM_DOUBLE:
                        return *(double*)pPtr;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                    case Type.TYPE_UNITYENGINE_VECTOR2:
                        return new Vector2(*(float*)(pPtr + 0), *(float*)(pPtr + 4));
                    case Type.TYPE_UNITYENGINE_VECTOR3:
                        return new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8));
                    case Type.TYPE_UNITYENGINE_COLOR:
                        return new Color(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8));
                    case Type.TYPE_UNITYENGINE_COLOR32:
                        return new Color32(*(byte*)(pPtr + 0), *(byte*)(pPtr + 1), *(byte*)(pPtr + 2), *(byte*)(pPtr + 3));
                    case Type.TYPE_UNITYENGINE_VECTOR4:
                        return new Vector4(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12));
                    case Type.TYPE_UNITYENGINE_QUATERNION:
                        return new Quaternion(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12));
                    case Type.TYPE_UNITYENGINE_VECTOR2INT:
                        return new Vector2Int(*(int*)(pPtr + 0), *(int*)(pPtr + 4));
                    case Type.TYPE_UNITYENGINE_VECTOR3INT:
                        return new Vector3Int(*(int*)(pPtr + 0), *(int*)(pPtr + 4), *(int*)(pPtr + 8));
                    case Type.TYPE_UNITYENGINE_RECT:
                        return new Rect(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12));
                    case Type.TYPE_UNITYENGINE_RECTINT:
                        return new RectInt(*(int*)(pPtr + 0), *(int*)(pPtr + 4), *(int*)(pPtr + 8), *(int*)(pPtr + 12));
                    case Type.TYPE_UNITYENGINE_RECTOFFSET:
                        return new RectOffset(*(int*)(pPtr + 0), *(int*)(pPtr + 4), *(int*)(pPtr + 8), *(int*)(pPtr + 12));
                    case Type.TYPE_UNITYENGINE_RAY2D:
                        return new Ray2D(new Vector2(*(float*)(pPtr + 0), *(float*)(pPtr + 4)),
                                         new Vector2(*(float*)(pPtr + 8), *(float*)(pPtr + 12)));
                    case Type.TYPE_UNITYENGINE_RAY:
                        return new Ray(new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8)),
                                       new Vector3(*(float*)(pPtr + 12), *(float*)(pPtr + 16), *(float*)(pPtr + 20)));
                    case Type.TYPE_UNITYENGINE_BOUNDS:
                        return new Bounds(new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8)),
                                          new Vector3(*(float*)(pPtr + 12), *(float*)(pPtr + 16), *(float*)(pPtr + 20)));
                    case Type.TYPE_UNITYENGINE_PLANE:
                        return new Plane(new Vector3(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8)), *(float*)(pPtr + 12));
                    case Type.TYPE_UNITYENGINE_RANGEINT:
                        return new RangeInt(*(int*)(pPtr + 0), *(int*)(pPtr + 4));
                    case Type.TYPE_UNITYENGINE_MATRIX4X4:
                        Matrix4x4 m = new Matrix4x4();
                        m.SetColumn(0, new Vector4(*(float*)(pPtr + 0), *(float*)(pPtr + 4), *(float*)(pPtr + 8), *(float*)(pPtr + 12)));
                        m.SetColumn(1, new Vector4(*(float*)(pPtr + 16), *(float*)(pPtr + 20), *(float*)(pPtr + 24), *(float*)(pPtr + 28)));
                        m.SetColumn(2, new Vector4(*(float*)(pPtr + 32), *(float*)(pPtr + 36), *(float*)(pPtr + 40), *(float*)(pPtr + 44)));
                        m.SetColumn(3, new Vector4(*(float*)(pPtr + 48), *(float*)(pPtr + 52), *(float*)(pPtr + 56), *(float*)(pPtr + 60)));
                        return m;
#endif          
                }
            }

            // A dna reference or boxed type
            if (pTypeDef->monoType == null) {
                if (pTypeDef->isValueType != 0) {
                    byte* pBoxedValue = Heap.AllocType(pTypeDef);
                    if (pTypeDef->instanceMemSize == 4) {
                        *(uint*)pBoxedValue = *(uint*)pPtr;
                    } else if (pTypeDef->instanceMemSize == 8) {
                        *(ulong*)pBoxedValue = *(ulong*)pPtr;
                    } else {
                        Mem.memcpy(pBoxedValue, pPtr, pTypeDef->instanceMemSize);
                    }
                    return DnaObject.WrapObject(pBoxedValue);
                } else {
                    return DnaObject.WrapObject(pPtr);
                }
            } else {
                System.Type monoType = H.ToObj(pTypeDef->monoType) as System.Type;
                if (monoType.IsEnum) {
                    return Enum.ToObject(monoType, *(int*)pPtr);
                } else {
                    if (monoType.IsValueType) {
                        object o = Activator.CreateInstance(monoType);
                        System.Runtime.InteropServices.Marshal.PtrToStructure((System.IntPtr)pPtr, o);
                    }
                }
            }

            Sys.Crash("Marshaling code not defined yet for this class");
            return null;
        }

        public static void MarshalToObject(byte* pPtr, out object o)
        {
            o = null;
            byte* pMem = *(byte**)pPtr;
            if (pMem == null)
                return;
            tMD_TypeDef* pTypeDef = Heap.GetType(pMem);
            if (pTypeDef->isValueType != 0) {
                o = MarshalToMonoObj(pTypeDef, *(byte**)pMem);
            } else {
                o = MarshalToMonoObj(pTypeDef, pMem);
            }
        }

#if UNITY_5 || UNITY_2017 || UNITY_2018
        public static void MarshalFromVector2(byte* pPtr, ref Vector2 v2)
        {
            *(float*)(pPtr + 0) = v2.x;
            *(float*)(pPtr + 4) = v2.y;
        }

        public static void MarshalFromVector3(byte* pPtr, ref Vector3 v3)
        {
            *(float*)(pPtr + 0) = v3.x;
            *(float*)(pPtr + 4) = v3.y;
            *(float*)(pPtr + 8) = v3.z;
        }

        public static void MarshalFromColor(byte* pPtr, ref Color c)
        {
            *(float*)(pPtr + 0) = c.r;
            *(float*)(pPtr + 4) = c.g;
            *(float*)(pPtr + 8) = c.b;
            *(float*)(pPtr + 12) = c.a;
        }

        public static void MarshalFromColor32(byte* pPtr, ref Color32 c32)
        {
            *(byte*)(pPtr + 0) = c32.r;
            *(byte*)(pPtr + 1) = c32.g;
            *(byte*)(pPtr + 2) = c32.b;
            *(byte*)(pPtr + 3) = c32.a;
        }

        public static void MarshalFromVector4(byte* pPtr, ref Vector4 v4)
        {
            *(float*)(pPtr + 0) = v4.x;
            *(float*)(pPtr + 4) = v4.y;
            *(float*)(pPtr + 8) = v4.z;
            *(float*)(pPtr + 12) = v4.w;
        }

        public static void MarshalFromQuaternion(byte* pPtr, ref Quaternion q)
        {
            *(float*)(pPtr + 0) = q.x;
            *(float*)(pPtr + 4) = q.y;
            *(float*)(pPtr + 8) = q.z;
            *(float*)(pPtr + 12) = q.w;
        }

        public static void MarshalFromVector2Int(byte* pPtr, ref Vector2Int v2i)
        {
            *(int*)(pPtr + 0) = v2i.x;
            *(int*)(pPtr + 4) = v2i.y;
        }

        public static void MarshalFromVector3Int(byte* pPtr, ref Vector3Int v3i)
        {
            *(int*)(pPtr + 0) = v3i.x;
            *(int*)(pPtr + 4) = v3i.y;
            *(int*)(pPtr + 8) = v3i.z;
        }

        public static void MarshalFromRect(byte* pPtr, ref Rect r)
        {
            *(float*)(pPtr + 0) = r.x;
            *(float*)(pPtr + 4) = r.y;
            *(float*)(pPtr + 8) = r.width;
            *(float*)(pPtr + 12) = r.height;
        }

        public static void MarshalFromRectInt(byte* pPtr, ref RectInt ri)
        {
            *(int*)(pPtr + 0) = ri.x;
            *(int*)(pPtr + 4) = ri.y;
            *(int*)(pPtr + 8) = ri.width;
            *(int*)(pPtr + 12) = ri.height;
        }

        public static void MarshalFromRectOffset(byte* pPtr, ref RectOffset ro)
        {
            *(float*)(pPtr + 0) = ro.left;
            *(float*)(pPtr + 4) = ro.top;
            *(float*)(pPtr + 8) = ro.right;
            *(float*)(pPtr + 12) = ro.bottom;
        }

        public static void MarshalFromRay2D(byte* pPtr, ref Ray2D r2d)
        {
            Vector2 r2do = r2d.origin;
            Vector2 r2dd = r2d.direction;
            *(float*)(pPtr + 0) = r2do.x;
            *(float*)(pPtr + 4) = r2do.y;
            *(float*)(pPtr + 8) = r2dd.x;
            *(float*)(pPtr + 12) = r2dd.y;
        }

        public static void MarshalFromRay(byte* pPtr, ref Ray r)
        {
            Vector3 ryo = r.origin;
            Vector3 ryd = r.direction;
            *(float*)(pPtr + 0) = ryo.x;
            *(float*)(pPtr + 4) = ryo.y;
            *(float*)(pPtr + 8) = ryo.z;
            *(float*)(pPtr + 12) = ryd.x;
            *(float*)(pPtr + 16) = ryd.y;
            *(float*)(pPtr + 20) = ryd.z;
        }

        public static void MarshalFromBounds(byte* pPtr, ref Bounds b)
        {
            Vector3 bct = b.center;
            Vector3 bex = b.extents;
            *(float*)(pPtr + 0) = bct.x;
            *(float*)(pPtr + 4) = bct.y;
            *(float*)(pPtr + 8) = bct.z;
            *(float*)(pPtr + 12) = bex.x;
            *(float*)(pPtr + 16) = bex.y;
            *(float*)(pPtr + 20) = bex.z;
        }

        public static void MarshalFromPlane(byte* pPtr, ref Plane p)
        {
            Vector3 pnm = p.normal;
            float pds = p.distance;
            *(float*)(pPtr + 0) = pnm.x;
            *(float*)(pPtr + 4) = pnm.y;
            *(float*)(pPtr + 8) = pnm.z;
            *(float*)(pPtr + 12) = pds;
        }

        public static void MarshalFromRangeInt(byte* pPtr, ref RangeInt ri)
        {
            *(int*)(pPtr + 0) = ri.start;
            *(int*)(pPtr + 4) = ri.length;
        }

        public static void MarshalFromMatrix4x4(byte* pPtr, ref Matrix4x4 m)
        {
            Vector4 c1 = m.GetColumn(0);
            *(float*)(pPtr + 0) = c1.x; *(float*)(pPtr + 4) = c1.y; *(float*)(pPtr + 8) = c1.z; *(float*)(pPtr + 12) = c1.w;
            Vector4 c2 = m.GetColumn(1);
            *(float*)(pPtr + 16) = c2.x; *(float*)(pPtr + 20) = c2.y; *(float*)(pPtr + 24) = c2.z; *(float*)(pPtr + 28) = c2.w;
            Vector4 c3 = m.GetColumn(2);
            *(float*)(pPtr + 32) = c3.x; *(float*)(pPtr + 36) = c3.y; *(float*)(pPtr + 40) = c3.z; *(float*)(pPtr + 44) = c3.w;
            Vector4 c4 = m.GetColumn(3);
            *(float*)(pPtr + 48) = c4.x; *(float*)(pPtr + 52) = c4.y; *(float*)(pPtr + 56) = c4.z; *(float*)(pPtr + 60) = c4.w;
        }
#endif          

        public static void MarshalFromMonoObj(tMD_TypeDef* pTypeDef, object obj, byte* pPtr, bool toStack = true)
        {
            if (obj is DnaObject) {
                if (pTypeDef->isValueType != 0) {
                    if (toStack) {
                        // Marshalling to mememory on the stack
                        if (pTypeDef->instanceMemSize == 4)
                            *(uint*)pPtr = *(uint*)((obj as DnaObject).dnaPtr);
                        else if (pTypeDef->instanceMemSize == 8)
                            *(ulong*)pPtr = *(ulong*)((obj as DnaObject).dnaPtr);
                        else
                            Mem.memcpy(pPtr, (obj as DnaObject).dnaPtr, pTypeDef->stackSize);
                    } else {
                        // Marshalling to memory in an instance or in an array
                        if (pTypeDef->instanceMemSize == 1)
                            *(byte*)pPtr = *(byte*)((obj as DnaObject).dnaPtr);
                        else if (pTypeDef->instanceMemSize == 2)
                            *(ushort*)pPtr = *(ushort*)((obj as DnaObject).dnaPtr);
                        else if (pTypeDef->instanceMemSize == 4)
                            *(uint*)pPtr = *(uint*)((obj as DnaObject).dnaPtr);
                        else if (pTypeDef->instanceMemSize == 8)
                            *(ulong*)pPtr = *(ulong*)((obj as DnaObject).dnaPtr);
                        else
                            Mem.memcpy(pPtr, (obj as DnaObject).dnaPtr, pTypeDef->instanceMemSize);
                    }
                } else {
                    *(byte**)pPtr = (obj as DnaObject).dnaPtr;
                }
                return;
            }

            switch (pTypeDef->typeInitId) {
                case Type.TYPE_SYSTEM_OBJECT:
                    MarshalFromObject(pPtr, ref obj);
                    return;
                case Type.TYPE_SYSTEM_STRING:
                    *(tSystemString**)pPtr = System_String.FromMonoString(obj as string);
                    return;
                case Type.TYPE_SYSTEM_BOOLEAN:
                    if (toStack)
                        *(uint*)pPtr = (uint)((bool)obj ? 1 : 0);
                    else
                        *(bool*)pPtr = (bool)obj;
                    return;
                case Type.TYPE_SYSTEM_BYTE:
                    if (toStack)
                        *(uint*)pPtr = (uint)(byte)obj;
                    else
                        *(byte*)pPtr = (byte)obj;
                    return;
                case Type.TYPE_SYSTEM_SBYTE:
                    if (toStack)
                        *(uint*)pPtr = (uint)(sbyte)obj;
                    else
                        *(sbyte*)pPtr = (sbyte)obj;
                    return;
                case Type.TYPE_SYSTEM_CHAR:
                    if (toStack)
                        *(uint*)pPtr = (uint)(char)obj;
                    else
                        *(char*)pPtr = (char)obj;
                    return;
                case Type.TYPE_SYSTEM_UINT16:
                    if (toStack)
                        *(uint*)pPtr = (uint)(ushort)obj;
                    else
                        *(ushort*)pPtr = (ushort)obj;
                    return;
                case Type.TYPE_SYSTEM_INT16:
                    if (toStack)
                        *(uint*)pPtr = (uint)(short)obj;
                    else
                        *(short*)pPtr = (short)obj;
                    return;
                case Type.TYPE_SYSTEM_UINT32:
                    *(uint*)pPtr = (uint)obj;
                    return;
                case Type.TYPE_SYSTEM_INT32:
                    *(int*)pPtr = (int)obj;
                    return;
                case Type.TYPE_SYSTEM_UINT64:
                    *(ulong*)pPtr = (ulong)obj;
                    return;
                case Type.TYPE_SYSTEM_INT64:
                    *(long*)pPtr = (long)obj;
                    return;
                case Type.TYPE_SYSTEM_SINGLE:
                    *(float*)pPtr = (float)obj;
                    return;
                case Type.TYPE_SYSTEM_DOUBLE:
                    *(double*)pPtr = (double)obj;
                    return;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                case Type.TYPE_UNITYENGINE_VECTOR2:
                    Vector2 v2 = (Vector2)obj;
                    *(float*)(pPtr + 0) = v2.x;
                    *(float*)(pPtr + 4) = v2.y;
                    return;
                case Type.TYPE_UNITYENGINE_VECTOR3:
                    Vector3 v3 = (Vector3)obj;
                    *(float*)(pPtr + 0) = v3.x;
                    *(float*)(pPtr + 4) = v3.y;
                    *(float*)(pPtr + 8) = v3.z;
                    return;
                case Type.TYPE_UNITYENGINE_COLOR:
                    Color c = (Color)obj;
                    *(float*)(pPtr + 0) = c.r;
                    *(float*)(pPtr + 4) = c.g;
                    *(float*)(pPtr + 8) = c.b;
                    *(float*)(pPtr + 12) = c.a;
                    return;
                case Type.TYPE_UNITYENGINE_COLOR32:
                    Color32 c32 = (Color32)obj;
                    *(byte*)(pPtr + 0) = c32.r;
                    *(byte*)(pPtr + 1) = c32.g;
                    *(byte*)(pPtr + 2) = c32.b;
                    *(byte*)(pPtr + 3) = c32.a;
                    return;
                case Type.TYPE_UNITYENGINE_VECTOR4:
                    Vector4 v4 = (Vector4)obj;
                    *(float*)(pPtr + 0) = v4.x;
                    *(float*)(pPtr + 4) = v4.y;
                    *(float*)(pPtr + 8) = v4.z;
                    *(float*)(pPtr + 12) = v4.w;
                    return;
                case Type.TYPE_UNITYENGINE_QUATERNION:
                    Quaternion q = (Quaternion)obj;
                    *(float*)(pPtr + 0) = q.x;
                    *(float*)(pPtr + 4) = q.y;
                    *(float*)(pPtr + 8) = q.z;
                    *(float*)(pPtr + 12) = q.w;
                    return;
                case Type.TYPE_UNITYENGINE_VECTOR2INT:
                    Vector2Int v2i = (Vector2Int)obj;
                    *(int*)(pPtr + 0) = v2i.x;
                    *(int*)(pPtr + 4) = v2i.y;
                    return;
                case Type.TYPE_UNITYENGINE_VECTOR3INT:
                    Vector3Int v3i = (Vector3Int)obj;
                    *(int*)(pPtr + 0) = v3i.x;
                    *(int*)(pPtr + 4) = v3i.y;
                    *(int*)(pPtr + 8) = v3i.z;
                    return;
                case Type.TYPE_UNITYENGINE_RECT:
                    Rect r = (Rect)obj;
                    *(float*)(pPtr + 0) = r.x;
                    *(float*)(pPtr + 4) = r.y;
                    *(float*)(pPtr + 8) = r.width;
                    *(float*)(pPtr + 12) = r.height;
                    return;
                case Type.TYPE_UNITYENGINE_RECTINT:
                    RectInt ri = (RectInt)obj;
                    *(int*)(pPtr + 0) = ri.x;
                    *(int*)(pPtr + 4) = ri.y;
                    *(int*)(pPtr + 8) = ri.width;
                    *(int*)(pPtr + 12) = ri.height;
                    return;
                case Type.TYPE_UNITYENGINE_RECTOFFSET:
                    RectOffset ro = (RectOffset)obj;
                    *(float*)(pPtr + 0) = ro.left;
                    *(float*)(pPtr + 4) = ro.top;
                    *(float*)(pPtr + 8) = ro.right;
                    *(float*)(pPtr + 12) = ro.bottom;
                    return;
                case Type.TYPE_UNITYENGINE_RAY2D:
                    Ray2D r2d = (Ray2D)obj;
                    Vector2 r2do = r2d.origin;
                    Vector2 r2dd = r2d.direction;
                    *(float*)(pPtr + 0) = r2do.x;
                    *(float*)(pPtr + 4) = r2do.y;
                    *(float*)(pPtr + 8) = r2dd.x;
                    *(float*)(pPtr + 12) = r2dd.y;
                    return;
                case Type.TYPE_UNITYENGINE_RAY:
                    Ray ry = (Ray)obj;
                    Vector3 ryo = ry.origin;
                    Vector3 ryd = ry.direction;
                    *(float*)(pPtr + 0) = ryo.x;
                    *(float*)(pPtr + 4) = ryo.y;
                    *(float*)(pPtr + 8) = ryo.z;
                    *(float*)(pPtr + 12) = ryd.x;
                    *(float*)(pPtr + 16) = ryd.y;
                    *(float*)(pPtr + 20) = ryd.z;
                    return;
                case Type.TYPE_UNITYENGINE_BOUNDS:
                    Bounds b = (Bounds)obj;
                    Vector3 bct = b.center;
                    Vector3 bex = b.extents;
                    *(float*)(pPtr + 0) = bct.x;
                    *(float*)(pPtr + 4) = bct.y;
                    *(float*)(pPtr + 8) = bct.z;
                    *(float*)(pPtr + 12) = bex.x;
                    *(float*)(pPtr + 16) = bex.y;
                    *(float*)(pPtr + 20) = bex.z;
                    return;
                case Type.TYPE_UNITYENGINE_PLANE:
                    Plane p = (Plane)obj;
                    Vector3 pnm = p.normal;
                    float pds = p.distance;
                    *(float*)(pPtr + 0) = pnm.x;
                    *(float*)(pPtr + 4) = pnm.y;
                    *(float*)(pPtr + 8) = pnm.z;
                    *(float*)(pPtr + 12) = pds;
                    return;
                case Type.TYPE_UNITYENGINE_RANGEINT:
                    RangeInt rin = (RangeInt)obj;
                    *(int*)(pPtr + 0) = rin.start;
                    *(int*)(pPtr + 4) = rin.length;
                    return;
                case Type.TYPE_UNITYENGINE_MATRIX4X4:
                    Matrix4x4 m = (Matrix4x4)obj;
                    Vector4 c1 = m.GetColumn(0);
                    *(float*)(pPtr + 0) = c1.x; *(float*)(pPtr + 4) = c1.y; *(float*)(pPtr + 8) = c1.z; *(float*)(pPtr + 12) = c1.w;
                    Vector4 c2 = m.GetColumn(1);
                    *(float*)(pPtr + 16) = c2.x; *(float*)(pPtr + 20) = c2.y; *(float*)(pPtr + 24) = c2.z; *(float*)(pPtr + 28) = c2.w;
                    Vector4 c3 = m.GetColumn(2);
                    *(float*)(pPtr + 32) = c3.x; *(float*)(pPtr + 36) = c3.y; *(float*)(pPtr + 40) = c3.z; *(float*)(pPtr + 44) = c3.w;
                    Vector4 c4 = m.GetColumn(3);
                    *(float*)(pPtr + 48) = c4.x; *(float*)(pPtr + 52) = c4.y; *(float*)(pPtr + 56) = c4.z; *(float*)(pPtr + 60) = c4.w;
                    return;
#endif          
            }

            if (pTypeDef->monoType != null) {
                if (pTypeDef->isValueType == 0) {
                    *(byte**)pPtr = Heap.AllocMonoObject(pTypeDef, obj);
                } else {
                    System.Type monoType = H.ToObj(pTypeDef->monoType) as System.Type;
                    if (monoType.IsEnum) {
                        *(int*)pPtr = (int)obj;
                    } else {
                        System.Runtime.InteropServices.Marshal.StructureToPtr(obj, (IntPtr)pPtr, false);
                    }
                }
                return;
            }

            Sys.Crash("Marshaling code not defined yet for this class");
        }

        public static void MarshalFromObject(byte* pPtr, ref object o)
        {
            if (o == null) {
                *(byte**)pPtr = null;
                return;
            }
            tMD_TypeDef* pTypeDef = GetTypeForMonoObject(o, null, null);
            if (pTypeDef->isValueType != 0) {
                // Box the value type
                *(byte**)pPtr = Heap.AllocType(pTypeDef);
                MarshalFromMonoObj(pTypeDef, o, *(byte**)pPtr);
            } else {
                // Put reference to ref type on stack
                MarshalFromMonoObj(pTypeDef, o, pPtr);
            }
        }

        public static fnInternalCall GetGenericTrampoline(tMD_MethodDef* pMethodDef, MethodBase methodBase)
        {
            int numParams = pMethodDef->numberOfParameters;
            int start = 0;
            object[] paramsAry = null;
            bool isConstructor = methodBase is ConstructorInfo;

            if (!MetaData.METHOD_ISSTATIC(pMethodDef)) {
                start = 1;
            }

            if (numParams > start) {
                paramsAry = new object[numParams - start];
            }

            return (_c, _t, _p, _r) => {
                object _this = null;
                uint thisOfs = 0;

                // Get this
                tMD_TypeDef* pThisType = null;
                if (isConstructor) {
                    thisOfs = sizeof(PTR);
                } else if (!MetaData.METHOD_ISSTATIC(pMethodDef)) {
                    if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[0])) {
                        pThisType = pMethodDef->pParams[0].pByRefTypeDef;
                        _this = MarshalToMonoObj(pThisType, *(byte**)_t);
                    } else {
                        pThisType = pMethodDef->pParams[0].pStackTypeDef;
                        _this = MarshalToMonoObj(pThisType, _t);
                    }
                    thisOfs = pMethodDef->pParams[0].pStackTypeDef->stackSize;
                }

                // Get params
                bool hasRefParam = false;
                if (numParams > start) {
                    for (int i = start; i < numParams; i++) {
                        tParameter* pParameter = &pMethodDef->pParams[i];
                        byte* pParamPtr = _p + (pParameter->offset - thisOfs);
                        if (MetaData.PARAM_ISBYREF(pParameter)) {
                            paramsAry[i - start] = MarshalToMonoObj(pParameter->pByRefTypeDef, *(byte**)pParamPtr);
                            hasRefParam = true;
                        } else {
                            paramsAry[i - start] = MarshalToMonoObj(pParameter->pStackTypeDef, pParamPtr);
                        }
                    }
                }

                // Call
                object retVal = methodBase.Invoke(_this, paramsAry);

                // Marshal this back (if needed)
                if (isConstructor) {
                    MarshalFromMonoObj(pMethodDef->pParentType, retVal, _t);
                } else if (pThisType != null && pThisType->isValueType != 0) {
                    if (MetaData.PARAM_ISBYREF(&pMethodDef->pParams[0])) {
                        MarshalFromMonoObj(pThisType, _this, *(byte**)_t);
                    } else {
                        MarshalFromMonoObj(pThisType, _this, _t);
                    }
                }

                // Marshal back any ref params (if needed)
                if (hasRefParam) {
                    for (int i = start; i < numParams; i++) {
                        tParameter* pParameter = &pMethodDef->pParams[i];
                        byte* pParamPtr = _p + pParameter->offset;
                        if (MetaData.PARAM_ISBYREF(pParameter)) {
                            MarshalFromMonoObj(pParameter->pByRefTypeDef, paramsAry[i - start], *(byte**)pParamPtr);
                        }
                    }
                }

                if (pMethodDef->pReturnType != null) {
                    MarshalFromMonoObj(pMethodDef->pReturnType, retVal, _r);
                }

                return null;
            };
        }

        public static fnInternalCall GetRefTypeTrampoline<T>(tMD_MethodDef* pMethodDef, MethodInfo methodInfo)
            where T : class
        {
            fnInternalCall func = null;
            int numParams = pMethodDef->numberOfParameters;

            if (methodInfo.ReturnType == typeof(void)) {
                if (numParams == 2 && pMethodDef->pParams[0].pByRefTypeDef == null && pMethodDef->pParams[1].pByRefTypeDef == null) {
                    tMD_TypeDef* pThisType = pMethodDef->pParams[0].pStackTypeDef;
                    uint typeInitId = pMethodDef->pParams[1].pStackTypeDef->typeInitId;
                    switch (typeInitId) {
                        case Type.TYPE_SYSTEM_OBJECT:
                            Action<T, object> callO = (Action<T, object>)System.Delegate.CreateDelegate(typeof(Action<T, object>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); object o; MarshalToObject(_p, out o); callO(_this as T, o); return null; };
                            break;
                        case Type.TYPE_SYSTEM_STRING:
                            Action<T, string> callS = (Action<T, string>)System.Delegate.CreateDelegate(typeof(Action<T, string>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); callS(_this as T, System_String.ToMonoString(*(tSystemString**)_p)); return null; };
                            break;
                        case Type.TYPE_SYSTEM_BOOLEAN:
                            Action<T, bool> callB = (Action<T, bool>)System.Delegate.CreateDelegate(typeof(Action<T, bool>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); callB(_this as T, *(uint*)_p != 0); return null; };
                            break;
                        case Type.TYPE_SYSTEM_INT32:
                            Action<T, int> callI = (Action<T, int>)System.Delegate.CreateDelegate(typeof(Action<T, int>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); callI(_this as T, *(int*)_p); return null; };
                            break;
                        case Type.TYPE_SYSTEM_INT64:
                            Action<T, long> callL = (Action<T, long>)System.Delegate.CreateDelegate(typeof(Action<T, long>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); callL(_this as T, *(long*)_p); return null; };
                            break;
                        case Type.TYPE_SYSTEM_SINGLE:
                            Action<T, float> callF = (Action<T, float>)System.Delegate.CreateDelegate(typeof(Action<T, float>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); callF(_this as T, *(float*)_p); return null; };
                            break;
                        case Type.TYPE_SYSTEM_DOUBLE:
                            Action<T, double> callD = (Action<T, double>)System.Delegate.CreateDelegate(typeof(Action<T, double>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); callD(_this as T, *(double*)_p); return null; };
                            break;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                        case Type.TYPE_UNITYENGINE_VECTOR2:
                            Action<T, Vector2> callV2 = (Action<T, Vector2>)System.Delegate.CreateDelegate(typeof(Action<T, Vector2>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Vector2 v2; MarshalToVector2(_p, out v2); callV2(_this as T, v2); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR3:
                            Action<T, Vector3> callV3 = (Action<T, Vector3>)System.Delegate.CreateDelegate(typeof(Action<T, Vector3>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Vector3 v3; MarshalToVector3(_p, out v3); callV3(_this as T, v3); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_COLOR:
                            Action<T, Color> callC = (Action<T, Color>)System.Delegate.CreateDelegate(typeof(Action<T, Color>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Color c; MarshalToColor(_p, out c); callC(_this as T, c); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR4:
                            Action<T, Vector4> callV4 = (Action<T, Vector4>)System.Delegate.CreateDelegate(typeof(Action<T, Vector4>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Vector4 v4; MarshalToVector4(_p, out v4); callV4(_this as T, v4); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_QUATERNION:
                            Action<T, Quaternion> callQ = (Action<T, Quaternion>)System.Delegate.CreateDelegate(typeof(Action<T, Quaternion>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Quaternion q; MarshalToQuaternion(_p, out q); callQ(_this as T, q); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RECT:
                            Action<T, Rect> callRC = (Action<T, Rect>)System.Delegate.CreateDelegate(typeof(Action<T, Rect>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Rect r; MarshalToRect(_p, out r); callRC(_this as T, r); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RECTOFFSET:
                            Action<T, RectOffset> callRO = (Action<T, RectOffset>)System.Delegate.CreateDelegate(typeof(Action<T, RectOffset>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); RectOffset ro; MarshalToRectOffset(_p, out ro); callRO(_this as T, ro); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RAY:
                            Action<T, Ray> callRY = (Action<T, Ray>)System.Delegate.CreateDelegate(typeof(Action<T, Ray>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Ray r; MarshalToRay(_p, out r); callRY(_this as T, r); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_BOUNDS:
                            Action<T, Bounds> callBD = (Action<T, Bounds>)System.Delegate.CreateDelegate(typeof(Action<T, Bounds>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Bounds b; MarshalToBounds(_p, out b); callBD(_this as T, b); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_MATRIX4X4:
                            Action<T, Matrix4x4> callM = (Action<T, Matrix4x4>)System.Delegate.CreateDelegate(typeof(Action<T, Matrix4x4>), methodInfo);
                            func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Matrix4x4 m; MarshalToMatrix4x4(_p, out m); callM(_this as T, m); return null; };
                            break;
#endif
                    }

                }
            } else if (numParams == 1 && pMethodDef->pParams[0].pByRefTypeDef == null) {
                tMD_TypeDef* pThisType = pMethodDef->pParams[0].pStackTypeDef;
                uint typeInitId = pMethodDef->pReturnType->typeInitId;
                switch (typeInitId) {
                    case Type.TYPE_SYSTEM_OBJECT:
                        Func<T, object> funcO = (Func<T, object>)System.Delegate.CreateDelegate(typeof(Func<object>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); object o = funcO(_this as T); MarshalFromObject(_r, ref o); return null; };
                        break;
                    case Type.TYPE_SYSTEM_STRING:
                        Func<T, string> funcS = (Func<T, string>)System.Delegate.CreateDelegate(typeof(Func<string>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); string s = funcS(_this as T); *(tSystemString**)_r = System_String.FromMonoString(s); return null; };
                        break;
                    case Type.TYPE_SYSTEM_BOOLEAN:
                        Func<T, bool> funcB = (Func<T, bool>)System.Delegate.CreateDelegate(typeof(Func<bool>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); bool b = funcB(_this as T); *(uint*)_r = (uint)(b ? 1 : 0); return null; };
                        break;
                    case Type.TYPE_SYSTEM_INT32:
                        Func<T, int> funcI = (Func<T, int>)System.Delegate.CreateDelegate(typeof(Func<int>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); int i = funcI(_this as T); *(int*)_r = i; return null; };
                        break;
                    case Type.TYPE_SYSTEM_INT64:
                        Func<T, long> funcL = (Func<T, long>)System.Delegate.CreateDelegate(typeof(Func<long>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); long l = funcL(_this as T); *(long*)_r = l; return null; };
                        break;
                    case Type.TYPE_SYSTEM_SINGLE:
                        Func<T, float> funcF = (Func<T, float>)System.Delegate.CreateDelegate(typeof(Func<float>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); float f = funcF(_this as T); *(float*)_r = f; return null; };
                        break;
                    case Type.TYPE_SYSTEM_DOUBLE:
                        Func<T, double> funcD = (Func<T, double>)System.Delegate.CreateDelegate(typeof(Func<double>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); double d = funcD(_this as T); *(double*)_r = d; return null; };
                        break;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                    case Type.TYPE_UNITYENGINE_VECTOR2:
                        Func<T, Vector2> funcV2 = (Func<T, Vector2>)System.Delegate.CreateDelegate(typeof(Func<T, Vector2>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Vector2 v2 = funcV2(_this as T); MarshalFromVector2(_r, ref v2); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_VECTOR3:
                        Func<T, Vector3> funcV3 = (Func<T, Vector3>)System.Delegate.CreateDelegate(typeof(Func<T, Vector3>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Vector3 v3 = funcV3(_this as T); MarshalFromVector3(_r, ref v3); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_COLOR:
                        Func<T, Color> funcC = (Func<T, Color>)System.Delegate.CreateDelegate(typeof(Func<T, Color>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Color c = funcC(_this as T); MarshalFromColor(_r, ref c); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_VECTOR4:
                        Func<T, Vector4> funcV4 = (Func<T, Vector4>)System.Delegate.CreateDelegate(typeof(Func<T, Vector4>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Vector4 v4 = funcV4(_this as T); MarshalFromVector4(_r, ref v4); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_QUATERNION:
                        Func<T, Quaternion> funcQ = (Func<T, Quaternion>)System.Delegate.CreateDelegate(typeof(Func<T, Quaternion>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Quaternion q = funcQ(_this as T); MarshalFromQuaternion(_r, ref q); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RECT:
                        Func<T, Rect> funcRC = (Func<T, Rect>)System.Delegate.CreateDelegate(typeof(Func<T, Rect>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Rect r = funcRC(_this as T); MarshalFromRect(_r, ref r); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RECTOFFSET:
                        Func<T, RectOffset> funcRO = (Func<T, RectOffset>)System.Delegate.CreateDelegate(typeof(Func<T, RectOffset>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); RectOffset ro = funcRO(_this as T); MarshalFromRectOffset(_r, ref ro); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RAY:
                        Func<T, Ray> funcRY = (Func<T, Ray>)System.Delegate.CreateDelegate(typeof(Func<T, Ray>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Ray r = funcRY(_this as T); MarshalFromRay(_r, ref r); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_BOUNDS:
                        Func<T, Bounds> funcBD = (Func<T, Bounds>)System.Delegate.CreateDelegate(typeof(Func<T, Bounds>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Bounds b = funcBD(_this as T); MarshalFromBounds(_r, ref b); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_MATRIX4X4:
                        Func<T, Matrix4x4> funcM = (Func<T, Matrix4x4>)System.Delegate.CreateDelegate(typeof(Func<T, Matrix4x4>), methodInfo);
                        func = (_c, _t, _p, _r) => { object _this = MarshalToMonoObj(pThisType, _t); Matrix4x4 m = funcM(_this as T); MarshalFromMatrix4x4(_r, ref m); return null; };
                        break;
#endif

                }
            }

            if (func == null)
                func = GetGenericTrampoline(pMethodDef, methodInfo);

            return func;
        }

        public delegate void RefAction<T1, T2>(ref T1 a, T2 b) where T1 : struct;
        public delegate T2 RefFunc<T1, T2>(ref T1 a) where T1 : struct;
        public delegate void MarshalToMethod<T>(byte* pPtr, out T value) where T : struct;
        public delegate void MarshalFromMethod<T>(byte* pPtr, ref T value) where T : struct;

        public static fnInternalCall GetValueTypeTrampoline<T>(tMD_MethodDef* pMethodDef, MethodInfo methodInfo, MarshalToMethod<T> marshalTo, MarshalFromMethod<T> marshalFrom)
            where T : struct
        {
            fnInternalCall func = null;
            int numParams = pMethodDef->numberOfParameters;

            if (methodInfo.ReturnType == typeof(void)) {
                if (numParams == 2 && pMethodDef->pParams[0].pByRefTypeDef == null && pMethodDef->pParams[1].pByRefTypeDef == null) {
                    tMD_TypeDef* pThisType = pMethodDef->pParams[0].pStackTypeDef;
                    uint typeInitId = pMethodDef->pParams[1].pStackTypeDef->typeInitId;
                    switch (typeInitId) {
                        case Type.TYPE_SYSTEM_OBJECT:
                            RefAction<T, object> callO = (RefAction<T, object>)System.Delegate.CreateDelegate(typeof(RefAction<T, object>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); object o; MarshalToObject(_p, out o); callO(ref _this, o); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_SYSTEM_STRING:
                            RefAction<T, string> callS = (RefAction<T, string>)System.Delegate.CreateDelegate(typeof(RefAction<T, string>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); callS(ref _this, System_String.ToMonoString(*(tSystemString**)_p)); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_SYSTEM_BOOLEAN:
                            RefAction<T, bool> callB = (RefAction<T, bool>)System.Delegate.CreateDelegate(typeof(RefAction<T, bool>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); callB(ref _this, *(uint*)_p != 0); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_SYSTEM_INT32:
                            RefAction<T, int> callI = (RefAction<T, int>)System.Delegate.CreateDelegate(typeof(RefAction<T, int>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); callI(ref _this, *(int*)_p); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_SYSTEM_INT64:
                            RefAction<T, long> callL = (RefAction<T, long>)System.Delegate.CreateDelegate(typeof(RefAction<T, long>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); callL(ref _this, *(long*)_p); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_SYSTEM_SINGLE:
                            RefAction<T, float> callF = (RefAction<T, float>)System.Delegate.CreateDelegate(typeof(RefAction<T, float>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); callF(ref _this, *(float*)_p); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_SYSTEM_DOUBLE:
                            RefAction<T, double> callD = (RefAction<T, double>)System.Delegate.CreateDelegate(typeof(RefAction<T, double>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); callD(ref _this, *(double*)_p); marshalFrom(_t, ref _this); return null; };
                            break;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                        case Type.TYPE_UNITYENGINE_VECTOR2:
                            RefAction<T, Vector2> callV2 = (RefAction<T, Vector2>)System.Delegate.CreateDelegate(typeof(RefAction<T, Vector2>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Vector2 v2; MarshalToVector2(_p, out v2); marshalFrom(_t, ref _this); callV2(ref _this, v2); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR3:
                            RefAction<T, Vector3> callV3 = (RefAction<T, Vector3>)System.Delegate.CreateDelegate(typeof(RefAction<T, Vector3>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Vector3 v3; MarshalToVector3(_p, out v3); marshalFrom(_t, ref _this); callV3(ref _this, v3); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_COLOR:
                            RefAction<T, Color> callC = (RefAction<T, Color>)System.Delegate.CreateDelegate(typeof(RefAction<T, Color>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Color c; MarshalToColor(_p, out c); callC(ref _this, c); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR4:
                            RefAction<T, Vector4> callV4 = (RefAction<T, Vector4>)System.Delegate.CreateDelegate(typeof(RefAction<T, Vector4>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Vector4 v4; MarshalToVector4(_p, out v4); callV4(ref _this, v4); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_QUATERNION:
                            RefAction<T, Quaternion> callQ = (RefAction<T, Quaternion>)System.Delegate.CreateDelegate(typeof(RefAction<T, Quaternion>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Quaternion q; MarshalToQuaternion(_p, out q); callQ(ref _this, q); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RECT:
                            RefAction<T, Rect> callRC = (RefAction<T, Rect>)System.Delegate.CreateDelegate(typeof(RefAction<T, Rect>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Rect r; MarshalToRect(_p, out r); callRC(ref _this, r); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RECTOFFSET:
                            RefAction<T, RectOffset> callRO = (RefAction<T, RectOffset>)System.Delegate.CreateDelegate(typeof(RefAction<T, RectOffset>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); RectOffset ro; MarshalToRectOffset(_p, out ro); callRO(ref _this, ro); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RAY:
                            RefAction<T, Ray> callRY = (RefAction<T, Ray>)System.Delegate.CreateDelegate(typeof(RefAction<T, Ray>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Ray r; MarshalToRay(_p, out r); callRY(ref _this, r); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_BOUNDS:
                            RefAction<T, Bounds> callBD = (RefAction<T, Bounds>)System.Delegate.CreateDelegate(typeof(RefAction<T, Bounds>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Bounds b; MarshalToBounds(_p, out b); callBD(ref _this, b); marshalFrom(_t, ref _this); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_MATRIX4X4:
                            RefAction<T, Matrix4x4> callM = (RefAction<T, Matrix4x4>)System.Delegate.CreateDelegate(typeof(RefAction<T, Matrix4x4>), methodInfo);
                            func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Matrix4x4 m; MarshalToMatrix4x4(_p, out m); callM(ref _this, m); marshalFrom(_t, ref _this); return null; };
                            break;
#endif
                    }

                }
            } else if (numParams == 1 && pMethodDef->pParams[0].pByRefTypeDef == null) {
                tMD_TypeDef* pThisType = pMethodDef->pParams[0].pStackTypeDef;
                uint typeInitId = pMethodDef->pReturnType->typeInitId;
                switch (typeInitId) {
                    case Type.TYPE_SYSTEM_OBJECT:
                        RefFunc<T, object> funcO = (RefFunc<T, object>)System.Delegate.CreateDelegate(typeof(RefFunc<T, object>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); object o = funcO(ref _this); MarshalFromObject(_r, ref o); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_SYSTEM_STRING:
                        RefFunc<T, string> funcS = (RefFunc<T, string>)System.Delegate.CreateDelegate(typeof(RefFunc<T, string>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); string s = funcS(ref _this); *(tSystemString**)_r = System_String.FromMonoString(s); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_SYSTEM_BOOLEAN:
                        RefFunc<T, bool> funcB = (RefFunc<T, bool>)System.Delegate.CreateDelegate(typeof(RefFunc<T, bool>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); bool b = funcB(ref _this); *(uint*)_r = (uint)(b ? 1 : 0); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_SYSTEM_INT32:
                        RefFunc<T, int> funcI = (RefFunc<T, int>)System.Delegate.CreateDelegate(typeof(RefFunc<T, int>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); int i = funcI(ref _this); *(int*)_r = i; marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_SYSTEM_INT64:
                        RefFunc<T, long> funcL = (RefFunc<T, long>)System.Delegate.CreateDelegate(typeof(RefFunc<T, long>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); long l = funcL(ref _this); *(long*)_r = l; marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_SYSTEM_SINGLE:
                        RefFunc<T, float> funcF = (RefFunc<T, float>)System.Delegate.CreateDelegate(typeof(RefFunc<T, float>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); float f = funcF(ref _this); *(float*)_r = f; marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_SYSTEM_DOUBLE:
                        RefFunc<T, double> funcD = (RefFunc<T, double>)System.Delegate.CreateDelegate(typeof(RefFunc<T, double>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); double d = funcD(ref _this); *(double*)_r = d; marshalFrom(_t, ref _this); return null; };
                        break;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                    case Type.TYPE_UNITYENGINE_VECTOR2:
                        RefFunc<T, Vector2> funcV2 = (RefFunc<T, Vector2>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Vector2>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Vector2 v2 = funcV2(ref _this); MarshalFromVector2(_r, ref v2); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_VECTOR3:
                        RefFunc<T, Vector3> funcV3 = (RefFunc<T, Vector3>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Vector3>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Vector3 v3 = funcV3(ref _this); MarshalFromVector3(_r, ref v3); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_COLOR:
                        RefFunc<T, Color> funcC = (RefFunc<T, Color>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Color>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Color c = funcC(ref _this); MarshalFromColor(_r, ref c); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_VECTOR4:
                        RefFunc<T, Vector4> funcV4 = (RefFunc<T, Vector4>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Vector4>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Vector4 v4 = funcV4(ref _this); MarshalFromVector4(_r, ref v4); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_QUATERNION:
                        RefFunc<T, Quaternion> funcQ = (RefFunc<T, Quaternion>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Quaternion>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Quaternion q = funcQ(ref _this); MarshalFromQuaternion(_r, ref q); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RECT:
                        RefFunc<T, Rect> funcRC = (RefFunc<T, Rect>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Rect>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Rect r = funcRC(ref _this); MarshalFromRect(_r, ref r); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RECTOFFSET:
                        RefFunc<T, RectOffset> funcRO = (RefFunc<T, RectOffset>)System.Delegate.CreateDelegate(typeof(RefFunc<T, RectOffset>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); RectOffset ro = funcRO(ref _this); MarshalFromRectOffset(_r, ref ro); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RAY:
                        RefFunc<T, Ray> funcRY = (RefFunc<T, Ray>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Ray>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Ray r = funcRY(ref _this); MarshalFromRay(_r, ref r); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_BOUNDS:
                        RefFunc<T, Bounds> funcBD = (RefFunc<T, Bounds>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Bounds>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Bounds b = funcBD(ref _this); MarshalFromBounds(_r, ref b); marshalFrom(_t, ref _this); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_MATRIX4X4:
                        RefFunc<T, Matrix4x4> funcM = (RefFunc<T, Matrix4x4>)System.Delegate.CreateDelegate(typeof(RefFunc<T, Matrix4x4>), methodInfo);
                        func = (_c, _t, _p, _r) => { T _this; marshalTo(_t, out _this); Matrix4x4 m = funcM(ref _this); MarshalFromMatrix4x4(_r, ref m); marshalFrom(_t, ref _this); return null; };
                        break;
#endif

                }
            }

            if (func == null)
                func = GetGenericTrampoline(pMethodDef, methodInfo);

            return func;
        }

        public static fnInternalCall GetStaticMethodTrampoline(tMD_MethodDef* pMethodDef, MethodInfo methodInfo)
        {
            fnInternalCall func = null;
            int numParams = pMethodDef->numberOfParameters;

            if (methodInfo.ReturnType == typeof(void)) {
                if (numParams == 0) {
                    Action call = (Action)System.Delegate.CreateDelegate(typeof(Action), methodInfo);
                    func = (_c, _t, _p, _r) => { call(); return null; };
                } else if (numParams == 1 && pMethodDef->pParams[0].pByRefTypeDef == null) {
                    uint typeInitId = pMethodDef->pParams[0].pStackTypeDef->typeInitId;
                    switch (typeInitId) {
                        case Type.TYPE_SYSTEM_OBJECT:
                            Action<object> callO = (Action<object>)System.Delegate.CreateDelegate(typeof(Action<object>), methodInfo);
                            func = (_c, _t, _p, _r) => { object o; MarshalToObject(_p, out o); callO(o); return null; };
                            break;
                        case Type.TYPE_SYSTEM_STRING:
                            Action<string> callS = (Action<string>)System.Delegate.CreateDelegate(typeof(Action<string>), methodInfo);
                            func = (_c, _t, _p, _r) => { callS(System_String.ToMonoString(*(tSystemString**)_p)); return null; };
                            break;
                        case Type.TYPE_SYSTEM_BOOLEAN:
                            Action<bool> callB = (Action<bool>)System.Delegate.CreateDelegate(typeof(Action<bool>), methodInfo);
                            func = (_c, _t, _p, _r) => { callB(*(uint*)_p != 0); return null; };
                            break;
                        case Type.TYPE_SYSTEM_INT32:
                            Action<int> callI = (Action<int>)System.Delegate.CreateDelegate(typeof(Action<int>), methodInfo);
                            func = (_c, _t, _p, _r) => { callI(*(int*)_p); return null; };
                            break;
                        case Type.TYPE_SYSTEM_INT64:
                            Action<long> callL = (Action<long>)System.Delegate.CreateDelegate(typeof(Action<long>), methodInfo);
                            func = (_c, _t, _p, _r) => { callL(*(long*)_p); return null; };
                            break;
                        case Type.TYPE_SYSTEM_SINGLE:
                            Action<float> callF = (Action<float>)System.Delegate.CreateDelegate(typeof(Action<float>), methodInfo);
                            func = (_c, _t, _p, _r) => { callF(*(float*)_p); return null; };
                            break;
                        case Type.TYPE_SYSTEM_DOUBLE:
                            Action<double> callD = (Action<double>)System.Delegate.CreateDelegate(typeof(Action<double>), methodInfo);
                            func = (_c, _t, _p, _r) => { callD(*(double*)_p); return null; };
                            break;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                        case Type.TYPE_UNITYENGINE_VECTOR2:
                            Action<Vector2> callV2 = (Action<Vector2>)System.Delegate.CreateDelegate(typeof(Action<Vector2>), methodInfo);
                            func = (_c, _t, _p, _r) => { Vector2 v2; MarshalToVector2(_p, out v2); callV2(v2); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR3:
                            Action<Vector3> callV3 = (Action<Vector3>)System.Delegate.CreateDelegate(typeof(Action<Vector3>), methodInfo);
                            func = (_c, _t, _p, _r) => { Vector3 v3; MarshalToVector3(_p, out v3); callV3(v3); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_COLOR:
                            Action<Color> callC = (Action<Color>)System.Delegate.CreateDelegate(typeof(Action<Color>), methodInfo);
                            func = (_c, _t, _p, _r) => { Color c; MarshalToColor(_p, out c); callC(c); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR4:
                            Action<Vector4> callV4 = (Action<Vector4>)System.Delegate.CreateDelegate(typeof(Action<Vector4>), methodInfo);
                            func = (_c, _t, _p, _r) => { Vector4 v4; MarshalToVector4(_p, out v4); callV4(v4); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_QUATERNION:
                            Action<Quaternion> callQ = (Action<Quaternion>)System.Delegate.CreateDelegate(typeof(Action<Quaternion>), methodInfo);
                            func = (_c, _t, _p, _r) => { Quaternion q; MarshalToQuaternion(_p, out q); callQ(q); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RECT:
                            Action<Rect> callRC = (Action<Rect>)System.Delegate.CreateDelegate(typeof(Action<Rect>), methodInfo);
                            func = (_c, _t, _p, _r) => { Rect r; MarshalToRect(_p, out r); callRC(r); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RECTOFFSET:
                            Action<RectOffset> callRO = (Action<RectOffset>)System.Delegate.CreateDelegate(typeof(Action<RectOffset>), methodInfo);
                            func = (_c, _t, _p, _r) => { RectOffset ro; MarshalToRectOffset(_p, out ro); callRO(ro); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_RAY:
                            Action<Ray> callRY = (Action<Ray>)System.Delegate.CreateDelegate(typeof(Action<Ray>), methodInfo);
                            func = (_c, _t, _p, _r) => { Ray r; MarshalToRay(_p, out r); callRY(r); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_BOUNDS:
                            Action<Bounds> callBD = (Action<Bounds>)System.Delegate.CreateDelegate(typeof(Action<Bounds>), methodInfo);
                            func = (_c, _t, _p, _r) => { Bounds b; MarshalToBounds(_p, out b); callBD(b); return null; };
                            break;
                        case Type.TYPE_UNITYENGINE_MATRIX4X4:
                            Action<Matrix4x4> callM = (Action<Matrix4x4>)System.Delegate.CreateDelegate(typeof(Action<Matrix4x4>), methodInfo);
                            func = (_c, _t, _p, _r) => { Matrix4x4 m; MarshalToMatrix4x4(_p, out m); callM(m); return null; };
                            break;
#endif
                    }

                }
            } else if (numParams == 0) {
                uint typeInitId = pMethodDef->pReturnType->typeInitId;
                switch (typeInitId) {
                    case Type.TYPE_SYSTEM_OBJECT:
                        Func<object> funcO = (Func<object>)System.Delegate.CreateDelegate(typeof(Func<object>), methodInfo);
                        func = (_c, _t, _p, _r) => { object o = funcO(); MarshalFromObject(_r, ref o); return null; };
                        break;
                    case Type.TYPE_SYSTEM_STRING:
                        Func<string> funcS = (Func<string>)System.Delegate.CreateDelegate(typeof(Func<string>), methodInfo);
                        func = (_c, _t, _p, _r) => { string s = funcS(); *(tSystemString**)_r = System_String.FromMonoString(s); return null; };
                        break;
                    case Type.TYPE_SYSTEM_BOOLEAN:
                        Func<bool> funcB = (Func<bool>)System.Delegate.CreateDelegate(typeof(Func<bool>), methodInfo);
                        func = (_c, _t, _p, _r) => { bool b = funcB(); *(uint*)_r = (uint)(b ? 1 : 0); return null; };
                        break;
                    case Type.TYPE_SYSTEM_INT32:
                        Func<int> funcI = (Func<int>)System.Delegate.CreateDelegate(typeof(Func<int>), methodInfo);
                        func = (_c, _t, _p, _r) => { int i = funcI(); *(int*)_r = i; return null; };
                        break;
                    case Type.TYPE_SYSTEM_INT64:
                        Func<long> funcL = (Func<long>)System.Delegate.CreateDelegate(typeof(Func<long>), methodInfo);
                        func = (_c, _t, _p, _r) => { long l = funcL(); *(long*)_r = l; return null; };
                        break;
                    case Type.TYPE_SYSTEM_SINGLE:
                        Func<float> funcF = (Func<float>)System.Delegate.CreateDelegate(typeof(Func<float>), methodInfo);
                        func = (_c, _t, _p, _r) => { float f = funcF(); *(float*)_r = f; return null; };
                        break;
                    case Type.TYPE_SYSTEM_DOUBLE:
                        Func<double> funcD = (Func<double>)System.Delegate.CreateDelegate(typeof(Func<double>), methodInfo);
                        func = (_c, _t, _p, _r) => { double d = funcD(); *(double*)_r = d; return null; };
                        break;
#if UNITY_5 || UNITY_2017 || UNITY_2018
                    case Type.TYPE_UNITYENGINE_VECTOR2:
                        Func<Vector2> funcV2 = (Func<Vector2>)System.Delegate.CreateDelegate(typeof(Func<Vector2>), methodInfo);
                        func = (_c, _t, _p, _r) => { Vector2 v2 = funcV2(); MarshalFromVector2(_r, ref v2); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_VECTOR3:
                        Func<Vector3> funcV3 = (Func<Vector3>)System.Delegate.CreateDelegate(typeof(Func<Vector3>), methodInfo);
                        func = (_c, _t, _p, _r) => { Vector3 v3 = funcV3(); MarshalFromVector3(_r, ref v3); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_COLOR:
                        Func<Color> funcC = (Func<Color>)System.Delegate.CreateDelegate(typeof(Func<Color>), methodInfo);
                        func = (_c, _t, _p, _r) => { Color c = funcC(); MarshalFromColor(_r, ref c); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_VECTOR4:
                        Func<Vector4> funcV4 = (Func<Vector4>)System.Delegate.CreateDelegate(typeof(Func<Vector4>), methodInfo);
                        func = (_c, _t, _p, _r) => { Vector4 v4 = funcV4(); MarshalFromVector4(_r, ref v4); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_QUATERNION:
                        Func<Quaternion> funcQ = (Func<Quaternion>)System.Delegate.CreateDelegate(typeof(Func<Quaternion>), methodInfo);
                        func = (_c, _t, _p, _r) => { Quaternion q = funcQ(); MarshalFromQuaternion(_r, ref q); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RECT:
                        Func<Rect> funcRC = (Func<Rect>)System.Delegate.CreateDelegate(typeof(Func<Rect>), methodInfo);
                        func = (_c, _t, _p, _r) => { Rect r = funcRC(); MarshalFromRect(_r, ref r); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RECTOFFSET:
                        Func<RectOffset> funcRO = (Func<RectOffset>)System.Delegate.CreateDelegate(typeof(Func<RectOffset>), methodInfo);
                        func = (_c, _t, _p, _r) => { RectOffset ro = funcRO(); MarshalFromRectOffset(_r, ref ro); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_RAY:
                        Func<Ray> funcRY = (Func<Ray>)System.Delegate.CreateDelegate(typeof(Func<Ray>), methodInfo);
                        func = (_c, _t, _p, _r) => { Ray r = funcRY(); MarshalFromRay(_r, ref r); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_BOUNDS:
                        Func<Bounds> funcBD = (Func<Bounds>)System.Delegate.CreateDelegate(typeof(Func<Bounds>), methodInfo);
                        func = (_c, _t, _p, _r) => { Bounds b = funcBD(); MarshalFromBounds(_r, ref b); return null; };
                        break;
                    case Type.TYPE_UNITYENGINE_MATRIX4X4:
                        Func<Matrix4x4> funcM = (Func<Matrix4x4>)System.Delegate.CreateDelegate(typeof(Func<Matrix4x4>), methodInfo);
                        func = (_c, _t, _p, _r) => { Matrix4x4 m = funcM(); MarshalFromMatrix4x4(_r, ref m); return null; };
                        break;
#endif

                }
            }

            if (func == null)
                func = GetGenericTrampoline(pMethodDef, methodInfo);

            return func;
        }

        public static tAsyncCall* CallMethodTrampoline(tJITCallNative* pCallNative, byte* pThis_, byte* pParams, byte* pReturnValue)
        {
            fnInternalCall func = null;
            tMD_MethodDef* pMethodDef = pCallNative->pMethodDef;
            MethodBase methodBase = H.ToObj(pMethodDef->monoMethodInfo) as MethodBase;
            MethodInfo methodInfo = methodBase as MethodInfo;

            if (methodInfo != null) {
                if (MetaData.METHOD_ISSTATIC(pCallNative->pMethodDef)) {
                    func = GetStaticMethodTrampoline(pMethodDef, methodInfo);
                } else {
#if UNITY_5 || UNITY_2017 || UNITY_2018
                    switch (pMethodDef->pParentType->typeInitId) {
                        case Type.TYPE_UNITYENGINE_VECTOR2:
                            func = GetValueTypeTrampoline<Vector2>(pMethodDef, methodInfo, MarshalToVector2, MarshalFromVector2);
                            break;
                        case Type.TYPE_UNITYENGINE_VECTOR3:
                            func = GetValueTypeTrampoline<Vector3>(pMethodDef, methodInfo, MarshalToVector3, MarshalFromVector3);
                            break;
                        case Type.TYPE_UNITYENGINE_QUATERNION:
                            func = GetValueTypeTrampoline<Quaternion>(pMethodDef, methodInfo, MarshalToQuaternion, MarshalFromQuaternion);
                            break;
                        case Type.TYPE_UNITYENGINE_RECT:
                            func = GetValueTypeTrampoline<Rect>(pMethodDef, methodInfo, MarshalToRect, MarshalFromRect);
                            break;
                    }
                    if (func == null) {
                        System.Type targetType = H.ToObj(pMethodDef->pParentType->monoType) as System.Type;
                        if (targetType.FullName == "UnityEngine.MonoBehaviour")
                            func = GetRefTypeTrampoline<MonoBehaviour>(pMethodDef, methodInfo);
                        else if (targetType.FullName == "UnityEngine.Behaviour")
                            func = GetRefTypeTrampoline<Behaviour>(pMethodDef, methodInfo);
                        else if (targetType.FullName == "UnityEngine.Component")
                            func = GetRefTypeTrampoline<Component>(pMethodDef, methodInfo);
                        else if (targetType.FullName == "UnityEngine.Transform")
                            func = GetRefTypeTrampoline<Transform>(pMethodDef, methodInfo);
                        else if (targetType.FullName == "UnityEngine.GameObject")
                            func = GetRefTypeTrampoline<GameObject>(pMethodDef, methodInfo);
                        else if (targetType.FullName == "UnityEngine.Object")
                            func = GetRefTypeTrampoline<UnityEngine.Object>(pMethodDef, methodInfo);
                    }
#endif
                }
            }

            if (func == null)
                func = GetGenericTrampoline(pMethodDef, methodBase);

            pCallNative->pMethodDef->monoMethodCall = new H(func);

            return func(pCallNative, pThis_, pParams, pReturnValue);
        }

        public static tMD_TypeDef* GetTypeForMonoType(System.Type monoType,
            tMD_TypeDef** ppClassTypeArgs, tMD_TypeDef** ppMethodTypeArgs)
        {
            byte* nameSpace = stackalloc byte[256];
            byte* name = stackalloc byte[256];

            if (monoType == null) {
                return null;
            }

            PTR typePtr = 0;
            tMD_TypeDef* pType = null;
            if (MonoType.monoTypes.TryGetValue(monoType, out typePtr)) {
                return (tMD_TypeDef*)typePtr;
            }

            System.TypeCode typeCode = System.Type.GetTypeCode(monoType);

            switch (typeCode) {
                case System.TypeCode.Object:
                    if (monoType == typeof(System.Object))
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_OBJECT];
                    break;
                case System.TypeCode.DBNull:
                    break;
                case System.TypeCode.Boolean:
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_BOOLEAN];
                    break;
                case System.TypeCode.Char:
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_CHAR];
                    break;
                case System.TypeCode.SByte:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_SBYTE];
                    break;
                case System.TypeCode.Byte:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_BYTE];
                    break;
                case System.TypeCode.Int16:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_INT16];
                    break;
                case System.TypeCode.UInt16:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_UINT16];
                    break;
                case System.TypeCode.Int32:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_INT32];
                    break;
                case System.TypeCode.UInt32:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_UINT32];
                    break;
                case System.TypeCode.Int64:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_INT64];
                    break;
                case System.TypeCode.UInt64:
                    if (!monoType.IsEnum)
                        pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_UINT64];
                    break;
                case System.TypeCode.Single:
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_SINGLE];
                    break;
                case System.TypeCode.Double:
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_DOUBLE];
                    break;
                case System.TypeCode.Decimal:
                    break;
                case System.TypeCode.DateTime:
                    break;
                case System.TypeCode.String:
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_STRING];
                    break;
            }

            if (pType == null) {
                if (monoType == typeof(void))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_VOID];
                else if (monoType == typeof(System.Enum))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ENUM];
                else if (monoType == typeof(System.Array))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_NO_TYPE];
                else if (monoType == typeof(object[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_OBJECT];
                else if (monoType == typeof(byte[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_BYTE];
                else if (monoType == typeof(char[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_CHAR];
                else if (monoType == typeof(int[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_INT32];
                else if (monoType == typeof(string[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_STRING];
                else if (monoType == typeof(object[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_OBJECT];
                else if (monoType == typeof(System.Type[]))
                    pType = Type.types[DnaUnity.Type.TYPE_SYSTEM_ARRAY_TYPE];
            }

            if (pType == null && monoType.IsArray) {
                tMD_TypeDef* pElemType = GetTypeForMonoType(monoType.GetElementType(), ppClassTypeArgs, ppMethodTypeArgs);
                pType = Type.GetArrayTypeDef(pElemType, null, null);
            }

            if (pType == null) {
                S.snprintf(nameSpace, 256, monoType.Namespace);
                S.snprintf(name, 256, monoType.Name);
                pType = CLIFile.FindTypeInAllLoadedAssemblies(nameSpace, name);
            }

            if (pType != null) {
                MonoType.monoTypes[monoType] = (PTR)pType;
            }

            return pType;
        }

        public static tMD_TypeDef* GetTypeForMonoObject(object obj,
            tMD_TypeDef** ppClassTypeArgs, tMD_TypeDef** ppMethodTypeArgs)
        {
            if (obj == null) {
                return Type.types[DnaUnity.Type.ELEMENT_TYPE_OBJECT];
            }
            if (obj is DnaObject) {
                byte* ptr = ((DnaObject)obj).dnaPtr;
                if (ptr == null)
                    return Type.types[DnaUnity.Type.ELEMENT_TYPE_OBJECT];
                return Heap.GetType(ptr);
            }
            return GetTypeForMonoType(obj.GetType(), ppClassTypeArgs, ppMethodTypeArgs);
        }

    }
}

