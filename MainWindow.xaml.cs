using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Graphing;
using Microsoft.UI.Xaml.Documents;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Windows.UI.Popups;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GraphUI3
{
    public sealed partial class MainWindow : Window
    {
        public static Random rng = new Random();

        string LoadedPath = "";
        Graph _g = new Graph("UntitledGraph");
        public Graph LoadedGraph { get => _g; set { _g = value; Title = "GraphUI3 - " + LoadedPath + " " + _g.Name; ReInitOnGraph(); } }
        
        public Dictionary<Node,UINode> nodes = new Dictionary<Node, UINode>();
        public Dictionary<Edge,UIEdge> edges = new Dictionary<Edge, UIEdge>();

        UINode? held = null;

        List<UINode> selection = new List<UINode>();

        bool _c = true;
        bool changes 
        { 
            get => _c; 
            set 
            { 
                _c = value;
                if (_c)
                    Title = "GraphUI3 - " + LoadedPath + " " + _g.Name + "*";
                else
                    Title = "GraphUI3 - " + LoadedPath + " " + _g.Name;                     
            } 
        }
        public static List<Brush> colors = new List<Brush>() { new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Pink), new SolidColorBrush(Colors.Blue), new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Purple) };
        
        public MainWindow()
        {
            InitializeComponent();
            Title = "GraphUI3 - " + LoadedPath + _g.Name + "*";
        }
        void ReInitOnGraph()
        {
            foreach(Node n in LoadedGraph.Nodes)
            {
                UINode toadd = new UINode(n, GraphCanvas);
                toadd.Move += MoveNode;
                toadd.OnSelect += SelectNode;
                toadd.DeleteRequest += DeleteNode;

                nodes.Add(n, toadd);
            }
            foreach(Edge e in LoadedGraph.Edges)
            {
                UIEdge toadd = new UIEdge(e, nodes[e.A], nodes[e.B], GraphCanvas);
                toadd.DeleteRequest += DeleteEdge;
                edges.Add(e, toadd);
            }
        }
        private UINode NewNode(double X = -1, double Y = -1)
        {
            Node graphnode = new Node();
            if(X > 0 && Y > 0)
            {
                graphnode.X = X;
                graphnode.Y = Y;
            }
            else
            {
                graphnode.X = GraphCanvas.ActualWidth / 2;
                graphnode.Y = GraphCanvas.ActualHeight / 2;
            }
            LoadedGraph.Nodes.Add(graphnode);

            UINode toadd = new UINode(graphnode, GraphCanvas);
            toadd.Move += MoveNode;
            toadd.OnSelect += SelectNode;
            toadd.DeleteRequest += DeleteNode;

            nodes.Add(graphnode, toadd);

            changes = true;

            return toadd;
        }
        private void GraphCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => NewNode(e.GetPosition(GraphCanvas).X, e.GetPosition(GraphCanvas).Y);
        //private void NewNode_Click(object sender, RoutedEventArgs e) => NewNode();

        private void SelectNode(object sender, SelectionEventArgs e)
        {
            if(e.isSelected)
                selection.Add(sender as UINode);
            else
                selection.Remove(sender as UINode);

            if(selection.Count == 2)
            {
                Edge graphedge = new Edge(selection.First().Child, selection.Last().Child);
                LoadedGraph.Edges.Add(graphedge);

                UIEdge toadd = new UIEdge(graphedge, selection.First(), selection.Last(), GraphCanvas);
                toadd.DeleteRequest += DeleteEdge;
                edges.Add(graphedge, toadd);

                foreach (UINode node in selection)
                    node.ResetSelect();

                selection.Clear();

                changes = true;
            }
            
        }

        void DeleteEdge(object sender, DeletionEventArgs e)
        {
            UIEdge edge = (UIEdge)sender;

            edge.DeleteRequest -= DeleteEdge;

            foreach (UIElement element in e.ToRemove)
                GraphCanvas.Children.Remove(element);

            LoadedGraph.Edges.Remove(edge.Child);
            edges.Remove(edge.Child);

            changes = true;
        }
        void DeleteNode(object sender, DeletionEventArgs e)
        {
            UINode todelete = (UINode)sender;

            todelete.DeleteRequest -= DeleteNode;
            todelete.Move -= MoveNode;
            todelete.OnSelect -= SelectNode;

            foreach(UIElement element in e.ToRemove)
                GraphCanvas.Children.Remove(element);   

            List<UIEdge> toremove = new List<UIEdge>();
            foreach (UIEdge edge in edges.Values.Where(x => x.A == todelete || x.B == todelete))
                toremove.Add(edge);

            foreach (UIEdge edge in toremove)
                edge.SendDeleteRequest();

            LoadedGraph.Nodes.Remove(todelete.Child);
            nodes.Remove(todelete.Child);

            changes = true;
        }

        private void MoveNode(object sender, MoveEventArgs e)
        {
            if (e.Move)
                held = (sender as UINode);
            else
                held = null;
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            
        }
        async Task<bool> Reset()
        {
            if (changes)
                if (!await ShowSaveDialog())
                    return false;

            foreach(UINode node in nodes.Values.ToList())
                node.SendDeleteRequest();

            GraphCanvas.Children.Clear();
            nodes.Clear();
            edges.Clear();

            return true;
        }
        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (await Reset())
                changes = true;           
        }
        private async void New_Click(object sender, RoutedEventArgs e)
        {
            if (await Reset())
            {
                LoadedPath = "";
                LoadedGraph = new Graph("UntitledGraph");
                changes = true;
            }             
        }
        private void LoadFile_Click(object sender, RoutedEventArgs e) => LoadGraph();

        async void LoadGraph()
        {
            if (!await ShowSaveDialog())
                return;

            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".grph");           
           
            IInitializeWithWindow withWindow = picker.As<IInitializeWithWindow>();
            IntPtr handle = this.As<IWindowNative>().WindowHandle;
            withWindow.Initialize(handle);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {             
                LoadedPath = file.Path;
                changes = false;
                await Reset();
                LoadedGraph = Graph.ParseFile(LoadedPath);
                LoadedGraph.Name = file.Name.Replace(".grph", "");
                changes = false;
            }

        }
        async Task<bool> SaveGraph()
        {
            if(LoadedPath != "")
            {
                using (StreamWriter sw = new StreamWriter(LoadedPath))
                {
                    sw.Write(LoadedGraph.ToString());
                }
                changes = false;
                return true;
            }
            return await SaveAsGraph();               
        }
        async Task<bool> SaveAsGraph()
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeChoices.Add("Graph", new List<string>() { ".grph" });
            picker.SuggestedFileName = LoadedGraph.Name;

            IInitializeWithWindow withWindow = picker.As<IInitializeWithWindow>();
            IntPtr handle = this.As<IWindowNative>().WindowHandle;
            withWindow.Initialize(handle);

            StorageFile file = await picker.PickSaveFileAsync();

            if (file == null)
                return false;

            changes = false;

            LoadedGraph.Name = file.Name.Replace(".grph", "");
            LoadedPath = file.Path;          
            if (!File.Exists(LoadedPath))
                File.Create(LoadedPath).Dispose();
            using (StreamWriter sw = new StreamWriter(LoadedPath))
            {
                sw.Write(LoadedGraph.ToString());
            }
            return true;
        }
        private async void SaveFile_Click(object sender, RoutedEventArgs e) => await SaveGraph();
        private async void SaveAsFile_Click(object sender, RoutedEventArgs e) => await SaveAsGraph();

        private void MainGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (held == null)
                return;
            
            double xpos = e.GetCurrentPoint(MainGrid).Position.X;
            double ypos = e.GetCurrentPoint(MainGrid).Position.Y;

            if (xpos < 0)
                xpos = 0;
            if (ypos < 0)
                ypos = 0;
            if (ypos > GraphCanvas.ActualHeight)
                ypos = GraphCanvas.ActualHeight;
            if (xpos > GraphCanvas.ActualWidth)
                xpos = GraphCanvas.ActualWidth;

            held.MoveTo(xpos, ypos);
            changes = true;

            foreach (UIEdge edge in edges.Values.Where(x => x.A == held || x.B == held))
                edge.MoveTo(xpos, ypos, held);
        }

        private async Task<bool> ShowSaveDialog()
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = MainGrid.XamlRoot;
            dialog.Title = "Save changes?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            ContentDialogResult result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {              
                changes = false;
                return await SaveGraph();
            }
            else if(result == ContentDialogResult.Secondary)
                return true;
            return false;
        }
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (changes && !await ShowSaveDialog())
                return;
            Close();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e) => GraphInfo.Text = LoadedGraph.ToString();

        #region Workaround stuff
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        internal interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
        #endregion


    }
}
