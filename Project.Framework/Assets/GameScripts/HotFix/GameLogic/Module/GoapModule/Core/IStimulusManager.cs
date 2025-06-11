namespace GameLogic.GoapModule
{

    public interface IStimulusManager
    {
        void AddStimulus(IStimulus stimulus);
        void RemoveStimulus(IStimulus stimulus);
        void Update();
    }
}