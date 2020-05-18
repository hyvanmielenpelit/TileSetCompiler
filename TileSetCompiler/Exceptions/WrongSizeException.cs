using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileSetCompiler.Exceptions
{
    public class WrongSizeException : ApplicationException
    {
        public Size WrongSize { get; set; }
        public Size RightSize { get; set; }

        public WrongSizeException()
        {

        }

        public WrongSizeException(string message) : base (message)
        {

        }

        public WrongSizeException(Size wrongSize, Size rightSize, string message) : base (message)
        {
            WrongSize = wrongSize;
            RightSize = rightSize;
        }

        public WrongSizeException(Size wrongSize, Size rightSize, string message, Exception innerException) : base(message, innerException)
        {
            WrongSize = wrongSize;
            RightSize = rightSize;
        }

        public WrongSizeException(int wrongX, int wrongY, int rightX, int rightY, string message) : base (message)
        {
            WrongSize = new Size(wrongX, wrongY);
            RightSize = new Size(rightX, rightY);
        }

        public WrongSizeException(int wrongX, int wrongY, int rightX, int rightY, string message, Exception innerException) : base(message, innerException)
        {
            WrongSize = new Size(wrongX, wrongY);
            RightSize = new Size(rightX, rightY);
        }
    }
}
