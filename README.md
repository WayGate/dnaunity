# dnaunity

*Current status: WIP*

- [x] C version modified to run in 32/64 bit mode correctly
- [x] All C code converted to C# (still basically C, just written with C# unsafe code)
- [x] Successfully builds in Unity
- [x] Successfully builds with IL2CPP for iOS target and WebGL target
- [x] Stub functions (memory, string, gchandle, sprintf, printf, etc.) minimally working (no NotImplementedExceptions)
- [X] CLIFile loading works loading a HelloWorld assembly.
- [X] Basic metadata tests pass.
- [X] Simple call method test works.
- [X] Simple "HelloWorld" prints to Unity console.
- [X] Working mapping of Mono/Unity API's, including working MonoBehaviors.
- [X] 32 bit and 64 bit mode tested/works (unit test results match)
- [X] Basic bootstrap unit test suite runs - most tests pass
- [X] Serialization from DNA and Mono objs to unity compatible data (byte[] array + List<UnityEngine.Object> + List<TypeInfo>)
- [ ] Deserialization from Unity data (byte[] array + List<UnityEngine.Object> + List<TypeInfo>) back to dna objects
- [ ] DnaMonoBehavior auto-serialization fully supported just like MonoBehavior
- [ ] Nice DnaMonoBehavior editor which displays state of script fields
- [ ] Utility to convert existing scenes/prefabs to DnaScript scenes/prefabs
- [ ] Comprehensive unit test suite runs - tests for all supported features pass
- [ ] Production ready
 
DotNetAnywhere interpreter ported to C#/Unity to run well with either Mono or IL2CPP.  Allows .NET assemblies
to be loaded dynamically from files or as asset bundles on the fly without having to be compiled into the 
original executable.

Details:

- Unlike original DNA, supports both 32 bit (WebGL) and 64 bit (iOS, Android, Windows, Mac) mem models.

- Runs as C# unsafe code.  Basically C code of DNA ported unmodified to C# unsafe code.

- Runnable/debuggable from within Unity/Visual Studio/Monodevelop without having to deal with C/.NET interop.

- Mixes managed/unsafe code to allow more flexible interop/access to mono/Unity classes.

- Implements Array, String, and other types beyond the basic types as wrappers around Mono/IL2CPP's
  version of those types.
  
- Uses fast call wrappers of most unity/mono classes/structs.  Falls back to slow System.Reflection wrappers for
  uncommon classes/structs (but tries to avoid obj allocations).
  
- Uses whitelist/blacklist to expose/hide Unity API's for security.  Defaults to highest level of security.

- No change to existing code.  Scripted MonoBehavior derived classes just work the same way they do in Unity.

- Simple one step click conversion of standard mono scene to scripted scene, where any monobehaviros defined in
  DLL assemblies marked as runtime scripted assemblies are converted to scripted monobehaviors.


