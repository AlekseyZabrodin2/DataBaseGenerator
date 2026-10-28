using System.Text;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public class DicomEncodingRule : IGeneratorRule<string>
    {
        private readonly Encoding _encoding;

        public DicomEncodingRule(Encoding encoding)
        {
            _encoding = encoding;
        }

        public string Generate()
        {
            if (_encoding == Encoding.UTF8)
            {
                return "ISO_IR 192";
            }

            if (_encoding == Encoding.Latin1)
            {
                return "ISO_IR 100";
            }

            return "ISO_IR 6";
        }

        
        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
