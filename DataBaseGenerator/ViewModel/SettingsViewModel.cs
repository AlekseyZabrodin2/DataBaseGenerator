using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.UI.Wpf.Model;
using DataBaseGenerator.UI.Wpf.Service;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ApplicationSettingsService _settingsService;

        public ApplicationSettings Settings { get; }

        [ObservableProperty]
        private string _saveStatus;

        [ObservableProperty]
        private bool _isSaveSuccessful;


        public SettingsViewModel(ApplicationSettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = settingsService.Settings;
        }



        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                await _settingsService.SaveAsync();

                IsSaveSuccessful = true;
                SaveStatus = "Настройки успешно сохранены.";
            }
            catch (Exception ex)
            {
                IsSaveSuccessful = false;
                SaveStatus = $"Не удалось сохранить настройки: {ex.Message}";
            }
        }

        public void ResetSaveStatus()
        {
            SaveStatus = string.Empty;
            IsSaveSuccessful = false;
        }
    }
}
