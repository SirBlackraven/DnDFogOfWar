using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfApp1
{
    public enum DrawMode
    {
        NONE,
        DRAW,
        GMODE,
        DELETE
    }

    public enum StretchMode
    {
        UNIFORM,
        FILL,
        UNIFORM_FILL,
        NONE
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DrawMode drawMode = DrawMode.NONE;

        public bool fileLoaded = false;
        private Scribe scribe = new Scribe();
        private string imageSource;
        private StretchMode stretchMode = StretchMode.UNIFORM;
  
        private Point mousePosition = new Point();
        private Point mobileFlagPosition = new Point(0, 0);

        private Point mouseClick;

        private double canvasLeft;
        private double canvasTop;

        public MainWindow()
        {
            ComponentDispatcher.ThreadIdle += new System.EventHandler(ComponentDispatcher_ThreadIdle);

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            string loadPath = AppDomain.CurrentDomain.BaseDirectory + "Images\\facepalm.jpg";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.rdb_Uniform.IsChecked = true;

            //set events
            this.positionFlag.PreviewMouseDown += new MouseButtonEventHandler(FlagMouseButtonDown);
            this.positionFlag.PreviewMouseMove += new MouseEventHandler(FlagMouseMove);
            this.positionFlag.PreviewMouseUp += new MouseButtonEventHandler(FlagMouseUp);
            //this.mobileFlag.TextInput += new TextCompositionEventHandler(myimg_TextInput);
            this.positionFlag.LostMouseCapture += new MouseEventHandler(LostMouseCapture);

        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if((this.drawMode == DrawMode.DRAW) || (this.drawMode == DrawMode.DELETE))
                {
                    this.drawMode = DrawMode.NONE; //cancel drawing.
                    
                    //clean up-maybe
                }
            }
        }

        void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            this.mousePosition = Mouse.GetPosition(this.Cnv);

            //update debug display
            this.lbl_MousePos.Content = "X: " + mousePosition.X.ToString() + ", Y:" + mousePosition.Y.ToString();

            //RENDER  AREA//
            if (this.drawMode == DrawMode.DRAW)
            {
                this.Cnv.Children.Clear();

                //paint all established lines for this draw
                scribe.DrawCurrentStrip(this.Cnv);

                //paint the temp. point-mouse cursor line
                scribe.DrawActiveLine(this.Cnv);

                //paint pre-existing work
                //scribe.DrawExistingStrips(this.Cnv);

                scribe.DrawAllPolygons(this.Cnv);
            }

            if (this.drawMode == DrawMode.NONE)
            {
                this.Cnv.Children.Clear();

                //scribe.DrawExistingStrips(this.Cnv);

                scribe.DrawAllPolygons(this.Cnv);

                //this.Cnv.Children.Add(this.mobileFlag);
            }
        }

        public void cmd_OpenImageFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                Uri uri = new Uri(filename);
                BitmapImage bmp = new BitmapImage(uri);

                this.Overlay.Stretch = Stretch.Uniform;
                this.Overlay.Source = bmp;

                this.imageSource = filename;
            }
        }

        public void cmd_SaveFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pak";
            dlg.Filter = "PAK Files (*.pak)|*.pak";
            dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "SaveFiles\\";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                DataManager dm = new DataManager();
                string filename = dlg.FileName;
                dm.SaveData(this.scribe, filename, this.imageSource);
            }
        }

        public void cmd_OpenProject(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pak";
            dlg.Filter = "PAK Files (*.pak)|*.pak";
            dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "SaveFiles\\";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string fileName = dlg.FileName;

                DataManager dm = new DataManager();
                string imageFile = "";
                Scribe newScribe = new Scribe();

                if (dm.LoadData(fileName, newScribe, ref imageFile))
                {
                    //if the load succeeded, swap the scribe objects 
                    //and set the display image
                    this.scribe = newScribe;
                    
                    Uri uri = new Uri(imageFile);
                    BitmapImage bmp = new BitmapImage(uri);
                    this.Overlay.Source = bmp;

                    this.imageSource = imageFile;
                }
            }
        }

        public void cmd_StretchChanged(object sender, RoutedEventArgs e)
        {
            if(this.rdb_Fill.IsChecked == true)
            {
                this.stretchMode = StretchMode.FILL;
                this.Overlay.Stretch = Stretch.Fill;
                return;
            }

            if(this.rdb_Uniform.IsChecked == true)
            {
                this.stretchMode = StretchMode.UNIFORM;
                this.Overlay.Stretch = Stretch.Uniform;
                return;
            }

            if(this.rdb_None.IsChecked == true)
            {
                this.stretchMode = StretchMode.NONE;
                this.Overlay.Stretch = Stretch.None;
                return;
            }

            if(this.rdb_UniformFill.IsChecked == true)
            {
                this.stretchMode = StretchMode.UNIFORM_FILL;
                this.Overlay.Stretch = Stretch.UniformToFill;
            }
        }

        public void Clicked_grid_Main(object sender, MouseEventArgs e)
        {
            //TODO: disabled for testing
            //Enable this to force the user to load an image first
            //if(this.fileLoaded == false)
            //{
            //    return;
            //}

            Point point = e.GetPosition(this.Cnv);

            //main activity switch
            switch (this.drawMode)
            {
                case DrawMode.NONE:

                    this.drawMode = DrawMode.DRAW;

                    scribe.PlaceInitialPoint();

                    break;

                case DrawMode.DRAW:

                    //if strip is done, finalize it.
                    //check to see if the current and the first are near:
                    if (scribe.CheckSnap())
                    {
                        scribe.PlaceFinalPoint(Cnv);

                        //stop drawing
                        this.drawMode = DrawMode.NONE;
                    }
                    else
                    {
                        //otherwise, add a new point
                        scribe.PlacePoint();
                    }

                    break;

                case DrawMode.GMODE:                    

                    scribe.CheckPolygons(point);

                    break;

                case DrawMode.DELETE:

                    //Point point = e.GetPosition(this.Cnv);

                    scribe.CheckPolygonsForDelete(point);

                    this.drawMode = DrawMode.NONE;
                    this.Cursor = Cursors.Arrow;

                    break;
            }
        }

        //update display and the scribe's registers
        private void grid_Main_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(this.Cnv);

            this.scribe.xRegister = (int)point.X;
            this.scribe.yRegister = (int)point.Y;
            this.lbl_MousePos.Content = "X: " + point.X.ToString() + ", Y:" + point.Y.ToString();
        }

        private void cmd_Exit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void cmb_ClearAll(object sender, RoutedEventArgs e)
        {
            this.drawMode = DrawMode.NONE;
            scribe.ClearAllArtifacts();
        }

        private void cmd_ToggleBorders(object sender, RoutedEventArgs e)
        {
            scribe.ToggleBorders();
        }

        private void cmd_ForceBorders(object sender, RoutedEventArgs e)
        {
            scribe.ToggleForceBorders();

            if(scribe.ForceBorders)
            {
                this.lbl_ForceBorders.Visibility = Visibility.Visible;
            }
            else
            {
                this.lbl_ForceBorders.Visibility = Visibility.Hidden;
            }
        }

        private void cmd_GMode(object sender, RoutedEventArgs e)
        {
            if(this.drawMode == DrawMode.GMODE)
            {
                this.drawMode = DrawMode.NONE;
            }
            else
            {
                this.drawMode = DrawMode.GMODE;
            }
        }

        private void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
 
        }

        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void LostMouseCapture(object sender, MouseEventArgs e)
        {

            ((Image)sender).ReleaseMouseCapture();

        }

        void FlagMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).ReleaseMouseCapture();
        }

        private void FlagMouseButtonDown(object sender, MouseEventArgs e)
        {
            mouseClick = e.GetPosition(null);

            canvasLeft = this.positionFlag.Margin.Left;
            canvasTop = this.positionFlag.Margin.Top;

            this.positionFlag.CaptureMouse();
        }

         private void FlagMouseMove(object sender, MouseEventArgs e)
        {
            if (this.positionFlag.IsMouseCaptured)
            {
                if((canvasLeft == Double.NaN ) || (canvasTop == Double.NaN))
                {
                    return;
                }
                
                Point mouseCurrent = e.GetPosition(null);

                double Left = mouseCurrent.X - canvasLeft;
                double Top = mouseCurrent.Y - canvasTop;

                Thickness newMargin = new Thickness(canvasLeft + Left, canvasTop + Top, 0, 0);

                this.positionFlag.Margin = newMargin;

                canvasLeft = this.positionFlag.Margin.Left;
                canvasTop = this.positionFlag.Margin.Top;
            }
        }

        private void ErasePolygon_Click(object sender, RoutedEventArgs e)
        {
            this.drawMode = DrawMode.DELETE;

            this.Cursor = Cursors.Cross;
        }
    }
}
