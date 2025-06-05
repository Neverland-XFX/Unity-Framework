using GameLogic.Binding.Registry;

namespace GameLogic.Binding.Converters
{
    public interface IConverterRegistry : IKeyValueRegistry<string, IConverter>
    {
    }
}
