using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace Behaviors
{
    /// <summary>
    /// Attach this behavior to ScatterViewItems to change their default behavior
    /// of bringing an activated item to the top. Using this behavior, you can for
    /// example achieve that non-finger touches from the Surface device to not
    /// bring items to the top.
    /// 
    /// Author: Dominik Schmidt (www.dominikschmidt.net)
    /// </summary>
    public class CustomTopmostBehavior
    {
        #region attached behavior

        /// <summary>
        /// To enable this behavior, set to true.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
           DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
               typeof(CustomTopmostBehavior), new FrameworkPropertyMetadata(false, OnBehaviorEnabled));

        public static bool GetIsEnabled(DependencyObject d)
        {
            return (bool)d.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// Enables or disables behavior for provided ScatterViewItem.
        /// </summary>
        /// <param name="d">Target object, must be a ScatterViewItem</param>
        /// <param name="value">True to enable behavior</param>
        public static void SetIsEnabled(DependencyObject d, bool value)
        {
            d.SetValue(IsEnabledProperty, value);
        } 

        #endregion

        #region static properties

        /// <summary>
        /// Condition that determines if a preview TouchDown brings
        /// an ScatterViewItem to the top.
        /// </summary>
        public static Predicate<TouchDevice> TestCondition { get; set; }

        static CustomTopmostBehavior()
        {
            // intialize default behavior: only bring to top for finger touches
            TestCondition = (t) =>
            {
                return t.GetIsFingerRecognized();
            };
        }
        
        #endregion

        #region helpers

        /// <summary>
        /// Activate this behavior for all ScatterViewItems
        /// </summary>
        public static void Activate()
        {
            // sets style for all ScatterViewItems
            Style sviStyle = new Style(typeof(ScatterViewItem));           
            sviStyle.Setters.Add(new Setter(CustomTopmostBehavior.IsEnabledProperty, true));
            Application.Current.Resources.Add(typeof(ScatterViewItem), sviStyle);
        }

        #endregion

        #region private methods

        private static void OnBehaviorEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScatterViewItem item = d as ScatterViewItem;
            if (item == null)
                throw new InvalidOperationException("CustomTopmostBehavior can only be attached to ScatterViewItem");

            if (e.NewValue is bool && (bool)e.NewValue)
            {
                // disable automatic topmost behavior
                item.IsTopmostOnActivation = false;

                // enable custom topmost behavior
                item.AddHandler(ScatterViewItem.PreviewTouchDownEvent, new RoutedEventHandler((_s, _e) =>
                {
                    TouchEventArgs t = _e as TouchEventArgs;
                    if (TestCondition(t.TouchDevice))
                    {
                        item.SetRelativeZIndex(RelativeScatterViewZIndex.Topmost);
                    }
                }), true);
            }
        }

        #endregion        
    }
}
