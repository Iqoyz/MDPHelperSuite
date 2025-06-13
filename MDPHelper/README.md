# MDPHelper Desktop Application

## Repository Overview

### 1. `fyp-MDPHelper-main` (Main Branch)
- This is the primary branch for the **MDPHelper** desktop application.
- Built using the **.NET MAUI** framework.
- Connects to a server to:
  - Download firmware files dynamically.
  - Check for application updates and download the latest version automatically.

### 2. `fyp-MDPHelper-feat-local-only` (Local-Only Branch)
- This branch contains code that **does not connect to any server**.
- Firmware files are managed and stored **locally** since no server communication occurs.

---

## Publishing the Desktop Application

### Windows

To publish the app as a **single executable** for Windows, run:
```bash
dotnet publish -f net8.0-windows10.0.19041.0 -c release -p:RuntimeIdentifierOverride=win10-x64 -p:WindowsPackageType=None -p:PublishSingleFile=true -p:WindowsAppSDKSelfContained=true
```
---

### MacOS (Mac Catalyst)

To publish for Mac Catalyst (requires macOS device with Xcode installed), run:
```bash
dotnet publish -f net8.0-maccatalyst -c debug -p:RuntimeIdentifier=maccatalyst-x64 -p:SelfContained=true
```

## Additional Notes

- Ensure you have the correct **.NET SDK version (8.0)** installed before publishing.
- For Mac publishing, the process must be run on a macOS device with **Xcode** installed.
- The published output will be optimized for the specified platform and bundled as a self-contained executable.

---
