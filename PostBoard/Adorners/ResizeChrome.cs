﻿using System.Windows;
using System.Windows.Controls;

namespace PostBoard
{
    public class ResizeChrome : Control
    {
        static ResizeChrome()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeChrome), new FrameworkPropertyMetadata(typeof(ResizeChrome)));

            
        }
    }
}
