using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1;
using RRR;
using Thuat_toan_SJF;
using srtnn;

namespace BTL_LTW
{
    public partial class TrangChu : Form
    {
        public TrangChu()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            SRTN fcfs = new SRTN();
            fcfs.ShowDialog();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            RoundRobin rr = new RoundRobin();
            rr.ShowDialog();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            SJF sJF = new SJF();
            sJF.ShowDialog();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            srtn srt = new srtn();
            srt.ShowDialog();
        }

       
    }
}
