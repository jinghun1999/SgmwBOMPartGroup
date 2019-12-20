using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBS
{
    public class ProcessB
    {
        public void SyncGroups()
        {
            var ls = DAL.GetBOMTable(@"select * from dbo.bom_bom with(nolock) where [物料单-项目] IN (
                            select[物料单-项目] from dbo.bom_bom with(nolock) group by [物料单-项目] having count(1) > 1
                        )", null);

            //var fs = ls.Where(p => p.Seq == 1).ToList();
            //List<RelationModel> list = new List<RelationModel>();
            int group = 0, i = 0;
            foreach (var item in ls)
            { 
                if(item.GroupId.HasValue)
                {
                    continue;
                }
                int deep = 0;
                group++;
                //item.group = group;
                Console.Write($"StartNewGroup={group}\t");
                Cal(new List<BOMModel> { item }, ref ls, ref group, ref i, ref deep);
                var ds = ls.Where(p => p.GroupId == group).ToList();
                DAL.SetGroupBatch(ds, group);
            }
        }

        public void Cal(List<BOMModel> ms, ref List<BOMModel> ls, ref int group, ref int i, ref int deep)
        {
            DateTime n = DateTime.Now;
            //foreach (var item in ls)
            //{
            //if (item.group.HasValue)// (deep > 10)//
            //{
            //    return;
            //}
            //else
            //{
                deep++;
            //}
            //item.group = group;
            //DAL.SetGroup(item, group);
            //i++;
            var d1 = ms.Select(p => p.ItemNo).ToList();
            var d2 = ms.Select(p => p.PartNo).ToList();
            Console.Write($"SetGroup2-{i}={group}\t");
            //var subs = ls.Where(p => (p.PartNo == item.PartNo || p.ItemNo == item.ItemNo) && !p.group.HasValue).ToList();
            //var subs = ls.Where(p => (d1.Contains(p.ItemNo) || d2.Contains(p.PartNo)) && !p.group.HasValue).ToList();
            //var subs1 = (from a in ls
            //             join b in d1 on a.ItemNo equals b into b1
            //             from bb in b1.DefaultIfEmpty()
            //             join c in d2 on a.PartNo equals c into c1
            //             from cc in c1.DefaultIfEmpty()
            //             where !a.GroupId.HasValue && (bb != null || cc != null)
            //             select a).ToList();
            //var subs = (from a in ls
            //            //where !a.GroupId.HasValue && d1.Contains(a.ItemNo)
            //            from d in d1 where !a.GroupId.HasValue && a.ItemNo == d
            //            select a).Union(
            //            from a in ls
            //                //join b in d2 on a.PartNo equals b into b1
            //                //where !a.GroupId.HasValue && d2.Contains(a.PartNo)
            //            from d in d2
            //            where !a.GroupId.HasValue && a.PartNo == d
            //            select a
            //            ).ToList();
            var subs = ls.Where(p => !p.GroupId.HasValue && (d1.Contains(p.ItemNo) || d2.Contains(p.PartNo))).ToList();
            Console.WriteLine("遍历耗时："+(DateTime.Now - n).TotalSeconds.ToString());

            //并循环 - Parallel.For(0, _files.Count(), i => { });
            //安全类型 -private static ConcurrentDictionary<string, string> RouteStrCD;

            if (subs.Count > 0)
            {
                foreach (var sb in subs)
                {
                    if (sb.GroupId.HasValue)
                    {
                        continue;
                    }
                    sb.GroupId = group;
                    //DAL.SetGroup(sb, group);
                    i++;
                    //sb.group = group;
                }
                Console.Write($"SetGroup-{deep}-{i}={group}\t");
                Cal(subs, ref ls, ref group, ref i, ref deep);
            }
            else
            {
                //group++;
                //Console.WriteLine("\nNew Group========" + group);
                //break;
                subs = new List<BOMModel>();
                deep = 0;
                return;
            }
            //}
        }
    }
}
