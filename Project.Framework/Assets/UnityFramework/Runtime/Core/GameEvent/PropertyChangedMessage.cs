namespace UnityFramework
{
    public class PropertyChangedMessage<T> : MessageBase
    {
        public PropertyChangedMessage(T oldValue, T newValue, string propertyName) : this(null, oldValue, newValue, propertyName)
        {
        }

        public PropertyChangedMessage(object sender, T oldValue, T newValue, string propertyName) : base(sender)
        {
            this.PropertyName = propertyName;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public string PropertyName { get; private set; }

        public T OldValue { get; private set; }

        public T NewValue { get; private set; }
    }
}