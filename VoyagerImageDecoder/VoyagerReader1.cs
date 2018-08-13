using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoyagerImageDecoder
{
    public class VoyagerReader1
    {

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private int x, y;

        public VoyagerReader1(PictureBox pictureBox1, PictureBox pictureBox2)
        {
            this.pictureBox1 = pictureBox1;
            this.pictureBox2 = pictureBox2;

            List<Segment> startLocations = new List<Segment>()
            {
                new Segment() { Channel = Channel.Left, Header = 88, CurrentPosition = 0, StartSample = 6000208, LengthSample = 1928181, WindowLengthSample = 3400 },
                new Segment() { Channel = Channel.Right, Header = 88, CurrentPosition = 0, StartSample = 6503334, LengthSample = 1841946, WindowLengthSample = 3400 }
            };

            foreach (Segment s in startLocations)
            {
                s.Start = (s.StartSample * 8) + s.Header;
                s.Length = s.LengthSample * 8;
                s.WindowLength = s.WindowLengthSample * 8;
                ReadWav(s);
            }
        }

        private void ReadWav(Segment s)
        {
            FileStream fr = new FileStream(@"C:\384kHzStereo.wav", FileMode.Open);
            WindowReader wr = null;

            Bitmap leftImage = null;
            Bitmap rightImage = null;

            if(s.Channel == Channel.Left)
                leftImage = new Bitmap(800, 384, PixelFormat.Format32bppRgb);
            else
                rightImage = new Bitmap(800, 384, PixelFormat.Format32bppRgb);

            x = 0;
            y = 0;

            for (int i = 0; i < 800; i++)
            {
                if (wr == null)
                {
                    wr = new WindowReader();
                    wr.Segment = s;
                    wr.PeakStart = 0;
                    wr.Segment.CurrentPosition = wr.Segment.Start;
                    fr.Seek(wr.Segment.Start, System.IO.SeekOrigin.Begin);
                }
                else
                {
                    wr.Segment.CurrentPosition += wr.PeakEnd * 8;
                    fr.Seek(wr.Segment.CurrentPosition, System.IO.SeekOrigin.Begin);
                }

                if (wr.Segment.CurrentPosition + wr.Segment.WindowLength >= wr.Segment.Start + wr.Segment.Length)
                {
                    continue;
                }

                wr.Samples = new byte[wr.Segment.WindowLength];
                fr.Read(wr.Samples, 0, wr.Samples.Length);

                Tuple<byte[], byte[]> data = SplitChannels(wr.Samples);

                if (wr.Segment.Channel == Channel.Left)
                {
                    wr = ProcessData(data.Item1, wr);
                    GenerateImage(wr.DataSum, ref leftImage);
                }
                else
                {
                    wr = ProcessData(data.Item2, wr);
                    GenerateImage(wr.DataSum, ref rightImage);
                }
            }

            fr.Close();

            if (leftImage != null)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox1.Image = leftImage;
                pictureBox1.Invalidate();
                pictureBox1.Update();
                pictureBox1.Refresh();
            }

            if (rightImage != null)
            {
                pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox2.Image = rightImage;
                pictureBox2.Invalidate();
                pictureBox2.Update();
                pictureBox2.Refresh();
            }

            //leftImage.Save(@"C:\1.bmp", ImageFormat.Bmp);
        }

        private void GenerateImage(float[] data, ref Bitmap img)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Color c;

                if (data[i] >= 0)
                    c = Color.FromArgb((int)(data[i] * 20000));
                else
                    c = Color.White;

                if (y > img.Height - 1)
                {
                    y = 0;
                    x++;
                }

                if (x > img.Width - 1)
                    x = 0;

                img.SetPixel(x, y++, c);
            }
        }

        private Tuple<byte[], byte[]> SplitChannels(byte[] bytes)
        {
            byte[] left = new byte[bytes.Length / 2];
            byte[] right = new byte[bytes.Length / 2];
            int index = 0;

            for (int i = 0; i < left.Length; i+=4)
            {
                left[i] = bytes[index++];
                left[i+1] = bytes[index++];
                left[i+2] = bytes[index++];
                left[i+3] = bytes[index++];
                right[i] = bytes[index++];
                right[i+1] = bytes[index++];
                right[i+2] = bytes[index++];
                right[i+3] = bytes[index++];
            }

            return new Tuple<byte[], byte[]>(left, right);
        }
              
        private WindowReader ProcessData(byte[] data, WindowReader wr)
        {
            float[] dataInt = ConvertFromByteArrayToFloatArray(data);
            wr.PeakEnd = FindPeak(dataInt);
            float[] newData = new float[wr.PeakEnd];
            Array.Copy(dataInt, newData, wr.PeakEnd);

            int d = wr.PeakEnd / 384;
            wr.DataSum = PixelPack(newData, d);

            return wr;
        }

        private float[] PixelPack(float[] data, int align)
        {
            float[] dataSum = new float[384];
            int index = 0;

            for (int j = 0; j < dataSum.Length; j++)
            {
                for (int i = 0; i < align; i++)
                {
                    dataSum[j] += data[index++];
                }
            }

            return dataSum;
        }

        private int FindPeak(float[] data)
        {
            float b = float.MinValue;
            int index = 1000;

            for (int i = 1000; i < data.Length; i++)
            {
                if (data[i] > b)
                {
                    b = data[i];
                    index = i;
                }
            }

            return index;
        }

        private float[] ConvertFromByteArrayToFloatArray(byte[] data)
        {
            float[] dataInt = new float[data.Length / 4];
            int index = 0;

            for (int i = 0; i < data.Length; i += 4)
            {
                dataInt[index++] = System.BitConverter.ToSingle(data, i);
            }

            return dataInt;
        }
    }
}
