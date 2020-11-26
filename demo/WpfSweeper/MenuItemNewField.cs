using SweeperModel;

namespace WpfSweeper
{
    /// <summary>
    /// Menu items for the predefined standard fields
    /// </summary>
    internal class MenuItemNewField : System.Windows.Controls.MenuItem
    {
        public FieldSize FieldSize {
            get;
        }

        public MenuItemNewField(FieldSize fieldSize)
        {
            FieldSize = fieldSize;
            Header = fieldSize.Name;
        }
    }
}
