# dnaunity

*Current status: WIP*

DotNetAnywhere interpreter ported to C#/Unity to run well with either Mono or IL2CPP.  Allows C# script assemblies
to be loaded dynamically from asset bundles.

Some details:

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

