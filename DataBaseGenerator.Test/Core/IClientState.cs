using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;

namespace DataBaseGenerator.Test.Core
{
    public interface IClientState
    {
        string Name { get; }

        Task<IClientState> GoToStateAsync(string stateName, TimeSpan timeout);

        Window GetMainWindow();

        bool IsState(Window window);

    }
}
