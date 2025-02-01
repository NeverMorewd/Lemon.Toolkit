using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System.Numerics;
using System;
using Avalonia.Controls.Shapes;
using Lemon.ModuleNavigation.Abstracts;

namespace Lemon.Toolkit.Views
{
    public partial class PlaygroundRenderView : UserControl, IView
    {
        private float rotationX = 0;
        private float rotationY = 0;
        private float zoomLevel = 1.0f;

        public PlaygroundRenderView()
        {
            InitializeComponent();

            Width = 800;
            Height = 600;

            //var canvas = new Canvas
            //{
            //    Background = Brushes.Black,
            //    Width = Width,
            //    Height = Height
            //};

            //Content = canvas;

            //PointerPressed += MainWindow_PointerPressed;
            //PointerMoved += MainWindow_PointerMoved;
            //PointerWheelChanged += MainWindow_PointerWheelChanged;

            //RenderTimer();
        }

        private Point lastPoint;
        private void MainWindow_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            lastPoint = e.GetPosition(this);
        }

        private void MainWindow_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed)
            {
                var currentPoint = e.GetPosition(this);
                var deltaX = (float)(currentPoint.X - lastPoint.X) * 0.5f;
                var deltaY = (float)(currentPoint.Y - lastPoint.Y) * 0.5f;

                rotationY += deltaX;
                rotationX += deltaY;

                lastPoint = currentPoint;
                Invalidate();
            }
        }

        private void MainWindow_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            zoomLevel += (float)e.Delta.Y * 0.1f;
            zoomLevel = Math.Clamp(zoomLevel, 0.5f, 3.0f);
            Invalidate();
        }

        private void RenderTimer()
        {
            var timer = new System.Timers.Timer(30);
            timer.Elapsed += (s, e) => Dispatcher.UIThread.Post(Invalidate);
            timer.Start();
        }

        private void Invalidate()
        {
            var canvas = Content as Canvas;
            if (canvas == null) return;

            canvas.Children.Clear();

            var cube = CreateCube();
            canvas.Children.Add(cube);
        }

        private Canvas CreateCube()
        {
            var cubeCanvas = new Canvas();
            var colors = new[]
            {
                Colors.Red, Colors.Green, Colors.Blue,
                Colors.Yellow, Colors.Purple, Colors.Cyan
            };

            var faces = new[]
            {
                new Vector3(0, 0, 1),    // 前面 
                new Vector3(0, 0, -1),   // 后面 
                new Vector3(1, 0, 0),    // 右面 
                new Vector3(-1, 0, 0),   // 左面 
                new Vector3(0, 1, 0),    // 顶面 
                new Vector3(0, -1, 0)    // 底面 
            };

            // Apply 3D transformations
            var rotationMatrix = Matrix4x4.CreateRotationX(rotationX * (float)Math.PI / 180) *
                                 Matrix4x4.CreateRotationY(rotationY * (float)Math.PI / 180);

            var transformedFaces = new Vector3[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                transformedFaces[i] = Vector3.Transform(faces[i], rotationMatrix);
            }

            // Sort faces by depth (Z-axis)
            var sortedFaces = new int[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                sortedFaces[i] = i;
            }
            Array.Sort(sortedFaces, (a, b) => transformedFaces[b].Z.CompareTo(transformedFaces[a].Z));

            // Draw faces in order of depth
            for (int i = 0; i < sortedFaces.Length; i++)
            {
                var face = CreateFace(transformedFaces[sortedFaces[i]], colors[sortedFaces[i]]);
                cubeCanvas.Children.Add(face);
            }

            // Apply zoom
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(zoomLevel, zoomLevel));

            cubeCanvas.RenderTransform = transformGroup;

            return cubeCanvas;
        }

        private Rectangle CreateFace(Vector3 normal, Color color)
        {
            var face = new Rectangle
            {
                Width = 200,
                Height = 200,
                Fill = new SolidColorBrush(color),
                Opacity = 0.8,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2
            };

            // Project 3D normal to 2D position
            double offset = 100;
            double x = normal.X * offset;
            double y = normal.Y * offset;

            // Position the face in the center of the canvas
            Canvas.SetLeft(face, Width / 2 + x - 100);
            Canvas.SetTop(face, Height / 2 + y - 100);

            return face;
        }
    }
}