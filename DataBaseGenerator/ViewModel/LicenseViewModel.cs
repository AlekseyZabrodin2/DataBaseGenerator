using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.UI.Wpf.Licensing;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LicenseViewModel : ObservableObject
    {
        private readonly LicenseService _licenseService = new();

        [ObservableProperty]
        public partial string LicenseStatus { get; set; }

        [ObservableProperty]
        public partial string ExpiryDate { get; set; }

        [ObservableProperty]
        public partial string DaysPassed { get; set; }

        public event Action<LicenseCheckResult>? LicenseActivated;



        public LicenseViewModel(LicenseCheckResult licenseResult)
        {
            LicenseStatus = licenseResult.State switch
            {
                LicenseState.Expired => "Пробный период закончился.",
                LicenseState.Invalid => "Данные лицензии повреждены или изменены.",
                LicenseState.StorageError => "Не удалось проверить состояние лицензии.",
                _ => "Лицензия недействительна."
            };

            ExpiryDate = licenseResult.ExpiryDateUtc.HasValue
                ? $"{licenseResult.ExpiryDateUtc.Value.ToLocalTime():dd.MM.yyyy}"
                : string.Empty;

            DaysPassed = licenseResult.State == LicenseState.Expired && licenseResult.ExpiryDateUtc.HasValue
                ? Math.Max(1, (int)Math.Ceiling((DateTime.UtcNow - licenseResult.ExpiryDateUtc.Value).TotalDays)).ToString()
                : string.Empty;
        }



        [RelayCommand]
        private async Task ActivateLicenseAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл лицензии",
                Filter = "License files (*.lic)|*.lic",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            var success = _licenseService.TryInstallLicense(dialog.FileName, out var result, out var error);

            if (!success)
            {
                LicenseStatus = result.State switch
                {
                    LicenseState.Expired => "Срок действия лицензии истёк.",
                    LicenseState.Invalid => "Лицензия недействительна.",
                    _ => "Не удалось установить лицензию."
                };

                return;
            }

            LicenseStatus = "Лицензия успешно активирована.";
            ExpiryDate = result.ExpiryDateUtc.HasValue
                ? result.ExpiryDateUtc.Value.ToLocalTime().ToString("dd.MM.yyyy")
                : "Бессрочная";

            DaysPassed = string.Empty;

            LicenseActivated?.Invoke(result);
        }
    }
}
