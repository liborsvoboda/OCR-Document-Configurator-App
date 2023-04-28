using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoxFileEditor
{

    public class TessBoxControl : Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsFailProperty =
            DependencyProperty.Register("IsFail", typeof(bool), typeof(TessBoxControl), new PropertyMetadata(default(bool), (d, a) => ((TessBoxControl)d).OnIsFailChanged((bool)a.NewValue)));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (TessBoxControl), new PropertyMetadata(default(bool), (d, a) => ((TessBoxControl)d).OnIsSelectedChanged((bool)a.NewValue)));


        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            throw new NotImplementedException();
        }

        public bool IsFail
        {
            get { return (bool)GetValue(IsFailProperty); }
            set { SetValue(IsFailProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }


        public static readonly DependencyProperty GroupValueIndexProperty =
         DependencyProperty.Register("GroupValueIndex", typeof(string), typeof(TessBoxControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty GroupValueProperty =
         DependencyProperty.Register("GroupValue", typeof(string), typeof(TessBoxControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty GroupSubValueIndexProperty =
         DependencyProperty.Register("GroupSubValueIndex", typeof(string), typeof(TessBoxControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty GroupSubValueProperty =
         DependencyProperty.Register("GroupSubValue", typeof(string), typeof(TessBoxControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (string), typeof (TessBoxControl), new PropertyMetadata(default(string)));

        public string GroupSubValueIndex
        {
            get { return (string)GetValue(GroupSubValueIndexProperty); }
            set { SetValue(GroupSubValueIndexProperty, value); }
        }

        public string GroupSubValue
        {
            get { return (string)GetValue(GroupSubValueProperty); }
            set { SetValue(GroupSubValueProperty, value); }
        }

        public string GroupValueIndex
        {
            get { return (string)GetValue(GroupValueIndexProperty); }
            set { SetValue(GroupValueIndexProperty, value); }
        }

        public string GroupValue
        {
            get { return (string)GetValue(GroupValueProperty); }
            set { SetValue(GroupValueProperty, value); }
        }

        public string Value
        {
            get { return (string) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Left
        {
            get { return Canvas.GetLeft(this); }
            set
            {
                Canvas.SetLeft(this, value);
                NotifyPropertyChanged("Left");
            }
        }

        public double Top
        {
            get { return Canvas.GetTop(this); }
            set
            {
                Canvas.SetTop(this, value);
                NotifyPropertyChanged("Top");
            }
        }

        private Border _normalBorder = null;
        private Border _selBorder = null;
        private Border _failBorder = null;

        static TessBoxControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TessBoxControl), new FrameworkPropertyMetadata(typeof(TessBoxControl)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TessBoxControl()
        {

        }

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if(handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _normalBorder = GetTemplateChild("normalBorder") as Border;
            _selBorder = GetTemplateChild("selBorder") as Border;
            _failBorder = GetTemplateChild("failBorder") as Border;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            NotifyPropertyChanged("RenderSize");
        }

        protected void OnIsSelectedChanged(bool isSelected)
        {
            if (isSelected)
            {
                _normalBorder.Opacity = 0;
                _selBorder.Opacity = 1;
                _failBorder.Opacity = 0;
            }
            else
            {
                _normalBorder.Opacity = 1;
                _selBorder.Opacity = 0;
            }
        }

        public void OnIsFailChanged(bool isFail)
        {
            if (!isFail)
            {
                _failBorder.Opacity = 0;
            }
            else
            {
                _failBorder.Opacity = 1;
            }
        }



        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            //return base.HitTestCore(hitTestParameters);
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

    }
}
