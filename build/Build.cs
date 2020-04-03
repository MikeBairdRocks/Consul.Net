using System;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;

[CheckBuildProjectConfigurations]
[DotNetVerbosityMapping]
[UnsetVisualStudioEnvironmentVariables]
[GitHubActions("Test", 
  GitHubActionsImage.UbuntuLatest,
  AutoGenerate = true,
  On = new[] { GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.Push },
  ImportGitHubTokenAs = nameof(GitHubToken),
  InvokedTargets = new[] { nameof(Test) })]
class Build : NukeBuild
{
  public static int Main() => Execute<Build>(x => x.Compile);

  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
  [Parameter("GitHub Token")] readonly string GitHubToken;

  [Solution] readonly Solution Solution;
  [GitRepository] readonly GitRepository GitRepository;
  [GitVersion] readonly GitVersion GitVersion;

  AbsolutePath SourceDirectory => RootDirectory / "src";
  AbsolutePath TestsDirectory => RootDirectory / "tests";
  AbsolutePath OutputDirectory => RootDirectory / "output";

  Target Clean => _ => _
      .Before(Restore)
      .Executes(() =>
      {
        SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        EnsureCleanDirectory(OutputDirectory);
      });

  Target Restore => _ => _
      .Executes(() =>
      {
        DotNetRestore(s => s
              .SetProjectFile(Solution));
      });

  Target Compile => _ => _
      .DependsOn(Restore)
      .Executes(() =>
      {
        DotNetBuild(s => s
              .SetProjectFile(Solution)
              .SetConfiguration(Configuration)
              .SetAssemblyVersion(GitVersion.AssemblySemVer)
              .SetFileVersion(GitVersion.AssemblySemFileVer)
              .SetInformationalVersion(GitVersion.InformationalVersion)
              .EnableNoRestore());
      });

  Target Test => _ => _
    .DependsOn(Clean)
    .DependsOn(Restore)
    .Executes(() =>
    {
      var containerName = $"consul-test.{DateTime.Now.Ticks}";
      var dockerRunSettings = new DockerContainerRunSettings()
        .EnableDetach()
        .SetName(containerName)
        .AddPublish("8500:8500")
        //.AddEnv("CONSUL_LOCAL_CONFIG={ \"bind_addr\": \"0.0.0.0\", \"server\": true, \"bootstrap\": true, \"acl_datacenter\": \"dc1\", \"acl_master_token\": \"eba37d50-2fd8-42f2-b9f6-9c7c7a55890e\", \"acl_default_policy\": \"allow\", \"encrypt\": \"d8wu8CSUrqgtjVsvcBPmhQ==\" }")
        .SetImage("consul:1.5.2")
        .SetCommand("agent -dev");

      DockerContainerRun(dockerRunSettings);

      var testSettings = new DotNetTestSettings()
        .SetConfiguration(Configuration)
        //.AddProperty("CollectCoverage", true)
        //.AddProperty("CoverletOutputFormat", "opencover")
        //.AddProperty("CoverletOutput", $"{OutputDirectory}/")
        //.AddProperty("Exclude", "\\\"[Consul.Net.Tests]*\\\"")
        .SetLogger("trx")
        .SetResultsDirectory(OutputDirectory)
        .SetVerbosity(DotNetVerbosity.Detailed)
        .EnableNoRestore();
      DotNetTest(testSettings);

      DockerContainerStop(stopSettings => stopSettings.SetContainers(containerName));
      DockerContainerRm(rmSettings => rmSettings.SetContainers(containerName).SetForce(true));
    });
}
