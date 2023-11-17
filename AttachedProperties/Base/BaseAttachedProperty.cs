using System;
using System.Windows;

namespace Illumine.LPR
{
    public abstract class BaseAttachedProperty<Parent, Property> where Parent : new()
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(Property), typeof(BaseAttachedProperty<Parent, Property>), new UIPropertyMetadata((object)default(Property), new PropertyChangedCallback(BaseAttachedProperty<Parent, Property>.OnValuePropertyChanged), new CoerceValueCallback(BaseAttachedProperty<Parent, Property>.OnValuePropertyUpdated)));

        public event Action<DependencyObject, DependencyPropertyChangedEventArgs> ValueChanged = (sender, e) => { };

        public event Action<DependencyObject, object> ValueUpdated = (sender, value) => { };

        public static Parent Instance { get; private set; } = new Parent();

        private static object OnValuePropertyUpdated(DependencyObject d, object value)
        {
            if (Instance is BaseAttachedProperty<Parent, Property> instance1)
                instance1.OnValueUpdated(d, value);
            if (Instance is BaseAttachedProperty<Parent, Property> instance2)
                instance2.ValueUpdated(d, value);
            return value;
        }

        private static void OnValuePropertyChanged(
          DependencyObject sender,
          DependencyPropertyChangedEventArgs e)
        {
            if (Instance is BaseAttachedProperty<Parent, Property> instance1)
                instance1.OnValueChanged(sender, e);
            if (!(Instance is BaseAttachedProperty<Parent, Property> instance2))
                return;
            instance2.ValueChanged(sender, e);
        }

        public static Property GetValue(DependencyObject d) => (Property)d.GetValue(ValueProperty);

        public static void SetValue(DependencyObject d, Property value) => d.SetValue(ValueProperty, value);

        public virtual void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public virtual void OnValueUpdated(DependencyObject d, object value)
        {
        }
    }
}
