namespace GameLogic.GoapModule
{
    public interface ICondition
    {
        string Key { get; }
        object Value { get; }
        bool IsMet(IGoapAgent agent, IGoapWorldState state);
    }
}