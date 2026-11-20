using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.UI.Wpf.Enums;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class PatientGenerationOptions : ObservableObject
    {
        public PatientGenerationOptions(string birthdateGroupName)
        {
            BirthdateGroupName = birthdateGroupName;
        }

        [JsonIgnore]
        public string BirthdateGroupName { get; }

        [ObservableProperty]
        public partial bool UseEngNames { get; set; }

        [ObservableProperty]
        public partial bool UseRusNames { get; set; }

        [ObservableProperty]
        public partial bool UseChinaNames { get; set; }

        [ObservableProperty]
        public partial bool UseEmptyStrings { get; set; }

        [ObservableProperty]
        public partial bool UseLongValues { get; set; }

        [ObservableProperty]
        public partial bool UseSpecialChars { get; set; }

        [ObservableProperty]
        public partial BirthdateMode BirthdateMode { get; set; } = BirthdateMode.Random;

        public void Reset()
        {
            UseEngNames = false;
            UseRusNames = false;
            UseChinaNames = false;
            UseEmptyStrings = false;
            UseLongValues = false;
            UseSpecialChars = false;
            BirthdateMode = BirthdateMode.Random;
        }

        public void CopyFrom(GenerationOptionsData data)
        {
            UseEngNames = data.UseEngNames;
            UseRusNames = data.UseRusNames;
            UseChinaNames = data.UseChinaNames;
            UseEmptyStrings = data.UseEmptyStrings;
            UseLongValues = data.UseLongValues;
            UseSpecialChars = data.UseSpecialChars;
            BirthdateMode = data.BirthdateMode;
        }

        public GenerationOptionsData ToData()
        {
            return new GenerationOptionsData
            {
                UseEngNames = UseEngNames,
                UseRusNames = UseRusNames,
                UseChinaNames = UseChinaNames,
                UseEmptyStrings = UseEmptyStrings,
                UseLongValues = UseLongValues,
                UseSpecialChars = UseSpecialChars,
                BirthdateMode = BirthdateMode
            };
        }

        public void ApplyTo(PatientGeneratorParameters parameters)
        {
            parameters.NamesEngGeneratorRule = UseEngNames;
            parameters.NamesRusGeneratorRule = UseRusNames;
            parameters.NamesChinaGeneratorRule = UseChinaNames;
            parameters.EmptyStringsGeneratorRule = UseEmptyStrings;
            parameters.LongValuesGeneratorRule = UseLongValues;
            parameters.SpecialCharsGeneratorRule = UseSpecialChars;
            parameters.RandomBirthdateGeneratorRule = BirthdateMode == BirthdateMode.Random;
            parameters.MissingBirthdateGeneratorRule = BirthdateMode == BirthdateMode.Missing;
            parameters.FutureBirthdateGeneratorRule = BirthdateMode == BirthdateMode.Future;
            parameters.Age0_17_GeneratorRule = BirthdateMode == BirthdateMode.Age0To17;
            parameters.Age18_60_GeneratorRule = BirthdateMode == BirthdateMode.Age18To60;
            parameters.Age61_120_GeneratorRule = BirthdateMode == BirthdateMode.Age61To120;
        }

        public void ApplyTo(PatientGeneratorDto parameters)
        {
            parameters.NamesEngGeneratorRule = UseEngNames;
            parameters.NamesRusGeneratorRule = UseRusNames;
            parameters.NamesChinaGeneratorRule = UseChinaNames;
            parameters.EmptyStringsGeneratorRule = UseEmptyStrings;
            parameters.LongValuesGeneratorRule = UseLongValues;
            parameters.SpecialCharsGeneratorRule = UseSpecialChars;
            parameters.RandomBirthdateGeneratorRule = BirthdateMode == BirthdateMode.Random;
            parameters.MissingBirthdateGeneratorRule = BirthdateMode == BirthdateMode.Missing;
            parameters.FutureBirthdateGeneratorRule = BirthdateMode == BirthdateMode.Future;
            parameters.Age0_17_GeneratorRule = BirthdateMode == BirthdateMode.Age0To17;
            parameters.Age18_60_GeneratorRule = BirthdateMode == BirthdateMode.Age18To60;
            parameters.Age61_120_GeneratorRule = BirthdateMode == BirthdateMode.Age61To120;
        }

        public void RefreshBirthdate()
        {
            OnPropertyChanged(nameof(BirthdateMode));
        }
    }

    public sealed class GenerationOptionsData
    {
        public bool UseEngNames { get; set; }
        public bool UseRusNames { get; set; }
        public bool UseChinaNames { get; set; }
        public bool UseEmptyStrings { get; set; }
        public bool UseLongValues { get; set; }
        public bool UseSpecialChars { get; set; }
        public BirthdateMode BirthdateMode { get; set; } = BirthdateMode.Random;
    }
}
