using System;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MandelbrotSet
{
    public partial class settingsform : Form
    {
        public ComplexM C {
            get
            {
                decimal r = 0, i = 0;
                decimal.TryParse(textBox1.Text, out r);
                decimal.TryParse(textBox2.Text, out i);
                return new ComplexM(r, i);
            }
            set
            {
                textBox1.Text = value.Real.ToString();
                textBox2.Text = value.Imaginary.ToString();
            }
        }
        public decimal Scaling
        {
            get
            {
                decimal s = 180;
                if (!decimal.TryParse(textBox5.Text, out s))
                    textBox5.Text = s.ToString();
                return s;
            }
            set
            {
                textBox5.Text = value.ToString();
            }
        }
        public int Iterations
        {
            get
            {
                int s = 80;
                if (!int.TryParse(textBox6.Text, out s))
                    textBox6.Text = s.ToString();
                return s;
            }
            set
            {
                textBox6.Text = value.ToString();
            }
        }
        public int Threads
        {
            get
            {
                int s = 4;
                if (!int.TryParse(textBox7.Text, out s))
                    textBox7.Text = s.ToString();
                return s;
            }
            set
            {
                textBox7.Text = value.ToString();
            }
        }
        public int qWidth
        {
            get
            {
                int s = 7680;
                if (!int.TryParse(textBox3.Text, out s))
                    textBox3.Text = s.ToString();
                return s;
            }
            set
            {
                textBox3.Text = value.ToString();
            }
        }
        public int qHeight
        {
            get
            {
                int s = 7680;
                if (!int.TryParse(textBox4.Text, out s))
                    textBox4.Text = s.ToString();
                return s;
            }
            set
            {
                textBox4.Text = value.ToString();
            }
        }

        public static float QualityProgress = 0;
        public static ProgressBar bar;

        public settingsform()
        {
            InitializeComponent();
            C = Program.C;
            Scaling = Program.scalefactor;
            Iterations = Program.itercount;
            Threads = Program.threads;
            qWidth = Program.qWidth;
            qHeight = Program.qHeight;
            bar = progressBar1;
            progressBar1.Value = (int)(QualityProgress * 1000);
            comboBox1.SelectedIndex = (int)Program.state;
            comboBox2.Items.Clear();
            for (int k = 0; k < Screen.AllScreens.Length; k++)
                comboBox2.Items.Add(k);
            comboBox2.SelectedIndex = Program.ScreenIndex;
        }
        ~settingsform()
        {
            bar = null;
        }

        void Apply()
        {
            Program.C = C;
            Program.scalefactor = Scaling;
            Program.itercount = Iterations;
            Program.threads = Threads;
            Program.qWidth = qWidth;
            Program.qHeight = qHeight;
            Program.state = (WinState)comboBox1.SelectedIndex;
            Program.ScreenIndex = comboBox2.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Apply();
            this.Close();
        }//Apply

        private void button2_Click(object sender, EventArgs e)
        {
            Apply();
            Program.QualiedImage(qWidth, qHeight);
        }//QualityScreen

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = (int)(QualityProgress * 1000d);
        }
    }
}
