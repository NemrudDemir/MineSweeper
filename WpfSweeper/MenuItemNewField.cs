using SweeperModel;

namespace WpfSweeper
{
    /// <summary>
    /// Menu items for the predefined standard fields
    /// </summary>
    class MenuItemNewField : System.Windows.Controls.MenuItem
    {
        public Field.Standards FieldType {
            get;
        }

        public MenuItemNewField(Field.Standards fieldType) : base()
        {
            FieldType = fieldType;
        }
    }
}
