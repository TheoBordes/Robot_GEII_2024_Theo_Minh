using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Core.Extensions;
using SciChart.Data.Model;
using System;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WpfWorldMap_NS
{
    public partial class WpfWorldMap : UserControl
    {

        private RotateTransform _ghostrotation;
        private CustomAnnotation _ghostrobot;
        private RotateTransform _rotation;
        private CustomAnnotation _robot;
        private DispatcherTimer _timer;
        public double _angle;
        public double _angleghost;


        public double pos_X_robot = 50;
        public double pos_Y_robot = 50;
        public double pos_X_ghost = 50;
        public double pos_Y_ghost = 50;
        public double xDataValue =50;
        public double yDataValue=50;
        public bool Start = false;




        public WpfWorldMap()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            //robot
            var triangle = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, -10),
                    new Point(-7, 7),
                    new Point(7, 7)
                },
                Fill = Brushes.Red,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var rotation = new RotateTransform(0);
            triangle.RenderTransform = rotation;
            this._rotation = rotation;

            this._robot = new CustomAnnotation
            {
                //X1 = 20,
                //Y1 = 30,
                X1 = 10,
                Y1 = 15,
                Content = triangle,
                HorizontalAnchorPoint = HorizontalAnchorPoint.Center,
                VerticalAnchorPoint = VerticalAnchorPoint.Center
            };


            sciChartSurface.Annotations.Add(_robot);

            // robot ghost
            var triangle2 = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, -10),
                    new Point(-7, 7),
                    new Point(7, 7)
                },
                Fill = Brushes.Yellow,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var rotation2 = new RotateTransform(0);
            triangle2.RenderTransform = rotation2;
            this._ghostrotation = rotation2;

            this._ghostrobot = new CustomAnnotation
            {
                //X1 = 20,
                //Y1 = 30,
                X1 = 10,
                Y1 = 15,
                Content = triangle2,
                HorizontalAnchorPoint = HorizontalAnchorPoint.Center,
                VerticalAnchorPoint = VerticalAnchorPoint.Center
            };
            sciChartSurface.Annotations.Add(_ghostrobot);



            double w = sciChartSurface.XAxis.VisibleRange.Max.ToDouble();
            var mur1 = new CustomAnnotation
            {
                X1 = w / 2,
                Y1 = 0,
                Content = new Rectangle
                {
                    Width = 1000,
                    Height = 10,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0,
                },
                HorizontalAnchorPoint = HorizontalAnchorPoint.Center,
                VerticalAnchorPoint = VerticalAnchorPoint.Center
            };
            //sciChartSurface.Annotations.Add(mur1);

            var mur2 = new CustomAnnotation
            {
                X1 = w / 2,
                Y1 = w,
                Content = new Rectangle
                {
                    Width = 100,
                    Height = 10,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0,
                },
                HorizontalAnchorPoint = HorizontalAnchorPoint.Center,
                VerticalAnchorPoint = VerticalAnchorPoint.Center
            };
            //sciChartSurface.Annotations.Add(mur2);

            var mur3 = new CustomAnnotation
            {
                X1 = 0,
                Y1 = w / 2,
                Content = new Rectangle
                {
                    Width = 10,
                    Height = 100,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0,
                },
                HorizontalAnchorPoint = HorizontalAnchorPoint.Center,
                VerticalAnchorPoint = VerticalAnchorPoint.Center
            };
            //sciChartSurface.Annotations.Add(mur3);

            var mur4 = new CustomAnnotation
            {
                X1 = w,
                Y1 = w / 2,
                Content = new Rectangle
                {
                    Width = 10,
                    Height = 100,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0,
                },
                HorizontalAnchorPoint = HorizontalAnchorPoint.Center,
                VerticalAnchorPoint = VerticalAnchorPoint.Center
            };
            //sciChartSurface.Annotations.Add(mur4);




            //var contour = new BoxAnnotation
            //{
            //    X1 = 0,
            //    Y1 = 0,
            //    X2 = 10,  // Remplace par ta taille max en X
            //    Y2 = 10,  // Remplace par ta taille max en Y
            //    BorderThickness = new Thickness(3),
            //    BorderBrush = Brushes.Green,
            //    Background = Brushes.Transparent,
            //    CoordinateMode = AnnotationCoordinateMode.Relative, // ou Absolute selon usage
            //    IsEditable = false
            //};

            //sciChartSurface.Annotations.Add(contour);

            // Timer pour faire bouger le cercle
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _timer.Tick += MoveRobot;
            _timer.Start();


        }
        private void MoveRobot(object? sender, EventArgs e)
        {
            this._robot.X1 = this.pos_X_robot;
            this._robot.Y1 = this.pos_Y_robot;
            this._ghostrobot.X1 = this.pos_X_ghost;
            this._ghostrobot.Y1 = this.pos_Y_ghost;
        }

        public void UpdatePosRobot(double X, double Y)
        {
            this.pos_X_robot = X;
            this.pos_Y_robot = Y;
        }
        public void UpdatePosRobotGhost(double X, double Y)
        {
            this.pos_X_ghost = X;
            this.pos_Y_ghost = Y;
        }

        public void UpdateOrientationRobot(double angleDegres)
        {
            if (_rotation == null) return;
            _angle =  angleDegres;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _rotation.Angle = _angle;
            });
        }

        public void UpdateOrientationRobotGhost(double angleDegres)
        {
            if (_ghostrotation == null) return;
            _angleghost = angleDegres;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _ghostrotation.Angle = _angleghost;
            });
        }
       
        private void SciChartSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sciChart = sender as SciChartSurface;
            if (sciChart == null)
                return;

            Point mousePoint = e.GetPosition(sciChart);

            xDataValue = sciChart.XAxis.GetCurrentCoordinateCalculator().GetDataValue(mousePoint.X);
            yDataValue = sciChart.YAxis.GetCurrentCoordinateCalculator().GetDataValue(mousePoint.Y);
            Start = true;
        }


    }
}
