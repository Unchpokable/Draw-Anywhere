using System;

namespace DrawAnywhere.Sys
{
    internal class ByRef<T>
        where T: struct
    {
        public event EventHandler<T> ValueChanged;

        public ByRef(T val)
        {
            _value = val;
        }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke(this, value);
            } 
        }
        public ref T RefValue => ref _value;

        private T _value;
    }
}
