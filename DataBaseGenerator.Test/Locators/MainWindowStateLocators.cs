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
    }
}
