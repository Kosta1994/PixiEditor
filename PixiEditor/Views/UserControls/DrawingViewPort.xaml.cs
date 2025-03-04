﻿using PixiEditor.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for DrawingViewPort.xaml.
    /// </summary>
    public partial class DrawingViewPort : UserControl
    {
        public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
            DependencyProperty.Register(nameof(MiddleMouseClickedCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(nameof(MouseUpCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty StylusButtonDownCommandProperty =
            DependencyProperty.Register(nameof(StylusButtonDownCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty StylusGestureCommandProperty =
            DependencyProperty.Register(nameof(StylusGestureCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty StylusButtonUpCommandProperty =
            DependencyProperty.Register(nameof(StylusButtonUpCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty StylusOutOfRangeCommandProperty =
            DependencyProperty.Register(nameof(StylusOutOfRangeCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseXOnCanvasProperty =
            DependencyProperty.Register(nameof(MouseXOnCanvas), typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MouseYOnCanvasProperty =
            DependencyProperty.Register(nameof(MouseYOnCanvas), typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public static readonly DependencyProperty GridLinesVisibleProperty =
            DependencyProperty.Register(nameof(GridLinesVisible), typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register(nameof(IsUsingZoomTool), typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false, ToolChanged));

        public static readonly DependencyProperty IsUsingMoveViewportToolProperty =
            DependencyProperty.Register(nameof(IsUsingMoveViewportTool), typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false, ToolChanged));

        public static readonly DependencyProperty CenterViewportTriggerProperty =
            DependencyProperty.Register(nameof(CenterViewportTrigger), typeof(ExecutionTrigger<Size>), typeof(DrawingViewPort),
                new PropertyMetadata(default(ExecutionTrigger<Size>), CenterViewportTriggerChanged));

        public static readonly DependencyProperty ZoomViewportTriggerProperty =
            DependencyProperty.Register(nameof(ZoomViewportTrigger), typeof(ExecutionTrigger<double>), typeof(DrawingViewPort),
                new PropertyMetadata(default(ExecutionTrigger<double>), ZoomViewportTriggerChanged));

        public static readonly DependencyProperty UseTouchGesturesProperty =
            DependencyProperty.Register(nameof(UseTouchGestures), typeof(bool), typeof(DrawingViewPort));

        public ICommand MiddleMouseClickedCommand
        {
            get => (ICommand)GetValue(MiddleMouseClickedCommandProperty);
            set => SetValue(MiddleMouseClickedCommandProperty, value);
        }

        public ICommand MouseMoveCommand
        {
            get => (ICommand)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public ICommand MouseDownCommand
        {
            get => (ICommand)GetValue(MouseDownCommandProperty);
            set => SetValue(MouseDownCommandProperty, value);
        }

        public ICommand MouseUpCommand
        {
            get => (ICommand)GetValue(MouseUpCommandProperty);
            set => SetValue(MouseUpCommandProperty, value);
        }

        public ICommand StylusButtonDownCommand
        {
            get => (ICommand)GetValue(StylusButtonDownCommandProperty);
            set => SetValue(StylusButtonDownCommandProperty, value);
        }

        public ICommand StylusButtonUpCommand
        {
            get => (ICommand)GetValue(StylusButtonUpCommandProperty);
            set => SetValue(StylusButtonUpCommandProperty, value);
        }

        public ICommand StylusGestureCommand
        {
            get => (ICommand)GetValue(StylusGestureCommandProperty);
            set => SetValue(StylusGestureCommandProperty, value);
        }

        public ICommand StylusOutOfRangeCommand
        {
            get => (ICommand)GetValue(StylusOutOfRangeCommandProperty);
            set => SetValue(StylusOutOfRangeCommandProperty, value);
        }

        public double MouseXOnCanvas
        {
            get => (double)GetValue(MouseXOnCanvasProperty);
            set => SetValue(MouseXOnCanvasProperty, value);
        }

        public double MouseYOnCanvas
        {
            get => (double)GetValue(MouseYOnCanvasProperty);
            set => SetValue(MouseYOnCanvasProperty, value);
        }

        public bool GridLinesVisible
        {
            get => (bool)GetValue(GridLinesVisibleProperty);
            set => SetValue(GridLinesVisibleProperty, value);
        }

        public bool IsUsingZoomTool
        {
            get => (bool)GetValue(IsUsingZoomToolProperty);
            set => SetValue(IsUsingZoomToolProperty, value);
        }

        public bool IsUsingMoveViewportTool
        {
            get => (bool)GetValue(IsUsingMoveViewportToolProperty);
            set => SetValue(IsUsingMoveViewportToolProperty, value);
        }
        public bool UseTouchGestures
        {
            get => (bool)GetValue(UseTouchGesturesProperty);
            set => SetValue(UseTouchGesturesProperty, value);
        }

        public ExecutionTrigger<Size> CenterViewportTrigger
        {
            get => (ExecutionTrigger<Size>)GetValue(CenterViewportTriggerProperty);
            set => SetValue(CenterViewportTriggerProperty, value);
        }

        public ExecutionTrigger<double> ZoomViewportTrigger
        {
            get => (ExecutionTrigger<double>)GetValue(ZoomViewportTriggerProperty);
            set => SetValue(ZoomViewportTriggerProperty, value);
        }

        public RelayCommand PreviewMouseDownCommand { get; private set; }
        private static void ToolChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var panel = (DrawingViewPort)sender;
            if (panel.IsUsingZoomTool)
                panel.zoombox.ZoomMode = Zoombox.Mode.ZoomTool;
            else if (panel.IsUsingMoveViewportTool)
                panel.zoombox.ZoomMode = Zoombox.Mode.MoveTool;
            else
                panel.zoombox.ZoomMode = Zoombox.Mode.Normal;
        }

        private static void CenterViewportTriggerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var viewport = (DrawingViewPort)sender;
            if (args.OldValue != null)
                ((ExecutionTrigger<Size>)args.OldValue).Triggered -= viewport.CenterZoomboxContent;
            ((ExecutionTrigger<Size>)args.NewValue).Triggered += viewport.CenterZoomboxContent;
        }

        private static void ZoomViewportTriggerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var viewport = (DrawingViewPort)sender;
            if (args.OldValue != null)
                ((ExecutionTrigger<double>)args.OldValue).Triggered -= viewport.ZoomZoomboxContent;
            ((ExecutionTrigger<double>)args.NewValue).Triggered += viewport.ZoomZoomboxContent;
        }

        private bool loaded = false;

        public DrawingViewPort()
        {
            PreviewMouseDownCommand = new RelayCommand(ProcessMouseDown);
            InitializeComponent();
        }

        private void CenterZoomboxContent(object sender, Size args)
        {
            zoombox.CenterContent(args);
        }
        private void ZoomZoomboxContent(object sender, double args)
        {
            zoombox.ZoomIntoCenter(args);
        }

        private void ProcessMouseDown(object parameter)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed && MiddleMouseClickedCommand.CanExecute(null))
                MiddleMouseClickedCommand.Execute(null);
        }

        private void OnCanvasLoaded(object sender, EventArgs e)
        {
            if (loaded)
                return;
            zoombox.CenterContent();
            loaded = true;
        }
    }
}
