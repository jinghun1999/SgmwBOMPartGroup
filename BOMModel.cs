using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBS
{
    public class RelationModel
    {
        public string GroupId { get; set; }
        public string PartNo { get; set; }
        public string ItemNo { get; set; }
        public string ValidStartDate { get; set; }
        public string NodeTicks { get; set; }
        public string SeqNo { get; set; }
        public DateTime CreateTime { get; set; }
    }
    public class BOMModel
    {
        public string ItemNo { get; set; }
        public string NodeTicks { get; set; }
        public string PartNo { get; set; }
        public string ValidStartDate { get; set; }
        public int Seq { get; set; }

        public int? GroupId { get; set; }
    }
}
