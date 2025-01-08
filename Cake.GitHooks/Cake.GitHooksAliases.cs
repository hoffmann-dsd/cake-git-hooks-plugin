using Cake.Core;
using Cake.Core.Annotations;
using Cake.Common;
using Cake.Common.Diagnostics;

namespace Cake.GitHooks;

[CakeAliasCategory("GitHooks")]
public static class GitHooks
{
    private static readonly GitHooksSettings DefaultSettings = new GitHooksSettings
    {
        SourcePath = "./hooks",
        DestinationPath = "./.git/hooks"
    };
    
    private static bool AreGitHooksUptoDate(this ICakeContext ctx, GitHooksSettings? settings = null)
    {
        GitHooksSettings effectiveSettings = settings ?? DefaultSettings;
        
        var hooksChanged = false;
        
        // Error when source files path does not exist
        if (!Directory.Exists(effectiveSettings.SourcePath))
        {
            throw new DirectoryNotFoundException(
                $"Git hooks source files not found at the specified path: {effectiveSettings.SourcePath}"
                );
        }
        
        // Ensure the destination directory exists
        if (!Directory.Exists(effectiveSettings.DestinationPath))
        {
            Directory.CreateDirectory(effectiveSettings.DestinationPath);
            hooksChanged = true;
        }

        // Compare each file in the source hooks directory with the destination hooks directory
        var hooksFiles = Directory.GetFiles(effectiveSettings.SourcePath);

        foreach (var hookFile in hooksFiles)
        {
            var hookFileName = Path.GetFileName(hookFile);
            var sourceFilePath = Path.Combine(effectiveSettings.SourcePath, hookFileName);
            var destinationFilePath = Path.Combine(effectiveSettings.DestinationPath, hookFileName);

            var expectedContent = File.ReadAllText(sourceFilePath);
            var actualContent = File.Exists(destinationFilePath)
                ? File.ReadAllText(destinationFilePath)
                : string.Empty;

            if (expectedContent != actualContent)
            {
                hooksChanged = true;
                break;
            }
        }

        return !hooksChanged;
    }

    private static void CopyGitHooks(this ICakeContext ctx, GitHooksSettings? settings = null)
    {
        GitHooksSettings effectiveSettings = settings ?? DefaultSettings;
        
        var hooksSourcePath = "./hooks";
        var hooksDestinationPath = "./.git/hooks";

        // Error when source files path does not exist
        if (!Directory.Exists(effectiveSettings.SourcePath))
        {
            throw new DirectoryNotFoundException(
                $"Git hooks source files not found at the specified path: {effectiveSettings.SourcePath}"
                );
        }
        
        // Ensure the destination directory exists
        if (!Directory.Exists(hooksDestinationPath))
        {
            Directory.CreateDirectory(hooksDestinationPath);
        }

        // Copy each file from the source hooks directory to the destination hooks directory
        var hooksFiles = Directory.GetFiles(hooksSourcePath);

        foreach (var hookFile in hooksFiles)
        {
            var hookFileName = Path.GetFileName(hookFile);
            var sourceFilePath = Path.Combine(hooksSourcePath, hookFileName);
            var destinationFilePath = Path.Combine(hooksDestinationPath, hookFileName);

            File.Copy(sourceFilePath, destinationFilePath);

            // Ensure the hook is executable
            File.SetAttributes(destinationFilePath, FileAttributes.Normal);

            var platform = Environment.OSVersion.Platform;
            if (platform == PlatformID.Unix || platform == PlatformID.MacOSX || (int)platform == 128)
            {
                LoggingAliases.Information(ctx, $"Marking {hookFileName} as executable.");
                ProcessAliases.StartProcess(ctx, "chmod", new Cake.Core.IO.ProcessSettings {
                    Arguments = $"+x {destinationFilePath}"
                });
            }
        }
    }

    [CakeMethodAlias]
    public static void DeployGitHooks(this ICakeContext ctx, Func<GitHooksSettings, GitHooksSettings>? hooksSettings = null)
    {
        var settings = DefaultSettings;
        settings = hooksSettings?.Invoke(settings) ?? DefaultSettings;

        if (ctx.AreGitHooksUptoDate(settings) == false)
        {
            LoggingAliases.Information(ctx, "One or more Git hooks are missing or outdated. Deploying the latest versions.");
            ctx.CopyGitHooks(settings);
        }
        else
        {
            LoggingAliases.Information(ctx, "All Git hooks are up-to-date.");
        }
    }
}

public class GitHooksSettings
{
    public string SourcePath { get; set; } = default!;
    public string DestinationPath { get; set; } = default!;
}
