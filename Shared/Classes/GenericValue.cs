using System;

namespace Archiver.Shared.Classes
{
    public class GenericValue
    {
        public Nullable<long> NumericValue { get; private set; } = null;
        public Nullable<bool> BooleanValue { get; private set; } = null;
        public string StringValue { get; private set; } = null;

        public GenericValue(string value)
        {
            this.StringValue = value;
        }

        public GenericValue(long value)
        {
            this.NumericValue = value;
        }

        public GenericValue(bool value)
        {
            this.BooleanValue = value;
        }
    }
}