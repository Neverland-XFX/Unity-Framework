namespace GameLogic.GoapModule
{
    public interface IPoolable
    {
        void OnRent();
        void OnReturn();
    }

}