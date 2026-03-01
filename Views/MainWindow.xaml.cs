using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using PotentialOverlay.Models;
using PotentialOverlay.Services;

namespace PotentialOverlay.Views
{
    public partial class MainWindow : Window
    {
        private OverlayWindow _overlay = new();
        private DispatcherTimer _timer = new();
        private OcrService _ocr = new();
        private PotentialData _data = new();
        
        // 대상 게임 창 이름
        private const string GameTitle = "StellaSora";

        // [비율 데이터] 1920x1080 기준
        // [Index] -> {TextX, TextY, TextW, TextH, MaskX, MaskY, MaskW, MaskH}
        private readonly double[,] _ratios = new double[,]
        {
            { 0.0859, 0.5046, 0.1770, 0.0555,   0.0599, 0.1852, 0.2291, 0.5370 }, // 좌측
            { 0.4115, 0.5046, 0.1770, 0.0555,   0.3854, 0.2315, 0.2291, 0.5370 }, // 중앙
            { 0.7370, 0.5046, 0.1770, 0.0555,   0.7109, 0.2315, 0.2291, 0.5370 }  // 우측
        };

        public MainWindow()
        {
            InitializeComponent();
            StatusText.Text = "ready";
            LoadJsonData();
            _overlay.Show();

            // 0.5초마다 게임 창 위치 추적
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += UpdateOverlayPosition;
            _timer.Start();

            if (!_ocr.IsAvailable) StatusText.Text = "오류: 한국어 OCR 팩을 설치해주세요.";
        }

        private void LoadJsonData()
        {
            try
            {
                if (File.Exists("potentials.json"))
                {
                    string json = File.ReadAllText("potentials.json");
                    _data = JsonConvert.DeserializeObject<PotentialData>(json) ?? new PotentialData();
                    StatusText.Text = "데이터 로드 완료";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"데이터 오류: {ex.Message}";
            }
        }

        private void UpdateOverlayPosition(object? sender, EventArgs e)
        {
            var rect = NativeMethods.GetGameRect(GameTitle);

            if (rect.Width <= 0)
            {
                _overlay.Visibility = Visibility.Hidden;
                return;
            }

            // 오버레이가 켜져있을 때만 위치 동기화
            if (OverlayToggle.IsChecked == true)
            {
                _overlay.Visibility = Visibility.Visible;
                _overlay.Left = rect.Left;
                _overlay.Top = rect.Top;
                _overlay.Width = rect.Width;
                _overlay.Height = rect.Height;
            }
        }

        private async void ManualScan_Click(object sender, RoutedEventArgs e)
        {
            var rect = NativeMethods.GetGameRect(GameTitle);
            if (rect.Width <= 0)
            {
                StatusText.Text = "게임을 찾을 수 없습니다.";
                return;
            }

            StatusText.Text = "분석 중...";
            _overlay.ClearAll();

            for (int i = 0; i < 3; i++)
            {
                // 1. OCR 영역 계산 (비율 * 현재창크기)
                int tx = rect.Left + (int)(rect.Width * _ratios[i, 0]);
                int ty = rect.Top + (int)(rect.Height * _ratios[i, 1]);
                int tw = (int)(rect.Width * _ratios[i, 2]);
                int th = (int)(rect.Height * _ratios[i, 3]);

                // 2. 텍스트 인식
                string text = await _ocr.RecognizeAsync(new System.Drawing.Rectangle(tx, ty, tw, th));
                
                // 3. JSON 데이터 매칭
                string resultType = "Ignore";
                
                // 모든 캐릭터 목록에서 검색 (이름이 포함되면 매칭)
                foreach (var list in _data.Values)
                {
                    var match = list.FirstOrDefault(p => text.Contains(p.Name.Replace(" ", "")));
                    if (match != null)
                    {
                        resultType = match.Type;
                        break;
                    }
                }

                // 4. 오버레이 그리기
                if (resultType != "Ignore")
                {
                    double mx = rect.Width * _ratios[i, 4];
                    double my = rect.Height * _ratios[i, 5];
                    double mw = rect.Width * _ratios[i, 6];
                    double mh = rect.Height * _ratios[i, 7];
                    
                    _overlay.UpdateCardVisual(i, resultType, mx, my, mw, mh);
                }
            }
            StatusText.Text = "분석 완료";
        }

        private void ReloadData_Click(object sender, RoutedEventArgs e) => LoadJsonData();

        private void OverlayToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_overlay != null)
                _overlay.Visibility = (OverlayToggle.IsChecked == true) ? Visibility.Visible : Visibility.Hidden;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _overlay.Close();
            _timer.Stop();
        }
    }
}