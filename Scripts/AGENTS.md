# AGENTS.md

## Encoding and line endings

This is a Windows-based C#/.NET repository that also builds and runs in Linux containers and Kubernetes.

When editing existing files:
- Preserve the file's existing encoding.
- Preserve the file's existing line ending style.
- Preserve UTF-8 BOM if the file already has one.
- Do not introduce mixed line endings.
- Do not reformat unrelated files.

When creating new files, follow these repository rules:

### Windows / .NET files

Use CRLF line endings and UTF-8 with BOM for:
- `.cs`
- `.csproj`
- `.sln`
- `.props`
- `.targets`
- `.config`
- `.resx`
- `.xaml`
- `.xml`
- `.json`
- `.md`
- `.txt`

### Linux / container / Kubernetes files

Use LF line endings and UTF-8 without BOM for:
- `.sh`
- `.bash`
- `Dockerfile`
- `Dockerfile.*`
- `*.dockerfile`
- `.yml`
- `.yaml`
- `.env`
- `*.env`
- `Makefile`
- `*.mk`
- `.conf`

For scripts or files that are executed inside Linux containers, always use LF and no BOM.

Before finishing:
- Check that modified files match `.editorconfig`.
- Check that modified files do not contain mixed line endings.
- Do not convert line endings or encoding for files that were not part of the requested change.