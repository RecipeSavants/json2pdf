using System;

namespace CreatePdfLibrary
{
    public class MessagePdfEventArgs : EventArgs
    {
        public string Msg { get; set; }

        public bool IsError { get; set; }

        public bool EndLine { get; set; }
    }
}
