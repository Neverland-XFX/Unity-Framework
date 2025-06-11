namespace GameLogic.GoapModule.Demo
{
    public class BoolCondition : ICondition
    {
        public string Key { get; }
        public object Value { get; }
        public BoolCondition(string key, bool value) { Key = key; Value = value; }
        public bool IsMet(IGoapAgent agent, IGoapWorldState state) =>
            state.ContainsKey(Key) && Equals(state.Get(Key), Value);
    }

}