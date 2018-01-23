# dnaunity

*Current status: WIP*

- [x] All C code converted to C#
- [x] Builds in Unity
- [ ] Stub functions (memory, string, gchandle, sprintf, printf, etc.) minimally working (no NotImplementedExceptions)
- [ ] CLIFile load works loading a HelloWorld assembly.
- [ ] Basic metadata tests pass.
- [ ] String, Array, Type, other basic internal .NET types switched to Mono versions.
- [ ] Simple "HelloWorld" prints to console.
- [ ] Wrapping of basic external classes and structs works (COW value types works)
- [ ] Fastcall and slowcall to Mono/Unity methods works.
- [ ] Basic mapping of Mono/Unity API's works for both fast/slow calling.
- [ ] 32 bit and 64 bit mode works
- [ ] Basic unit test suite runs
- [ ] More extensive unit test suite runs
- [ ] Utility to convert existing scenes/prefabs to scenes/prefabs which interface with loadable scripts working.
- [ ] Full unit test suite runs
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
  
- Wraps unity/mono structs as Copy on Write (COW) boxed objects in the DNA stack for simplicity.

- Uses fast internal call wrappers of most unity/mono classes/structs.  Falls back to slow System.Reflection wrappers for
  uncommon classes/structs.
  
- Requires whitelist of objects to expose.

Further Work:

- Support overridding MonoBehavior through DnaScript component.

- Support auto-serialization of script component properties in existing scenes/prefabs through DnaScript component byte[] data fields.

- Utility code to convert existing scene with standard MonoBehaviours to scene with DnaScript components.
