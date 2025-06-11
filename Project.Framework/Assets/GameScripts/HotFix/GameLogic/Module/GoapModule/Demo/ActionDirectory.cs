using System.Collections.Generic;

namespace GameLogic.GoapModule.Demo
{
    public class ActionDirectory : IActionDirectory
    {
        private readonly Dictionary<string, IActionTemplate> _actions = new();

        public bool TryGetHandler(string actionId, out IActionTemplate handler)
            => _actions.TryGetValue(actionId, out handler);

        public IEnumerable<IActionTemplate> GetAllHandlers()
            => _actions.Values;

        public void RegisterAction(string actionId, IActionTemplate handler)
            => _actions[actionId] = handler;

        public void UnregisterAction(string actionId)
            => _actions.Remove(actionId);
    }
}