using System;

namespace RecipeCardLibrary
{
    public class MessageEventArgs : EventArgs
    {
        public string Msg { get; set; }

        public bool IsError { get; set; }

        public bool EndLine { get; set; }
    }
}
