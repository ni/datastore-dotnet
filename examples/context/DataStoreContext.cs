using System.Security.Cryptography;
using System.Text;

namespace NI.DataStore.Utilities
{
    /// <summary>
    /// A context manager for running a data store in an isolated environment.
    /// Implements IDisposable to support the 'using' statement pattern.
    /// </summary>
    public class DataStoreContext : IDisposable
    {
        // Environment variable names
        private const string DISCOVERY_SERVICE_CLUSTER_ID_ENV_VAR = "NIDiscovery_ClusterId";
        private const string DATA_STORE_DATABASE_PATH_ENV_VAR = "NIDATASTORE_DATASTORESETTINGS__SQLITEDATABASEPATH";
        private const string DATA_STORE_DATA_FILES_DIRECTORY_PATH_ENV_VAR = "NIDATASTORE_DATASTORESETTINGS__DATAFILESDIRECTORY";
        private const string DATA_STORE_INGEST_DIRECTORY_PATH_ENV_VAR = "NIDATASTORE_DATASTORESETTINGS__INGESTDIRECTORY";
        private const string DATA_STORE_FAILED_INGEST_DIRECTORY_PATH_ENV_VAR = "NIDATASTORE_DATASTORESETTINGS__FAILEDINGESTDIRECTORY";
        private const string DATA_STORE_TDMS_EXPIRATION_SECONDS_ENV_NAME = "NIDATASTORE_DATASTORESETTINGS__TDMSFILECACHEEXPIRATIONSECONDS";
        
        private const string DEFAULT_FOLDER_NAME = "temp_data";

        private readonly string? _baseDirectoryPath;
        private readonly Dictionary<string, string?> _originalEnvironment;
        private bool _initialized = false;
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
            _originalEnvironment = new Dictionary<string, string?>();
            Initialize();
        }

        /// <summary>
        /// Initializes the data store context by setting up necessary environment variables.
        /// This is called automatically when using the 'using' statement.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            InitializeEnvironment();
            _initialized = true;
        }

        /// <summary>
        /// Cleans up the data store context by resetting environment variables.
        /// This is called automatically when using the 'using' statement.
        /// </summary>
        public void Close()
        {
            if (!_initialized)
            {
                return;
            }

            RestoreEnvironment();
            _initialized = false;
        }

        /// <summary>
        /// Disposes the DataStoreContext and restores environment variables.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string GetClusterId()
        {
            return GetBaseDirectoryHash();
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
                    Close();
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
                DISCOVERY_SERVICE_CLUSTER_ID_ENV_VAR,
                DATA_STORE_DATABASE_PATH_ENV_VAR,
                DATA_STORE_DATA_FILES_DIRECTORY_PATH_ENV_VAR,
                DATA_STORE_INGEST_DIRECTORY_PATH_ENV_VAR,
                DATA_STORE_FAILED_INGEST_DIRECTORY_PATH_ENV_VAR,
                DATA_STORE_TDMS_EXPIRATION_SECONDS_ENV_NAME
            };

            foreach (var envVar in environmentVariables)
            {
                _originalEnvironment[envVar] = Environment.GetEnvironmentVariable(envVar);
            }
        }

        private void InitializeClusterId()
        {
            string clusterId = GetClusterId();
            Environment.SetEnvironmentVariable(DISCOVERY_SERVICE_CLUSTER_ID_ENV_VAR, clusterId);
        }

        private void InitializeDataStorePaths()
        {
            string baseDirectoryPath = GetBaseDirectoryPath();

            string metadataDbPath = Path.Combine(baseDirectoryPath, "MetadataStore.db");
            string dataFilesDir = Path.Combine(baseDirectoryPath, "DataFiles");
            string ingestDir = Path.Combine(baseDirectoryPath, "Ingest");
            string failedIngestDir = Path.Combine(baseDirectoryPath, "FailedIngest");

            Environment.SetEnvironmentVariable(DATA_STORE_DATABASE_PATH_ENV_VAR, metadataDbPath);
            Environment.SetEnvironmentVariable(DATA_STORE_DATA_FILES_DIRECTORY_PATH_ENV_VAR, dataFilesDir);
            Environment.SetEnvironmentVariable(DATA_STORE_INGEST_DIRECTORY_PATH_ENV_VAR, ingestDir);
            Environment.SetEnvironmentVariable(DATA_STORE_FAILED_INGEST_DIRECTORY_PATH_ENV_VAR, failedIngestDir);
            Environment.SetEnvironmentVariable(DATA_STORE_TDMS_EXPIRATION_SECONDS_ENV_NAME, "0");
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

        private string GetBaseDirectoryPath()
        {
            if (!string.IsNullOrEmpty(_baseDirectoryPath))
            {
                return _baseDirectoryPath;
            }

            // The location of the example data should shared among all
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
            return Path.Combine(examplesDirectory, DEFAULT_FOLDER_NAME);
        }

        private void RestoreEnvironment()
        {
            foreach (var kvp in _originalEnvironment)
            {
                string environmentVariable = kvp.Key;
                string? originalValue = kvp.Value;

                if (originalValue == null)
                {
                    // The environment variable was not originally set; remove it
                    Environment.SetEnvironmentVariable(environmentVariable, null);
                }
                else
                {
                    // Restore the original value
                    Environment.SetEnvironmentVariable(environmentVariable, originalValue);
                }
            }
        }

        private static string GetPathToThisFile([System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            return filePath;
        }
    }
}
