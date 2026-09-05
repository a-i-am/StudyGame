using System;
using System.Collections.Generic;

public class BindableProperty<T>
{
    private T _value;
    public Action<T> OnValueChanged;

    public T Value
    {
        get => _value;
        set
        {
            // 기존 값과 들어온 값이 다를 때만 이벤트 발생 (최적화)
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
    public BindableProperty(T initialValue = default)
    {
        _value = initialValue;
    }

    public IDisposable Subscribe(Action<T> action)
    {
        OnValueChanged += action;
        return new Subscription(() => OnValueChanged -= action);
    }
    private class Subscription : IDisposable
    {
        private Action _unsubscribeAction;
        public Subscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }
        public void Dispose()
        {
            _unsubscribeAction?.Invoke();
            _unsubscribeAction = null;
        }
    }

}
