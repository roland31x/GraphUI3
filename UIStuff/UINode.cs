using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;
using Windows.UI;
using System.Collections.Generic;
using Windows.Foundation;
using Graphing;
using System.Reflection.Metadata.Ecma335;
using GraphUI3.UIStuff;

namespace GraphUI3
{
#nullable enable
    public class UINode : IDeletable, IChangeable
    {
        public static readonly Brush BaseBodyColor = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
        public Node Child { get; private set; } 
        public double Value { get => Child.Value; private set { Child.Value = value; ChangedEvent?.Invoke(this, new System.EventArgs()); } }
        public string Name 
        { 
            get => Child.Name; 
            private set 
            { 
                Child.Name = value; 
                NameLabel.Text = value;
                ChangedEvent?.Invoke(this, new System.EventArgs());
            } 
        }
        public Point Location { get => new Point(Child.X, Child.Y); private set { Child.X = value.X; Child.Y = value.Y; ChangedEvent?.Invoke(this, new System.EventArgs()); } }

        readonly static SolidColorBrush SelectedOutline = new SolidColorBrush(Colors.Red);
        readonly static SolidColorBrush DeselectedOutline = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));

        Ellipse Body { get; set; }
        TextBlock NameLabel { get; set; }
        Ellipse BodyHitBox { get; set; }
        Ellipse SelectionBorder { get; set; }

        Brush _c = BaseBodyColor;
        public Brush BodyColor { get { return _c; } set { _c = value; Body.Fill = _c; } }

        bool _s = false;
        bool Selected
        {
            get => _s;
            set
            {
                _s = value;
                if (_s)
                    SelectionBorder.Fill = SelectedOutline;
                else
                    SelectionBorder.Fill = DeselectedOutline;
            }
        }

        public UINode(Node child, Canvas parent)
        {
            Child = child;
            Body = new Ellipse() { Height = 50, Width = 50, Fill = BodyColor };
            NameLabel = new TextBlock() { FontSize = 20, Text = Name };
            BodyHitBox = new Ellipse() { Height = 50, Width = 50, Fill = new SolidColorBrush(Colors.Transparent) };
            SelectionBorder = new Ellipse() { Height = 56, Width = 56, Fill = DeselectedOutline };
            BodyHitBox.PointerPressed += Body_MouseDown;
            BodyHitBox.PointerReleased += Body_MouseUp;
            BodyHitBox.DoubleTapped += BodyHitBox_DoubleTapped;

            SpawnOn(parent);

        }

        public delegate void MoveHandler(object sender, MoveEventArgs e);
        public event MoveHandler? Move;

        public delegate void SelectionHandler(object sender, SelectionEventArgs e);
        public event SelectionHandler? OnSelect;

        public event IChangeable.ChangedEventHandler? ChangedEvent;
        public event IDeletable.DeletionRequestHandler? DeleteRequest;
        public void SendDeleteRequest() => DeleteRequest?.Invoke(this, new DeletionEventArgs(GetAllElements()));
        public List<UIElement> GetAllElements() => new List<UIElement>() { Body, BodyHitBox, SelectionBorder, NameLabel };


        public void SetColor(Brush b) => BodyColor = b;
        void SpawnOn(Canvas canvas)
        {
            canvas.Children.Add(Body);
            canvas.Children.Add(NameLabel);
            canvas.Children.Add(BodyHitBox);
            canvas.Children.Add(SelectionBorder);

            MoveTo(Child.X, Child.Y);

            Canvas.SetZIndex(SelectionBorder, 1);
            Canvas.SetZIndex(Body, 2);
            Canvas.SetZIndex(NameLabel, 3);
            Canvas.SetZIndex(BodyHitBox, 4);
        }
        public void MoveTo(double X, double Y)
        {
            Location = new Point(X, Y);

            Canvas.SetTop(Body, Y - Body.Height / 2);
            Canvas.SetLeft(Body, X - Body.Width / 2);
            Canvas.SetTop(BodyHitBox, Y - Body.Height / 2);
            Canvas.SetLeft(BodyHitBox, X - Body.Width / 2);
            Canvas.SetTop(NameLabel, Y - Body.Height / 2);
            Canvas.SetLeft(NameLabel, X - Body.Width / 2);
            Canvas.SetTop(SelectionBorder, Y - SelectionBorder.Height / 2);
            Canvas.SetLeft(SelectionBorder, X - SelectionBorder.Width / 2);
        }
        private void BodyHitBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        { 
            Selected = !Selected;
            OnSelect?.Invoke(this, new SelectionEventArgs(Selected));
        }
        public void ResetSelect() => Selected = false;
        private void Body_MouseUp(object sender, PointerRoutedEventArgs e) => Move?.Invoke(this, new MoveEventArgs(false));
        private void Body_MouseDown(object sender, PointerRoutedEventArgs e)
        {          
            bool leftclick = false;
            bool rightclick = false;
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(Body).Properties;
                if (properties.IsLeftButtonPressed)
                    leftclick = true;
                else if (properties.IsRightButtonPressed)
                    rightclick = true;
            }

            if (leftclick)
                Move?.Invoke(this, new MoveEventArgs(true));
            else if (rightclick)
                ShowOptionsFlyout(e.GetCurrentPoint(Body).Position);          
        }
        void ShowOptionsFlyout(Point position)
        {
            Flyout fly = new Flyout();
            fly.Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Top;

            StackPanel sp = new StackPanel();
            fly.Content = sp;

            TextBox tb = new TextBox();
            tb.Text = Name;
            tb.Header = "Name:";
            tb.Margin = new Thickness(0, 0, 0, 10);
            tb.TextChanged += Tb_TextChanged;


            sp.Children.Add(tb);

            Button btn = new Button();
            btn.Content = "DELETE";
            btn.Background = new SolidColorBrush(Colors.Red);
            btn.Click += DeleteNode_Click;
            btn.HorizontalAlignment = HorizontalAlignment.Center;

            sp.Children.Add(btn);

            fly.ShowAt(BodyHitBox, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions(){ Position = position });
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e) => SendDeleteRequest();

        private void Tb_TextChanged(object sender, TextChangedEventArgs e) => Name = (sender as TextBox)!.Text.Replace("_","");
    }
}
