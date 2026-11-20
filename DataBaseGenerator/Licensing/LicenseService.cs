using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.Licensing
{
    public sealed class LicenseService
    {
        private const int TrialDays = 30;
        private const int ClockToleranceMinutes = 5;
        private const string RegistryPath =@"Software\DataBaseGenerator";
        private const string RegistryValueName = "TrialState";
        private static readonly string LicenseDirectory = Path.Combine(AppContext.BaseDirectory, "License");
        private static readonly string LicenseFilePath = Path.Combine(LicenseDirectory, "activation.lic");
        private static readonly string PublicKeyFilePath = Path.Combine(LicenseDirectory, "public_key.pem");
        private static readonly string[] FilePaths =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DataBaseGenerator", "trial.dat"),

            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DataBaseGenerator", "trial.dat")
        };

        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("DataBaseGenerator.Trial.v1");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public LicenseCheckResult Check()
        {            
            try
            {
                if (File.Exists(LicenseFilePath))
                {
                    var publicKey = File.ReadAllText(PublicKeyFilePath);
                    var validator = new LicenseValidator(publicKey);

                    if (validator.TryValidate(LicenseFilePath, out var payload, out var error))
                    {
                        return new LicenseCheckResult
                        {
                            State = LicenseState.Licensed,
                            DaysLeft = 0,
                            ExpiryDateUtc = payload!.ExpiresAtUtc
                        };
                    }

                    return new LicenseCheckResult
                    {
                        State = error == "Срок действия лицензии истёк."
                            ? LicenseState.Expired
                            : LicenseState.Invalid
                    };
                }

                var fileData = ReadFile(FilePaths[0]);
                var localData = ReadFile(FilePaths[1]);
                var registryData = ReadRegistry();

                bool hasAnyData =
                    fileData != null ||
                    localData != null ||
                    registryData != null;

                // Первый запуск.
                if (!hasAnyData)
                {
                    var now = DateTime.UtcNow;

                    var trialState = new TrialState
                    {
                        StartedAtUtc = now,
                        LastSeenUtc = now
                    };

                    SaveAll(trialState);

                    return CreateResult(trialState, now);
                }

                // Проверяем, что все существующие копии совпадают.
                var state = fileData ?? localData ?? registryData!;

                //state.StartedAtUtc = DateTime.UtcNow.AddDays(-20);
                //state.LastSeenUtc = DateTime.UtcNow;
                //SaveAll(state);

                //return CreateResult(state, DateTime.UtcNow);

                if (!IsSameState(state, fileData) ||
                    !IsSameState(state, localData) ||
                    !IsSameState(state, registryData))
                {
                    return Invalid();
                }

                var currentTime = DateTime.UtcNow;

                // Проверка корректности данных.
                if (state.StartedAtUtc == default ||
                    state.LastSeenUtc == default ||
                    state.StartedAtUtc.Kind != DateTimeKind.Utc ||
                    state.LastSeenUtc.Kind != DateTimeKind.Utc ||
                    state.LastSeenUtc < state.StartedAtUtc)
                {
                    return Invalid();
                }

                // Проверка отката системных часов.
                if (currentTime <
                    state.LastSeenUtc.AddMinutes(-ClockToleranceMinutes))
                {
                    return Invalid();
                }

                // Обновляем последнее время запуска.
                if (currentTime > state.LastSeenUtc)
                {
                    state.LastSeenUtc = currentTime;
                }

                // Восстанавливаем отсутствующие копии и обновляем время.
                SaveAll(state);

                return CreateResult(state, currentTime);
            }
            catch (CryptographicException)
            {
                return Invalid();
            }
            catch (Exception)
            {
                return new LicenseCheckResult
                {
                    State = LicenseState.StorageError
                };
            }
        }

        private static LicenseCheckResult CreateResult(TrialState state, DateTime currentTime)
        {
            var expiryDate = state.StartedAtUtc.AddDays(TrialDays);

            if (currentTime >= expiryDate)
            {
                return new LicenseCheckResult
                {
                    State = LicenseState.Expired,
                    DaysLeft = 0,
                    ExpiryDateUtc = expiryDate
                };
            }

            var daysLeft = (int)Math.Ceiling(
                (expiryDate - currentTime).TotalDays);

            return new LicenseCheckResult
            {
                State = LicenseState.Trial,
                DaysLeft = daysLeft,
                ExpiryDateUtc = expiryDate
            };
        }

        private static bool IsSameState(TrialState reference, TrialState? candidate)
        {
            if (candidate == null)
                return true;

            return reference.StartedAtUtc == candidate.StartedAtUtc &&
                   reference.LastSeenUtc == candidate.LastSeenUtc;
        }

        private static LicenseCheckResult Invalid()
        {
            return new LicenseCheckResult
            {
                State = LicenseState.Invalid
            };
        }

        private static TrialState? ReadFile(string path)
        {
            if (!File.Exists(path))
                return null;

            var encryptedData = File.ReadAllBytes(path);

            var jsonBytes = ProtectedData.Unprotect(
                encryptedData,
                Entropy,
                DataProtectionScope.CurrentUser);

            return JsonSerializer.Deserialize<TrialState>(jsonBytes);
        }

        private static TrialState? ReadRegistry()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);

            if (key?.GetValue(RegistryValueName) is not byte[] encryptedData)
                return null;

            var jsonBytes = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);

            return JsonSerializer.Deserialize<TrialState>(jsonBytes);
        }

        private static void SaveAll(TrialState state)
        {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(state, JsonOptions);

            var encryptedData = ProtectedData.Protect(jsonBytes, Entropy, DataProtectionScope.CurrentUser);

            foreach (var path in FilePaths)
            {
                SaveFile(path, encryptedData);
            }

            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath)
                ?? throw new InvalidOperationException(
                    "Unable to create license registry key.");

            key.SetValue(RegistryValueName, encryptedData, RegistryValueKind.Binary);
        }

        private static void SaveFile(string path, byte[] data)
        {
            var directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Invalid license storage path.");

            Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp";

            File.WriteAllBytes(tempPath, data);
            File.Move(tempPath, path, true);
        }

        public bool TryInstallLicense(string sourcePath, out LicenseCheckResult result, out string error)
        {
            result = new LicenseCheckResult
            {
                State = LicenseState.Invalid
            };

            error = string.Empty;

            try
            {
                var publicKey = File.ReadAllText(PublicKeyFilePath);
                var validator = new LicenseValidator(publicKey);

                if (!validator.TryValidate(sourcePath, out var payload, out error))
                {
                    result = new LicenseCheckResult
                    {
                        State = error == "Срок действия лицензии истёк."
                            ? LicenseState.Expired
                            : LicenseState.Invalid,
                        ExpiryDateUtc = payload?.ExpiresAtUtc
                    };

                    return false;
                }

                Directory.CreateDirectory(LicenseDirectory);

                var tempPath = LicenseFilePath + ".tmp";

                File.Copy(sourcePath, tempPath, true);
                File.Move(tempPath, LicenseFilePath, true);

                result = new LicenseCheckResult
                {
                    State = LicenseState.Licensed,
                    ExpiryDateUtc = payload!.ExpiresAtUtc
                };

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;

                result = new LicenseCheckResult
                {
                    State = LicenseState.StorageError
                };

                return false;
            }
        }
    }
}
