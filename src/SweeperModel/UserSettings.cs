namespace SweeperModel
{
    public class UserSettings
    {
        private static UserSettings _default;
        public static UserSettings Default {
            get {
                if(_default == null) {
                    _default = new UserSettings {
                        DoOpenNearbyRecursive = true
                    };
                }
                return _default;
            }
        }

        public bool DoOpenNearbyRecursive {
            get; set;
        }
    }
}
