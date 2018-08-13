using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoyagerImageDecoder
{
    public class WindowReader
    {
        public byte[] Samples { get; set; }
        public float[] DataSum { get; set; }
        public List<float[]> DataSumList { get; set; }
        public int PeakStart { get; set; }
        public int PeakEnd { get; set; }
        public Segment Segment { get; set; }
    }
}
