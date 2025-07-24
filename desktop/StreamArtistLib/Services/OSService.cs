using AForge.Video.DirectShow;
using StreamArtist.Services;
using StreamArtistLib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace StreamArtistLib.Services
{
    public class OSService
    {
        public List<WebCam> GetWebCams()
        {
            var list = new List<WebCam>();
            FilterInfoCollection videoDevices;

            // Enumerate video devices
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo device in videoDevices)
            {
                var wc = new WebCam();
                wc.Name = device.Name;
                list.Add(wc);
            }
            return list;
        }
    }
}
