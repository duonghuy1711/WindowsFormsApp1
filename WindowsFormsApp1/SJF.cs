using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BTL_LTW;
namespace Thuat_toan_SJF
{
    public partial class SJF : Form
    {
        private List<Process> processes = new List<Process>();
        private List<Tuple<string, int, int>> ganttDataToPaint;
        private int gap = 50; // Khoảng trống giữa các thanh biểu đồ
        public SJF()
        {
            InitializeComponent();
            InitializeDataGridView();

            // Đăng ký sự kiện Paint và Resize cho panel1
            panel1.Paint += panel1_Paint;
            panel1.Resize += panel1_Resize;
        }

        private void InitializeDataGridView()
        {
            // Thiết lập cột cho DataGridView1
            dataGridView1.Columns.Add("Name", "Tên");
            dataGridView1.Columns.Add("ArrivalTime", "Thời gian đến");
            dataGridView1.Columns.Add("BurstTime", "Thời gian thực hiện");

            // Thiết lập cột cho DataGridView2
            dataGridView2.Columns.Add("", "Tên");
            dataGridView2.Columns.Add("", "Thời gian bắt đầu");
            dataGridView2.Columns.Add("", "Thời gian chờ");
            dataGridView2.Columns.Add("", "Thời gian hoàn thành");
            dataGridView2.Columns.Add("", "Thời gian lưu lại");
        }
        private void button1_Click(object sender, EventArgs e)
                {
                    if (!string.IsNullOrEmpty(txtArrivalTime.Text) && !string.IsNullOrEmpty(txtBurstTime.Text))
                    {
                        int arrivalTime = Convert.ToInt32(txtArrivalTime.Text);
                        int burstTime = Convert.ToInt32(txtBurstTime.Text);

                        Process process = new Process
                        {
                            Name = $"P{processes.Count + 1}",
                            ArrivalTime = arrivalTime,
                            BurstTime = burstTime,
                            BurstTimeRemaining = burstTime
                        };

                        processes.Add(process);

                        dataGridView1.Rows.Add(process.Name, process.ArrivalTime, process.BurstTime);
                        txtArrivalTime.Text = "";
                        txtBurstTime.Text = "";
                        txtArrivalTime.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Vui lòng nhập thời gian đến và thời gian thực hiện.");
                    }
                }

        private void button2_Click(object sender, EventArgs e)
        {
            if (processes.Count > 0)
            {
                int totalWaitTime = 0;
                int currentTime = 0;

                List<Process> remainingProcesses = new List<Process>(processes);

                ganttDataToPaint = new List<Tuple<string, int, int>>();

                while (remainingProcesses.Count > 0)
                {
                    var eligibleProcesses = remainingProcesses
                        .Where(p => p.ArrivalTime <= currentTime)
                        .OrderBy(p => p.BurstTimeRemaining);

                    if (eligibleProcesses.Any())
                    {
                        var shortestJob = eligibleProcesses.First();
                        remainingProcesses.Remove(shortestJob);

                        int waitTime = currentTime - shortestJob.ArrivalTime;
                        totalWaitTime += waitTime;

                        dataGridView2.Rows.Add(shortestJob.Name, currentTime, waitTime, currentTime + shortestJob.BurstTime, currentTime + shortestJob.BurstTime - shortestJob.ArrivalTime);

                        // Add data to Gantt chart
                        ganttDataToPaint.Add(new Tuple<string, int, int>(shortestJob.Name, currentTime, currentTime + shortestJob.BurstTime));

                        currentTime += shortestJob.BurstTime;
                    }
                    else
                    {
                        currentTime++;
                    }
                }

                double averageWaitTime = (double)totalWaitTime / processes.Count;
                txttb.Text = Convert.ToString(averageWaitTime);

                // Sort DataGridView by process name
                dataGridView2.Sort(dataGridView2.Columns[0], System.ComponentModel.ListSortDirection.Ascending);

                // Vẽ biểu đồ Gantt
                panel1.Invalidate(); // Kích hoạt sự kiện Paint của panel1

                panel1.Visible = true;
            }
        }

        

        private void PaintGanttChart(Graphics g)
        {
            if (ganttDataToPaint != null && ganttDataToPaint.Count > 0)
            {
                int barHeight = 30;
                int panelWidth = panel1.Width;
                int labelY = 40;

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
                        g.DrawString($"{ganttData.Item1}:{processEnd}", Font, Brushes.Black, new PointF(processStart * panelWidth / ganttDataToPaint.Last().Item3 + processDuration / 2, labelY));

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
            // Vẽ lại biểu đồ Gantt khi panel1 thay đổi kích thước
            panel1.Invalidate();
        }

        public class Process
        {
            public string Name { get; set; }
            public int ArrivalTime { get; set; }
            public int BurstTime { get; set; }
            public int BurstTimeRemaining { get; set; }
        }

        private void SJF_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Xóa dữ liệu và làm sạch giao diện
            processes.Clear();
            ganttDataToPaint?.Clear(); // Nếu ganttDataToPaint không null, xóa dữ liệu
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            txttb.Text = string.Empty;
            panel1.Visible = false;

            // Vẽ lại biểu đồ Gantt khi panel1 thay đổi kích thước
            panel1.Invalidate();
        }
    }
}
