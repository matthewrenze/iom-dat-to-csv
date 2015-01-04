using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IomDatToCsv
{
    public class InvalidFileFormatException : Exception
    {
        private const string ErrorMessage = "File cannot be converted because it has an unrecognized data format.";

        public InvalidFileFormatException()
            : base(ErrorMessage)
        {
            
        }
    }
}
