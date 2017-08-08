using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CaptionThis.Models
{
    public class Video
    {
        // unique id of the video being indexed (from service)
        public string Id { get; set; }

        // the user who submitted the video
        public string OwnerId { get; set; }

        // the name of the video to be readable in the VI site
        [Required]
        public string Name { get; set; }

        // language of the video
        public string Language { get; set; }

        // url of the video file to be indexed
        // TODO: Support uploading
        [Required]
        public string Url { get; set; }

        // whether the video indexing information is public or not
        public bool Public { get; set; }

        // processing status
        public string State { get; set; }

        // VTT file URL when completed
        public string VttUrl { get; set; }


    }
}
