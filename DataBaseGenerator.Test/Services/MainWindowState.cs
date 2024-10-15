using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataBaseGenerator.Test.Core;
using DataBaseGenerator.Test.Locators;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using NLog;

namespace DataBaseGenerator.Test.Services
{
    public class MainWindowState : IClientState
    {
        private readonly Window _window;
        private readonly ConditionFactory _conditionFactory;

        private TimeSpan _nextStateDelay = TimeSpan.FromSeconds(1);
        private string _firstName;
        private const int _nextStateRetry = 30;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public string Name { get; } = "MainWindowState";

        private MainWindowStateLocators _mainWindowStateLocators;

        public MainWindowState(Window window, ConditionFactory conditionFactory)
        {
            _window = window;
            _conditionFactory = conditionFactory;
            _mainWindowStateLocators = new(_window, _conditionFactory);
        }


        public async Task<IClientState> GoToStateAsync(string stateName, TimeSpan timeout)
        {
            IClientState state = null;

            var cancellationTokenSource = new CancellationTokenSource();

            var toStateTask = Task.Run(() =>
            {
                state = this;
                while (!state.Name.Equals(stateName))
                {
                    state = this;
                }

            }, cancellationTokenSource.Token);

            var task = await Task.WhenAny(toStateTask, Task.Delay(timeout));
            if (task != toStateTask)
            {
                cancellationTokenSource.Cancel();
            }

            return state;
        }

        public Window GetMainWindow()
        {
            return _window;
        }

        public bool IsState(Window window)
        {
            _logger.Trace("Entering in IClientState IsState in MainWindowState");

            var result = false;

            var connectButton = _mainWindowStateLocators.ConnectButton;
            if (connectButton != null)
            {
                result = true;
            }

            _logger.Debug($"MainWindowState State - [{result}]");
            return result;
        }


        public void ClickConnectButton()
        {
            var connectButton = _mainWindowStateLocators.ConnectButton;
            connectButton.Click();
        }

        public bool CheckDialogWindowOpen()
        {
            var result = false;

            var dialogWindow = _mainWindowStateLocators.DialogWindow;

            if (dialogWindow != null)
            {
                result = true;
            }

            
            return result;
            
        }
        public int GetViewAllPatientTableRowCount()
        {
            var viewAllPatientTable = _mainWindowStateLocators.ViewAllPatientTable;
            viewAllPatientTable.DrawHighlight();
            int rowCount = viewAllPatientTable.AsGrid().RowCount;
            return rowCount;
        }

        public void ClickDeleteAllPatientButton()
        {
            var DeleteAllPatientButton = _mainWindowStateLocators.DeleteAllPatientButton;
            DeleteAllPatientButton.DrawHighlight();
            DeleteAllPatientButton.Click();
        }

        public void ClickAddPatientButton()
        {
            var AddPatientButton = _mainWindowStateLocators.AddPatientButton;
            AddPatientButton.DrawHighlight();
            AddPatientButton.Click();
        }

        public void InputPatientCountTextBox()
        {
            var PatientCountTextBox = _mainWindowStateLocators.PatientCountTextBox;
                PatientCountTextBox.DrawHighlight();
                PatientCountTextBox.AsTextBox().Focus();
                PatientCountTextBox.AsTextBox().Text = "7";
        }

        public void CloseDialogWindow()
        {
            for (int i = 0; i < 3; i++)
            {
                _mainWindowStateLocators.DialogWindowOkButton.DrawHighlight().Click();
            }
          //  _mainWindowStateLocators.DialogWindowOkButton.DrawHighlight().Click();
           // _mainWindowStateLocators.DialogWindowOkButton.DrawHighlight().Click();
           // _mainWindowStateLocators.DialogWindowOkButton.DrawHighlight().Click();

        }
    }
}
