using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace DataBaseGenerator.UI.Wpf.Licensing
{
    public sealed class LicenseValidator
    {
        private readonly string _publicKeyPem;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };


        public LicenseValidator(string publicKeyPem)
        {
            _publicKeyPem = publicKeyPem;
        }


        public bool TryValidate(string filePath, out LicensePayload? payload, out string error)
        {
            payload = null;
            error = string.Empty;

            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonSerializer.Deserialize<LicenseDocument>(json, JsonOptions);

                if (document is null ||
                    string.IsNullOrWhiteSpace(document.Payload) ||
                    string.IsNullOrWhiteSpace(document.Signature))
                {
                    error = "Некорректный файл лицензии.";
                    return false;
                }

                var payloadBytes = Convert.FromBase64String(document.Payload);
                var signature = Convert.FromBase64String(document.Signature);

                using var rsa = RSA.Create();
                rsa.ImportFromPem(_publicKeyPem);

                var isSignatureValid = rsa.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                if (!isSignatureValid)
                {
                    error = "Подпись лицензии недействительна.";
                    return false;
                }

                payload = JsonSerializer.Deserialize<LicensePayload>(payloadBytes, JsonOptions);
                if (payload is null)
                {
                    error = "Не удалось прочитать данные лицензии.";
                    return false;
                }

                if (payload.Product != "DataBaseGenerator")
                {
                    error = "Лицензия предназначена для другого продукта.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(payload.LicenseId) || string.IsNullOrWhiteSpace(payload.Customer))
                {
                    error = "В лицензии отсутствуют обязательные данные.";
                    return false;
                }

                var now = DateTime.UtcNow;
                if (payload.IssuedAtUtc == default || payload.IssuedAtUtc > now.AddMinutes(5))
                {
                    error = "Дата выпуска лицензии некорректна.";
                    return false;
                }

                if (payload.ExpiresAtUtc.HasValue && now >= payload.ExpiresAtUtc.Value)
                {
                    error = "Срок действия лицензии истёк.";
                    return false;
                }

                return true;
            }
            catch (Exception ex) when (
                ex is IOException or
                UnauthorizedAccessException or
                JsonException or
                FormatException or
                CryptographicException or
                ArgumentException)
            {
                error = $"Ошибка проверки лицензии: {ex.Message}";
                return false;
            }
        }
    }
}
