namespace GameLogic.Execution
{
    public abstract class AbstractExecutor
    {
        static AbstractExecutor()
        {
            Executors.Create();
        }
    }
}