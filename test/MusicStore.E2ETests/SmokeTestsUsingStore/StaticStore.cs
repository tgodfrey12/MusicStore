﻿using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;

namespace E2ETests
{
    internal class StaticStore : IDisposable
    {
        public const string AspNetCoreStoreZipLocationEnvironmentVariableName = "ASPNETCORE_STORE_ZIP_LOCATION";
        private readonly ILogger _logger;

        public StaticStore(string storeDirectory, ILogger logger)
        {
            StoreDirectory = storeDirectory;
            _logger = logger;

            var rootPath = Environment.GetEnvironmentVariable(AspNetCoreStoreZipLocationEnvironmentVariableName);
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new InvalidOperationException(
                    $"The environment variable '{AspNetCoreStoreZipLocationEnvironmentVariableName}' is not defined or is empty.");
            }

            var zipFile = Path.Combine(rootPath, "Build.RS.winx64.zip");
            if (!File.Exists(zipFile))
            {
                throw new InvalidOperationException($"Could not find file '{zipFile}'");
            }

            ZipFile.ExtractToDirectory(zipFile, StoreDirectory);
        }

        public string StoreDirectory { get; }

        public void Dispose()
        {
            if (Helpers.PreservePublishedApplicationForDebugging)
            {
                _logger.LogInformation("Skipping deleting the store and working directory as it has been disabled");
            }
            else
            {
                _logger.LogInformation("Deleting the storing and working directory.");

                RetryHelper.RetryOperation(
                        () => Directory.Delete(StoreDirectory, recursive: true),
                        e => _logger.LogError($"Failed to delete directory : {e.Message}"),
                        retryCount: 3,
                        retryDelayMilliseconds: 100);
            }
        }
    }

}