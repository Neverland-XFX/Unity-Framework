namespace GameLogic.GoapModule
{
    public interface IPool<T>
    {
        T Rent(params object[] args);
        void Return(T obj);
    }
}