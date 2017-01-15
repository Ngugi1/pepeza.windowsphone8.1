using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Notices
{
    public class TFile
    {
        [PrimaryKey]
        public int id { get; set; }
        [Ignore]
        public string fileTypeAndSize { get; set; }
        public long size { get; set; }
        public string link { get; set; }
        public string fileName { get; set; }
        public string mimeType { get; set; }
        public long dateCreated { get; set; }
        public long dateUpdated { get; set; }
        public int attachmentId { get; set; }
        public string uniqueFileName { get; set; }
    }
}
