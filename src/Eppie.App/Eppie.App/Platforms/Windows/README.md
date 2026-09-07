# Platforms/Windows

This folder is for WinAppSDK-specific assets and configuration files. The Uno.Sdk includes the contents of this folder as `Content` (with `CopyToOutputDirectory=PreserveNewest`) when building the `*-windows10.*` target, and excludes it from every other target.

Typical contents:

- Visual assets referenced from `Package.appxmanifest`:
  - `SplashScreen.scale-*.png`
  - `SmallTile.scale-*.png`, `MediumTile.scale-*.png`, `WideTile.scale-*.png`, `LargeTile.scale-*.png`
  - `AppIcon.scale-*.png` and `AppIcon.targetsize-*.png` variants
  - `PackageLogo.scale-*.png`
- `app.manifest` if you need to override the default
- Any other Windows-only resource shipped inside the produced `.msix`

Reference these files from `Package.appxmanifest` using `Platforms\Windows\...` paths — they are placed at the same relative path inside the package.

This README is a placeholder and can be deleted once you add your first asset.
