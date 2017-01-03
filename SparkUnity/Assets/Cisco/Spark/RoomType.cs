namespace Cisco.Spark
{
    public enum RoomType
    {
        Direct,
        Group,
        Unsupported
    }

    public class RoomTypeLookup
    {
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

        public static string ToApi(RoomType type) {
            switch (type) {
                case RoomType.Direct:
                    return "direct";
                case RoomType.Group:
                    return "group";
                default:
                    return "";
            }
        }
    }
}