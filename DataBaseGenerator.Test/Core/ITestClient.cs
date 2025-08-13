using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Test.Core
{
    public interface ITestClient
    {
        Task<IClientState> StartAsync(TimeSpan timeout);

        void Kill();
    }
}
