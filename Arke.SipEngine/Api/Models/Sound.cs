using System;
using System.Collections.Generic;
using System.Text;

namespace Arke.SipEngine.Api.Models
{
    public class Sound
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public ICollection<SoundFormat> Formats { get; set; }
    }

    public class SoundFormat
    {
        public string Language { get; set; }
        public string Format { get; set; }
    }
}
