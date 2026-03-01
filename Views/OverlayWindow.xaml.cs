using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shapes;
using PotentialOverlay.Services;

namespace PotentialOverlay.Views
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 오버레이 창을 클릭 통과 상태로 설정
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, extendedStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TOOLWINDOW);
        }

        public void UpdateCardVisual(int index, string type, double x, double y, double w, double h)
        {
            Rectangle mask = index switch { 0 => Mask0, 1 => Mask1, 2 => Mask2, _ => null! };
            TextBlock pick = index switch { 0 => Pick0, 1 => Pick1, 2 => Pick2, _ => null! };

            if (mask == null) return;

            // 마스크 위치 설정
            Canvas.SetLeft(mask, x); Canvas.SetTop(mask, y);
            mask.Width = w; mask.Height = h;

            // 픽 텍스트 위치 설정 (카드 중앙)
            Canvas.SetLeft(pick, x + (w - 80) / 2); 
            Canvas.SetTop(pick, y + (h - 30) / 2);

            // 상태에 따른 표시
            mask.Visibility = (type == "Discard") ? Visibility.Visible : Visibility.Collapsed;
            pick.Visibility = (type == "Pick") ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ClearAll()
        {
            Mask0.Visibility = Mask1.Visibility = Mask2.Visibility = Visibility.Collapsed;
            Pick0.Visibility = Pick1.Visibility = Pick2.Visibility = Visibility.Collapsed;
        }
    }
}