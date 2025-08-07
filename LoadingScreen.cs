using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysbotMacro
{
    public partial class LoadingScreen : Form
    {
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Timer glitchTimer;
        private System.Windows.Forms.Timer progressTimer;
        private int animationFrame = 0;
        private int glitchFrame = 0;
        private int progressValue = 0;
        private Random random = new Random();
        private bool glitchActive = false;
        private Color neonBlue = Color.FromArgb(0, 255, 255);
        private Color neonPink = Color.FromArgb(255, 20, 147);
        private Color neonGreen = Color.FromArgb(57, 255, 20);
        private Color switchRed = Color.FromArgb(230, 45, 75);
        private Color switchBlue = Color.FromArgb(0, 174, 239);
        
        public LoadingScreen()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.Black;
            this.ClientSize = new Size(800, 600);
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoadingScreen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Loading";
            this.TopMost = true;
            this.WindowState = FormWindowState.Normal;
            
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer | 
                         ControlStyles.ResizeRedraw, true);
            
            this.Paint += LoadingScreen_Paint;
            this.Load += LoadingScreen_Load;
            
            this.ResumeLayout(false);
        }
        
        private void LoadingScreen_Load(object sender, EventArgs e)
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
            
            glitchTimer = new System.Windows.Forms.Timer();
            glitchTimer.Interval = 200;
            glitchTimer.Tick += GlitchTimer_Tick;
            glitchTimer.Start();
            
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 100;
            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Start();
            
            Task.Run(async () =>
            {
                await Task.Delay(3500);
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => this.Close()));
                }
                else
                {
                    this.Close();
                }
            });
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationFrame++;
            if (animationFrame > 360) animationFrame = 0;
            this.Invalidate();
        }
        
        private void GlitchTimer_Tick(object sender, EventArgs e)
        {
            glitchFrame++;
            glitchActive = random.Next(0, 10) < 3;
            this.Invalidate();
        }
        
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (progressValue < 100)
            {
                progressValue += random.Next(1, 5);
                if (progressValue > 100) progressValue = 100;
            }
        }
        
        private void LoadingScreen_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            
            DrawBackground(g);
            DrawTitle(g);
            DrawProgressBar(g);
            DrawLoadingText(g);
            DrawNeonEffects(g);
            DrawSwitchLogo(g);
        }
        
        private void DrawBackground(Graphics g)
        {
            Rectangle rect = this.ClientRectangle;
            
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect, Color.FromArgb(10, 10, 30), Color.FromArgb(5, 5, 15), LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }
            
            for (int i = 0; i < 50; i++)
            {
                int x = (int)((animationFrame * 2 + i * 20) % (Width + 100));
                int y = i * 12;
                int alpha = (int)(50 * Math.Sin((animationFrame + i) * 0.1));
                if (alpha < 0) alpha = -alpha;
                
                using (Pen pen = new Pen(Color.FromArgb(alpha, neonBlue), 1))
                {
                    g.DrawLine(pen, x - 50, y, x, y);
                }
            }
            
            if (glitchActive)
            {
                using (SolidBrush glitchBrush = new SolidBrush(Color.FromArgb(20, neonPink)))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Rectangle glitchRect = new Rectangle(
                            random.Next(0, Width),
                            random.Next(0, Height),
                            random.Next(50, 200),
                            random.Next(2, 10)
                        );
                        g.FillRectangle(glitchBrush, glitchRect);
                    }
                }
            }
        }
        
        private void DrawTitle(Graphics g)
        {
            string title = "NINTENDO SWITCH";
            string subtitle = "SYSBOT REMOTE";
            
            using (Font titleFont = new Font("Consolas", 42, FontStyle.Bold))
            using (Font subtitleFont = new Font("Consolas", 28, FontStyle.Bold))
            {
                SizeF titleSize = g.MeasureString(title, titleFont);
                SizeF subtitleSize = g.MeasureString(subtitle, subtitleFont);
                
                float titleX = (Width - titleSize.Width) / 2;
                float titleY = Height / 2 - 100;
                float subtitleX = (Width - subtitleSize.Width) / 2;
                float subtitleY = titleY + titleSize.Height + 10;
                
                if (glitchActive && glitchFrame % 3 == 0)
                {
                    titleX += random.Next(-5, 6);
                    subtitleX += random.Next(-3, 4);
                }
                
                using (GraphicsPath titlePath = new GraphicsPath())
                using (GraphicsPath subtitlePath = new GraphicsPath())
                {
                    titlePath.AddString(title, titleFont.FontFamily, (int)titleFont.Style, titleFont.Size, new PointF(titleX, titleY), StringFormat.GenericDefault);
                    subtitlePath.AddString(subtitle, subtitleFont.FontFamily, (int)subtitleFont.Style, subtitleFont.Size, new PointF(subtitleX, subtitleY), StringFormat.GenericDefault);
                    
                    using (Pen glowPen = new Pen(switchRed, 8))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        glowPen.LineJoin = LineJoin.Round;
                        g.DrawPath(glowPen, titlePath);
                        g.FillPath(textBrush, titlePath);
                    }
                    
                    using (Pen glowPen = new Pen(switchBlue, 6))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        glowPen.LineJoin = LineJoin.Round;
                        g.DrawPath(glowPen, subtitlePath);
                        g.FillPath(textBrush, subtitlePath);
                    }
                }
            }
        }
        
        private void DrawProgressBar(Graphics g)
        {
            Rectangle progressRect = new Rectangle(Width / 2 - 200, Height - 150, 400, 20);
            Rectangle fillRect = new Rectangle(progressRect.X, progressRect.Y, 
                (int)(progressRect.Width * progressValue / 100.0), progressRect.Height);
            
            using (Pen borderPen = new Pen(neonBlue, 2))
            {
                g.DrawRectangle(borderPen, progressRect);
            }
            
            if (fillRect.Width > 0)
            {
                using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                    fillRect, neonGreen, neonBlue, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(fillBrush, fillRect);
                }
                
                using (Pen glowPen = new Pen(Color.FromArgb(100, neonGreen), 6))
                {
                    g.DrawRectangle(glowPen, fillRect);
                }
            }
            
            string progressText = $"{progressValue}%";
            using (Font font = new Font("Consolas", 12, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                SizeF textSize = g.MeasureString(progressText, font);
                g.DrawString(progressText, font, brush, 
                    progressRect.X + progressRect.Width / 2 - textSize.Width / 2,
                    progressRect.Y + progressRect.Height + 10);
            }
        }
        
        private void DrawLoadingText(Graphics g)
        {
            string[] loadingMessages = {
                "INITIALIZING SYSBOT REMOTE INTERFACE...",
                "LOADING SYSBOT REMOTE INTERFACE...",
                "READY FOR OPERATION"
            };
            
            int messageIndex = Math.Min((progressValue / 34), loadingMessages.Length - 1);
            string message = loadingMessages[messageIndex];
            
            if (glitchActive && random.Next(0, 10) < 3)
            {
                char[] chars = message.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    if (random.Next(0, 10) < 2)
                    {
                        chars[i] = (char)random.Next(33, 127);
                    }
                }
                message = new string(chars);
            }
            
            using (Font font = new Font("Consolas", 14, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(neonGreen))
            {
                SizeF textSize = g.MeasureString(message, font);
                float x = (Width - textSize.Width) / 2;
                float y = Height - 100;
                
                using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(50, neonGreen)))
                {
                    g.DrawString(message, font, glowBrush, x + 1, y + 1);
                    g.DrawString(message, font, glowBrush, x - 1, y - 1);
                }
                g.DrawString(message, font, brush, x, y);
            }
        }
        
        private void DrawNeonEffects(Graphics g)
        {
            float pulse = (float)(0.5 + 0.5 * Math.Sin(animationFrame * 0.1));
            
            using (Pen neonPen = new Pen(Color.FromArgb((int)(100 * pulse), neonPink), 3))
            {
                g.DrawRectangle(neonPen, 20, 20, Width - 40, Height - 40);
            }
            
            for (int i = 0; i < 8; i++)
            {
                float angle = (animationFrame + i * 45) * (float)Math.PI / 180;
                int centerX = Width / 2;
                int centerY = Height / 2 - 50;
                int radius = 150 + (int)(30 * Math.Sin(animationFrame * 0.05 + i));
                
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));
                
                using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb((int)(150 * pulse), neonBlue)))
                {
                    g.FillEllipse(dotBrush, x - 3, y - 3, 6, 6);
                }
            }
        }
        
        private void DrawSwitchLogo(Graphics g)
        {
            int logoSize = 60;
            int logoX = Width - logoSize - 30;
            int logoY = 30;
            
            using (SolidBrush redBrush = new SolidBrush(switchRed))
            using (SolidBrush blueBrush = new SolidBrush(switchBlue))
            using (SolidBrush grayBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            {
                Rectangle leftJoycon = new Rectangle(logoX, logoY, logoSize / 3, logoSize);
                Rectangle rightJoycon = new Rectangle(logoX + logoSize * 2 / 3, logoY, logoSize / 3, logoSize);
                Rectangle screen = new Rectangle(logoX + logoSize / 3, logoY + logoSize / 4, logoSize / 3, logoSize / 2);
                
                g.FillRectangle(blueBrush, leftJoycon);
                g.FillRectangle(redBrush, rightJoycon);
                g.FillRectangle(grayBrush, screen);
                
                using (Pen glowPen = new Pen(Color.FromArgb(100, Color.White), 2))
                {
                    g.DrawRectangle(glowPen, new Rectangle(logoX - 2, logoY - 2, logoSize + 4, logoSize + 4));
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Stop();
                animationTimer?.Dispose();
                glitchTimer?.Stop();
                glitchTimer?.Dispose();
                progressTimer?.Stop();
                progressTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}