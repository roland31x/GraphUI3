using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Runtime.InteropServices;
using WinRT;
using Graphing;
using Windows.UI;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GraphUI3
{
    public sealed partial class MainWindow : Window
    {
        #region static stuff
        public static Random rng = new Random();
        public static List<Brush> colors = new List<Brush>() 
        { 
            new SolidColorBrush(Colors.Red), 
            new SolidColorBrush(Colors.Orange), 
            new SolidColorBrush(Colors.Yellow), 
            new SolidColorBrush(Colors.Yellow), 
            new SolidColorBrush(Colors.LimeGreen), 
            new SolidColorBrush(Colors.Green), 
            new SolidColorBrush(Colors.Cyan), 
            new SolidColorBrush(Colors.Blue), 
            new SolidColorBrush(Colors.Purple) 
        };
        #endregion

        string LoadedPath = "";
        Graph _g = new Graph("UntitledGraph");
        public Graph LoadedGraph { get => _g; set { _g = value; TitleBlock.Text = "GraphUI3 - " + LoadedPath + " " + _g.Name; ReInitOnGraph(); } }
       
        public Dictionary<Node,UINode> nodes = new Dictionary<Node, UINode>();
        public Dictionary<Edge,UIEdge> edges = new Dictionary<Edge, UIEdge>();
        UINode? held = null;

        bool _ss = false;
        bool specialselection
        {
            get
            {
                return _ss;
            }
            set
            {
                _ss = value;
                if (_ss)
                    AlgoButton.IsEnabled = false;
                else
                    AlgoButton.IsEnabled = true;
            }
        }
        bool cancelalgo = false;

        List<UINode> selection = new List<UINode>();

        bool FindAllPaths = false;

        bool _c = true;
        bool changes 
        { 
            get => _c; 
            set 
            { 
                _c = value;
                if (_c)
                    TitleBlock.Text = "GraphUI3 - " + LoadedPath + " " + _g.Name + "*";
                else
                    TitleBlock.Text = "GraphUI3 - " + LoadedPath + " " + _g.Name;                     
            } 
        }

        bool _l = false;
        DispatcherTimer stucktimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        bool loading
        {
            get => _l;
            set
            {
                _l = value;
                if (_l)
                {
                    stucktimer.Start();
                    Overlay.Visibility = Visibility.Visible;
                    LoadingRing.IsActive = true;
                }
                else
                {
                    stucktimer.Stop();
                    Overlay.Visibility = Visibility.Collapsed;
                    LoadingRing.IsActive = false;
                }
            }
        }
        
        public MainWindow(string path)
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            stucktimer.Tick += Stucktimer_Tick;

            if(path == null)
                TitleBlock.Text = "GraphUI3 -  " + LoadedPath + _g.Name + "*";
            else
            {
                LoadedPath = path;
                LoadedGraph = Graph.ParseFile(LoadedPath);
                LoadedGraph.Name = path.Split('\\').Last().Replace(".grph", "");
                changes = false;
            }

        }

        private async void Stucktimer_Tick(object? sender, object e)
        {
            stucktimer.Stop();
            if (await ShowStuckDialog())
                Close();
            if(loading)
                stucktimer.Start();
        }

        #region GRAPH UI MANIPULATION
        void ReInitOnGraph()
        {            
            foreach(Node n in LoadedGraph.Nodes)
            {
                UINode toadd = new UINode(n, GraphCanvas);
                toadd.Move += MoveNode;
                toadd.OnSelect += SelectNode;
                toadd.DeleteRequest += DeleteNode;
                toadd.ChangedEvent += ChangedEvent;
                nodes.Add(n, toadd);
            }
            foreach(Edge e in LoadedGraph.Edges)
            {
                UIEdge toadd = new UIEdge(e, nodes[e.A], nodes[e.B], GraphCanvas);
                toadd.DeleteRequest += DeleteEdge;
                toadd.ChangedEvent += ChangedEvent;
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
            toadd.ChangedEvent += ChangedEvent;

            nodes.Add(graphnode, toadd);
            changes = true;

            return toadd;
        }

        private void ChangedEvent(object sender, EventArgs e) => changes = true;

        private void GraphCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => NewNode(e.GetPosition(GraphCanvas).X, e.GetPosition(GraphCanvas).Y);

        private void SelectNode(object sender, SelectionEventArgs e)
        {
            if(e.isSelected)
                selection.Add(sender as UINode);
            else
                selection.Remove(sender as UINode);

            if(selection.Count == 2 && !specialselection)
            {
                Edge graphedge = new Edge(selection.First().Child, selection.Last().Child);
                LoadedGraph.Edges.Add(graphedge);

                UIEdge toadd = new UIEdge(graphedge, selection.First(), selection.Last(), GraphCanvas);

                // remove this once i implement multiple edges between same pair of nodes
                if (!edges.Values.Where(x => (x.A == toadd.A && x.B == toadd.B) || (x.A == toadd.B && x.B == toadd.A)).Any())
                {
                    toadd.DeleteRequest += DeleteEdge;
                    edges.Add(graphedge, toadd);
                }

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
            edge.ChangedEvent -= ChangedEvent;

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
            todelete.ChangedEvent -= ChangedEvent;

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

            foreach (UIEdge edge in edges.Values.Where(x => x.A == held || x.B == held))
                edge.MoveTo(xpos, ypos, held);
        }
        #endregion

        #region EDIT BAR STUFF
        private void ResetWeights_Click(object sender, RoutedEventArgs e)
        {
            edges.Values.ToList().ForEach(x => x.SetWeight(0));
        }
        private async void AutoWeights_Click(object sender, RoutedEventArgs e)
        {
            if (UIEdge.AutoDist)
            {
                UIEdge.AutoDist = false;
                (sender as MenuFlyoutItem)!.Background = new SolidColorBrush(Colors.Transparent);
                return;
            }

            (sender as MenuFlyoutItem)!.Background = new SolidColorBrush(Colors.Purple);
            UIEdge.AutoDist = true;
            while (UIEdge.AutoDist)
            {
                edges.Values.ToList().ForEach(x => x.SetWeight(GraphExt.EuclidDist(x.Child.A, x.Child.B)));
                await Task.Delay(100);
            }
            
        }
        #endregion

        #region FILE INPUT/OUTPUT
        async Task<bool> Reset(bool force = false)
        {
            if (changes && !force)
                if (!await ShowSaveDialog())
                    return false;

            nodes.Values.ToList().ForEach(x => x.SendDeleteRequest());

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
                try
                {
                    LoadedGraph = Graph.ParseFile(LoadedPath);
                }
                catch(Exception)
                {
                    ContentDialog dialog = new ContentDialog();

                    dialog.XamlRoot = MainGrid.XamlRoot;
                    dialog.Title = "Failed to load file!";
                    dialog.PrimaryButtonText = "Ok";

                    await dialog.ShowAsync();

                    LoadedPath = "";
                    LoadedGraph = new Graph("UntitledGraph");
                    changes = true;
                    return;
                }
                
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

            changes = false;
            return true;
        }
        private async void SaveFile_Click(object sender, RoutedEventArgs e) => await SaveGraph();
        private async void SaveAsFile_Click(object sender, RoutedEventArgs e) => await SaveAsGraph();
        private async Task<bool> ShowSaveDialog()
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = MainGrid.XamlRoot;
            dialog.Title = "Save changes?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.HorizontalContentAlignment = HorizontalAlignment.Center;

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

        private async Task<bool> ShowStuckDialog()
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = MainGrid.XamlRoot;
            dialog.Title = $"Oops, looks like you started a hefty algorithm...{Environment.NewLine}The program might be stuck for a while, consider saving and quitting.";
            dialog.PrimaryButtonText = "Save & Quit";
            dialog.SecondaryButtonText = "Quit without saving";
            dialog.CloseButtonText = "Wait";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.HorizontalContentAlignment = HorizontalAlignment.Center;

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                changes = false;
                await SaveGraph();
                return true;
            }
            else if (result == ContentDialogResult.Secondary)
                return true;
            return false;
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (changes && !await ShowSaveDialog())
                return;
            Close();
        }
        #endregion

        #region RAW INFO 
        private void RawToClipboard_Click(object sender, RoutedEventArgs args)
        {
            var package = new DataPackage();
            package.SetText(LoadedGraph.ToString());
            Clipboard.SetContent(package);
        }
        private void RawButton_Click(object sender, RoutedEventArgs e) => GraphInfo.Text = LoadedGraph.ToString();
        #endregion

        #region UI COLORING
        private void ResetColors_Click(object sender, RoutedEventArgs e)
        {
            nodes.Values.ToList().ForEach(x => x.SetColor(UINode.BaseBodyColor));
            edges.Values.ToList().ForEach(x => x.SetColor(UIEdge.BaseColor));
        }
        void ColorAllEdges(List<UIEdge> edges, Brush b) => edges.ForEach(x => x.SetColor(b));

        public Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 5);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, (byte)ascending, 0);
                case 1:
                    return Color.FromArgb(255, (byte)descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, (byte)ascending);
                case 3:
                    return Color.FromArgb(255, 0, (byte)descending, 255);
                default: // case 4
                    return Color.FromArgb(255, (byte)ascending, 0, 255);
            }
        }
        #endregion

        #region ALGORITHMS STUFF

        #region ALGORITHMS
        private async void Color_Click(object sender, RoutedEventArgs e)
        {
            loading = true;
            AlgoFlyout.Hide();

            Dictionary<Node, int> colored = await Task.Run(() => LoadedGraph.GraphColor());

            foreach (Node node in colored.Keys)
                nodes[node].SetColor(colors[colored[node]]);

            loading = false;
        }
        private async void Hamilton_Click(object sender, RoutedEventArgs e)
        {
            loading = true;
            AlgoFlyout.Hide();

            List<List<Node>> ham = await Task.Run(() => LoadedGraph.Hamilton(returnallpaths: FindAllPaths, cycle: false));         

            ShowNodePathFlyoutMenu(ham);

            loading = false;
        }       

        private async void HamiltonCycle_Click(object sender, RoutedEventArgs e)
        {
            loading = true;
            AlgoFlyout.Hide();

            List<List<Node>> hamc = await Task.Run(() => LoadedGraph.Hamilton(returnallpaths: FindAllPaths, cycle: true));

            loading = false;

            ShowNodePathFlyoutMenu(hamc);
        }

        private async void Euler_Click(object sender, RoutedEventArgs e)
        {
            loading = true;
            AlgoFlyout.Hide();

            List<List<Edge>> eup = await Task.Run(() => LoadedGraph.Euler(returnallpaths: FindAllPaths, cycle: false));

            loading = false;

            ShowNodePathFlyoutMenu(eup);
        }
        private async void EulerCycle_Click(object sender, RoutedEventArgs e)
        {
            loading = true; 
            AlgoFlyout.Hide();

            List<List<Edge>> euc = await Task.Run(() => LoadedGraph.Euler(returnallpaths: FindAllPaths, cycle: true));

            loading = false;

            ShowNodePathFlyoutMenu(euc);
        }
        private async void Dijkstra_Click(object sender, RoutedEventArgs e)
        {
            specialselection = true;
            AlgoFlyout.Hide();

            ShowSpecialSelectionInfo();

            while (selection.Count < 2 && !cancelalgo)
            {
                await Task.Delay(10);
            }

            specialselection = false;

            UnloadSpecialSelection();
            if (cancelalgo)
            {
                cancelalgo = false;
                ShowCancelAlgoInfo();
               
                foreach (UINode node in selection)
                    node.ResetSelect();
                selection.Clear();

                return;
            }

            

            Node start = selection.First().Child;
            Node target = selection.Last().Child;
            
            foreach (UINode node in selection)
                node.ResetSelect();
            selection.Clear();

            await Task.Delay(100);

            loading = true;

            (List<Node> path, double dist) = await Task.Run(() => LoadedGraph.Dijkstra(start, target));

            Flyout fly = new Flyout();

            StackPanel spmain = new StackPanel();
            fly.Content = spmain;

            TextBlock tb = new TextBlock() 
            { 
                Text = (dist == -1 ? "No path found between the two nodes!." : $"Shortest distance between the two nodes is {dist}."), 
                Margin = new Thickness(0, 0, 0, 10) 
            };
            spmain.Children.Add(tb);

            fly.ShowAt(GraphCanvas, new FlyoutShowOptions() { Position = new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2) });

            ColorAllEdges(edges.Values.ToList(), UIEdge.BaseColor);

            List<UIEdge> tocolor = new List<UIEdge>();
            for (int i = 0; i < path.Count - 1; i++)
                tocolor.Add(edges[LoadedGraph.Edges.First(x => (x.A == path[i] && x.B == path[i + 1]) || (x.B == path[i] && x.A == path[i + 1]))]);

            for (int i = 0; i < tocolor.Count; i++)
                tocolor[i].SetColor(new SolidColorBrush(Rainbow((float)i / (float)tocolor.Count)));

            loading = false;
        }
        private async void Kruskal_Click(object sender, RoutedEventArgs e)
        {
            loading = true;
            AlgoFlyout.Hide();

            List<Edge> mst = await Task.Run(() => LoadedGraph.Kruskal());
            if (!mst.Any())
            {
                MainInfoBar.Severity = InfoBarSeverity.Error;
                MainInfoBar.Title = "Error";
                MainInfoBar.IsOpen = true;
                MainInfoBar.Message = "No minimum spanning tree found! Graph is disconnected!";
                MainInfoBar.IsClosable = true;
                infoactionbutton.Visibility = Visibility.Collapsed;
            }
            Graph newgraph = new Graph(LoadedGraph.Name);
            foreach (Node n in nodes.Keys)
                newgraph.Nodes.Add(n);
            mst.ForEach(x => newgraph.Edges.Add(x));

            await Reset(true);
            LoadedGraph = newgraph;
            changes = true;

            loading = false;
        }

        void ShowCancelAlgoInfo()
        {
            MainInfoBar.Severity = InfoBarSeverity.Success;
            MainInfoBar.Title = "Special Selection";
            MainInfoBar.Message = "Special Selection deactivated";
            MainInfoBar.IsOpen = true;
            infoactionbutton.Visibility = Visibility.Collapsed;
            MainInfoBar.IsClosable = true;
        }
        void ShowSpecialSelectionInfo()
        {
            MainInfoBar.Severity = InfoBarSeverity.Informational;
            MainInfoBar.Title = "Special Selection";
            MainInfoBar.Message = "Special Selection is active, to disable it press:";
            MainInfoBar.IsOpen = true;
            infoactionbutton.Visibility = Visibility.Visible;
            infoactionbutton.Content = "Cancel Selection";
            infoactionbutton.Click += StopAlgorithm;
            MainInfoBar.IsClosable = false;
        }
        void UnloadSpecialSelection()
        {
            infoactionbutton.Click -= StopAlgorithm;
            MainInfoBar.IsOpen = false;
        }
        void StopAlgorithm(object sender, RoutedEventArgs e) => cancelalgo = true;

        #endregion

        #region UI AUX
        private void ListSelect_Click(object sender, RoutedEventArgs e)
        {
            ColorAllEdges(edges.Values.ToList(), UIEdge.BaseColor);

            List<UIEdge> tocolor = new List<UIEdge>();

            if ((sender as Button)!.Tag is List<Node> clicked)
                for (int i = 0; i < clicked.Count - 1; i++)
                    tocolor.Add(edges[LoadedGraph.Edges.First(x => (x.A == clicked[i] && x.B == clicked[i + 1]) || (x.B == clicked[i] && x.A == clicked[i + 1]))]);
            else
                ((sender as Button)!.Tag as List<Edge>)!.ForEach(x => tocolor.Add(edges[x]));


            for (int i = 0; i < tocolor.Count; i++)
                tocolor[i].SetColor(new SolidColorBrush(Rainbow((float)i / (float)tocolor.Count)));

        }

        void ShowNodePathFlyoutMenu(List<List<Node>> paths)
        {
            Flyout fly = new Flyout();
            fly.Placement = FlyoutPlacementMode.Top;

            StackPanel spmain = new StackPanel();
            fly.Content = spmain;
            fly.Closed += NodePathFlyout_Closed;

            TextBlock tb = new TextBlock() { Text = "Select one of the paths to show:", Margin = new Thickness(0, 0, 0, 10) };
            spmain.Children.Add(tb);

            if (!paths.Any())
                tb.Text = "No paths Found!";

            ScrollViewer sw = new ScrollViewer() { MaxHeight = 200 };
            spmain.Children.Add(sw);

            StackPanel sp = new StackPanel();
            sw.Content = sp;

            foreach (List<Node> list in paths)
            {
                Button bt = new Button();
                string content = "";
                list.ForEach(x => content += LoadedGraph.Nodes.IndexOf(x) + " ");
                bt.Content = content;
                sp.Children.Add(bt);
                bt.Tag = list;
                bt.Click += ListSelect_Click;
                bt.HorizontalAlignment = HorizontalAlignment.Center;
            }

            fly.ShowAt(GraphCanvas, new FlyoutShowOptions() { Position = new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2) });
        }
        void ShowNodePathFlyoutMenu(List<List<Edge>> paths)
        {
            Flyout fly = new Flyout();
            fly.Placement = FlyoutPlacementMode.Top;

            StackPanel spmain = new StackPanel();
            fly.Content = spmain;
            fly.Closed += NodePathFlyout_Closed;

            TextBlock tb = new TextBlock() { Text = "Select one of the paths to show:", Margin = new Thickness(0, 0, 0, 10) };
            spmain.Children.Add(tb);

            if (!paths.Any())
                tb.Text = "No paths Found!";

            ScrollViewer sw = new ScrollViewer() { MaxHeight = 200 };
            spmain.Children.Add(sw);

            StackPanel sp = new StackPanel();
            sw.Content = sp;

            foreach (List<Edge> list in paths)
            {
                Button bt = new Button();
                string content = "";
                list.ForEach(x => content += LoadedGraph.Edges.IndexOf(x) + " ");
                bt.Content = content;
                sp.Children.Add(bt);
                bt.Tag = list;
                bt.Click += ListSelect_Click;
                bt.HorizontalAlignment = HorizontalAlignment.Center;
            }

            fly.ShowAt(GraphCanvas, new FlyoutShowOptions() { Position = new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2) });
        }

        private void NodePathFlyout_Closed(object? sender, object e)
        {
            Flyout f = (Flyout)sender!;
            StackPanel spmain = (StackPanel)f.Content;
            ScrollViewer sw = (ScrollViewer)spmain.Children.First(x => x is ScrollViewer);
            StackPanel sp = (StackPanel)sw.Content;
            foreach (Button b in sp.Children.OfType<Button>())
                b.Click -= ListSelect_Click;

            f.Closed -= NodePathFlyout_Closed;
            f = null!;
        }

        #endregion

        #region SETTINGS

        private void AllPathToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (AllPathToggleSwitch.IsOn)
                FindAllPaths = true;
            else
                FindAllPaths = false;
        }
        #endregion

        #endregion

        private async void GitHubDocs_Click(object sender, RoutedEventArgs e)
        {
            _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(@"https://github.com/roland31x/GraphUI3/blob/master/README.md"));
        }

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
