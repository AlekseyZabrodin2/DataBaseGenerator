using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules
{
    public interface IGeneratorRule<T>
    {
        T Generate();
    }

    public interface IGeneratorRule<T, Tin>
    {
        T Generate(Tin parameter);
    }
}
