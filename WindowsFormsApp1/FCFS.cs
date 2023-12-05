using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RRR
{
    public partial class SRTN : Form
    {
        private List<Tuple<string, int, int>> ganttDataToPaint;
        private Dictionary<string, Color> processColors;

        public SRTN()
        {
            InitializeComponent();

            panel1.Paint += panel1_Paint;
            panel1.Resize += panel1_Resize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtArrivalTime.Text != string.Empty && txtBurstTime.Text != string.Empty)
            {
                int count = dataGridView1.RowCount;

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                row.Cells[0].Value = "P" + int.Parse(count.ToString());
                row.Cells[1].Value = txtArrivalTime.Text;
                row.Cells[2].Value = txtBurstTime.Text;
                dataGridView1.Rows.Add(row);

                txtArrivalTime.Text = string.Empty;
                txtBurstTime.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Hãy nhập đầy đủ thông tin", "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtArrivalTime.Text = string.Empty;
                txtBurstTime.Text = string.Empty;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            copy(dataGridView1, dataGridView2);
            sapxep(dataGridView2);
            calculateTurnaroundTimes(dataGridView2);

            label5.Text = AvgWaitingTime(dataGridView2).ToString();

            panel1.Invalidate();
        }

        public void copy(DataGridView view1, DataGridView view2)
        {
            int count = dataGridView1.RowCount - 1;
            for (int i = 0; i < count; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView2);
                row.Cells[0].Value = dataGridView1.Rows[i].Cells[0].Value;
                row.Cells[1].Value = dataGridView1.Rows[i].Cells[1].Value;
                row.Cells[2].Value = dataGridView1.Rows[i].Cells[2].Value;
                dataGridView2.Rows.Add(row);
            }
        }

        public void sapxep(DataGridView view2)
        {
            int count = dataGridView2.RowCount - 1;
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString()) > int.Parse(dataGridView2.Rows[j].Cells[1].Value.ToString()))
                    {
                        string name = dataGridView2.Rows[i].Cells[0].Value.ToString();
                        int xh = int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                        int sd = int.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());

                        dataGridView2.Rows[i].Cells[0].Value = dataGridView2.Rows[j].Cells[0].Value.ToString();
                        dataGridView2.Rows[i].Cells[1].Value = dataGridView2.Rows[j].Cells[1].Value.ToString();
                        dataGridView2.Rows[i].Cells[2].Value = dataGridView2.Rows[j].Cells[2].Value.ToString();

                        dataGridView2.Rows[j].Cells[0].Value = name.ToString();
                        dataGridView2.Rows[j].Cells[1].Value = xh.ToString();
                        dataGridView2.Rows[j].Cells[2].Value = sd.ToString();
                    }
                }
            }
        }

        public float AvgWaitingTime(DataGridView view2)
        {
            int count = view2.RowCount - 1;
            float tong = 0;
            for (int i = 0; i < count; i++)
            {
                tong += int.Parse(view2.Rows[i].Cells[5].Value.ToString());
            }

            return tong / count;
        }

        public void calculateTurnaroundTimes(DataGridView view2)
        {
            dataGridView2.Rows[0].Cells[3].Value = int.Parse(dataGridView2.Rows[0].Cells[1].Value.ToString()) + int.Parse(dataGridView2.Rows[0].Cells[2].Value.ToString());
            dataGridView2.Rows[0].Cells[4].Value = int.Parse(dataGridView2.Rows[0].Cells[3].Value.ToString()) - int.Parse(dataGridView2.Rows[0].Cells[1].Value.ToString());
            dataGridView2.Rows[0].Cells[5].Value = int.Parse(dataGridView2.Rows[0].Cells[4].Value.ToString()) - int.Parse(dataGridView2.Rows[0].Cells[2].Value.ToString());

            for (int i = 1; i < dataGridView2.Rows.Count - 1; i++)
            {
                dataGridView2.Rows[i].Cells[3].Value = int.Parse(dataGridView2.Rows[i - 1].Cells[3].Value.ToString()) + int.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());
                dataGridView2.Rows[i].Cells[4].Value = int.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString()) - int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                dataGridView2.Rows[i].Cells[5].Value = int.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString()) - int.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());
            }

            ganttDataToPaint = new List<Tuple<string, int, int>>();
            processColors = new Dictionary<string, Color>();

            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                if (dataGridView2.Rows[i].Cells[0].Value != null)
                {
                    string processName = dataGridView2.Rows[i].Cells[0].Value.ToString();
                    int processStart = int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                    int processEnd = int.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());
                    ganttDataToPaint.Add(Tuple.Create(processName, processStart, processEnd));

                    Color processColor = GetNextColor(i);
                    processColors[processName] = processColor;
                }
            }
        }

        private void PaintGanttChart(Graphics g)
        {
            if (ganttDataToPaint != null && ganttDataToPaint.Count > 0)
            {
                int barHeight = 30;
                int panelWidth = panel1.Width;
                int labelY = 50;

                int colorIndex = 0;
                int lastProcessEnd = 0;

                foreach (var ganttData in ganttDataToPaint)
                {
                    var processRows = dataGridView2.Rows.Cast<DataGridViewRow>().Where(row => row.Cells[0].Value != null && row.Cells[0].Value.ToString() == ganttData.Item1);

                    if (processRows.Any())
                    {
                        int processStart = ganttData.Item2;
                        int processEnd = ganttData.Item3;
                        int processDuration = processEnd - processStart;

                        Color processColor = GetNextColor(colorIndex);
                        Pen pen = new Pen(processColor, barHeight);

                        // Vẽ đoạn thanh Gantt của tiến trình
                        g.DrawLine(pen, processStart * panelWidth / ganttDataToPaint.Last().Item3, 10, processEnd * panelWidth / ganttDataToPaint.Last().Item3, 10);

                        // Đặt nhãn cho tiến trình tại giữa đoạn thanh Gantt
                        float labelX = processStart * panelWidth / ganttDataToPaint.Last().Item3 + processDuration / 4;
                        float labelWidth = g.MeasureString($"{ganttData.Item1}:{processEnd}", Font).Width;
                        float labelXCentered = labelX - labelWidth / 5;

                        g.DrawString($"{ganttData.Item1}:{processEnd}", Font, Brushes.Black, new PointF(labelXCentered, labelY));

                        colorIndex = (colorIndex + 1) % ProcessColors.Length;

                        // Vẽ đoạn thanh Gantt cho khoảng thời gian trống giữa các tiến trình
                        if (processStart > lastProcessEnd)
                        {
                            int gapDuration = processStart - lastProcessEnd;
                            g.DrawLine(Pens.White, lastProcessEnd * panelWidth / ganttDataToPaint.Last().Item3, 10, processStart * panelWidth / ganttDataToPaint.Last().Item3, 10);
                        }

                        lastProcessEnd = processEnd; // Cập nhật thời điểm kết thúc cuối cùng
                    }
                }
            }
        }

        private static Color[] ProcessColors = { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Purple, Color.Orange };

        private Color GetNextColor(int index)
        {
            return ProcessColors[index % ProcessColors.Length];
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            PaintGanttChart(e.Graphics);
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (ganttDataToPaint != null)
            {
                ganttDataToPaint.Clear(); // Nếu ganttDataToPaint không null, xóa dữ liệu
            }

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            label5.Text = string.Empty;
            panel1.Visible = false;

            // Gọi lại hàm calculateTurnaroundTimes để cập nhật dữ liệu mới
            calculateTurnaroundTimes(dataGridView2);

            // Hiển thị lại panel và vẽ lại biểu đồ Gantt
            panel1.Invalidate();
        }
    }
}
