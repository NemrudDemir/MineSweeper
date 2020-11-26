namespace SweeperModel
{
    public class UserSettings
    {
        private static UserSettings _default;
        public static UserSettings Default =>
            _default ?? (_default = new UserSettings
            {
                DoOpenNearbyRecursive = true
            });

        public bool DoOpenNearbyRecursive {
            get; set;
        }
    }
}
