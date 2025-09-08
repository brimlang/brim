---
level: E
title: Host Stubs
authors: ['trippwill']
date: 2025-09-08
status: draft
---
# Proposal: Leveraging componentize-dotnet for Brim Host Stubs

## Motivation
Brim components lower to **WIT + WasmGC**. To make them easy to host from .NET, we need a path for generating host stubs. Instead of maintaining our own C# binding generator, we can leverage the existing **componentize-dotnet** toolchain.

## Background
- Brim compiler (`brimc`) emits `package.wit` and `component.wasm` artifacts【brim_wasm_wit_interop.md】.
- componentize-dotnet takes WIT packages and produces idiomatic C# bindings and glue projects.
- Wasmtime’s .NET API (`Wasmtime.Component`) can instantiate components and wire WASI imports directly.

## Proposal
1. **Default hosting stub for .NET**
   - Brim provides a `brim host --target dotnet` command.
   - This command shells out to `componentize-dotnet` when available, passing in the `package.wit` emitted by `brimc`.
   - The result is a C# project with strongly-typed bindings for the component exports/imports.

2. **Modes of use**
   - **Native Host Stub (lightweight)**: Brim emits a minimal `Program.cs` using Wasmtime.Component to run the module. This works without `componentize-dotnet` and is ideal for dogfooding.
   - **Componentize Bindings (full)**: If `--use-componentize` is specified, Brim invokes `componentize-dotnet` to generate rich bindings from the WIT package. This produces idiomatic async C# methods and project scaffolding.

3. **Example Flow**
   ```bash
   brim build hello.brim
   # emits ./out/hello.wasm + ./out/package.wit

   # Generate .NET host stub
   brim host --target dotnet --use-componentize --out ./hosts/hello-dotnet

   # Result
   hosts/hello-dotnet/
     Hello.Host.csproj
     GeneratedBindings.cs   # from componentize-dotnet
     Program.cs             # demo entrypoint
   ```

   `Program.cs` would look like:
   ```csharp
   using Hello; // generated namespace

   var host = new HelloApi();
   var result = host.Greet("Brim");
   Console.WriteLine(result);
   ```

4. **Benefits**
   - **No duplication**: Brim doesn’t need its own WIT→C# binding generator.
   - **Familiarity**: .NET developers use idiomatic async/Task-based APIs.
   - **Extensible**: Early adopters can add their own `.NET` components via the same toolchain.

5. **Decisions**
   - Brim will ship a `brim host --target dotnet` command that defaults to a minimal Wasmtime host.
   - Optionally, with `--use-componentize`, Brim integrates with `componentize-dotnet` for richer stubs.
   - Hosting stubs are intended for **dogfooding and early adoption**, not long-term production runtime support.

6. **Deferrals**
   - Exact CLI flags (`--use-componentize`, `--mode`) can be finalized when wiring prototype.
   - Version pinning for `componentize-dotnet` toolchain will be handled in the Brim toolchain repo.
