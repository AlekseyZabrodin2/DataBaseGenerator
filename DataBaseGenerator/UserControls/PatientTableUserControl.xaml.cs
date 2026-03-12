using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for PatientTableUserControl.xaml
    /// </summary>
    public partial class PatientTableUserControl : UserControl
    {
        public static readonly DependencyProperty AllPatientsProperty =
            DependencyProperty.Register(
                nameof(AllPatients),
                typeof(ObservableCollection<Patient>),
                typeof(PatientTableUserControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedPatientProperty =
            DependencyProperty.Register(
                nameof(SelectedPatient),
                typeof(Patient),
                typeof(PatientTableUserControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AddPatientCommandProperty =
            DependencyProperty.Register(
                nameof(AddPatientCommand),
                typeof(IRelayCommand),
                typeof(PatientTableUserControl),
                new PropertyMetadata(null));



        public ObservableCollection<Patient> AllPatients
        {
            get => (ObservableCollection<Patient>)GetValue(AllPatientsProperty);
            set => SetValue(AllPatientsProperty, value);
        }

        public Patient SelectedPatient
        {
            get => (Patient)GetValue(SelectedPatientProperty);
            set => SetValue(SelectedPatientProperty, value);
        }

        public IRelayCommand AddPatientCommand
        {
            get => (IRelayCommand)GetValue(AddPatientCommandProperty);
            set => SetValue(AddPatientCommandProperty, value);
        }



        public PatientTableUserControl()
        {
            InitializeComponent();
        }
    }
}
