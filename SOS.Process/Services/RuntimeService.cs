using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Process;


namespace SOS.Process.Services
{
    public class RuntimeService : Interfaces.IRuntimeService
    {
        private readonly ILogger<RuntimeService> _logger;
        private readonly IEnumerable<ApplicationConfiguration> _applications;
        private string _databaseName;
        private byte? _instance;

        private bool PopulateEnvironment()
        {
            _logger.LogInformation("Hämtar nuvarande konfiguration från server");

            if (!GetAppsettings())
            {
                _logger.LogError("Misslyckades att hämta nuvarande konfiguration");
                return false;
            }

            // Use first application as master to read current environment
            var application = _applications.First();
            var appSettings = ReadAppsettings(application.UniqueSettingsFile);
            var currentDatabaseName = (string)appSettings.MongoDbConfiguration.DatabaseName;
            var regex = new Regex(@"(?<=-)\d$");
            if (regex.IsMatch(currentDatabaseName))
            {
                _instance = byte.Parse(regex.Match(currentDatabaseName).Value);
                _databaseName = new Regex(@"^[\w|-]+(?=-\d$)").Match(currentDatabaseName).Value;
            }
            else
            {
                // Remove, used during dev
                _databaseName = currentDatabaseName;
            }
           

            return true;
        }

        /// <summary>
        /// Get app settings file from servers
        /// </summary>
        /// <returns></returns>
        private bool GetAppsettings()
        {
            try
            {
                var success = true;
                foreach (var application in _applications)
                {
                    if (application.ServerName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                    {
                        File.Copy($"{ application.Folder }\\{ application.SettingsFile }", $".\\{ application.UniqueSettingsFile }", true);
                    }
                    else
                    {
                        var commands = new List<string>
                        {
                            $"$s = New-PSSession -computerName '{ application.ServerName }' ",
                            $"Copy-Item -FromSession $s -path '{ application.Folder }\\{ application.SettingsFile }' -destination '.\\{ application.UniqueSettingsFile }' -Recurse ",
                            "Remove-PSSession $s"
                        };

                        success = success && ExecuteProcess(commands);
                    }
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get settings files");
                return false;
            }
        }

        /// <summary>
        /// Read app settings file
        /// </summary>
        /// <returns></returns>
        private dynamic ReadAppsettings(string fileName)
        {
            using var fileStream = new FileStream($".\\{fileName}", FileMode.Open);

            using var reader = new StreamReader(fileStream);
            var settings = JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd());
            reader.Close();

            return settings;
        }

        /// <summary>
        /// Update app settings file
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="fileName"></param>
        private void WriteAppSettings(dynamic fileContent, string fileName)
        {
            using var fileStream = File.Create($".\\{fileName}");

            using var writer = new StreamWriter(fileStream);
            var settings = JsonConvert.SerializeObject(fileContent, Formatting.Indented);

            writer.Write(settings);
            writer.Close();
        }

        /// <summary>
        /// Toggle app settings file on server
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private async Task<bool> ToggleSource()
        {
            var success = true;

            try
            {
                foreach (var application in _applications)
                {
                    // Read current app settings and switch instance
                    var appSettings = ReadAppsettings(application.UniqueSettingsFile);
                    appSettings.MongoDbConfiguration.DatabaseName = DatabaseName;

                    // Save configuration change
                    WriteAppSettings(appSettings, application.UniqueSettingsFile);

                    if (application.ServerName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                    {
                        File.Copy($".\\{ application.UniqueSettingsFile }", $"{ application.Folder }\\{ application.SettingsFile }", true);
                    }
                    else
                    {
                        // Copy updated settings to server
                        var commands = new List<string>
                        {
                            $"$s = New-PSSession -computerName '{ application.ServerName }' ",
                            $"Copy-Item -ToSession $s -Path '.\\{ application.UniqueSettingsFile }' -destination '{ application.Folder }\\{ application.SettingsFile }' -Recurse ",
                            $"Invoke-Command -Session $s -Scriptblock {{ Restart-WebAppPool { application.ApplicationPool } }} ",
                            "Remove-PSSession $s"
                        };

                        success = success && ExecuteProcess(commands);
                    }

                    _logger.LogInformation($"Aktiverat miljö { appSettings.MongoDbConfiguration.DatabaseName } på { application.ServerName }: { success } ");
                }

                if (success)
                {
                    _instance = InactiveInstance;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }

            return success;
        }

        private bool ExecuteProcess(IEnumerable<string> commands)
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    string.Join("\n", commands))
                {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();

            using var reader = process.StandardOutput;
            var res = reader.ReadToEnd();
            
            return res.Equals(string.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="runTimeConfiguration"></param>
        /// <param name="logger"></param>
        public RuntimeService(RunTimeConfiguration runTimeConfiguration, ILogger<RuntimeService> logger)
        {
            if (runTimeConfiguration == null)
            {
                throw new ArgumentNullException(nameof(runTimeConfiguration));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _applications = runTimeConfiguration.Applications;
        }

        /// <inheritdoc />
        public byte ActiveInstance => _instance ?? 0;

        /// <inheritdoc />
        public string DatabaseName => $"{_databaseName}-{InactiveInstance}";

        /// <inheritdoc />
        public byte InactiveInstance => (byte)(ActiveInstance == 0 ? 1 : 0);

        /// <inheritdoc />
        public bool Initialize()
        {
            if (PopulateEnvironment())
            {
                _logger.LogInformation($"Aktiv miljö: { ActiveInstance }");

                return true;
            } 
            
            _logger.LogError("Misslyckades med att hämta nuvarande miljö");
            
            return false;
        }

        /// <inheritdoc />
        public void OverrideInstance(byte instance)
        {
            if (ActiveInstance == instance)
            {
                _instance = InactiveInstance;
            }
            _logger.LogInformation($"Tvingad uppdatering av miljö: { InactiveInstance }");
        }

        /// <inheritdoc />
        public async Task ToggleInstanceAsync()
        {
            await ToggleSource();
        }

        
    }
}
