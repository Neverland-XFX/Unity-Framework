using System.Collections.Generic;

namespace GameLogic.GoapModule
{
    public interface IActionDirectory
    {
        bool TryGetHandler(string actionId, out IActionTemplate handler);
        IEnumerable<IActionTemplate> GetAllHandlers();
        void RegisterAction(string actionId, IActionTemplate handler);
        void UnregisterAction(string actionId);
    }
}