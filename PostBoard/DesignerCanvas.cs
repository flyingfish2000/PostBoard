using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using Microsoft.Win32;
using PostBoard.Adorners;
using System.Windows.Media;

namespace PostBoard
{
    public class DesignerCanvas : Canvas
    {
        private Point? dragStartPoint = null;

        public IEnumerable<DesignerItem> SelectedItems
        {
            get
            {
                var selectedItems = from item in this.Children.OfType<DesignerItem>()
                                    where item.IsSelected == true
                                    select item;

                return selectedItems;
            }
        }

        public void DeletePost(PostEditor postEditor)
        {
            DesignerItem toBeDeleted = null;
            foreach(DesignerItem item in this.Children.OfType<DesignerItem>()){
                if (item.Content == postEditor)
                {
                    toBeDeleted = item;
                }
            }

            this.Children.Remove(toBeDeleted);
        }

        public void SaveAllPosts(string workingFolder, XmlDocument xmlDoc, XmlElement rootEle)
        {
            int counter = 0;
            foreach (DesignerItem item in this.Children.OfType<DesignerItem>())
            {
                double width = item.Width;
                double height = item.Height;
                double top = (double)item.GetValue(Canvas.TopProperty);
                double left = (double)item.GetValue(Canvas.LeftProperty);

                XmlElement postEle = xmlDoc.CreateElement("Post");
                XmlAttribute widthAttr = CreateAttribute(xmlDoc, width, "width");
                postEle.Attributes.Append(widthAttr);

                XmlAttribute heightAttr = CreateAttribute(xmlDoc, height, "height");
                postEle.Attributes.Append(heightAttr);

                XmlAttribute topAttr = CreateAttribute(xmlDoc, top, "top");
                postEle.Attributes.Append(topAttr);

                XmlAttribute leftAttr = CreateAttribute(xmlDoc, left, "left");
                postEle.Attributes.Append(leftAttr);

                XmlAttribute colorAttr = xmlDoc.CreateAttribute("color");               
                colorAttr.Value = ((PostEditor)item.Content).BackgroundColor.ToString();
                postEle.Attributes.Append(colorAttr);

                XmlAttribute timeAttr = xmlDoc.CreateAttribute("time");
                timeAttr.Value = item.CreationTime.ToString();
                postEle.Attributes.Append(timeAttr);

                string fileName = workingFolder + "post" + counter;
                item.Save(fileName);

                XmlAttribute contentAttr = xmlDoc.CreateAttribute("content");
                contentAttr.Value = fileName;
                postEle.Attributes.Append(contentAttr);

                rootEle.AppendChild(postEle);
                counter++;
            }
        }

        public void LoadAllPosts(XmlElement docEle)
        {  
            XmlNodeList postsEle = docEle.ChildNodes;
            int posts = postsEle.Count;

            for (int i = 0; i < posts; i++)
            {
                XmlElement postEle = postsEle[i] as XmlElement;
                if (postEle != null)
                {
                    double top = double.Parse(postEle.GetAttribute("top"));
                    double left = double.Parse(postEle.GetAttribute("left"));

                    double width = double.Parse(postEle.GetAttribute("width"));
                    double height = double.Parse(postEle.GetAttribute("height"));

                    Color backgroundColor = Colors.LightYellow;
                    if (postEle.HasAttribute("color"))
                    {
                        string colorAttr = postEle.GetAttribute("color");
                        backgroundColor = (Color)ColorConverter.ConvertFromString(colorAttr);

                    }

                    DateTime time = DateTime.Parse( postEle.GetAttribute("time"));

                    string postFile = postEle.GetAttribute("content");

                    Point location = new Point(left, top);

                    CreateTextPostAt(location, width, height, time, postFile, backgroundColor);

                }
            }
        }

        private XmlAttribute CreateAttribute(XmlDocument xmlDoc, double attrValue, string attrName)
        {
            XmlAttribute attr = xmlDoc.CreateAttribute(attrName);
            attr.Value = attrValue.ToString();
            return attr;
        }

        public void DeselectAll()
        {
            foreach (DesignerItem item in this.SelectedItems)
            {
                item.IsSelected = false;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            //CreateTextPostAt(e.GetPosition(this));
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Key input = e.Key;
            if (input == Key.Delete)
            {
                System.Diagnostics.Debug.WriteLine("delete pressed");
            }
        }
        

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (e.Source == this)
                    {
                        this.dragStartPoint = new Point?(e.GetPosition(this));
                        this.DeselectAll();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private string SelectImageFromDisk()
        {
            string selectedImageFile = string.Empty;
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                        "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                        "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                selectedImageFile = op.FileName;
            }
            return selectedImageFile;
        }

        public void CreateTextPostAt(Point position)
        {
            DesignerItem newItem = null;

            UserControl content = new PostEditor();
            content.Margin = new Thickness(2, 10, 2, 2);
                                    
            if (content != null)
            {
                newItem = new DesignerItem();
                newItem.Content = content;
                ((PostEditor)content).SetCreationTime(newItem.CreationTime);

                if (content.MinHeight != 0 && content.MinWidth != 0)
                {
                    newItem.Width = content.MinWidth * 2; ;
                    newItem.Height = content.MinHeight * 2;
                }
                else
                {
                    newItem.Width = 250;
                    newItem.Height = 285;
                }
                DesignerCanvas.SetLeft(newItem, position.X); // Math.Max(0, position.X - newItem.Width / 2));
                DesignerCanvas.SetTop(newItem, position.Y); // Math.Max(0, position.Y - newItem.Height / 2));
                this.Children.Add(newItem);

                this.DeselectAll();
                newItem.IsSelected = true;

                newItem.SaveMethod = ((PostEditor)content).SavePost;
            }

        }

        public void CreateTextPostAt(Point position, Color postColor)
        {
            DesignerItem newItem = null;

            UserControl content = new PostEditor();
            content.Margin = new Thickness(2, 10, 2, 2);

            if (content != null)
            {
                newItem = new DesignerItem();
                newItem.Content = content;
                ((PostEditor)content).SetCreationTime(newItem.CreationTime);
                ((PostEditor)content).BackgroundColor = postColor;

                if (content.MinHeight != 0 && content.MinWidth != 0)
                {
                    newItem.Width = content.MinWidth * 2; ;
                    newItem.Height = content.MinHeight * 2;
                }
                else
                {
                    newItem.Width = 250;
                    newItem.Height = 285;
                }
                DesignerCanvas.SetLeft(newItem, position.X); // Math.Max(0, position.X - newItem.Width / 2));
                DesignerCanvas.SetTop(newItem, position.Y); // Math.Max(0, position.Y - newItem.Height / 2));
                this.Children.Add(newItem);

                this.DeselectAll();
                newItem.IsSelected = true;

                newItem.SaveMethod = ((PostEditor)content).SavePost;
            }

        }

        private void CreateTextPostAt(Point position, double width, double height, DateTime creationTime, string postFile, Color background)
        {
            DesignerItem newItem = null;

            UserControl content = new PostEditor();
            
            content.Margin = new Thickness(2, 10, 2, 2);

            if (content != null)
            {
                newItem = new DesignerItem();
                newItem.CreationTime = creationTime;

                newItem.Content = content;
                ((PostEditor)content).BackgroundColor = background;
                ((PostEditor)content).SetCreationTime(newItem.CreationTime);
                ((PostEditor)content).LoadPost(postFile);
                
                newItem.Width = width;
                newItem.Height = height;

                DesignerCanvas.SetLeft(newItem, position.X);
                DesignerCanvas.SetTop(newItem, position.Y);
                this.Children.Add(newItem);

                this.DeselectAll();
                newItem.IsSelected = true;

                newItem.SaveMethod = ((PostEditor)content).SavePost;
            }

        }
        

        private void CreatePostAt(Point position)
        {
            DesignerItem newItem = null;
            //FrameworkElement content = new RichTextBox();
            string imageFile = SelectImageFromDisk();
            if (imageFile == string.Empty)
                return; 

            Image content = new Image();
            BitmapImage bmImage = new BitmapImage();
            bmImage.BeginInit();
            bmImage.UriSource = new Uri(imageFile, UriKind.Absolute);
            bmImage.EndInit();
            content.Source = bmImage;

            //content.IsHitTestVisible = false;

            if (content != null)
            {
                newItem = new DesignerItem();
                newItem.Content = content;

                if (content.MinHeight != 0 && content.MinWidth != 0)
                {
                    newItem.Width = content.MinWidth * 2; ;
                    newItem.Height = content.MinHeight * 2;
                }
                else
                {
                    newItem.Width = 65;
                    newItem.Height = 65;
                }
                DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                this.Children.Add(newItem);

                this.DeselectAll();
                newItem.IsSelected = true;

            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragStartPoint = null;
            }

            if (this.dragStartPoint.HasValue)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, this.dragStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }

                e.Handled = true;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            string xamlString = e.Data.GetData("DESIGNER_ITEM") as string;
            if (!String.IsNullOrEmpty(xamlString))
            {
                DesignerItem newItem = null;
                FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(xamlString))) as FrameworkElement;

                if (content != null)
                {
                    newItem = new DesignerItem();
                    newItem.Content = content;

                    Point position = e.GetPosition(this);
                    if (content.MinHeight != 0 && content.MinWidth != 0)
                    {
                        newItem.Width = content.MinWidth * 2; ;
                        newItem.Height = content.MinHeight * 2;
                    }
                    else
                    {
                        newItem.Width = 65;
                        newItem.Height = 65;
                    }
                    DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                    DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    this.Children.Add(newItem);

                    this.DeselectAll();
                    newItem.IsSelected = true;
                }

                e.Handled = true;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            foreach (UIElement element in Children)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }

            // add some extra margin
            size.Width += 10;
            size.Height += 10;
            return size;
        }
    }
}
