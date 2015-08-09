using System;
using System.Collections.Generic;
using System.IO;
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

namespace PostBoard
{
    /// <summary>
    /// Interaction logic for PostEditor.xaml
    /// </summary>
    public partial class PostEditor : UserControl
    {
        private DateTime _createTime = DateTime.Now;

        public PostEditor()
        {
            InitializeComponent();
            //ShowCreationTime();
        }

        public void SetFocus()
        {
            this._postEdit.Focus();
        }

        public void ShowCreationTime()
        {
            this._createTimeLabel.Text = _createTime.ToShortDateString() + " " + _createTime.ToShortTimeString();
        }

        public void SetCreationTime(DateTime time)
        {
            _createTime = time;
            ShowCreationTime();
        }

        public void SavePost(string postFile)
        {
            FileStream fileStream = new FileStream(postFile, FileMode.Create);
            TextRange range = new TextRange(_postEdit.Document.ContentStart, _postEdit.Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
            fileStream.Close();
        }

        public void LoadPost(string postFile)
        {
            FileStream fileStream = new FileStream(postFile, FileMode.Open);
            TextRange range = new TextRange(_postEdit.Document.ContentStart, _postEdit.Document.ContentEnd);
            range.Load(fileStream, DataFormats.Rtf);
            fileStream.Close();

        }


    }
}
