namespace GameLogic.GoapModule
{
    public interface ITarget
    {
        UnityEngine.Vector3 Position { get; }
        bool IsValid();
        void SetPosition(UnityEngine.Vector3 pos);
    }
}