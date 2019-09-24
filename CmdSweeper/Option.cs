using System;

namespace CmdSweeper
{
    internal class Option
    {
        internal string Caption {
            get;
        }

        internal Action Action {
            get;
        }

        internal Option(string name, Action action)
        {
            Caption = name;
            Action = action;
        }
    }
}
