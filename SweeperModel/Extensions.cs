using System;
using System.ComponentModel;
using System.Globalization;

namespace SweeperModel
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the description of the enum if available otherwise its name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">the enumValue</param>
        /// <returns>Description of the enum if available otherwise its name</returns>
        public static string ToDescription<T>(this T e) where T : IConvertible
        {
            var type = e.GetType();
            Array values = Enum.GetValues(type);

            foreach(int val in values) {
                if(val == e.ToInt32(CultureInfo.InvariantCulture)) {
                    var memInfo = type.GetMember(type.GetEnumName(val));
                    var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if(descriptionAttributes.Length > 0) {
                        // we're only getting the first description we find
                        // others will be ignored
                        return ((DescriptionAttribute)descriptionAttributes[0]).Description;
                    }
                }
            }
            return Enum.GetName(type, e);
        }
    }
}
