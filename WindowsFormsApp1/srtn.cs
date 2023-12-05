using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;
using BTL_LTW;
namespace srtnn
{
    public partial class srtn : Form
    {
        private List<Tuple<string, int, int>> ganttDataToPaint;
        public  int[] mang;
        public  int[] mangxh;
        public static int chay = 0;
     //   public  int[] manght;

        public srtn()
        {
            InitializeComponent();
            InitializeDataGridView();

            // Đăng ký sự kiện Paint và Resize cho panel1
            panel1.Paint += panel1_Paint;
            panel1.Resize += panel1_Resize;
        }

        private void InitializeDataGridView()
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // nếu 2 ô nhập khác rỗng thì add vào datagridview
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
            // Xóa dữ liệu cũ trên biểu đồ Gantt
            ganttDataToPaint = new List<Tuple<string, int, int>>();

            // Copy dữ liệu từ dataGridView1 sang dataGridView2
            copy(dataGridView1, dataGridView2);

            // Sắp xếp dữ liệu theo thời gian đến
            sapxep(dataGridView2);

            // Khởi tạo mảng và giá trị ban đầu
            mang = new int[dataGridView1.RowCount - 1];
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                mang[i] = -1;
            }

            mangxh = new int[dataGridView1.RowCount - 1];
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                mangxh[i] = int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
            }

            // Tính toán và hiển thị kết quả
            calculateTurnaroundTimes(dataGridView2);
            label5.Text = AvgWaitingTime(dataGridView2).ToString();

            // Tạo dữ liệu cho biểu đồ Gantt
            CreateGanttData();

            // Hiển thị biểu đồ Gantt trên panel1
            panel1.Invalidate();

        }

        public  int index_mang()
        {
            int min = mang[0];
            int ind=0;
            for (int i = 0; i < mang.Length; i++)
            {
                if (mang[i] < min && mang[i] > 0)
                {
                    min = mang[i];
                    ind = i;
                }
            }
            return ind;
        }

        public int demso0()
        {
            int dem = 0;
            for (int i = 0; i < mang.Length; i++)
            {
                if (mang[i] == 0)
                {
                    dem++;
                }
            }
            return dem;
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
            //while (demso0() != dataGridView1.Rows.Count - 1)
            while ( mang.Count(element => element == 0) != dataGridView1.RowCount-1 )
                {
                if (Array.IndexOf(mangxh, chay) != -1) // chay == xuất hiện
                {
                    mang[Array.IndexOf(mangxh, chay)] = int.Parse(dataGridView2.Rows[Array.IndexOf(mangxh, chay)].Cells[2].Value.ToString()); // add sử dụng vào mang

                    int inn = index_mang();
                    mang[index_mang()]--;

                    if (mang[inn] == 0)
                    {
                        dataGridView2.Rows[inn].Cells[3].Value = int.Parse((chay + 1).ToString());                    
                    }

                }

                else // chạy  != xuất hiện
                {
                    int ii = index_mang();
                    mang[index_mang()]--;

                    if (mang[ii] == 0)
                    {
                        dataGridView2.Rows[ii].Cells[3].Value = int.Parse((chay + 1).ToString());                       
                    }
                }
                chay++;
            }

            for(int i = 0; i < dataGridView2.RowCount - 1; i++)
            {
                dataGridView2.Rows[i].Cells[4].Value = int.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString()) - int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                dataGridView2.Rows[i].Cells[5].Value = int.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString()) - int.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());
            }
        }

        private void CreateGanttData()
        {
            ganttDataToPaint = new List<Tuple<string, int, int>>();
            int currentTime = 0;

            for (int i = 0; i < dataGridView2.RowCount - 1; i++)
            {
                int processStart = int.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                int processEnd = int.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());

                // Thêm mỗi lượt chạy của tiến trình vào biểu đồ Gantt
                ganttDataToPaint.Add(new Tuple<string, int, int>(dataGridView2.Rows[i].Cells[0].Value.ToString(), processStart, processEnd));

                currentTime = processEnd; // Cập nhật thời điểm hiện tại
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

                for (int i = 0; i < ganttDataToPaint.Count; i++)
                {
                    int processStart = ganttDataToPaint[i].Item2;
                    int processEnd = ganttDataToPaint[i].Item3;
                    int processDuration = processEnd - processStart;

                    Color processColor = GetNextColor(colorIndex);
                    Pen pen = new Pen(processColor, barHeight);

                    // Vẽ đoạn thanh Gantt của tiến trình
                    g.DrawLine(pen, processStart * panelWidth / ganttDataToPaint.Last().Item3, labelY, processEnd * panelWidth / ganttDataToPaint.Last().Item3, labelY);

                    // Đặt nhãn cho tiến trình tại giữa đoạn thanh Gantt
                    g.DrawString($"{ganttDataToPaint[i].Item1}:{processEnd}", Font, Brushes.Black, new PointF(processStart * panelWidth / ganttDataToPaint.Last().Item3 + processDuration / 2, labelY));

                    colorIndex = (colorIndex + 1) % ProcessColors.Length;
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



        private void button4_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            label5.Text = string.Empty;

        }
    }
}
