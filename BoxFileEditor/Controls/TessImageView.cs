using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class TessImageView : MultiSelector
    {
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof (BitmapSource), typeof (TessImageView), new PropertyMetadata(default(BitmapSource)));

        public BitmapSource Image
        {
            get { return (BitmapSource) GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        private Image _backImage = null;
        private Grid _boxHost = null;
        private Canvas _rubberBandHost = null;
        private Rectangle _rubberBand = null;

        private TessBoxControl _mouseDownHitBox = null;
        private Point _mouseDownPos;

        public event EventHandler DeleteSelected;
        public event EventHandler MergeSelected;
        public event CreateBoxHandler CreateBox;

        static TessImageView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TessImageView), new FrameworkPropertyMetadata(typeof(TessImageView)));
        }

        public TessImageView()
        {
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
           // MainWindow.DataChanged = true;
            var box = SelectedItem as TessBoxControl;
            if (e.Key == Key.Left)
            {
                if (box != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        if (box.Width >= 5)
                        {
                            MainWindow.DataChanged = true;
                            box.Width--;
                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (box.Left >= 10) {
                            MainWindow.DataChanged = true;
                            box.Left -= 10;
                        }
                    }
                    else
                    {
                        if (box.Left > 1) {
                            MainWindow.DataChanged = true;
                            box.Left--;
                        }
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                if (box != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        if (box.Width < (Image.Width - box.Left - 1))
                        {
                            MainWindow.DataChanged = true;
                            box.Width++;
                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (box.Left < Image.Width - (10 + box.Width)) {
                            MainWindow.DataChanged = true;
                            box.Left += 10;
                        }
                    }
                    else
                    {
                        if (box.Left < Image.Width - (1+box.Width)) {
                            MainWindow.DataChanged = true;
                            box.Left++;
                        }
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (box != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        if (box.Height >= 5) {
                            MainWindow.DataChanged = true;
                            box.Height--;
                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    { 
                        if (box.Top >= 10) {
                            MainWindow.DataChanged = true;
                            box.Top -= 10;
                        }
                    }
                    else
                    { 
                        if (box.Top >= 1) {
                            MainWindow.DataChanged = true;
                            box.Top--;
                        }
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (box != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        if (box.Height < (Image.Height - box.Top - 1)) {
                            MainWindow.DataChanged = true;
                            box.Height++;
                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (box.Top < Image.Height - (10+box.Height)) {
                            box.Top += 10;
                            MainWindow.DataChanged = true;
                        }
                    }
                    else
                    {
                        if (box.Top < Image.Height - (1+box.Height)) {
                            MainWindow.DataChanged = true;
                            box.Top++;
                        }
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                if (box != null)
                {
                    var selContainer = ItemContainerGenerator.ContainerFromItem(box);
                    int index = ItemContainerGenerator.IndexFromContainer(selContainer);
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        index--;
                        if (index < 0) index = Items.Count;
                    }
                    else
                    {
                        index++;
                        if (index >= Items.Count) index = 0;
                    }
                    var nextContainer = ItemContainerGenerator.ContainerFromIndex(index);
                    if (nextContainer != null)
                    {
                        var nextBox = ItemContainerGenerator.ItemFromContainer(nextContainer);
                        SelectedItem = nextBox;
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && box != null)
            {
                var handler = DeleteSelected;
                if(handler != null)
                    handler.Invoke(this, new EventArgs());
                e.Handled = true;
                MainWindow.DataChanged = true;
            }
            else if (e.Key == Key.Insert && box != null)
            {
                var handler = MergeSelected;
                if(handler != null)
                    handler.Invoke(this, new EventArgs());
                e.Handled = true;
                MainWindow.DataChanged = true;
            }
            base.OnPreviewKeyDown(e);
        }

        public void ScrollIntoView(object item)
        {
            var container = ItemContainerGenerator.ContainerFromItem(item) as TessBoxControl;
            if (container != null)
            {
                var targetRect = new Rect(new Point(container.Left, container.Top), new Size(container.Width, container.Height));
                targetRect.Inflate(100, 50);
                _boxHost.BringIntoView(targetRect);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            _boxHost = GetTemplateChild("boxHost") as Grid;
            _backImage = GetTemplateChild("backImage") as Image;
            _rubberBandHost = GetTemplateChild("rubberBandHost") as Canvas;

            _boxHost.MouseLeftButtonDown += _boxHost_MouseLeftButtonDown;
            _boxHost.MouseLeftButtonUp += _boxHost_MouseLeftButtonUp;
            _boxHost.MouseMove += _boxHost_MouseMove;
        }

        private IEnumerable<TessBoxControl> GetHitBoxes(Rect rect)
        {
            var boxes = new List<TessBoxControl>();
            for (int i = 0; i < Items.Count; i++)
            {
                var box = ItemContainerGenerator.ContainerFromIndex(i) as TessBoxControl;
                var boxRect = new Rect(box.Left, box.Top, box.Width, box.Height);
                if(rect.Contains(boxRect))
                    boxes.Add(box);
            }

            return boxes;
        }

        private TessBoxControl GetHitBox(Point pt)
        {
            var result = VisualTreeHelper.HitTest(_boxHost, pt);
            if (result != null)
            {
                var hitControl = result.VisualHit;
                if (hitControl != null && !(hitControl is TessBoxControl))
                    hitControl = UIHelper.GetParent<TessBoxControl>(hitControl);

                if (hitControl != null && hitControl is TessBoxControl)
                    return hitControl as TessBoxControl;
            }
            return null;
        }

        void _boxHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_boxHost.IsMouseCaptured)
            {
                _mouseDownPos = e.GetPosition(_boxHost);
                _boxHost.CaptureMouse();

                _mouseDownHitBox = GetHitBox(_mouseDownPos);
                if (_mouseDownHitBox != null)
                    _mouseDownHitBox.Focus();
                else
                    Focus();
            }
            e.Handled = true;
        }

        void _boxHost_MouseMove(object sender, MouseEventArgs e)
        {
            if (_boxHost.IsMouseCaptured)
            {
                var mousePos = e.GetPosition(_boxHost);

                if (_rubberBand == null)
                {
                    _rubberBand = new Rectangle();
                    _rubberBand.IsHitTestVisible = false;
                    _rubberBand.Stroke = Brushes.LightGray;
                    _rubberBandHost.Children.Add(_rubberBand);
                }

                var left = (Math.Floor(Math.Min(_mouseDownPos.X, mousePos.X)) >= 0) ? Math.Floor(Math.Min(_mouseDownPos.X, mousePos.X)) : 0;
                var top = (Math.Floor(Math.Min(_mouseDownPos.Y, mousePos.Y)) >= 0) ? Math.Floor(Math.Min(_mouseDownPos.Y, mousePos.Y)) : 0;

                var width = (Math.Floor(Math.Abs(_mouseDownPos.X - mousePos.X)) <= Image.Width -left )? Math.Floor(Math.Abs(_mouseDownPos.X - mousePos.X)) : Image.Width-left;
                var height = (Math.Floor(Math.Abs(_mouseDownPos.Y - mousePos.Y)) <= Image.Height -top)? Math.Floor(Math.Abs(_mouseDownPos.Y - mousePos.Y)) : Image.Height-top;

                _rubberBand.Width = width;
                _rubberBand.Height = height;
                Canvas.SetLeft(_rubberBand, left);
                Canvas.SetTop(_rubberBand, top);
            }
        }

        void _boxHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (_boxHost.IsMouseCaptured)
            {
                var mouseUpPos = e.GetPosition(_boxHost);
                if (_rubberBand != null)
                {
                    _rubberBandHost.Children.Remove(_rubberBand);
                    _rubberBand = null;
                }
                _boxHost.ReleaseMouseCapture();

                var selectRec = new Rect(_mouseDownPos, mouseUpPos);
                //if they are holding one of the ctrl keys, make a new box...
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (selectRec.Size.Width >= 2 && selectRec.Height >= 2 && selectRec.Size.Width <= Image.Width && selectRec.Height <= Image.Height
                        && selectRec.Left >= 0 && selectRec.Top >=0 && selectRec.Left <= Image.Width - selectRec.Left && selectRec.Top <= Image.Height - selectRec.Top
                        )
                    {
                        var hanlder = CreateBox;
                        if(hanlder != null)
                            hanlder.Invoke(this, selectRec);
                    }
                }
                else
                {
                    var mouseUpHitBox = GetHitBox(mouseUpPos);
                    if (_mouseDownHitBox == null)
                    {
                        var boxes = GetHitBoxes(selectRec);
                        SelectedItems.Clear();
                        foreach (var box in boxes)
                            SelectedItems.Add(box);
                    }
                    else
                    {
                        if (_mouseDownHitBox == mouseUpHitBox)
                        {
                            //mouse down and up on the same box!
                            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                                SelectedItems.Clear();

                            if (mouseUpHitBox.IsSelected)
                                SelectedItems.Remove(mouseUpHitBox);
                            else
                                SelectedItems.Add(mouseUpHitBox);
                        }
                        else
                        {
                            var boxes = GetHitBoxes(selectRec);
                            SelectedItems.Clear();
                            foreach (var box in boxes)
                                SelectedItems.Add(box);
                        }
                    }
                }
                
                _mouseDownHitBox = null;
            }
            e.Handled = true;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            foreach (TessBoxControl box in e.RemovedItems)
                box.IsSelected = false;

            foreach (TessBoxControl box in e.AddedItems)
                box.IsSelected = true;

        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TessBoxControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TessBoxControl;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
        }

    }

    public delegate void CreateBoxHandler(object sender, Rect bounds);
}
