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

        public void DeselectAll()
        {
            foreach (DesignerItem item in this.SelectedItems)
            {
                item.IsSelected = false;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            switch (e.ChangedButton)
            {
                case MouseButton.Right:
                    //CreatePostAt(e.GetPosition(this));
                    CreateTextPostAt(e.GetPosition(this));
                    e.Handled = true;
                    break;
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

        private void CreateTextPostAt(Point position)
        {
            DesignerItem newItem = null;

            RichTextBox content = new RichTextBox();
            content.Margin = new Thickness(2, 10, 2, 2);
                                    
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
                    //RubberbandAdorner adorner = new RubberbandAdorner(this, this.dragStartPoint);
                   //if (adorner != null)
                    //{
                    //    adornerLayer.Add(adorner);
                    //}
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
