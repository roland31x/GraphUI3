using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Input;
using Windows.Foundation;
using Graphing;
using Windows.UI;
using GraphUI3.UIStuff;

namespace GraphUI3
{
    #nullable enable
    public class UIEdge : IDeletable, IChangeable
    {
        public static bool AutoDist = false;
        public readonly static Brush BaseColor = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));

        public Edge Child { get; private set; }
        public double Value { get => Child.Weight; set { Child.Weight = value; WeightCalc(); ChangedEvent?.Invoke(this, new EventArgs()); } }

        public UINode A { get; }
        public UINode B { get; }
        public Line LineBody { get; private set; } = new Line() { Stroke = BaseColor, StrokeThickness = 5, Fill = new SolidColorBrush(Colors.Black) };
        public TextBlock WeightLabel { get; private set; }


        public event IDeletable.DeletionRequestHandler? DeleteRequest;
        public event IChangeable.ChangedEventHandler? ChangedEvent;
        public void SendDeleteRequest() => DeleteRequest?.Invoke(this, new DeletionEventArgs(GetAllElements()));
        List<UIElement> GetAllElements() => new List<UIElement>() { LineBody, WeightLabel };

        public UIEdge(Edge child, UINode a, UINode b, Canvas parent)
        {
            Child = child;
            A = a;
            B = b;        
            LineBody.PointerPressed += Line_PointerPressed;
            WeightLabel = new TextBlock() { FontSize = 20 };
            WeightCalc();

            MoveTo(a.Location.X, a.Location.Y, a);
            MoveTo(b.Location.X, b.Location.Y, b);

            SpawnOn(parent);
        }

        public void SetColor(Brush b)
        {
            LineBody.Stroke = b;
        }
        void SpawnOn(Canvas canvas)
        {
            canvas.Children.Add(LineBody);
            canvas.Children.Add(WeightLabel);
        }
        void Line_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                if (e.GetCurrentPoint(LineBody).Properties.IsRightButtonPressed)
                    ShowOptionsFlyout(e.GetCurrentPoint(LineBody).Position);
        }
        void ShowOptionsFlyout(Point position)
        {
            Flyout fly = new Flyout();
            fly.Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Top;

            StackPanel sp = new StackPanel();
            fly.Content = sp;

            TextBox tb = new TextBox();
            tb.Text = Value.ToString();
            tb.Header = "Weight:";
            tb.Margin = new Thickness(0, 0, 0, 10);
            tb.TextChanged += Tb_TextChanged;
            if (UIEdge.AutoDist)
                tb.IsEnabled = false;

            sp.Children.Add(tb);

            Button btn = new Button();
            btn.Content = "DELETE";
            btn.Background = new SolidColorBrush(Colors.Red);
            btn.Click += DeleteEdge_Click;
            btn.HorizontalAlignment = HorizontalAlignment.Center;

            sp.Children.Add(btn);

            fly.ShowAt(LineBody, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions() { Position = position });
        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string text = tb.Text;
            if(tb.Text == "")
            {
                Value = 0;
                tb.Background = new SolidColorBrush(Colors.Black);
                return;
            }
            if (double.TryParse(text, out double value) && (value >= 1 || value == 0))
            {
                if (value >= 0)
                {
                    Value = value;
                    tb.Background = new SolidColorBrush(Colors.Black);
                }
                else
                    tb.Background = new SolidColorBrush(Colors.Red);
            }
            else
                tb.Background = new SolidColorBrush(Colors.Red);
        }

        private void DeleteEdge_Click(object sender, RoutedEventArgs e) => SendDeleteRequest();
        public void SetWeight(double value) => Value = value;
        void WeightCalc()
        {
            if (Value == 0)
                WeightLabel.Text = "";
            else
                WeightLabel.Text = Math.Round(Value,2).ToString();
        }
        
       
        public void MoveTo(double x, double y, UINode end)
        {
            if (end == A)
            {
                LineBody.X1 = x;
                LineBody.Y1 = y;
            }
            else
            {
                LineBody.X2 = x;
                LineBody.Y2 = y;
            }
            Canvas.SetLeft(WeightLabel, (LineBody.X1 + LineBody.X2) / 2);
            Canvas.SetTop(WeightLabel, (LineBody.Y1 + LineBody.Y2) / 2);
        }
    }
}
