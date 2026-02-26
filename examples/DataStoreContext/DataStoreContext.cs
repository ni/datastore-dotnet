using System.Security.Cryptography;
using System.Text;

namespace NationalInstruments.DataStore.Utilities
{
    /// <summary>
    /// A context manager for running a data store in an isolated environment.
    /// Implements IDisposable to support the 'using' statement pattern.
    /// </summary>
    public class DataStoreContext : IDisposable
    {
        // Environment variable names
        private const string DiscoveryServiceClusterIdEnvVar = "NIDiscovery_ClusterId";
        private const string DataStoreDatabasePathEnvVar = "NIDATASTORE_DATASTORESETTINGS__SQLITEDATABASEPATH";
        private const string DataStoreDataFilesDirectoryPathEnvVar = "NIDATASTORE_DATASTORESETTINGS__DATAFILESDIRECTORY";
        private const string DataStoreIngestDirectoryPathEnvVar = "NIDATASTORE_DATASTORESETTINGS__INGESTDIRECTORY";
        private const string DataStoreFailedIngestDirectoryPathEnvVar = "NIDATASTORE_DATASTORESETTINGS__FAILEDINGESTDIRECTORY";
        private const string DataStoreTdmsExpirationSecondsEnvName = "NIDATASTORE_DATASTORESETTINGS__TDMSFILECACHEEXPIRATIONSECONDS";
        
        private const string DefaultFolderName = "temp_data";

        private readonly string? _baseDirectoryPath;
        private readonly Dictionary<string, string?> _originalEnvironment = new();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the DataStoreContext class.
        /// </summary>
        /// <param name="baseDirectoryPath">
        /// An optional base directory path specifying where the data store files will be located.
        /// If not provided, a default path will be used.
        /// </param>
        public DataStoreContext(string? baseDirectoryPath = null)
        {
            _baseDirectoryPath = baseDirectoryPath;
            InitializeEnvironment();
        }

        /// <summary>
        /// Disposes the DataStoreContext and restores environment variables.
        /// This is called automatically when using the 'using' statement.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    RestoreEnvironment();
                }
                _disposed = true;
            }
        }

        private void InitializeEnvironment()
        {
            SaveOriginalEnvironment();
            InitializeClusterId();
            InitializeDataStorePaths();
        }

        private void SaveOriginalEnvironment()
        {
            var environmentVariables = new[]
            {
                DiscoveryServiceClusterIdEnvVar,
                DataStoreDatabasePathEnvVar,
                DataStoreDataFilesDirectoryPathEnvVar,
                DataStoreIngestDirectoryPathEnvVar,
                DataStoreFailedIngestDirectoryPathEnvVar,
                DataStoreTdmsExpirationSecondsEnvName
            };

            foreach (var environmentVariable in environmentVariables)
            {
                _originalEnvironment[environmentVariable] = Environment.GetEnvironmentVariable(environmentVariable);
            }
        }

        private void InitializeClusterId()
        {
            string clusterId = GetClusterId();
            Environment.SetEnvironmentVariable(DiscoveryServiceClusterIdEnvVar, clusterId);
        }

        private void InitializeDataStorePaths()
        {
            string baseDirectoryPath = GetBaseDirectoryPath();

            string metadataDbPath = Path.Combine(baseDirectoryPath, "MetadataStore.db");
            string dataFilesDir = Path.Combine(baseDirectoryPath, "DataFiles");
            string ingestDir = Path.Combine(baseDirectoryPath, "Ingest");
            string failedIngestDir = Path.Combine(baseDirectoryPath, "FailedIngest");

            Environment.SetEnvironmentVariable(DataStoreDatabasePathEnvVar, metadataDbPath);
            Environment.SetEnvironmentVariable(DataStoreDataFilesDirectoryPathEnvVar, dataFilesDir);
            Environment.SetEnvironmentVariable(DataStoreIngestDirectoryPathEnvVar, ingestDir);
            Environment.SetEnvironmentVariable(DataStoreFailedIngestDirectoryPathEnvVar, failedIngestDir);
            Environment.SetEnvironmentVariable(DataStoreTdmsExpirationSecondsEnvName, "0");
        }

        private string GetBaseDirectoryHash()
        {
            string baseDirectoryPath = GetBaseDirectoryPath();
            string resolvedPath = Path.GetFullPath(baseDirectoryPath);
            
            using (var sha256 = SHA256.Create())
            {
                byte[] pathBytes = Encoding.UTF8.GetBytes(resolvedPath);
                byte[] hashBytes = sha256.ComputeHash(pathBytes);
                
                // Convert to hex string and take first 32 characters
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Math.Min(16, hashBytes.Length); i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private string GetClusterId()
        {
            return GetBaseDirectoryHash();
        }

        private string GetBaseDirectoryPath()
        {
            if (!string.IsNullOrEmpty(_baseDirectoryPath))
            {
                return _baseDirectoryPath;
            }

            // The location of the example data should be shared among all
            // examples, so we place it in a known location relative to the
            // source code. We can't use the current working directory since
            // that can be different based on how the example is run, and we
            // can't use a temp directory since we want the data to persist
            // across runs of different examples.
            var thisFilePath = GetPathToThisFile();
            var thisDirectory = Path.GetDirectoryName(thisFilePath);
            if (thisDirectory == null)
            {
                throw new InvalidOperationException("Could not determine directory of this file");
            }

            var parentInfo = Directory.GetParent(thisDirectory);
            if (parentInfo == null)
            {
                throw new InvalidOperationException("Could not determine parent directory");
            }

            var examplesDirectory = parentInfo.FullName;
            return Path.Combine(examplesDirectory, DefaultFolderName);
        }

        private void RestoreEnvironment()
        {
            foreach (var (environmentVariable, originalValue) in _originalEnvironment)
            {
                Environment.SetEnvironmentVariable(environmentVariable, originalValue);
            }
        }

        private static string GetPathToThisFile([System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            return filePath;
        }
    }
}
