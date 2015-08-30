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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PostBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        private string MyDoc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string WorkingFolder = "";
        private const string Storage = "Posts.xml";
        public MainWindow()
        {
            InitializeComponent();
            WorkingFolder = MyDoc + @"\PostBoard\";
            if (!System.IO.Directory.Exists(WorkingFolder))
            {
                System.IO.Directory.CreateDirectory(WorkingFolder);
            }
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.Write("button clicked");
            PostEditor source = e.Source as PostEditor;
            DependencyObject parent = VisualTreeHelper.GetParent(source);

            this.MyDesignerCanvas.DeletePost(source);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.Write("closing");
            string storageFile = WorkingFolder + Storage;

            SaveToFile(storageFile);


        }

        private void SaveToFile(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            if (System.IO.File.Exists(fileName))
            {
                xmlDoc.Load(fileName);
                //XmlElement docEle = xmlDoc.DocumentElement;
                //docEle.RemoveAll();
                xmlDoc.RemoveAll();
            }

            XmlElement root = xmlDoc.CreateElement("Posts");
            this.MyDesignerCanvas.SaveAllPosts(WorkingFolder, xmlDoc, root);
            xmlDoc.AppendChild(root);

            xmlDoc.Save(fileName);
        }

        private void LoadFromFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);
                XmlElement docEle = xmlDoc.DocumentElement; // element <Posts>
                MyDesignerCanvas.LoadAllPosts(docEle);    
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // check whether there are persisted posts
            string storageFile = WorkingFolder + Storage;
            LoadFromFile(storageFile);
        }

        private void OnYellowNoteClicked(object sender, RoutedEventArgs e)
        {
            MyDesignerCanvas.CreateTextPostAt(new Point(50, 50));
        }

        private void OnBlueNoteClicked(object sender, RoutedEventArgs e)
        {
            MyDesignerCanvas.CreateTextPostAt(new Point(50, 50), Colors.LightSkyBlue);
        }

        private void OnGreenNoteClicked(object sender, RoutedEventArgs e)
        {
            MyDesignerCanvas.CreateTextPostAt(new Point(50, 50), Colors.Honeydew);
        }

        private void OnPinkNoteClicked(object sender, RoutedEventArgs e)
        {
            MyDesignerCanvas.CreateTextPostAt(new Point(50, 50), Colors.Pink);
        }
    }
}
