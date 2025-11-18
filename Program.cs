using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GraphApp
{
    public partial class Form1 : Form
    {
        private Button drawButton;
        private RadioButton lineGraphRadio;
        private RadioButton pointGraphRadio;
        private CheckBox doubleBufferCheckBox;
        private Label infoLabel;
        
        // Параметри функції
        private const double X_START = 2.3;
        private const double X_END = 7.8;
        private const double DELTA_X = 0.9;
        
        // Режими візуалізації
        private enum GraphMode { Line, Point }
        private GraphMode currentMode = GraphMode.Line;

        public Form1()
        {
            InitializeComponent();
            
            // Включаємо подвійну буферизацію за замовчуванням
            this.DoubleBuffered = true;
            
            // Підписуємося на події
            this.Resize += Form1_Resize;
            this.Paint += Form1_Paint;
        }

        private void InitializeComponent()
        {
            this.drawButton = new Button();
            this.lineGraphRadio = new RadioButton();
            this.pointGraphRadio = new RadioButton();
            this.doubleBufferCheckBox = new CheckBox();
            this.infoLabel = new Label();
            this.SuspendLayout();
            
            // 
            // drawButton
            // 
            this.drawButton.Location = new Point(12, 12);
            this.drawButton.Name = "drawButton";
            this.drawButton.Size = new Size(120, 30);
            this.drawButton.TabIndex = 0;
            this.drawButton.Text = "Намалювати";
            this.drawButton.UseVisualStyleBackColor = true;
            this.drawButton.Click += new EventHandler(this.drawButton_Click);
            
            // 
            // lineGraphRadio
            // 
            this.lineGraphRadio.AutoSize = true;
            this.lineGraphRadio.Checked = true;
            this.lineGraphRadio.Location = new Point(150, 12);
            this.lineGraphRadio.Name = "lineGraphRadio";
            this.lineGraphRadio.Size = new Size(130, 19);
            this.lineGraphRadio.TabIndex = 1;
            this.lineGraphRadio.TabStop = true;
            this.lineGraphRadio.Text = "Лінійний графік";
            this.lineGraphRadio.UseVisualStyleBackColor = true;
            this.lineGraphRadio.CheckedChanged += new EventHandler(this.GraphMode_Changed);
            
            // 
            // pointGraphRadio
            // 
            this.pointGraphRadio.AutoSize = true;
            this.pointGraphRadio.Location = new Point(150, 37);
            this.pointGraphRadio.Name = "pointGraphRadio";
            this.pointGraphRadio.Size = new Size(130, 19);
            this.pointGraphRadio.TabIndex = 2;
            this.pointGraphRadio.Text = "Точковий графік";
            this.pointGraphRadio.UseVisualStyleBackColor = true;
            this.pointGraphRadio.CheckedChanged += new EventHandler(this.GraphMode_Changed);
            
            // 
            // doubleBufferCheckBox
            // 
            this.doubleBufferCheckBox.AutoSize = true;
            this.doubleBufferCheckBox.Checked = true;
            this.doubleBufferCheckBox.CheckState = CheckState.Checked;
            this.doubleBufferCheckBox.Location = new Point(300, 12);
            this.doubleBufferCheckBox.Name = "doubleBufferCheckBox";
            this.doubleBufferCheckBox.Size = new Size(110, 19);
            this.doubleBufferCheckBox.TabIndex = 3;
            this.doubleBufferCheckBox.Text = "DoubleBuffer";
            this.doubleBufferCheckBox.UseVisualStyleBackColor = true;
            this.doubleBufferCheckBox.CheckedChanged += new EventHandler(this.DoubleBuffer_Changed);
            
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new Point(300, 37);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new Size(200, 15);
            this.infoLabel.TabIndex = 4;
            this.infoLabel.Text = "Режим: Лінійний";
            
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 600);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.doubleBufferCheckBox);
            this.Controls.Add(this.pointGraphRadio);
            this.Controls.Add(this.lineGraphRadio);
            this.Controls.Add(this.drawButton);
            this.Name = "Form1";
            this.Text = "Графік функції y = (6x + 4) / (sin(3x) - x)";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Обчислення Y для заданого X
        private double CalculateY(double x)
        {
            double denominator = Math.Sin(3 * x) - x;
            return (6 * x + 4) / denominator;
        }

        // Малювання графіка
        private void DrawGraph(Graphics g)
        {
            // Очищаємо форму
            g.Clear(Color.White);

            // Визначаємо область для малювання
            int leftMargin = 80;
            int topMargin = 60;
            int rightMargin = 40;
            int bottomMargin = 80;
            
            Rectangle drawingArea = new Rectangle(
                leftMargin, 
                topMargin, 
                this.ClientSize.Width - leftMargin - rightMargin, 
                this.ClientSize.Height - topMargin - bottomMargin
            );

            if (drawingArea.Width <= 0 || drawingArea.Height <= 0)
                return;

            // Обчислюємо всі точки та знаходимо мін/макс значення Y
            List<PointF> points = new List<PointF>();
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            for (double x = X_START; x <= X_END + DELTA_X / 2; x += DELTA_X)
            {
                try
                {
                    double y = CalculateY(x);
                    
                    if (!double.IsInfinity(y) && !double.IsNaN(y))
                    {
                        points.Add(new PointF((float)x, (float)y));
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
                catch { }
            }

            if (points.Count < 2)
            {
                g.DrawString($"Знайдено точок: {points.Count}. Потрібно мінімум 2.", 
                    this.Font, Brushes.Red, 10, 100);
                return;
            }

            // Додаємо відступи для Y
            double rangeY = maxY - minY;
            minY -= rangeY * 0.1;
            maxY += rangeY * 0.1;

            // Створюємо пензлі
            Pen axisPen = new Pen(Color.Black, 2);
            // Створюємо пензлі для сітки з пунктиром і світлішим кольором
            Pen gridPen = new Pen(Color.FromArgb(60, 180, 180, 180), 1); // Дуже світло-сірий, прозорий
            gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            // Включаємо антиаліасинг для плавності
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen graphPen = new Pen(Color.SlateBlue, 3);
            Font font = new Font("Arial", 9);

            // Малюємо рамку
            g.DrawRectangle(axisPen, drawingArea);

            // Малюємо сітку (клітинки)
            int gridXCount = 10; // Кількість клітинок по X
            int gridYCount = 10; // Кількість клітинок по Y
            float gridWidth = (float)drawingArea.Width / gridXCount;
            float gridHeight = (float)drawingArea.Height / gridYCount;
            for (int i = 1; i < gridXCount; i++)
            {
                float x = drawingArea.Left + i * gridWidth;
                g.DrawLine(gridPen, x, drawingArea.Top, x, drawingArea.Bottom);
            }
            for (int j = 1; j < gridYCount; j++)
            {
                float y = drawingArea.Top + j * gridHeight;
                g.DrawLine(gridPen, drawingArea.Left, y, drawingArea.Right, y);
            }

            // Функція для перетворення координат
            float ScaleX(double x)
            {
                return drawingArea.Left + (float)((x - X_START) / (X_END - X_START) * drawingArea.Width);
            }

            float ScaleY(double y)
            {
                return drawingArea.Bottom - (float)((y - minY) / (maxY - minY) * drawingArea.Height);
            }

            // Малюємо осі координат, якщо 0 в діапазоні
            if (minY <= 0 && maxY >= 0)
            {
                float y0 = ScaleY(0);
                g.DrawLine(new Pen(Color.Black, 1), drawingArea.Left, y0, drawingArea.Right, y0);
            }

            // Малюємо поділки та значення на осі X
            Pen tickPen = new Pen(Color.Black, 1);
            for (double x = X_START; x <= X_END; x += DELTA_X)
            {
                float xPos = ScaleX(x);
                // Вертикальна поділка
                g.DrawLine(tickPen, xPos, drawingArea.Bottom, xPos, drawingArea.Bottom + 5);
                // Значення
                string xValue = x.ToString("F1");
                SizeF textSize = g.MeasureString(xValue, font);
                g.DrawString(xValue, font, Brushes.Black, xPos - textSize.Width / 2, drawingArea.Bottom + 7);
            }

            // Малюємо поділки та значення на осі Y (5-6 поділок)
            int numYTicks = 6;
            double yStep = (maxY - minY) / (numYTicks - 1);
            for (int i = 0; i < numYTicks; i++)
            {
                double y = minY + i * yStep;
                float yPos = ScaleY(y);
                // Горизонтальна поділка
                g.DrawLine(tickPen, drawingArea.Left - 5, yPos, drawingArea.Left, yPos);
                // Значення
                string yValue = y.ToString("F2");
                SizeF textSize = g.MeasureString(yValue, font);
                g.DrawString(yValue, font, Brushes.Black, drawingArea.Left - textSize.Width - 7, yPos - textSize.Height / 2);
            }

            // Малюємо сітку (опціонально)
            Pen gridPenLight = new Pen(Color.LightGray, 1);
            gridPenLight.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            
            // Вертикальні лінії сітки
            for (double x = X_START; x <= X_END; x += DELTA_X)
            {
                float xPos = ScaleX(x);
                g.DrawLine(gridPenLight, xPos, drawingArea.Top, xPos, drawingArea.Bottom);
            }
            
            // Горизонтальні лінії сітки
            for (int i = 0; i < numYTicks; i++)
            {
                double y = minY + i * yStep;
                float yPos = ScaleY(y);
                g.DrawLine(gridPenLight, drawingArea.Left, yPos, drawingArea.Right, yPos);
            }

            // Малюємо графік згідно з обраним режимом
            if (currentMode == GraphMode.Line)
            {
                // Лінійний режим - з'єднуємо точки
                for (int i = 0; i < points.Count - 1; i++)
                {
                    float x1 = ScaleX(points[i].X);
                    float y1 = ScaleY(points[i].Y);
                    float x2 = ScaleX(points[i + 1].X);
                    float y2 = ScaleY(points[i + 1].Y);

                    g.DrawLine(graphPen, x1, y1, x2, y2);
                }
                
                // Малюємо точки поверх ліній
                foreach (var point in points)
                {
                    float x = ScaleX(point.X);
                    float y = ScaleY(point.Y);
                    g.FillEllipse(Brushes.Red, x - 3, y - 3, 6, 6);
                }
            }
            else // GraphMode.Point
            {
                // Точковий режим - тільки маркери без ліній
                foreach (var point in points)
                {
                    float x = ScaleX(point.X);
                    float y = ScaleY(point.Y);
                    // Трохи більші маркери для точкового режиму
                    g.FillEllipse(Brushes.Blue, x - 4, y - 4, 8, 8);
                    // Додаємо обводку для кращої видимості
                    g.DrawEllipse(new Pen(Color.DarkBlue, 1), x - 4, y - 4, 8, 8);
                }
            }

            // Малюємо підписи
            string xLabel = $"X: [{X_START:F1}, {X_END:F1}], ΔX = {DELTA_X}";
            string yLabel = $"Y: [{minY:F2}, {maxY:F2}]";
            
            g.DrawString(xLabel, font, Brushes.Black, drawingArea.Left, drawingArea.Bottom + 5);
            g.DrawString(yLabel, font, Brushes.Black, 5, drawingArea.Top);
            
            string formula = "y = (6x + 4) / (sin(3x) - x)";
            SizeF formulaSize = g.MeasureString(formula, font);
            g.DrawString(formula, font, Brushes.Black, 
                drawingArea.Right - formulaSize.Width, 
                drawingArea.Bottom + 5);

            // Інформація про кількість точок та режим
            string modeText = currentMode == GraphMode.Line ? "Лінійний" : "Точковий";
            string bufferText = this.DoubleBuffered ? "з DoubleBuffer" : "без DoubleBuffer";
            g.DrawString($"Точок: {points.Count} | Режим: {modeText} | {bufferText}", 
                font, Brushes.Blue, drawingArea.Left, topMargin - 20);

            // Очищаємо ресурси
            axisPen.Dispose();
            gridPen.Dispose();
            graphPen.Dispose();
            font.Dispose();
        }

        // Обробник зміни режиму графіка
        private void GraphMode_Changed(object sender, EventArgs e)
        {
            if (lineGraphRadio.Checked)
            {
                currentMode = GraphMode.Line;
                infoLabel.Text = "Режим: Лінійний (з з'єднаними відрізками)";
            }
            else if (pointGraphRadio.Checked)
            {
                currentMode = GraphMode.Point;
                infoLabel.Text = "Режим: Точковий (тільки маркери)";
            }
            this.Invalidate();
        }
        
        // Обробник зміни DoubleBuffer
        private void DoubleBuffer_Changed(object sender, EventArgs e)
        {
            this.DoubleBuffered = doubleBufferCheckBox.Checked;
            string bufferStatus = this.DoubleBuffered ? "увімкнений" : "вимкнений";
            MessageBox.Show($"DoubleBuffer {bufferStatus}. Змініть розмір вікна, щоб побачити різницю.", 
                "Зміна буферизації", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Invalidate();
        }

        // Обробник натискання кнопки
        private void drawButton_Click(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        // Обробник події малювання форми
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawGraph(e.Graphics);
        }

        // Обробник зміни розміру вікна
        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
}
