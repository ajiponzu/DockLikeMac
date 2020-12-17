using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace DoclikeMac
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public int AnimationId;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //画面解像度(横)
        private double screenWidth;

        //画面解像度(縦)
        private double screenHeight;

        //調整高さ幅
        private const int pad = 1;

        //ウィンドウy座標最小値
        private double minY;

        //ウィンドウの表示・非表示アニメーションのためのメンバ
        private DispatcherTimer timer = null;

        //ウィンドウ表示・非表示スピード
        private readonly int spf = 16;

        private float delta = -4f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            /* 画面中央下に配置 */
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight - pad;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight;
            minY = screenHeight - Height + pad;
        }

        /// <summary>
        /// 引用コード
        /// ぼかし効果の適用
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EnableBlur(this);
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        internal static void EnableBlur(Window win)
        {
            var windowHelper = new WindowInteropHelper(win);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.AccentFlags = 2;
            accent.GradientColor = 0x66FFFFFF;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        /// <summary>
        /// updateスレッド生成
        /// </summary>
        /// <returns>t: DispatcherTimer</returns>
        private DispatcherTimer CreateTimer()
        {
            var t = new DispatcherTimer(DispatcherPriority.SystemIdle)
            {
                Interval = TimeSpan.FromMilliseconds(spf),
            };

            t.Tick += (sender, e) =>
            {
                Top -= delta;
                //所定の位置まで動かして止める
                if (Top < minY && delta > 0)
                {
                    Top = minY;
                    t.Stop();
                }
                else if (Top > screenHeight && delta < 0)
                {
                    Top = screenHeight;
                    t.Stop();
                }
            };

            return t;
        }

        /// <summary>
        /// dpiが変更された時のウィンドウ再配置メソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight - pad;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight;
            minY = screenHeight - Height + pad;
        }

        /// <summary>
        /// ウィンドウを動かす処理
        /// </summary>
        private void AnimationWindow()
        {
            delta *= -1;
            if (timer != null) timer.Stop();
            timer = CreateTimer();
            timer.Start();
            GC.Collect();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationWindow();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimationWindow();
        }
    }
}