namespace GameLogic.Binding
{
    public interface IInvoker
    {
        object Invoke(params object[] args);
    }

    public interface IInvoker<T> : IInvoker
    {
        object Invoke(T arg);
    }

    public interface IInvoker<T0, T1> : IInvoker
    {
        object Invoke(T0 t0, T1 t1);
    }

    public interface IInvoker<T0, T1, T2> : IInvoker
    {
        object Invoke(T0 t0, T1 t1, T2 t2);
    }

    public interface IInvoker<T0, T1, T2, T3> : IInvoker
    {
        object Invoke(T0 t0, T1 t1, T2 t2, T3 t3);
    }
}