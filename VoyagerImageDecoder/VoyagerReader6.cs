/* Copyright 2018 Mark McCarron
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 * 
 * 
 * 
 * 
 * VoyagerReader6
 * 
 * This file reads and decodes images, in stereo IEEE Float format, from the Voyager Golden Record.
 * 
 * Selecting a segment:
 * The start sample should be the first peak after the sync tone for each image.
 * The length is to the last peak where the frequency of the sync pulse changes.
 * 
 * Sync pulse trains are at a nominal rate of of between 116-125Hz (about 3072-3300 samples per second based up a sample rate of 384000).
 * A good windowlength for this rate is 3400.
 * Each pulse represents a column of image data 384 pixels in length.
 * 
 * 
 * Notes:
 * The primary difference between this and VoyagerReader5 is that the contrast changes have been undone.  An Offset has been added which has the
 * effect of compressing the selected range of the time domain signal.  Think of an accordian.  This is similar to a tuner, but it is a makeshift
 * approach to dealing with timing jitter.  Controls have been added to the left of the form to adjust the offset.  In the default image for the
 * right channel, try values 0 and 217 and 655 to observe the differences.
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace VoyagerImageDecoder
{
    public class VoyagerReader6
    {
        private int x, y;
        private List<Bitmap> leftExtractedImages;
        private List<Bitmap> rightExtractedImages;
        private float[] leftSamples, rightSamples;

        public VoyagerReader6()
        {
            leftExtractedImages = new List<Bitmap>();
            rightExtractedImages = new List<Bitmap>();
        }

        //Presumes stereo IEEE float format
        public Tuple<List<Bitmap>, List<Bitmap>> ExtractImages(List<ImageSegment> imageParams, string filePath, int windowLength)
        {
            //Example of imageParams for the Golden Record
            // Values based upon analysis of Wav file here:
            //https://drive.google.com/drive/folders/0B0Swx_1rwA6XcFFLc29ncFJSZmM
            //
            //List<ImageSegment> startLocations = new List<ImageSegment>()
            //{
            //    new ImageSegment() { Channel = StereoChannel.Left, Header = 88, StartSample = 6000208, LengthSample = 1928181 },
            //    new ImageSegment() { Channel = StereoChannel.Right, Header = 88, StartSample = 6503334, LengthSample = 1841946 },
            //    new ImageSegment() { Channel = StereoChannel.Left, Header = 88, StartSample = 8312903, LengthSample = 1946880 }
            //};

            foreach (ImageSegment s in imageParams)
            {
                s.Start = (s.StartSample * 8) + s.Header;
                s.Length = s.LengthSample * 8;
                ReadWav(s, filePath, windowLength);
            }

            return new Tuple<List<Bitmap>, List<Bitmap>>(leftExtractedImages, rightExtractedImages);
        }

        private void ReadWav(ImageSegment s, string filePath, int windowLength)
        {
            byte[] buffer = new byte[s.Length];

            //Read entire segment to memory
            using (FileStream fr = new FileStream(filePath, FileMode.Open))
            {
                fr.Seek(s.Start, System.IO.SeekOrigin.Begin);
                fr.Read(buffer, 0, buffer.Length);
            }

            //Convert from Stereo interleave format to channels of samples
            Tuple<byte[], byte[]> channelBuffer = SplitChannels(buffer);
            float[] leftSampleBuffer = ConvertFromByteArrayToFloatArray(channelBuffer.Item1);
            float[] rightSampleBuffer = ConvertFromByteArrayToFloatArray(channelBuffer.Item2);

            if (s.Channel == StereoChannel.Left)
            {
                //Invert the values (stored inverted on the Golden Record)
                leftSampleBuffer = Invert(leftSampleBuffer);

                //Luminosity is stored as positive/negative values.
                //Offsetting everything into positive territory makes
                //mapping to C# objects easier.
                //This process will also normalise the luminosity.
                leftSampleBuffer = OffsetColor(leftSampleBuffer);


                //Process channels
                leftSamples = leftSampleBuffer;
                List<ImageColumn> leftic = ProcessChannel(leftSampleBuffer, windowLength, s.Offset);

                //Generate the images
                GenerateImage(leftic, ref leftExtractedImages);
            }
            else
            {
                rightSampleBuffer = Invert(rightSampleBuffer);
                rightSampleBuffer = OffsetColor(rightSampleBuffer);
                rightSamples = rightSampleBuffer;
                List<ImageColumn> rightic = ProcessChannel(rightSampleBuffer, windowLength, s.Offset);
                GenerateImage(rightic, ref rightExtractedImages);
            }

        }

        private void GenerateImage(List<ImageColumn> ic, ref List<Bitmap> images)
        {
            Bitmap img = new Bitmap(ic.Count, 384, PixelFormat.Format32bppArgb);

            x = 0;
            y = 0;

            foreach (ImageColumn im in ic)
            {
                for (int i = 0; i < im.PixelPack.Length; i++)
                {
                    Color c;

                    if (im.PixelPack[i] >= 0)
                        c = GreyscaleMapper(im.PixelPack[i], im.d);
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

            images.Add(img);
        }

        public Color GreyscaleMapper(float data, int d)
        {
            int i = 0;  //1.0f = black
            return Color.FromArgb((int)(200 - (255 * (data / d))), 0, 0, 0);
        }

        public List<Bitmap> ReprocessLeftChannel(int offset)
        {
            leftExtractedImages.Clear();
            GenerateImage(ProcessChannel(leftSamples, 3400, offset), ref leftExtractedImages);
            return leftExtractedImages;
        }

        public List<Bitmap> ReprocessRightChannel(int offset)
        {
            rightExtractedImages.Clear();

            GenerateImage(ProcessChannel(rightSamples, 3400, offset), ref rightExtractedImages); 

            return rightExtractedImages;
        }

        private List<ImageColumn> ProcessChannel(float[] samples, int windowLength, int offset)
        {
            List<ImageColumn> ics = new List<ImageColumn>();
            int currentIndex = 0;

            for (int j = 0; j < samples.Length / windowLength; j++)
            {
                ImageColumn ic = new ImageColumn();
                ic.Index = j;

                //Read a windowlength, find its peaks, extract image data
                float[] sampleBuffer = new float[windowLength];
                Array.Copy(samples, currentIndex, sampleBuffer, 0, windowLength);
                ic.Start = FindStartPeak(sampleBuffer) + offset;
                ic.End = FindEndPeak(sampleBuffer);

                ic.RawSamples = new float[ic.End - ic.Start];
                Array.Copy(sampleBuffer, ic.Start, ic.RawSamples, 0, ic.RawSamples.Length);

                //Demodulate the image from AM signal
                ic.d = (ic.End - ic.Start) / 384;
                ic.PixelPack = PixelPack(ic.RawSamples, ic.d);

                ics.Add(ic);

                //Move to next image
                currentIndex += ic.End;
            }

            return ics;
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

        private float[] Invert(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = -data[i];

            return data;
        }

        public float[] OffsetColor(float[] data)
        {
            float b = data.Min();

            for (int i = 0; i < data.Length; i++)
                data[i] += Math.Abs(b);

            return data;
        }

        private int FindStartPeak(float[] data)
        {
            float b = float.MinValue;
            int index = 0;

            for (int i = 0; i < 1000; i++)
            {
                if (data[i] > b)
                {
                    b = data[i];
                    index = i;
                }
            }

            return index;
        }

        private int FindEndPeak(float[] data)
        {
            float b = float.MaxValue;
            int index = 2500;

            for (int i = 2500; i < data.Length; i++)
            {
                if (data[i] < b)
                {
                    b = data[i];
                    index = i;
                }
            }

            return index;
        }
    }
}
