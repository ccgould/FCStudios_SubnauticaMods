namespace FCSCommon.Generics
{
    public class CustomProp<T>
    {
        private T _value;

        public T Value
        {
            get
            {
                // insert desired logic here
                return _value;
            }
            set
            {
                // insert desired logic here
                _value = value;
            }
        }

        public static implicit operator T(CustomProp<T> value)
        {
            return value.Value;
        }

        public static implicit operator CustomProp<T>(T value)
        {
            return new CustomProp<T> { Value = value };
        }
    }
}
