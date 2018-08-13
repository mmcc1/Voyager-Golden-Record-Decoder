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
    public class VoyagerReader2
    {
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        Bitmap leftImage = null;
        Bitmap rightImage = null;
        private int x, y;

        public VoyagerReader2(PictureBox pictureBox1, PictureBox pictureBox2)
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
                ReadWav(s);
            }
        }

        private void ReadWav(Segment s)
        {
            FileStream fr = new FileStream(@"C:\384kHzStereo.wav", FileMode.Open);
            WindowReader wr = new WindowReader();
            wr.Segment = s;
            wr.Segment.CurrentPosition = wr.Segment.Start;
            fr.Seek(wr.Segment.Start, System.IO.SeekOrigin.Begin);

            x = 0;
            y = 0;

            wr.Samples = new byte[wr.Segment.Length];
            fr.Read(wr.Samples, 0, wr.Samples.Length);

            Tuple<byte[], byte[]> data = SplitChannels(wr.Samples);

            if (wr.Segment.Channel == Channel.Left)
            {
                wr = ProcessData(data.Item1, wr);
                GenerateImage(wr.DataSumList, ref leftImage);
            }
            else
            {
                wr = ProcessData(data.Item2, wr);
                GenerateImage(wr.DataSumList, ref rightImage);
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

        private void GenerateImage(List<float[]> data, ref Bitmap img)
        {
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data.ElementAt(i).Length; j++)
                {
                    Color c;

                    if (data.ElementAt(i)[j] >= 0)
                        c = Color.FromArgb((int)(data.ElementAt(i)[j] * 20000));
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
        }

        private Tuple<byte[], byte[]> SplitChannels(byte[] bytes)
        {
            byte[] left = new byte[bytes.Length / 2];
            byte[] right = new byte[bytes.Length / 2];
            int index = 0;

            for (int i = 0; i < left.Length; i += 4)
            {
                left[i] = bytes[index++];
                left[i + 1] = bytes[index++];
                left[i + 2] = bytes[index++];
                left[i + 3] = bytes[index++];
                right[i] = bytes[index++];
                right[i + 1] = bytes[index++];
                right[i + 2] = bytes[index++];
                right[i + 3] = bytes[index++];
            }

            return new Tuple<byte[], byte[]>(left, right);
        }

        private WindowReader ProcessData(byte[] data, WindowReader wr)
        {
            float[] dataInt = ConvertFromByteArrayToFloatArray(data);

            //TODO: Divide by a time frame
            int numberOfColumns = dataInt.Length / 3299;

            if (wr.Segment.Channel == Channel.Left)
                leftImage = new Bitmap(numberOfColumns, 384, PixelFormat.Format32bppRgb);
            else
                rightImage = new Bitmap(numberOfColumns, 384, PixelFormat.Format32bppRgb);

            wr.DataSumList = PixelPack(dataInt, numberOfColumns);

            return wr;
        }

        private List<float[]> PixelPack(float[] data, int numberOfColumns)
        {
            List<float[]> pp = new List<float[]>();
            List<float[]> pp2 = new List<float[]>();
            int index2 = 0;

            //Break into columns of samples
            for (int j = 0; j < numberOfColumns; j++)
            {
                float[] column = new float[data.Length / numberOfColumns];

                for (int i = 0; i < column.Length; i++)
                {
                    column[i] = data[index2++];
                }

                pp.Add(column);
            }

            index2 = 0;

            //Pack samples into 384
            for (int i = 0; i < pp.Count; i++)
            {
                float[] column = new float[384];

                for (int j = 0; j < column.Length; j++)
                {
                    for (int k = 0; k < pp.ElementAt(i).Length / column.Length; k++)
                    {
                        column[j] += data[index2++];
                    }
                }

                pp2.Add(column);
            }

            return pp2;
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
