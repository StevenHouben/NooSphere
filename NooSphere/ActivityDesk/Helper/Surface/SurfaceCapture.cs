using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Surface.Core;
using System.Windows;
using System.Windows.Interop;

namespace ActivityDesk.Helper.Surface
{
    public class SurfaceCapture
    {
        private TouchTarget _touchTarget;

        private byte[] _surfaceImage;

        private ImageMetrics _surfaceMetrics;

        private long _lastFrameTimestamp;

        private double _frameRate;

        // the higher the value the slower the frame rate adopts to changes
        private const double _frameRateSmoothingFactor = .9;        

        public double FrameRate
        {
            get
            {
                return _frameRate;
            }
        }

        public SurfaceCapture(Window main)
        {
            InitializeSurfaceCore(main);
        }

        private void InitializeSurfaceCore(Window main)
        {
            _touchTarget = new TouchTarget(new WindowInteropHelper(main).Handle, EventThreadChoice.OnBackgroundThread);
            _touchTarget.EnableInput();            
            _touchTarget.EnableImage(ImageType.Normalized);
            _touchTarget.FrameReceived += OnTouchTargetFrameReceived;
        }

        private void OnTouchTargetFrameReceived(object sender, FrameReceivedEventArgs e)
        {            
            // get image from Surface
            if (_surfaceImage == null)
            {
                int paddingLeft, paddingRight;
                e.TryGetRawImage(
                    ImageType.Normalized,
                    // get entire screen, not just working area (which excludes task bar)
                    InteractiveSurface.PrimarySurfaceDevice.Top,
                    InteractiveSurface.PrimarySurfaceDevice.Left,
                    InteractiveSurface.PrimarySurfaceDevice.Width,
                    InteractiveSurface.PrimarySurfaceDevice.Height,
                    out _surfaceImage,
                    out _surfaceMetrics,
                    out paddingLeft,
                    out paddingRight);
            }
            else
            {
                e.UpdateRawImage(
                    ImageType.Normalized,
                    _surfaceImage,
                    InteractiveSurface.PrimarySurfaceDevice.Top,
                    InteractiveSurface.PrimarySurfaceDevice.Left,
                    InteractiveSurface.PrimarySurfaceDevice.Width,
                    InteractiveSurface.PrimarySurfaceDevice.Height);
            }

            // create EmguCV image and fire event
            Image<Gray, byte> emguCvImage = CreateEmguCvImage(_surfaceImage, _surfaceMetrics);         
            OnImage(emguCvImage);

            UpdateFrameRate(e.FrameTimestamp);
        }

        // Surface image (byte array) to EmguCV image
        private Image<Gray, byte> CreateEmguCvImage(byte[] image, ImageMetrics metrics)
        {
            return new Image<Gray, byte>(metrics.Width, metrics.Height) { Bytes = image };
        }

        private void UpdateFrameRate(long frameTimestamp)
        {
            _frameRate = _frameRateSmoothingFactor * _frameRate +
                (1 - _frameRateSmoothingFactor) * (1 / TimeSpan.FromTicks(frameTimestamp - _lastFrameTimestamp).TotalSeconds);
            _lastFrameTimestamp = frameTimestamp;
        }
       
        public event EventHandler<SurfaceImageEventArgs> Image;

        private void OnImage(Image<Gray, byte> image)
        {
            if (Image != null)
            {
                Image(this, new SurfaceImageEventArgs(image));
            }
        }    
    }
}
