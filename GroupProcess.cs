using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBS
{
    public class GroupProcess
    {
        public void Do()
        {
            //var d1 = GetDT("SELECT * FROM dbo.BOM_BOM WITH(NOLOCK) WHERE group_id is null");
            //int group = 1;
            //foreach(var s in d1)
            //{
            //    DAL.SetGroup(s, group);
            //    GET(s, ref group);
            //}

            //var d1 = ;
            int group = 1;
            while (DAL.GetBom("and group_id is null") != null)
            {
                var s = DAL.GetBom("and group_id is null");
                if (s != null)
                {
                    Console.WriteLine("GROUP " + group);
                    DAL.SetGroup(s, group);
                    this.Sp(s, ref group);
                    group++;
                }
            }
        }
        private void Sp(BOMModel s, ref int group)
        {
            DAL.SetGroup(s, group);
            var children = DAL.GetBOMTable(null, $"where (零件='{s.PartNo}' or [物料单-项目]='{s.ItemNo}') and group_id is null");
            if (children.Count > 0)
            {
                foreach (var d in children)
                {
                    DAL.SetGroup(d, group);
                    Sp(d, ref group);
                }
            }
            else
            {
                return;
            }
            //else
            //{
            //    group++;
            //}
        }
    }
}
