namespace GameTimeDirector.Events
{
    public static class EventManager
    {
        public static PlayerEvents PlayerEvents { get; private set; } = new();
    }
}