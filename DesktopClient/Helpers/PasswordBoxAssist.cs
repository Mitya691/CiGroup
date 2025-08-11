﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DesktopClient.Helpers
{
    public static class PasswordBoxAssist
    {
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",typeof(bool), typeof(PasswordBoxAssist), new PropertyMetadata(false, OnAttach));
        public static void SetAttach(DependencyObject o, bool value) => o.SetValue(AttachProperty, value);
        public static bool GetAttach(DependencyObject o) => (bool)o.GetValue(AttachProperty);


        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.RegisterAttached(
                "HasText",
                typeof(bool),
                typeof(PasswordBoxAssist),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static bool GetHasText(DependencyObject o) => (bool)o.GetValue(HasTextProperty);
        public static void SetHasText(DependencyObject o, bool value) => o.SetValue(HasTextProperty, value);


        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword", 
                typeof(string), 
                typeof(PasswordBoxAssist), 
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnBoundPasswordChanged));
        public static bool GetIsUpdating(DependencyObject o) => (bool)o.GetValue(BoundPasswordProperty);
        public static void SetIsUpdating(DependencyObject o, bool value) => o.SetValue(BoundPasswordProperty, value);


        private static void OnAttach(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb)
            {
                if ((bool)e.NewValue)
                {
                    pb.PasswordChanged -= Pb_PasswordChanged;
                    pb.PasswordChanged += Pb_PasswordChanged;

                    // первичная инициализация
                    SetHasText(pb, pb.SecurePassword.Length > 0);
                }
                else
                {
                    pb.PasswordChanged -= Pb_PasswordChanged;
                }
            }
        }

        private static void Pb_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            SetHasText(pb, pb.SecurePassword.Length > 0);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var pb = d as PasswordBox;
            if (pb == null) return;

            // если пришёл апдейт из VM — кладём его в PasswordBox
            if (!GetIsUpdating(pb))
            {
                SetIsUpdating(pb, true);
                pb.Password = e.NewValue?.ToString() ?? string.Empty;
                SetHasText(pb, pb.SecurePassword.Length > 0);
                SetIsUpdating(pb, false);
            }
        }
    }
}

