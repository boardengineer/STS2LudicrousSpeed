# STS2 Mod Template

A template for creating Slay the Spire 2 mods.

## Setup

1. Clone or copy this template
2. Rename the following to match your mod name (e.g. `MyAwesomeMod`):
   - `ModTemplate.json` → `MyAwesomeMod.json` (update `id`, `name`, `author`, `description` inside)
   - `ModTemplate.csproj` → `MyAwesomeMod.csproj` (update `AssemblyName` and `RootNamespace`)
   - `ModTemplate.sln` → `MyAwesomeMod.sln` (update the project reference inside)
3. In `src/Plugin.cs`, update:
   - The `namespace` to match your mod
   - The Harmony id (e.g. `com.yourname.myawesomemod`)
   - The hello world message or replace with your own logic
4. Verify `STS2GameDir` in the `.csproj` points to your STS2 install

## Build

```
dotnet build
```

The built DLL is automatically copied to the STS2 `mods` folder.
