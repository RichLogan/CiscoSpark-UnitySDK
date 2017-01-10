namespace Cisco.Spark
{
    /// <summary>
    /// Supported Room Types.
    /// </summary>
    public enum RoomType
    {
        Direct,
        Group,
        Unsupported
    }

    /// <summary>
    /// Extensions for RoomType to/from API representations.
    /// </summary>
    public static class RoomTypeExtensions
    {
        /// <summary>
        /// Gets the API representation of a RoomType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToApi(this RoomType type) {
            switch (type) {
                case RoomType.Direct:
                    return "direct";
                case RoomType.Group:
                    return "group";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Converts a API representation of a Room's type to a RoomType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static RoomType FromApi(string type)
        {
            switch (type)
            {
                case "direct":
                    return RoomType.Direct;
                case "group":
                    return RoomType.Group;
                default:
                    UnityEngine.Debug.LogWarning("Unsupported Room Type detected");
                    return RoomType.Unsupported;
            }
        }
    }
}