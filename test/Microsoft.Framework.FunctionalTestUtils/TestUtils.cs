// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Framework.Runtime;

namespace Microsoft.Framework.FunctionalTestUtils
{
    public static class TestUtils
    {
        public static DisposableDir CreateTempDir()
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirPath);
            return tempDirPath;
        }

        public static int Exec(
            string program,
            string commandLine,
            out string stdOut,
            out string stdErr,
            IDictionary<string, string> environment = null,
            string workingDir = null)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                WorkingDirectory = workingDir,
                FileName = program,
                Arguments = commandLine,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (environment != null)
            {
                foreach (var pair in environment)
                {
                    processStartInfo.EnvironmentVariables[pair.Key] = pair.Value;
                }
            }

            var process = Process.Start(processStartInfo);
            stdOut = process.StandardOutput.ReadToEnd();
            stdErr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode;
        }

        public static string GetBuildArtifactsFolder()
        {
            var kRuntimeRoot = ProjectResolver.ResolveRootDirectory(Directory.GetCurrentDirectory());
            return Path.Combine(kRuntimeRoot, "artifacts", "build");
        }

        public static IEnumerable<DisposableDir> GetKreHomePaths()
        {
            var buildArtifactDir = GetBuildArtifactsFolder();
            var kreNupkgs = new List<string>();
            kreNupkgs.Add(Directory.GetFiles(buildArtifactDir, "KRE-CLR-amd64.*.nupkg", SearchOption.TopDirectoryOnly).First());
            kreNupkgs.Add(Directory.GetFiles(buildArtifactDir, "KRE-CLR-x86.*.nupkg", SearchOption.TopDirectoryOnly).First());
            kreNupkgs.Add(Directory.GetFiles(buildArtifactDir, "KRE-CoreCLR-amd64.*.nupkg", SearchOption.TopDirectoryOnly).First());
            kreNupkgs.Add(Directory.GetFiles(buildArtifactDir, "KRE-CoreCLR-x86.*.nupkg", SearchOption.TopDirectoryOnly).First());
            foreach (var nupkg in kreNupkgs)
            {
                var kreName = Path.GetFileNameWithoutExtension(nupkg);
                var krePath = CreateTempDir();
                var kreRoot = Path.Combine(krePath, "packages", kreName);
                System.IO.Compression.ZipFile.ExtractToDirectory(nupkg, kreRoot);
                File.Copy(nupkg, Path.Combine(kreRoot, kreName + ".nupkg"));
                yield return krePath;
            }
        }

        public static void DeleteFolder(string path)
        {
            var retryNum = 3;
            for (int i = 0; i < retryNum; i++)
            {
                try
                {
                    Directory.Delete(path, recursive: true);
                    return;
                }
                catch (Exception)
                {
                    if (i == retryNum - 1)
                    {
                        throw;
                    }
                }
            }
        }

        public static string GetKreVersion()
        {
            var kreNupkg = Directory.EnumerateFiles(GetBuildArtifactsFolder(), "KRE-*.nupkg").FirstOrDefault();
            var kreName = Path.GetFileNameWithoutExtension(kreNupkg);
            var segments = kreName.Split(new[] { '.' }, 2);
            return segments[1];
        }

        public static string ComputeSHA(string path)
        {
            using (var sourceStream = File.OpenRead(path))
            {
                var sha512Bytes = SHA512.Create().ComputeHash(sourceStream);
                return Convert.ToBase64String(sha512Bytes);
            }
        }

    }
}
