namespace GameLogic.GoapModule
{
    public class GoapCondition : ICondition
    {
        public string Key { get; }
        public object Value { get; }
        public GoapCondition(string key, object value) { Key = key; Value = value; }
        public bool IsMet(IGoapAgent agent, IGoapWorldState state)
        {
            return state.ContainsKey(Key) && Equals(state.Get(Key), Value);
        }
    }
}