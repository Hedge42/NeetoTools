namespace Neeto
{

    public enum CollisionType : int
    {
        CollisionEnter = 0,
        CollisionExit = 1,
        CollisionStay = 2,
        TriggerEnter = 3,
        TriggerExit = 4,
        TriggerStay = 5
    }

    public static class CollisionTypeExtension
    {
        public static bool IsTrigger(this CollisionType _) => (int)_ >= 3;
        public static bool IsEnter(this CollisionType _) => (int)_ % 3 == 0;
        public static bool IsExit(this CollisionType _) => (int)_ % 3 == 1;
        public static bool IsStay(this CollisionType _) => (int)_ % 3 == 2;
    }
}
