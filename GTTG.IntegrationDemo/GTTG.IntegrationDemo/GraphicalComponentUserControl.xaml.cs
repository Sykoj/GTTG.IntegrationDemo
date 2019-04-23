using System;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

using GTTG.Core.Component;
using GTTG.Core.Time;
using GTTG.IntegrationDemo.MouseInput;

namespace GTTG.IntegrationDemo {

    /// <summary>
    /// Interaction logic for GraphicalComponentUserControl.xaml
    /// </summary>
    public partial class GraphicalComponentUserControl : SKElement, IMouseInputTarget {

        private const float ContentHalfTime = 8;
        private static readonly DateTimeInterval ContentDateTimeInterval
            = new DateTimeInterval(DateTime.Now.AddHours(-ContentHalfTime), DateTime.Now.AddHours(ContentHalfTime));
        private static readonly DateTimeInterval ViewDateTimeInterval 
            = new DateTimeInterval(DateTime.Now.AddHours(-ContentHalfTime/2), DateTime.Now.AddHours(ContentHalfTime/2));
        private static readonly DateTimeContext DateTimeContext 
            = new DateTimeContext(ContentDateTimeInterval, ViewDateTimeInterval);

        private GraphicalComponent _graphicalComponent;
        private DragProcessor _dragProcessor;
        private SKPaint _timePaint;

        public GraphicalComponentUserControl() {
            InitializeComponent();
            Loaded += (_,__) => OnLoaded();
            SizeChanged += (_,__) => OnSizeChanged();
        }

        private void OnLoaded() {

            _graphicalComponent = new GraphicalComponent();
            _graphicalComponent.TryChangeDateTimeContext(DateTimeContext);
            _dragProcessor = new DragProcessor();
            _timePaint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill, IsAntialias = true, TextSize = 40 };

            var source = new WpfMouseInputSource(this);
            source.LeftUp.Subscribe(LeftUp);
            source.LeftDown.Subscribe(LeftDown);
            source.Move.Subscribe(Move);
            source.Scroll.Subscribe(Scroll);
            source.Leave.Subscribe(Leave);

            OnSizeChanged();
            InvalidateVisual();
        }

        private void OnSizeChanged() {
            _graphicalComponent?.TryResizeView(CanvasSize.Width, CanvasSize.Height);
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {

            if (_graphicalComponent == null) return;
            e.Surface.Canvas.Clear();
            e.Surface.Canvas.SetMatrix(_graphicalComponent.GlobalMatrix);
            
            var contentStart = _graphicalComponent.GlobalDateTimeInterval.Start; 
            foreach (var dateTime in _graphicalComponent.GlobalDateTimeInterval.GetDateTimesByPeriod(contentStart, TimeSpan.FromHours(1))) {

                var x = _graphicalComponent.GetGlobalHorizontalPosition(dateTime);
                var y = _graphicalComponent.GlobalHeight / 2;

                e.Surface.Canvas.DrawText(dateTime.Hour.ToString(), new SKPoint(x, y), _timePaint);
            }
        }

        public void LeftUp(MouseInputArgs args) {
            _dragProcessor.TryExitDrag(args);
        }

        public void LeftDown(MouseInputArgs args) {
            _dragProcessor.TryInitializeDrag(args);
        }

        public void Move(MouseInputArgs args) {

            if (!_dragProcessor.IsEnabled) {
                return;
            }

            var translation = _dragProcessor.GetTranslation(args);
            var viewProvider = _graphicalComponent;

            /*
               prevents tearing while content height is equal to view height and translation vector is not 0 in Y,
               which would results in unmodified state (change set Y to 0), same for X and width in following condition
            */
            if (viewProvider.GlobalMatrix.ScaleY.Equals(1.0f) && viewProvider.GlobalHeight.Equals(viewProvider.ViewHeight)) {
                translation.Y = 0;
            }
            if (viewProvider.GlobalMatrix.ScaleX.Equals(1.0f) && viewProvider.GlobalWidth.Equals(viewProvider.ViewWidth)) {
                translation.X = 0;
            }

            var result = _graphicalComponent.TryTranslate(translation);

            if (result == TranslationTransformationResult.ViewModified) {
                InvalidateVisual();
            }
        }

        public void Scroll(MouseZoomArgs args) {

            var result = _graphicalComponent.TryScale(new SKPoint(args.X, args.Y), args.Delta);
            if (result != ScaleTransformationResult.ViewUnmodified) {
                InvalidateVisual();
            }
        }

        public void Leave(MouseInputArgs args) {
            _dragProcessor.TryExitDrag(args);
        }

        public void RightUp(MouseInputArgs args) {
            // Do nothing.
        }

        public void RightDown(MouseInputArgs args) {
            // Do nothing.
        }

        public void ScrollUp(MouseZoomArgs args) {
            // Do nothing.
        }

        public void ScrollDown(MouseZoomArgs args) {
            // Do nothing.
        }

        public void Enter(MouseInputArgs args) {
            // Do nothing.
        }
    }
}
