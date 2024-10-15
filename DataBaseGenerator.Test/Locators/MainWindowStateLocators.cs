using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace DataBaseGenerator.Test.Locators
{
    public class MainWindowStateLocators
    {

        static Window _window;
        static ConditionFactory _conditionFactory;

        public MainWindowStateLocators(Window window, ConditionFactory conditionFactory)
        {
            _window = window;
            _conditionFactory = conditionFactory;
        }

        public AutomationElement ConnectButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("ConnectButton")).AsButton();
        public AutomationElement DialogWindow => _window.FindFirstDescendant(_conditionFactory.ByLocalizedControlType("диалоговое окно"));
        public AutomationElement DialogWindowOkButton => _window.FindFirstDescendant(_conditionFactory.ByLocalizedControlType("кнопка"));

        public AutomationElement AddIdPatieentTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddIdPatieentTextBox")).AsTextBox();
        public AutomationElement AddFamilyTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddFamilyTextBox")).AsTextBox();
        public AutomationElement AddNameTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddNameTextBox")).AsTextBox();
        public AutomationElement AddMiddleNameTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddMiddleNameTextBox")).AsTextBox();
        public AutomationElement AddAdressTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddAdressTextBox")).AsTextBox();
        public AutomationElement SelecedGenderComboBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("SelecedGenderComboBox")).AsComboBox();
        public AutomationElement AddWorkPlaseTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddWorkPlaseTextBox")).AsTextBox();
        public AutomationElement AddInfoTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddInfoTextBox")).AsTextBox();
        public AutomationElement AddOnePatientButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddOnePatientButton")).AsButton();
        public AutomationElement CancelAddPatientButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("CancelAddPatientButton")).AsButton();
        public AutomationElement PatientCountTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("PatientCountTextBox")).AsTextBox();
        public AutomationElement AddPatientButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddPatientButton")).AsButton();
        public AutomationElement RefreshPatientsButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("RefreshPatientsButton")).AsButton();
        public AutomationElement DeleteFirstPatientButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("DeleteFirstPatientButton")).AsButton();
        public AutomationElement DeleteAllPatientButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("DeleteAllPatientButton")).AsButton();
        public AutomationElement DeleteAllTablesButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("DeleteAllTablesButton")).AsButton();
        public AutomationElement WorkListCountTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("WorkListCountTextBox")).AsTextBox();
        public AutomationElement ModalityComboBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("ModalityComboBox")).AsComboBox();
        public AutomationElement AeTitleBoxTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AeTitleBoxTextBox")).AsTextBox();
        public AutomationElement AddWorkListButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("AddWorkListButton")).AsButton();
        public AutomationElement RefreshWorkListButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("RefreshWorkListButton")).AsButton();
        public AutomationElement DeleteFirstWorkListButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("DeleteFirstWorkListButton")).AsButton();
        public AutomationElement DeleteAllWorkListButton => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("DeleteAllWorkListButton")).AsButton();
        public AutomationElement UpdateTextTextBox => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("UpdateTextTextBox")).AsTextBox();
        public AutomationElement ViewAllPatientTable => _window.FindFirstDescendant(_conditionFactory.ByAutomationId("ViewAllPatientTable"));

    }
}
