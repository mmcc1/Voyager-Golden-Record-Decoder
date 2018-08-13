using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace VoyagerImageDecoder
{
    public partial class Form1 : Form
    {
        private int x, y;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.BackgroundImageLayout = ImageLayout.Center;
            pictureBox2.BackgroundImageLayout = ImageLayout.Center;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            VoyagerReader3 vr = new VoyagerReader3(pictureBox1, pictureBox2);
        }
    }

}
