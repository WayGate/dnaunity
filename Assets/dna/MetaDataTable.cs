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

#pragma warning disable CS0169

namespace DnaUnity
{

    public static class MetaDataTable
    {
        public const int MD_TABLE_TYPEREF                  = 0x01;
        public const int MD_TABLE_TYPEDEF                  = 0x02;
        public const int MD_TABLE_FIELDDEF                 = 0x04;
        public const int MD_TABLE_METHODDEF                = 0x06;
        public const int MD_TABLE_PARAM                    = 0x08;
        public const int MD_TABLE_INTERFACEIMPL            = 0x09;
        public const int MD_TABLE_MEMBERREF                = 0x0a;
        public const int MD_TABLE_CONSTANT                 = 0x0b;
        public const int MD_TABLE_CUSTOMATTRIBUTE          = 0x0c;
        public const int MD_TABLE_FIELDMARSHAL             = 0x0d;
        public const int MD_TABLE_CLASSLAYOUT              = 0x0f;
        public const int MD_TABLE_FIELDLAYOUT              = 0x10;
        public const int MD_TABLE_STANDALONESIG            = 0x11;
        public const int MD_TABLE_EVENTMAP                 = 0x12;
        public const int MD_TABLE_EVENT                    = 0x14;
        public const int MD_TABLE_PROPERTYMAP              = 0x15;
        public const int MD_TABLE_PROPERTY                 = 0x17;
        public const int MD_TABLE_METHODSEMANTICS          = 0x18;
        public const int MD_TABLE_METHODIMPL               = 0x19;
        public const int MD_TABLE_MODULEREF                = 0x1a;
        public const int MD_TABLE_TYPESPEC                 = 0x1b;
        public const int MD_TABLE_IMPLMAP                  = 0x1c;
        public const int MD_TABLE_FIELDRVA                 = 0x1d;
        public const int MD_TABLE_ASSEMBLY                 = 0x20;
        public const int MD_TABLE_ASSEMBLYREF              = 0x23;
        public const int MD_TABLE_MANIFESTRESOURCE         = 0x28;
        public const int MD_TABLE_NESTEDCLASS              = 0x29;
        public const int MD_TABLE_GENERICPARAM             = 0x2A;
        public const int MD_TABLE_METHODSPEC               = 0x2B;
        public const int MD_TABLE_GENERICPARAMCONSTRAINT   = 0x2C;
    }

    // First, the combined tables

    public unsafe struct tMDC_ToFieldDef
    {
    	public tMD_FieldDef *pFieldDef;
    }

    public unsafe struct tMDC_ToMethodDef
    {
    	public tMD_MethodDef *pMethodDef;
    }

    public unsafe struct tMDC_ToTypeDef
    {
        public tMD_TypeDef *pTypeDef;
    }


    // Second, the raw metadata tables

    // Table 0x00 - Module
    public unsafe struct tMD_Module
    {
    	// Module name - index into string heap
        public /*STRING*/byte* name;
    	// GUID for module version - index into GUID heap
        public /*GUID_*/byte* mvID;
    }

    // Table 0x01 - TypeRef
    public unsafe struct tMD_TypeRef
    {
    	// Combined
    	public tMD_TypeDef *pTypeDef;

    	// Table index into various tables
        public /*IDX_TABLE*/uint resolutionScope;
        // Padding 64 bit
        public uint padding;
    	// Name of type ref - index into string heap
        public /*STRING*/byte* name;
    	// Namespace of type ref - index into string heap
        public /*STRING*/byte* nameSpace;
    }

    // Table 0x02 - TypeDef
    public unsafe struct tMD_TypeDef
    {
    	// Combined
    	public tMD_TypeDef *pTypeDef;
    	// MetaData pointer
        public tMetaData *pMetaData;

    	// Type attribute flags
        public /*FLAGS32*/uint flags;
        // Type Id of DNA built in types (index into type init array)
        public uint typeInitId;
        // Name of type def - index into string heap
        public /*STRING*/byte* name;
    	// Namespace of type def - index into string heap
        public /*STRING*/byte* nameSpace;
    	// The type that this type extends (inherits from)
        public /*IDX_TABLE*/uint extends;
    	// The first entry in the Field table of the fields of this type def
        public /*IDX_TABLE*/uint fieldList;
    	// The first entry in the Method table of the methods of this type def
        public /*IDX_TABLE*/uint methodList;

    	// Has this entry had its extended info filled?
        public byte isFilled;
    	// Is this the last entry in this table?
        public byte isLast;
    	// Is this a value type?
        public byte isValueType;
    	// The type of evaluation stack entry needed for this type
        public byte stackType;
    	// Total memory size of instances of this type (its in-memory representation) (not static fields)
        public uint instanceMemSize;
        // Data alignment for this type
        public uint alignment;
    	// The parent type definition
        public tMD_TypeDef *pParent;
    	// The virtual method table
        public tMD_MethodDef **pVTable;
    	// The number of virtual methods in the vTable
        public uint numVirtualMethods;
        // Padding 3
        public uint padding3;
    	// Pointer to the memory for any static fields in this type. This will be null if type has no static fields
        public byte* pStaticFields;
    	// Has the static constructor been executed yet?
        public byte isTypeInitialised;
    	// Is this a generic definition (a generic core type)?
        public byte isGenericDefinition;
    	// Is this TypeDef primed - this means that:
    	// numPrimedFields, numPrimedMethods, numVirtualMethods
    	// have been pre-set.
        public byte isPrimed;
        // Is either a mono reference type, or has a mono reference type as a base class
        public byte hasMonoBase;
    	// Padding 4
        public uint padding4;
    	// If this type has a static constructor, then store it here. null if no static constructor
        public tMD_MethodDef *pStaticConstructor;
    	// The size of this type when in an array
        public uint arrayElementSize;
    	// The size of this type when on the stack (or in a field)
        public uint stackSize;
    	// How many interfaces does this type implement
        public uint numInterfaces;
        // Padding 5
        public uint padding5;
    	// All interfaces that this type implements are mapped here
        public tInterfaceMap *pInterfaceMaps;
    	// The original table index of this TypeDef
        public /*IDX_TABLE*/uint tableIndex;
        // Padding 6
        public uint padding6;
    	// If this is a generic type definition, then store any instantiatations here (in a linked list)
        public tGenericInstance *pGenericInstances;
    	// If this is a generic instance, then store link to its core definition type
        public tMD_TypeDef *pGenericDefinition;
    	// If this is a generic instance, then store the class type args
        public tMD_TypeDef **ppClassTypeArgs;
    	// If this type is System.Array, then this stores the element type
        public tMD_TypeDef *pArrayElementType;
    	// The number of fields in this type. This includes and static fields, but not inherited fields
        public uint numFields;
        // Padding 7
        public uint padding7;
    	// Links to all the fields (in memory order), including statics (not inherited)
        public tMD_FieldDef **ppFields;
    	// The memory needed for static fields, in bytes
        public uint staticFieldSize;
    	// The number of methods in this type. This includes static methods, but not inherited methods
        public uint numMethods;
    	// Links to all method in this type, including statics, not inherited
        public tMD_MethodDef **ppMethods;
    	// If this is a nested class, this records which type it is nested within.
        public tMD_TypeDef *pNestedIn;
    	// If this type has a finalizer, point to it here
        public tMD_MethodDef *pFinalizer;
    	// Pointer to the heap object which is the Type class object for this type.
    	// This is only allocated as needed, so defaults to null
        public /*HEAP_PTR*/byte* typeObject;
        // The GCHandle to a MonoType object if this is a wrapper around a mono type, null if a native type
        public /*GCHandle*/ void* monoType;

        #if DEBUG
        public string nameS { get { return S.str(name); } }
        public string nameSpaceS { get { return S.str(nameSpace); } }
        #endif
    }

#if NO
    public unsafe struct TypeDefPtr
    {
        private tMD_TypeDef* td;

        public TypeDefPtr(tMD_TypeDef* p)
        {
            td = p;
        }

        // MetaData pointer
        //public tMetaData* pMetaData;

        // Type attribute flags
        public /*FLAGS32*/uint flags { get { return td->flags; } }
        // Type Id of DNA built in types (index into type init array)
        public uint typeInitId { get { return td->typeInitId; } }
        // Name of type def - index into string heap
        public string name { get { return S.str(td->name); } }
        // Namespace of type def - index into string heap
        public string nameSpace { get { return S.str(td->nameSpace); } }
        // The type that this type extends (inherits from)
        public /*IDX_TABLE*/uint extends { get { return td->extends; } }
        // The first entry in the Field table of the fields of this type def
        public /*IDX_TABLE*/uint fieldList { get { return td->fieldList; } }
        // The first entry in the Method table of the methods of this type def
        public /*IDX_TABLE*/uint methodList { get { return td->methodList; } }

        // Has this entry had its extended info filled?
        public byte isFilled { get { return td->isFilled; } }
        // Is this the last entry in this table?
        public byte isLast { get { return td->isLast; } }
        // Is this a value type?
        public byte isValueType { get { return td->isValueType; } }
        // The type of evaluation stack entry needed for this type
        public byte stackType { get { return td->stackType; } }
        // Total memory size of instances of this type (its in-memory representation) (not static fields)
        public uint instanceMemSize { get { return td->instanceMemSize; } }
        // Data alignment for this type
        public uint alignment {  get { return td->alignment; } }
        // The parent type definition
        public object pParent { get { return td->pParent != null ? (object)(new TypeDefPtr(td->pParent)) : null; } }
        // The virtual method table
        //public tMD_MethodDef** pVTable;
        // The number of virtual methods in the vTable
        public uint numVirtualMethods { get { return td->numVirtualMethods; } }
        // Padding 3
        //public uint padding3;
        // Pointer to the memory for any static fields in this type. This will be null if type has no static fields
        //public byte* pStaticFields;
        // Has the static constructor been executed yet?
        public byte isTypeInitialised {  get { return td->isTypeInitialised; } }
        // Is this a generic definition (a generic core type)?
        public byte isGenericDefinition {  get { return td->isGenericDefinition; } }
        // Is this TypeDef primed - this means that:
        // numPrimedFields, numPrimedMethods, numVirtualMethods
        // have been pre-set.
        public byte isPrimed {  get { return td->isPrimed; } }
        // Is either a mono reference type, or has a mono reference type as a base class
        public byte hasMonoBase { get { return td->hasMonoBase; } }
        // Padding 4
        // public uint padding4;
        // If this type has a static constructor, then store it here. null if no static constructor
        // public tMD_MethodDef* pStaticConstructor;
        // The size of this type when in an array
        public uint arrayElementSize {  get { return td->arrayElementSize; } }
        // The size of this type when on the stack (or in a field)
        public uint stackSize { get { return td->stackSize; } }
        // How many interfaces does this type implement
        public uint numInterfaces { get { return td->numInterfaces; } }
        // Padding 5
        //public uint padding5;
        // All interfaces that this type implements are mapped here
        //public tInterfaceMap* pInterfaceMaps;
        // The original table index of this TypeDef
        //public /*IDX_TABLE*/uint tableIndex;
        // Padding 6
        //public uint padding6;
        // If this is a generic type definition, then store any instantiatations here (in a linked list)
        //public tGenericInstance* pGenericInstances;
        // If this is a generic instance, then store link to its core definition type
        //public tMD_TypeDef* pGenericDefinition;
        // If this is a generic instance, then store the class type args
        //public tMD_TypeDef** ppClassTypeArgs;
        // If this type is System.Array, then this stores the element type
        //public tMD_TypeDef* pArrayElementType;
        // The number of fields in this type. This includes and static fields, but not inherited fields
        public uint numFields { get { return td->numFields; } }
        // Padding 7
        //public uint padding7;
        // Links to all the fields (in memory order), including statics (not inherited)
        public object[] ppFields
        {
            get
            {
                object[] fields = new object[td->numFields];
                for (int i = 0; i < td->numFields; i++)
                {
                    fields[i] = new FieldDefPtr(td->ppFields[i]);
                }
                return fields;
            }
        }
        // The memory needed for static fields, in bytes
        public uint staticFieldSize { get { return td->staticFieldSize; } }
        // The number of methods in this type. This includes static methods, but not inherited methods
        public uint numMethods { get { return td->numMethods; } }
        // Links to all method in this type, including statics, not inherited
        public object[] ppMethods
        {
            get
            {
                object[] methods = new object[td->numMethods];
                for (int i = 0; i < td->numMethods; i++)
                {
                    methods[i] = new MethodDefPtr(td->ppMethods[i]);
                }
                return methods;
            }
        }
        // If this is a nested class, this records which type it is nested within.
        public object pNestedIn { get { return (td->pNestedIn != null) ? (object)(new TypeDefPtr(td->pNestedIn)) : null; }  }
        // If this type has a finalizer, point to it here
        public object pFinalizer { get { return (td->pFinalizer != null) ? (object)(new MethodDefPtr(td->pFinalizer)) : null; } }
        // Pointer to the heap object which is the Type class object for this type.
        // This is only allocated as needed, so defaults to null
        //public /*HEAP_PTR*/byte* typeObject 
        // The GCHandle to a MonoType object if this is a wrapper around a mono type, null if a native type
        public System.Type monoType { get { return (td->monoType != null) ? (System.Type)(H.ToObj(td->monoType)) : null;  } }
    }
#endif

    // Table 0x04 - FieldDef
    public unsafe struct tMD_FieldDef
    {
    	// Combined
    	public tMD_FieldDef *pFieldDef;
    	// MetaData pointer
        public tMetaData *pMetaData;

    	// Flags - FieldAttributes
        public /*FLAGS16*/ushort flags;
    	// Padding dummy entry
        public short padding0;
        // Padding dummy entry
        public uint padding1;
        // Name of the field
        public /*STRING*/byte* name;
    	// Signature of the field
        public /*BLOB_*/byte* signature;

    	// The type of this field
        public tMD_TypeDef *pType;
    	// The type that contains this field
        public tMD_TypeDef *pParentType;
    	// The field offset within its containing type
        public uint memOffset;
    	// The size in bytes that this field takes up in the memory representation
        public uint memSize;
    	// The original table index of this FieldDef
        public /*IDX_TABLE*/uint tableIndex;
        // Padding dummy entry
        public uint padding2;
    	// If this is a static field, then the absolute address of this field is stored here.
    	// If this field has an RVA, then the pointer to the memory location is stored here.
    	// If this is a literal field, then this is a pointer to the tMD_Constant literal definition.
        public byte* pMemory;
        // The GCHandle to the mono Reflection.FieldInfo for field if this is mono type, null if not
        public /*GCHandle*/ void* monoFieldInfo;
        // The GCHandle to field getter method if this is mono type, null if not
        public /*GCHandle*/ void* monoGetter;
        // The GCHandle to field setter method if this is mono type, null if not
        public /*GCHandle*/ void* monoSetter;

#if DEBUG
        public string nameS { get { return S.str(name); } }
#endif
    }

#if NO
    public unsafe struct FieldDefPtr
    {
        private tMD_FieldDef* fd;

        public FieldDefPtr(tMD_FieldDef* p)
        {
            fd = p;
        }

        // MetaData pointer
        //public tMetaData* pMetaData;

        // Flags - FieldAttributes
        public /*FLAGS16*/ushort flags { get { return fd->flags; } }
        // Name of the field
        public string name { get { return S.str(fd->name); } }
        // Signature of the field
        public /*BLOB_*/byte* signature {  get { return fd->signature; } }
        // The type of this field
        public object pType {  get { return fd->pType != null ? (object)(new TypeDefPtr(fd->pType)) : null; } }
        // The type that contains this field
        public object pParentType { get { return fd->pParentType != null ? (object)(new TypeDefPtr(fd->pParentType)) : null; } }
        // The field offset within its containing type
        public uint memOffset {  get { return fd->memOffset; } }
        // The size in bytes that this field takes up in the memory representation
        public uint memSize { get { return fd->memSize; } }
        // The original table index of this FieldDef
        public /*IDX_TABLE*/uint tableIndex { get { return fd->tableIndex; } }
        // Padding dummy entry
        //public uint padding2;
        // If this is a static field, then the absolute address of this field is stored here.
        // If this field has an RVA, then the pointer to the memory location is stored here.
        // If this is a literal field, then this is a pointer to the tMD_Constant literal definition.
        public byte* pMemory {  get { return fd->pMemory; } }
        // The GCHandle to the mono Reflection.FieldInfo for field if this is mono type, null if not
        public /*GCHandle*/ object monoFieldInfo { get { return H.ToObj(fd->monoFieldInfo); } }
        // The GCHandle to field getter method if this is mono type, null if not
        public /*GCHandle*/ object monoGetter {  get { return H.ToObj(fd->monoGetter); } }
        // The GCHandle to field setter method if this is mono type, null if not
        public /*GCHandle*/ object monoSetter {  get { return H.ToObj(fd->monoSetter); } }
    }
#endif

    // Table 0x06 - MethodDef
    public unsafe struct tMD_MethodDef
    {
    	// Combined
    	public tMD_MethodDef *pMethodDef;
    	// MetaData pointer
        public tMetaData *pMetaData;

    	// RVA converted to pointer. Code for this method
        public byte *pCIL;
    	// Flags - MethodImplAttributes
        public /*FLAGS16*/ushort implFlags;
    	// Flags - MethodAttribute
        public /*FLAGS16*/ushort flags;
        // Padding
        public uint padding0;
    	// Name of method
        public /*STRING*/byte* name;
    	// Signature of method
        public /*BLOB_*/byte* signature;
    	// The first entry in the Param table of the parameters of this method def
        public /*IDX_TABLE*/uint paramList;
        // Padding
        public uint padding1;
    	// If this method has been JITted, then this points to it
        public tJITted *pJITted;
    	// Has the extra infomation in this method been filled in yet?
        public byte isFilled;
    	// Set true if this method has generic parameters
        public byte isGenericDefinition;
    	// The number of parameters for this method. This includes the 'this' parameter if non-static method
        public ushort numberOfParameters;
        // Padding
        public uint padding2;
    	// The parameter information for this method, including the 'this' parameter if non-static method
        public tParameter *pParams;
    	// The size in bytes needed for the parameters, including the 'this' parameter if non-static method
        public uint parameterStackSize;
        // Padding
        public uint padding3;
    	// The method return type
        public tMD_TypeDef *pReturnType;
    	// The type that this method is a part of
        public tMD_TypeDef *pParentType;
    	// The original table index of this MethodDef
        public /*IDX_TABLE*/uint tableIndex;
    	// If this is a virtual method then this contains the offset into the vTable for this method.
    	// This offset is the table index - not the byte offset.
        public uint vTableOfs;
    	// If this is method has generic parameters, then store the method type args
        public tMD_TypeDef **ppMethodTypeArgs;
    	// If this is a generic core method, then store type instances here.
        public tGenericMethodInstance *pGenericMethodInstances;
        // The GCHandle to the mono Reflection.MethodInfo for method if this is mono type, null if not
        public /*GCHandle*/ void* monoMethodInfo;
        // The GCHandle to the mono call delegate for method if this is mono type, null if not
        public /*GCHandle*/ void* monoMethodCall;

#if DIAG_METHOD_CALLS
    	// Number of times this method has been called
        public uint callCount;
        // Padding
        public uint padding4;
    	// Total time (inclusive of children) in this function
        public ulong totalTime;
#endif

#if DEBUG
        public string nameS { get { return S.str(name); } }
#endif
    }

#if NO
    public unsafe struct MethodDefPtr
    {
        private tMD_MethodDef* md;

        public MethodDefPtr(tMD_MethodDef* p)
        {
            md = p;
        }

        // MetaData pointer
        //public tMetaData* pMetaData;

        // RVA converted to pointer. Code for this method
        public byte* pCIL { get { return md->pCIL; } }
        // Flags - MethodImplAttributes
        public /*FLAGS16*/ushort implFlags { get { return md->implFlags; } }
        // Flags - MethodAttribute
        public /*FLAGS16*/ushort flags { get { return md->flags; } }
        // Padding
        //public uint padding0;
        // Name of method
        public string name { get { return S.str(md->name); } }
        // Signature of method
        public /*BLOB_*/byte* signature { get { return md->signature; } }
        // The first entry in the Param table of the parameters of this method def
        public /*IDX_TABLE*/uint paramList { get { return md->paramList; } }
        // Padding
        //public uint padding1;
        // If this method has been JITted, then this points to it
        public tJITted* pJITted { get { return md->pJITted; } }
        // Has the extra infomation in this method been filled in yet?
        public byte isFilled { get { return md->isFilled; } }
        // Set true if this method has generic parameters
        public byte isGenericDefinition { get { return md->isGenericDefinition; } }
        // The number of parameters for this method. This includes the 'this' parameter if non-static method
        public ushort numberOfParameters { get { return md->numberOfParameters; } }
        // Padding
        //public uint padding2;
        // The parameter information for this method, including the 'this' parameter if non-static method
        public tParameter* pParams { get { return md->pParams; } }
        // The size in bytes needed for the parameters, including the 'this' parameter if non-static method
        public uint parameterStackSize {  get { return md->parameterStackSize; } }
        // Padding
        // public uint padding3;
        // The method return type
        public object pReturnType {  get { return md->pReturnType != null ? (object)(new TypeDefPtr(md->pReturnType)) : null; } }
        // The type that this method is a part of
        public object pParentType { get { return md->pParentType != null ? (object)(new TypeDefPtr(md->pParentType)) : null; } }
        // The original table index of this MethodDef
        public /*IDX_TABLE*/uint tableIndex { get { return md->tableIndex; } }
        // If this is a virtual method then this contains the offset into the vTable for this method.
        // This offset is the table index - not the byte offset.
        public uint vTableOfs {  get { return md->vTableOfs; } }
        // If this is method has generic parameters, then store the method type args
        public tMD_TypeDef** ppMethodTypeArgs {  get { return md->ppMethodTypeArgs; } }
        // If this is a generic core method, then store type instances here.
        public tGenericMethodInstance* pGenericMethodInstances {  get { return md->pGenericMethodInstances; } }
        // The GCHandle to the mono Reflection.MethodInfo for method if this is mono type, null if not
        public object monoMethodInfo {  get { return H.ToObj(md->monoMethodInfo); } }
        // The GCHandle to the mono call delegate for method if this is mono type, null if not
        public object monoMethodCall { get { return H.ToObj(md->monoMethodCall); } }
    }
#endif

    // Table 0x08 - Param
    public unsafe struct tMD_Param
    {
    	// Flags - ParamAttributes
        public /*FLAGS16*/ushort flags;
    	// The sequence number of the parameter. 0 is the return value, 1+ are the parameters
        public ushort sequence;
        // Padding
        public uint padding;
    	// The name of the parameter (optional)
        public /*STRING*/byte* name;
    };

    // Table 0x09 - InterfaceImpl
    public unsafe struct tMD_InterfaceImpl
    {
    	// The class that implements...
        public /*IDX_TABLE*/uint class_;
    	// ...this interface
        public /*IDX_TABLE*/uint interface_;
    };

    // Table 0x0A - MemberRef
    public unsafe struct tMD_MemberRef
    {
        // Combined (tMD_MethodDef and tMD_FieldDef)
        public tMD_MethodDef *pMethodOrFieldDef;
    //	tMD_FieldDef *pFieldDef;

    	// Type of member, coded index: MemberRefParent
        public /*IDX_TABLE*/uint class_;
        // Padding
        public uint padding;
    	// Name of the member
        public /*STRING*/byte* name;
    	// Signature of the member
        public /*BLOB_*/byte* signature;
    };

    // Table 0x0B - Constant
    public unsafe struct tMD_Constant
    {
    	// The ELEMENT_TYPE of the constant - 'void' is Type.ELEMENT_TYPE_CLASS with a 0 blob index
    	public byte type;
    	// Padding
    	public fixed byte padding0[3];
    	// The parent of this constant - HasConstant encoded table index
        public /*IDX_TABLE*/uint parent;
    	// The value of the constant, index in the BLOB heap
        public /*BLOB_*/byte* value;
    };

    // Table 0x0C - CustomAttribute
    public unsafe struct tMD_CustomAttribute
    {
    	// Parent
        public /*IDX_TABLE*/uint parent;
    	// Type
        public /*IDX_TABLE*/uint type;
    	// value of attribute
        public /*BLOB_*/byte* value;
    };

    // Table 0x0D - FieldMarshal
    public unsafe struct tMD_FieldMarshal
    {
        // Parent
        public /*IDX_TABLE*/uint parent;
        // Padding 
        public uint padding;
        // value of attribute
        public /*BLOB_*/byte* nativeType;
    };

    // Table 0x0E - DeclSecurity
    public unsafe struct tMD_DeclSecurity
    {
    	// The security action
        public ushort action;
    	// Padding
        public ushort padding0;
    	// The parent typedef, methoddef or assembly of this security info - HasDeclSecurity coded index
        public /*IDX_TABLE*/uint parent;
    	// The security permission set
        public /*BLOB_*/byte* permissionSet;
    };

    // Table 0x0F - ClassLayout
    public unsafe struct tMD_ClassLayout
    {
    	// The packing size
        public ushort packingSize;
    	// Padding
        public ushort padding;
    	// The class size
        public uint classSize;
    	// The parent TypeDef
        public /*IDX_TABLE*/uint parent;
    };

    // Table 0x10 - FieldLayout
    public unsafe struct tMD_FieldLayout
    {
        // The field offset
        public uint offset;
        // The field index
        public /*IDX_TABLE*/uint field;
    };

    // Table 0x11 - StandAloneSig
    public unsafe struct tMD_StandAloneSig
    {
        public /*BLOB_*/byte* signature;
    }

    // Table 0x12 - EventMap
    public unsafe struct tMD_EventMap
    {
    	// Index into TypeDef table
        public /*IDX_TABLE*/uint parent;
    	// Index into Event table. Marks the start of a continuous run of events owned by this type.
        public /*IDX_TABLE*/uint eventList;
    };

    // Table 0x14 - Event
    public unsafe struct tMD_Event
    {
    	// Flags of type eventAttributes
        public /*FLAGS16*/ushort eventFlags;
    	// Padding
        public ushort padding0;
        // Padding
        public uint padding1;
    	// The name of the event
        public /*STRING*/byte* name;
    	// The type of this event. A TypeDefOrRef index. This is NOT the type to which this event belongs.
        public /*IDX_TABLE*/uint eventType;
        // Padding
        public uint padding2;
    };

    // Table 0x15 - PropertyMap

    public unsafe struct tMD_PropertyMap
    {
    	// Parent - index into TypeDef table
        public /*IDX_TABLE*/uint parent;
    	// PropertyList - index into Property table
        public /*IDX_TABLE*/uint propertyList;
    };

    // Table 0x17 - Property
    public unsafe struct tMD_Property
    {
    	// Flags - PropertyAttributes
        public /*FLAGS16*/ushort flags;
    	// Padding dummy entry
        public short padding0;
        // Padding dummy entry
        public int padding1;
    	// Name
        public /*STRING*/byte* name;
    	// The type signature
        public /*BLOB_*/byte* typeSig;
    };

    // Table 0x18 - MethodSemantics
    public unsafe struct tMD_MethodSemantics
    {
    	// semantics flags - MethodSemanticsAttributes
        public /*FLAGS16*/ushort semantics;
    	// Padding dummy entry
        public short padding;
    	// method - entry into MethodDef table
        public /*IDX_TABLE*/uint method;
    	// HasSemantics coded entry - index into Event or Property tables
        public /*IDX_TABLE*/uint association;
    }

    // Table 0x19 - MethodImpl
    public unsafe struct tMD_MethodImpl
    {
    	// Index into TypeDef table
        public /*IDX_TABLE*/uint class_;
    	// The method to use as the interface implementation. Coded index MethodDefOrRef
        public /*IDX_TABLE*/uint methodBody;
    	// The method declaration that is being overriden. Coded index MethodDefOrRef
        public /*IDX_TABLE*/uint methodDeclaration;
    }

    // Table 0x1A - ModuleRef
    public unsafe struct tMD_ModuleRef
    {
    	// The module name referenced
        public /*STRING*/byte* name;
    }

    // Table 0x1B - TypeSpec
    public unsafe struct tMD_TypeSpec
    {
    	// Combined
        public tMD_TypeDef *pTypeDef;
    	// MetaData pointer
        public tMetaData *pMetaData;

    	// The signature of the type
        public /*BLOB_*/byte* signature;
    }

    // Table 0x1C - ImplMap
    public unsafe struct tMD_ImplMap
    {
    	// Mapping flags of type PInvokeAttributes
        public ushort mappingFlags;
    	// padding
        public ushort padding1;
    	// A MemberForwarded coded index, specifying which member is forwarded. Note that only members are allowed.
        public /*IDX_TABLE*/uint memberForwarded;
    	// The import name
        public /*STRING*/byte* importName;
    	// The module ref (scope) of the import
        public /*IDX_TABLE*/uint importScope;
        // Padding
        public uint padding2;
    }

    // Table 0x1D - FieldRVA
    public unsafe struct tMD_FieldRVA
    {
    	// The RVA pointer of the initial data for the field
        public byte* rva;
    	// Index into the field table
        public /*IDX_TABLE*/uint field;
        // Padding
        public uint padding;
    }

    // Table 0x20 - Assembly
    public unsafe struct tMD_Assembly
    {
    	// Hash algorithm ID of type AssemblyHashAlgorithm
        public uint hashAlgID;
    	// Version info
        public ushort majorVersion, minorVersion, buildNumber, revisionNumber;
    	// Flags - AssemblyFlags
        public /*FLAGS32*/uint flags;
    	// Public key
        public /*BLOB_*/byte* publicKey;
    	// Name
        public /*STRING*/byte* name;
    	// Culture
        public /*STRING*/byte* culture;
    }

    // Table 0x23 - AssemblyRef
    public unsafe struct tMD_AssemblyRef
    {
    	// Version info
        public ushort majorVersion, minorVersion, buildNumber, revisionNumber;
    	// Flags - AssemblyFlags
        public /*FLAGS32*/uint flags;
        // Padding
        public uint padding;
    	// Public key or token
        public /*BLOB_*/byte* publicKeyOrToken;
    	// Name
        public /*STRING*/byte* name;
    	// Culture
        public /*STRING*/byte* culture;
    	// Hash value
        public /*BLOB_*/byte* hashValue;
    }

    // Table 0x28 - ManifestResource
    public unsafe struct tMD_ManifestResource
    {
        // Resource offset
        public /*IDX_TABLE*/uint offset;
        // Manifest resource flags (1 public, 2 private)
        public /*IDX_TABLE*/uint flags;
        // Name
        public /*STRING*/byte* name;
        // A coded index into File, AssemblyRef, or ExportedType table
        public /*IDX_TABLE*/uint implementation;
        // Padding
        public uint padding;
    }

    // Table 0x29 - NestedClass
    public unsafe struct tMD_NestedClass
    {
    	// The TypeDef of the class that is nested
        public /*IDX_TABLE*/uint nestedClass;
    	// The TypeDef of the class in which nestedClass is enclosed
        public /*IDX_TABLE*/uint enclosingClass;
    }

    // Table 0x2A - Generic param
    public unsafe struct tMD_GenericParam
    {
    	// The number of this generic parameter. Numbered left-to-right, starting from 0
        public ushort number;
    	// Flags - GenericParamAttributes
        public /*FLAGS16*/ushort flags;
    	// Owner - the TypeDef or MethodDef that owns this parameter - TypeOrMethodDef coded index
        public /*IDX_TABLE*/uint owner;
    	// The name of the parameter
        public /*STRING*/byte* name;
    }

    // Table 0x2B - MethodSpec
    public unsafe struct tMD_MethodSpec
    {
    	// Combined
        public tMD_MethodDef *pMethodDef;
    	// MetaData pointer
        public tMetaData *pMetaData;

    	// Index into MethodDef or MethodRef specifying which method this spec refers to
        public /*IDX_TABLE*/uint method;
        // Padding
        public uint padding;
    	// Index into blob heap, holding the signature of this instantiation
        public /*BLOB_*/byte* instantiation;
    }

    // Table 0x2C - GenericParamConstraint
    public unsafe struct tMD_GenericParamConstraint
    {
    	// The generic param that this constraint applies to
    	public tMD_GenericParam *pGenericParam;
    	// The type of the constraint (coded index TypeDefOrRef)
        public /*IDX_TABLE*/uint constraint;
        // Padding
        public uint padding;
    }

}
