---
level: E
title: Binaryen Interop
authors: ['trippwill']
date: 2025-09-08
status: draft
---

# Proposal: Integrate Binaryen.NET (RID-aware libbinaryen bindings) into brimlang/brim

## Objective
Host a first-party, NuGet-packable, RID-aware **Binaryen.NET** binding inside the `brim` monorepo so we can iterate on helpers (SafeHandles, high-level helpers) alongside the Brim toolchain.

## High-level plan
1. **Repository layout**
   ```text
   brim/
     dotnet/
       Binaryen.NET/                 # managed package (NuGet)
         Binaryen.NET.csproj
         Binaryen/
           Native/BinaryenPInvoke.cs
           Handles/ModuleHandle.cs
           Module.cs
         runtimes/<rid>/native/<libbinaryen.*>
       Binaryen.NET.Tests/           # basic tests
     native/
       binaryen/                     # scripts & cmake presets to build libbinaryen per RID
         build.ps1
         build.sh
         CMakePresets.json
     .github/workflows/binaryen-dotnet.yml
   ```

2. **Package characteristics**
   - `PackageId`: `Binaryen.NET`
   - `TargetFramework`: `net8.0`
   - Modern P/Invoke with `[LibraryImport("binaryen")]` and `SafeHandle` types.
   - Ships native binaries under `runtimes/<rid>/native/` for: `win-x64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`.
   - Helpers: `Module.Create()`, `Module.Optimize()`, `Module.Write()` (two-phase write), later more builders/passes.
   - License: Apache-2.0 (adjust if needed).

3. **CI & distribution**
   - GitHub Actions matrix builds **libbinaryen** from source via CMake per RID, drops artifacts into `runtimes/`.
   - `dotnet pack` produces `.nupkg` + `.snupkg`; published on tag.
   - Optional: publish to NuGet.org using `NUGET_API_KEY` secret; always publish to GitHub Packages.

4. **Versioning policy**
   - `Binaryen.NET` versions track Binaryen commit (in metadata) and Brim toolchain minor: e.g., `0.1.0` with `binaryen: <git-sha>` in `abi.json` (later).

5. **Why in-repo**
   - Enables rapid iteration for our Brim Binaryen helpers while we build the core emitter.
   - Single PR surfaces both managed wrappers and native build changes.

## Risks & mitigations
- **ABI drift in Binaryen**: Pin to a known commit; encode commit in CI and package metadata; add a weekly “update binaryen” chore.
- **RID coverage**: Start with five RIDs; add more on demand.
- **Size**: Shared libs can be large; consider `strip` symbols in CI for release builds.

## Milestones
- **M0**: Skeleton + CI scaffold compiles and packs with placeholder natives.
- **M1**: Build real libbinaryen for linux-x64 and win-x64; tests create/opt/write successfully.
- **M2**: Add osx-x64, osx-arm64, linux-arm64; publish pre-release `0.1.0-alpha`.
- **M3**: Expand P/Invoke surface; docs & samples; `0.1.0`.
