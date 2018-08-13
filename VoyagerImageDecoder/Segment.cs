using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoyagerImageDecoder
{
    public enum Channel
    {
        Left,
        Right
    }
    public class Segment
    {
        public Channel Channel { get; set; }
        public int Header { get; set; }
        public int Start { get; set; }
        public int StartSample { get; set; }
        public int CurrentPosition { get; set; }
        public int Length { get; set; }
        public int LengthSample { get; set; }
        public int WindowLength { get; set; }
        public int WindowLengthSample { get; set; }
    }
}
