# Publishing SOS API Client NuGet packages

The SOS API client is distributed as two NuGet packages:

- `SOS.Api.Client` contains the typed clients and higher-level services.
- `SOS.Api.Contracts` contains the public request and response contracts used by the client.

`SOS.Api.Client` depends on `SOS.Api.Contracts`. The release pipeline therefore builds and publishes both packages with the same version to the `ADB-Common` Azure Artifacts feed.

## Package feed

The packages are published to the project-scoped `ADB-Common` feed:

```text
https://devops.slu.se/ArtData/Home/_packaging/ADB-Common/nuget/v3/index.json
```

The pipeline definition is located at:

```text
devops/pipelines/nuget/sos-api-client.yaml
```

## Release a new version

Normal releases should be made from a tested commit on `master`:

1. Merge the changes through a pull request.
2. Update the local `master` branch.
3. Create an annotated release tag on the commit to publish.
4. Push the tag to Azure DevOps.
5. Follow the pipeline run and verify both packages in `ADB-Common`.

For example, to publish version `0.2.0`:

```powershell
git switch master
git pull origin master
git tag -a sos-api-client-v0.2.0 -m "Release SOS API Client 0.2.0"
git push origin sos-api-client-v0.2.0
```

The tag must use this format:

```text
sos-api-client-v<major>.<minor>.<patch>[-prerelease]
```

Examples of valid tags:

```text
sos-api-client-v0.2.0
sos-api-client-v1.0.0
sos-api-client-v1.1.0-beta.1
sos-api-client-v1.1.0-rc.2
```

Follow semantic versioning when selecting the next version:

- Increment `patch` for backwards-compatible bug fixes.
- Increment `minor` for backwards-compatible functionality.
- Increment `major` for breaking changes after version 1.0.
- Use a suffix such as `-beta.1` or `-rc.1` for a prerelease.

Package versions are immutable. Never move an already published release tag or attempt to overwrite an existing package version. Publish a new patch version instead.

## Verify a release

After the pipeline succeeds:

1. Open **Artifacts > ADB-Common** in the Azure DevOps `Home` project.
2. Verify that both `SOS.Api.Contracts` and `SOS.Api.Client` exist with the expected version.
3. Open the `SOS.Api.Client` package and verify that its dependencies include `SOS.Api.Contracts`.
4. Install the package in a test project and run a restore.

Example:

```powershell
dotnet add package SOS.Api.Client --version 0.2.0 --source "https://devops.slu.se/ArtData/Home/_packaging/ADB-Common/nuget/v3/index.json" --interactive
dotnet restore --interactive
```

Consumers can also add the feed to `NuGet.Config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="ADB-Common" value="https://devops.slu.se/ArtData/Home/_packaging/ADB-Common/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

Do not store credentials or personal access tokens in `NuGet.Config`. Use the Azure Artifacts authentication flow provided under **Connect to feed**.
