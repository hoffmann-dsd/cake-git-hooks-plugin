# Cake.GitHooks

## Overview

Cake.GitHooks is a cake addin that provides functionality to manage your git hooks in your cake scripts. It provides a set of methods to check availability and to deploy your git hooks. These extensions aim to simplify distributing and updating git hooks across the contributors of your project.

## Usage

Git hooks are sourced from a folder in your project. The addin installs the hooks to your local git repository.

### DeployGitHooks

Deploy hooks to the local `.git/hooks` directory.

```csharp
DeployGitHooks();
```

### Configuration

To configure the source and destination path of your hooks, you can build `GitHooksSettings` for each method.

```csharp
DeployGitHooks(settings => 
{
    settings.SourcePath = "./custom-hooks/";
    settings.DestinationPath = "./.git/hooks/";
    return settings;
});
```
The default setting of the git hooks **source** directory is `./hooks`. The git hooks are deployed to the **destination** directory `./.git/hooks`.

## Example build script

```csharp
Task("Setup-GitHooks")
    .Does(() =>
{
    DeployGitHooks();
});

RunTarget("Setup-GitHooks");
```

## Getting started
The package is published to the GitHub Packages NuGet registry. To use the package in your Cake build script project, you must add the GitHub Packages NuGet registry to your Cake build script project's NuGet.config file. For more information, see the following link:

https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry#installing-a-package

Add the Cake.GitHooks NuGet package to your Cake build script project.
```csharp
#addin nuget:?package=Cake.GitHooks
```
Import the namespace in your Cake build script.
```csharp
using Cake.GitHooks;
```
Use the provided aliases in your Cake build script as needed.