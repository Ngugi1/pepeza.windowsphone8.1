using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Shared.Server.ServerModels.Notices
{
    public class FileNoticeModel 
    {
        public int boardId { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public string content { get; set; }
        public StorageFile  file { get; set; }
    }
}
