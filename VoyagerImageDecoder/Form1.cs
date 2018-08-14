using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VoyagerImageDecoder
{
    public partial class Form1 : Form
    {
        VoyagerReader6 vr;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Image = vr.ReprocessLeftChannel((int)numericUpDown1.Value)[0];
            pictureBox1.Invalidate();
            pictureBox1.Refresh();
            Application.DoEvents();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
            pictureBox2.Image = vr.ReprocessRightChannel((int)numericUpDown2.Value)[0];
            
            pictureBox2.Invalidate();
            pictureBox2.Refresh();
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
            //VoyagerReader3 vr = new VoyagerReader3(pictureBox1, pictureBox2);


            //To use VoyagerReader3, comment out the below section and uncomment the above line.  No further changes, other than updating
            //the file path in the VoyagerReader3 class are required.

            List<ImageSegment> imageLocations = new List<ImageSegment>()
            {
                new ImageSegment() { Channel = StereoChannel.Left, Header = 88, StartSample = 6000208, LengthSample = 1928181 },
                //new ImageSegment() { Channel = StereoChannel.Right, Header = 88, StartSample = 6503334, LengthSample = 1841946 },
                //new ImageSegment() { Channel = StereoChannel.Left, Header = 88, StartSample = 8312903, LengthSample = 1946880 },
                //new ImageSegment() { Channel = StereoChannel.Right, Header = 88, StartSample = 8657173, LengthSample = 1908929 },
                //new ImageSegment() { Channel = StereoChannel.Left, Header = 88, StartSample = 21715098, LengthSample = 1787198 },
                //new ImageSegment() { Channel = StereoChannel.Left, Header = 88, StartSample = 28675794, LengthSample = 1821805 },
                new ImageSegment() { Channel = StereoChannel.Right, Header = 88, StartSample = 30786540, LengthSample = 1834844, Offset = 655 }
            };

            
            vr = new VoyagerReader6();
            Tuple<List<Bitmap>, List<Bitmap>> images = vr.ExtractImages(imageLocations, @"C:\384kHzStereo.wav", 3400);

            pictureBox1.Image = images.Item1[0];
            pictureBox2.Image = images.Item2[0];


        }
    }

}
