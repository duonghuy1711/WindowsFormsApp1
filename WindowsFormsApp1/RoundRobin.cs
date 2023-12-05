using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using BTL_LTW;
namespace RRR
{
    public partial class RoundRobin : Form
    {
        private List<Process> processes = new List<Process>();
        private List<Tuple<string, int, int>> ganttDataToPaint;
        public RoundRobin()
        {
            InitializeComponent();

            panel1.Paint += panel1_Paint;
            panel1.Resize += panel1_Resize;
        }

        // Calculate average waiting time
        private double CalculateAverageWaitingTime()
        {
            double totalWaitingTime = 0;

            foreach (var process in processes)
            {
                totalWaitingTime += process.WaitingTime;
            }

            return totalWaitingTime / processes.Count;
        }

        // Done, Add
        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtArrivalTime.Text) && !string.IsNullOrEmpty(txtBurstTime.Text))
            {
                int arrivalTime = Convert.ToInt32(txtArrivalTime.Text);
                int burstTime = Convert.ToInt32(txtBurstTime.Text);

                Process process = new Process
                {
                    Id = $"P{processes.Count + 1}",
                    ArrivalTime = arrivalTime,
                    BurstTime = burstTime,
                    RemainingTime = burstTime
                };

                processes.Add(process);

                dataGridView1.Rows.Add(process.Id, process.ArrivalTime, process.BurstTime);
                txtArrivalTime.Text = "";
                txtBurstTime.Text = "";
                txtArrivalTime.Focus();
            }
            else
            {
                MessageBox.Show("Vui lòng nhập thời gian đến và thời gian thực hiện.");
            }

        }

        // Done, Run
        private void button2_Click(object sender, EventArgs e)
        {
            int timeQuantum = int.Parse(quantumLabel.Text);

            processes = processes.OrderBy(p => p.ArrivalTime).ToList();

            List<Process> queue = new List<Process>(processes);
            List<Process> completedProcesses = new List<Process>();

            int currentTime = 0;

            ganttDataToPaint = new List<Tuple<string, int, int>>();

            while (queue.Count > 0)
            {
                Process currentProcess = queue.First();
                queue.RemoveAt(0);

                int executionTime = Math.Min(timeQuantum, currentProcess.RemainingTime);
                currentProcess.RemainingTime -= executionTime;

                currentTime += executionTime;

                ganttDataToPaint.Add(new Tuple<string, int, int>(currentProcess.Id, currentTime - executionTime, currentTime));

                if (currentProcess.RemainingTime == 0)
                {
                    currentProcess.CompletionTime = currentTime;
                    currentProcess.TurnaroundTime = currentProcess.CompletionTime - currentProcess.ArrivalTime;
                    currentProcess.WaitingTime = currentProcess.TurnaroundTime - currentProcess.BurstTime;

                    completedProcesses.Add(currentProcess);
                }
                else
                {
                    queue.Add(currentProcess);
                }
            }

            // Display the results in dataGridView2
            dataGridView2.Rows.Clear();
            foreach (var process in completedProcesses)
            {
                dataGridView2.Rows.Add(process.Id, process.ArrivalTime, process.BurstTime,
                                       process.TurnaroundTime, process.WaitingTime);
            }

            double averageWaitingTime = CalculateAverageWaitingTime();
            label5.Text = averageWaitingTime.ToString();

            // Gọi hàm vẽ biểu đồ Gantt
            panel1.Invalidate();
        }


        // Done, Delete
        private void button4_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            label5.Text = string.Empty;

            dataGridView1.Visible = true;
            dataGridView2.Visible = true;
            processes.Clear();
        }

        private void PaintGanttChart(Graphics g)
        {
            if (ganttDataToPaint != null && ganttDataToPaint.Count > 0)
            {
                int barHeight = 40;
                int panelWidth = panel1.Width;

                int colorIndex = 0;

                foreach (var ganttData in ganttDataToPaint)
                {
                    var processRows = dataGridView2.Rows.Cast<DataGridViewRow>().Where(row => row.Cells[0].Value != null && row.Cells[0].Value.ToString() == ganttData.Item1);

                    if (processRows.Any())
                    {
                        int processStart = ganttData.Item2;
                        int processEnd = ganttData.Item3;

                        Color processColor = GetNextColor(colorIndex);
                        SolidBrush brush = new SolidBrush(processColor);

                        int rectWidth = (processEnd - processStart) * panelWidth / ganttDataToPaint.Last().Item3;
                        int rectX = processStart * panelWidth / ganttDataToPaint.Last().Item3;
                        int rectY = 10;

                        // Fill Rectangle
                        g.FillRectangle(brush, rectX, rectY, rectWidth, barHeight);

                        // Draw Process ID and Time below the Rectangle
                        string processText = $"{ganttData.Item1}:{processEnd}";
                        g.DrawString(processText, Font, Brushes.Black, new PointF(rectX + rectWidth / 2 - 30, rectY + barHeight));

                        colorIndex = (colorIndex + 1) % ProcessColors.Length;
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
    }

    public class Process
    {
        public string Id { get; set; }
        public int ArrivalTime { get; set; }
        public int BurstTime { get; set; }
        public int RemainingTime { get; set; }
        public int CompletionTime { get; set; }
        public int WaitingTime { get; set; }
        public int TurnaroundTime { get; set; }

    }


}