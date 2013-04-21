// (c) Copyright 2011 - Morten Nielsen.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = System.Windows.Point;

namespace ISSLocator.Controls
{
    /// <summary>
    /// Augmented Reality Panel Control
    /// </summary>
    public sealed class ARPanel : Panel
    {
        private const double PI_OVER_180 = 0.01745329252;

        const PageOrientation LandscapeLeft = PageOrientation.Landscape;
        const PageOrientation LandscapeRight = PageOrientation.LandscapeRight;
        const PageOrientation Portrait = PageOrientation.Portrait;

        private static bool IsDesignMode = System.ComponentModel.DesignerProperties.IsInDesignTool;

        /// <summary>The frame this control is hosted in - used to detect page orientation changes.</summary>
        private PhoneApplicationFrame _frame;
        /// <summary>The motion sensor used.</summary>
        public Microsoft.Devices.Sensors.Motion motion;

        /// <summary>Topocentric coordinate system (x=north, z=up, y=west, center=device).</summary>
        private Matrix world;
        /// <summary>The view matrix places the camera at 0,0,0, pointing in the
        /// specified direction. This view changes with the page orientation 
        /// to compensate for screen coordinate system changing relative to 
        /// motion sensor coordinate system.</summary>
        private Matrix view;

        /// <summary>
        /// Initializes a new instance of the <see cref="ARPanel">Augmented Reality Panel</see> control.
        /// </summary>
        public ARPanel()
        {
            SizeChanged += panel_SizeChanged;
            Loaded += panel_Loaded;
            Unloaded += panel_Unloaded;
            view = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
            world = Matrix.CreateWorld(Vector3.Zero, new Vector3(0, 0, -1), new Vector3(0, 1, 0));

            if (IsDesignMode)
                _attitude = Matrix.Identity; //This will make control render design time looking horizontal north
        }

        private void panel_Loaded(object sender, RoutedEventArgs e)
        {
            _frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (_frame != null)
            {
                _frame.OrientationChanged += _frame_OrientationChanged;
                UpdateViewMatrix(_frame.Orientation);
            }
        }

        private void panel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_frame != null)
            {
                _frame.OrientationChanged -= _frame_OrientationChanged;
                _frame = null;
            }
        }

        private void panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _viewport = null; //If the size changes, it invalidates the viewport
            _cameraProjection = null; //camera projection relies on viewport - force reset
            this.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
            InvalidateArrange();
        }

        //TODO: check this for bug.
        private void _frame_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            lock (this)
            {
                //If the frame changes page orientation, update the view matrix
                UpdateViewMatrix(e.Orientation);
            }
        }

        private void UpdateViewMatrix(PageOrientation orientation)
        {
            if (orientation == LandscapeLeft)
            {
                view = Microsoft.Xna.Framework.Matrix.CreateLookAt(
                    new Microsoft.Xna.Framework.Vector3(0, 0, 1),
                    Microsoft.Xna.Framework.Vector3.Zero,
                    Microsoft.Xna.Framework.Vector3.Right);
            }
            else if (orientation == LandscapeRight)
            {
                view = Microsoft.Xna.Framework.Matrix.CreateLookAt(
                    new Microsoft.Xna.Framework.Vector3(0, 0, 1),
                    Microsoft.Xna.Framework.Vector3.Zero,
                    Microsoft.Xna.Framework.Vector3.Left);	
            }
            else //if (orientation == PageOrientation.PortraitUp)
            {
                view = Microsoft.Xna.Framework.Matrix.CreateLookAt(
                    new Microsoft.Xna.Framework.Vector3(0, 0, 1),
                    Microsoft.Xna.Framework.Vector3.Zero,
                    Microsoft.Xna.Framework.Vector3.Up);	
            }
            //Camera projection relies on the view matrix - invalidate camera projection
            _cameraProjection = null;
            InvalidateArrange();
        }

        /// <summary>
        /// Converts a screen point to a direction.
        /// </summary>
        /// <param name="screen">The screen coordinate relative to upper left of the <see cref="ARPanel"/>.</param>
        /// <returns></returns>
        public Point ScreenToDirection(Point screen)
        {
            double x = 0;
            double y = 0;
            Vector3 nearSource = new Vector3((float)screen.X, (float)screen.Y, 0.0f);
            Vector3 farSource = new Vector3((float)screen.X, (float)screen.Y, 1.0f);
            var nearPoint = Unproject(nearSource);
            var farPoint = Unproject(farSource);
            Vector3 direction = farPoint - nearPoint;
            var l = direction.Length();
            y = Math.Asin(direction.Y / l) / PI_OVER_180;
            x = Math.Atan2(direction.Z, direction.X) / PI_OVER_180 + 90;
            if (x > 180)
                x -= 360;
            return new Point(y, x);
        }
		
        /// <summary>
        /// Converts polar coordinates into a direction/vector
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector3 PolarToVector(double px, double py, double radius)
        {
            var O = (py - 90) * PI_OVER_180; // / 180d * Math.PI;
            var W = (90 - px) * PI_OVER_180; // / 180d * Math.PI;
            var x = (float)((Math.Cos(O) * Math.Sin(W)) * radius);
            var y = (float)((Math.Cos(W)) * radius);
            var z = (float)((Math.Sin(O) * Math.Sin(W)) * radius);
            return new Vector3(x, y, z);
        }
	
        /// <summary>
        /// Starts reading data from the motion sensor.
        /// This method throws an exception if <see cref="Microsoft.Devices.Sensors.Motion.IsSupported"/>
        /// is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if motion sensor is not supported</exception>
        public void Start()
        {
            if (!IsRunning)
            {
                if (Microsoft.Devices.Sensors.Motion.IsSupported)
                {
                    motion = new Microsoft.Devices.Sensors.Motion();
                    motion.CurrentValueChanged += motion_CurrentValueChanged;
                    motion.Start();
                    IsRunning = true;
                }
                else
                {
                    throw new InvalidOperationException("Motion sensor not supported on this device");
                }
            }
        }

        /// <summary>
        /// Stops reading data from the motion sensor.
        /// </summary>
        public void Stop()
        {
            if (motion != null)
            {
                motion.CurrentValueChanged -= motion_CurrentValueChanged;
                motion.Stop();

                motion = null;
                IsRunning = false;
            }
        }

        private void motion_CurrentValueChanged(object sender, Microsoft.Devices.Sensors.SensorReadingEventArgs<Microsoft.Devices.Sensors.MotionReading> e)
        {
            lock (_attitudeLock)
            {
                //When device changes orientation, the attitude value is invalidated
                _attitude = null;
            }
            //When the device changes orientation, it invalidates the placement of the elements
            Dispatcher.BeginInvoke(() => InvalidateArrange());
        }

        private Matrix? _attitude;
        private object _attitudeLock = new object();

        private Matrix Attitude
        {
            get
            {
                Matrix att;
                lock (_attitudeLock)
                {
                    if (!_attitude.HasValue)
                    {
                        if (motion != null &&
                            motion.IsDataValid
                        )
                        {
                            // Get the RotationMatrix from the MotionReading.
                            // Rotate it 90 degrees around the X axis to put it in the XNA Framework coordinate system.
                            _attitude = att = Matrix.CreateRotationX(MathHelper.PiOver2) * CurrentReading;
                        }
                        else
                            return Matrix.Identity;
                    }
                    else
                        att = _attitude.Value;
                }
                return att;
            }
        }

        private Matrix CurrentReading
        {
            get
            {
                return
                motion.CurrentValue.Attitude.RotationMatrix;
            }
        }

        private Matrix? _cameraProjection;

        /// <summary>
        /// Projection matrix defines perspective distortion and near/far plane.
        /// </summary>
        private Matrix CameraProjection
        {
            get
            {
                if (!_cameraProjection.HasValue)
                {
                    var fov = FieldOfView;
                    if (_frame != null && _frame.Orientation == Portrait)
                    {
                        fov = fov / Viewport.AspectRatio;
                    }
                    _cameraProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians((float)fov), Viewport.AspectRatio, 1f, 12f);
                }
                return _cameraProjection.Value;
            }
        }

        private Microsoft.Xna.Framework.Graphics.Viewport? _viewport;

        private Microsoft.Xna.Framework.Graphics.Viewport Viewport
        {
            get
            {
                if (!_viewport.HasValue)
                {
                    _viewport = new Microsoft.Xna.Framework.Graphics.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
                    _cameraProjection = null; //camera projection depends on viewport - force a reset
                }
                return _viewport.Value;
            }
        }

        private Vector3 Project(Vector3 vector)
        {
            // Projects the point from 3D space into screen coordinates.
            return Viewport.Project(vector, CameraProjection, view, world * Attitude);
        }

        private Vector3 Unproject(Vector3 vector)
        {
            //Projects the vector from screen coordinates into 3D space
            return Viewport.Unproject(vector, CameraProjection, view, world * Attitude);
        }

        /// <summary>
        /// Provides the behavior for the Arrange pass of Silverlight layout.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this
        /// object should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size that is used after the element is arranged in layout.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (ActualWidth > 0 && ActualHeight > 0 && (motion != null && motion.IsDataValid || IsDesignMode))
            {
                this.FirsVectorInView = null;

                BoundingFrustum viewFrustum = new BoundingFrustum(Attitude * view * CameraProjection);
                foreach (var child in Children)
                {
                    object posObj = child.GetValue(DirectionProperty);
                    if (posObj is Point && !double.IsNaN(((Point)posObj).X))
                    {
                        Point p = (Point)posObj;
                        Vector3 direction = PolarToVector(p.X, p.Y, 10);
                        var size = child.DesiredSize;
                        //Create a bounding sphere around the element for hittesting against the current frustum
                        //This size is not entirely right... size we have is screen size but we use the world size. 
                        //*.008 seems to roughly fit as conversion factor for now
                        var box = new BoundingSphere(direction, (float)Math.Max(size.Width, size.Height) * .008f);
                        if (viewFrustum.Contains(box) != ContainmentType.Disjoint) //partially or fully inside camera frustum
                        {
                            Vector3 projected = Project(direction);

                            if (this.FirsVectorInView == null)
                            {
                                this.FirsVectorInView = projected;
                            }

                            if (!float.IsNaN(projected.X) && !float.IsNaN(projected.Y))
                            {
                                //Arrange element centered on projected coordinate
                                double x = projected.X - size.Width * .5;
                                double y = projected.Y - size.Height * .5;
                                child.Arrange(new Rect(x, y, size.Width, size.Height));
                                continue;
                            }
                        }
                    }
                    //if we fall through to here, it's because the element is outside the view,
                    //or placement can't be calculated
                    child.Arrange(new Rect(0, 0, 0, 0));
                }
                return finalSize;
            }
            else
                return base.ArrangeOverride(finalSize);
        }

        public bool IsInView(Point directionPoint)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(Attitude * view * CameraProjection);
            object posObj = directionPoint;
            if (posObj is Point && !double.IsNaN(((Point)posObj).X))
            {
                Point p = (Point)posObj;
                Vector3 direction = PolarToVector(p.X, p.Y, 10);
                //Create a bounding sphere around the element for hittesting against the current frustum
                //This size is not entirely right... size we have is screen size but we use the world size. 
                //*.008 seems to roughly fit as conversion factor for now
                var box = new BoundingSphere(direction, 10 * .008f);
                if (viewFrustum.Contains(box) != ContainmentType.Disjoint) //partially or fully inside camera frustum
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Provides the behavior for the Measure pass of Silverlight layout. 
        /// </summary>
        /// <param name="availableSize">The available size that this object can
        /// give to child objects. Infinity (<see cref="F:System.Double.PositiveInfinity"/>) 
        /// can be specified as a value to indicate that the object will size to 
        /// whatever content is available.</param>
        /// <returns>
        /// The size that this object determines it needs during layout, based
        /// on its calculations of the allocated sizes for child objects; or 
        /// based on other considerations, such as a fixed container size.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var child in Children)
                child.Measure(availableSize);
            return availableSize;
        }

        /// <summary>
        /// Gets or sets the field of view of the viewport (if you have the camera as a background,
        /// this should match the FOV of the lens).
        /// </summary>
        /// <value>The field of view.</value>
        public double FieldOfView
        {
            get
            {
                return (double)GetValue(FieldOfViewProperty);
            }
            set
            {
                SetValue(FieldOfViewProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="FieldOfView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FieldOfViewProperty = DependencyProperty.Register("FieldOfView", typeof(double), typeof(ARPanel), new PropertyMetadata(35d, OnFieldOfViewChanged));

        private static void OnFieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ARPanel panel = (ARPanel)d;
            panel._cameraProjection = null;
            panel.InvalidateArrange();
        }

        /// <summary>
        /// Gets the value of the Direction property from the specified System.Windows.FrameworkElement.
        /// </summary>
        /// <param name="obj">The element from which to read the property value.</param>
        /// <returns>The value of the Direction property.</returns>
        public static Point GetDirection(DependencyObject obj)
        {
            return (Point)obj.GetValue(DirectionProperty);
        }

        /// <summary>
        /// Sets the value of the Direction property
        ///  to the specified System.Windows.FrameworkElement.
        /// </summary>
        /// <param name="obj">The element on which to set the Direction property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetDirection(DependencyObject obj, Point value)
        {
            obj.SetValue(DirectionProperty, value);
        }

        /// <summary>
        /// Identifies the Direction dependency property.
        /// </summary>
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.RegisterAttached("Direction", typeof(System.Windows.Point), typeof(ARPanel), new PropertyMetadata(new Point(), OnDirectionPropertyChanged));
      
        private static void OnDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement)
            {
                (d as UIElement).InvalidateArrange();
            }
        }
	
        /// <summary>
        /// Gets value indicating whether the motion sensor is active.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this sensor is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; private set; }

        public Vector3? FirsVectorInView { get; private set; }
    }
}