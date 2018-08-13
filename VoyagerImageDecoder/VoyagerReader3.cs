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
    public class VoyagerReader3
    {
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private int x, y, d;

        public VoyagerReader3(PictureBox pictureBox1, PictureBox pictureBox2)
        {
            this.pictureBox1 = pictureBox1;
            this.pictureBox2 = pictureBox2;

            List<Segment> startLocations = new List<Segment>()
            {
                new Segment() { Channel = Channel.Left, Header = 88, CurrentPosition = 0, StartSample = 6000208, LengthSample = 1928181, WindowLengthSample = 3400 },
                new Segment() { Channel = Channel.Right, Header = 88, CurrentPosition = 0, StartSample = 6503334, LengthSample = 1841946, WindowLengthSample = 3400 }
                //new Segment() { Channel = Channel.Left, Header = 88, CurrentPosition = 0, StartSample = 8312903, LengthSample = 1946880, WindowLengthSample = 3400 }
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

            if (s.Channel == Channel.Left)
                leftImage = new Bitmap(512, 384, PixelFormat.Format32bppArgb);
            else
                rightImage = new Bitmap(512, 384, PixelFormat.Format32bppArgb);

            x = 0;
            y = 0;

            for (int i = 0; i < 512; i++)
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
                //MakeGrayscale3(leftImage).Save(@"C:\2.png", ImageFormat.Png);
            }

            if (rightImage != null)
            {
                pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox2.Image = rightImage;
                pictureBox2.Invalidate();
                pictureBox2.Update();
                pictureBox2.Refresh();
            }
        }

        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
         new float[] {.3f, .3f, .3f, 0, 0},
         new float[] {.59f, .59f, .59f, 0, 0},
         new float[] {.11f, .11f, .11f, 0, 0},
         new float[] {0, 0, 0, 1, 0},
         new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private void GenerateImage(float[] data, ref Bitmap img)
        {
            OffsetColor(data);

            for (int i = 0; i < data.Length; i++)
            {
                Color c;

                if (data[i] >= 0)
                    c = GreyscaleMapper(data[i]);
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

        public Color GreyscaleMapper(float data)
        {
            int i = 0;  //1.0f = black
            return Color.FromArgb((int)(200 - (255 * (data / d))), 0, 0, 0);
        }

        public float[] OffsetColor(float[] data)
        {
            float a = data.Max();
            float b = data.Min();

            for (int i = 0; i < data.Length; i++)
                data[i] += Math.Abs(b);

            return data;
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
            dataInt = Invert(dataInt);
            wr.PeakStart = FindPeak(dataInt); 
            wr.PeakEnd = FindPeakLow(dataInt);
            float[] newData = new float[wr.PeakEnd - wr.PeakStart];
            Array.Copy(dataInt, wr.PeakStart, newData, 0, newData.Length);

            d = (wr.PeakEnd - wr.PeakStart) / 384;
            wr.DataSum = PixelPack(newData, d);

            return wr;
        }

        private float[] Invert(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = -data[i];

            return data;
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
            int index = 0;

            for (int i = 0; i < data.Length / 2; i++)
            {
                if (data[i] > b)
                {
                    b = data[i];
                    index = i;
                }
            }

            return index;
        }

        private int FindPeakLow(float[] data)
        {
            float b = float.MaxValue;
            int index = 1000;

            for (int i = 1000; i < data.Length; i++)
            {
                if (data[i] < b)
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
