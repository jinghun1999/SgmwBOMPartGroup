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

    public class Process
    {
        private ConcurrentDictionary<int, BOMModel> ls1;
        public void SyncGroups()
        {
            ls1 = new ConcurrentDictionary<int, BOMModel>();
            var ls = DAL.GetBOMTable(null, null);
            Parallel.For(0, ls.Count(), k =>
            {
                ls1.TryAdd(k, ls[k]);
            });

            //var fs = ls.Where(p => p.Seq == 1).ToList();
            //List<RelationModel> list = new List<RelationModel>();
            int group = 0, i = 0;
            //foreach (var item in ls)
            for (int _j = 0; _j <= ls1.Count; _j++)
            {
                if (ls1[_j].GroupId.HasValue)
                {
                    continue;
                }
                int deep = 0;
                group++;
                //item.group = group;
                Console.Write($"SetGroup1-{i}={group}\t");
                //Cal(new ConcurrentDictionary<int, BOMModel>().TryAdd(0, ls1[_j]) , ref group, ref i, ref deep);
                var l = new ConcurrentDictionary<int, BOMModel>();
                l.TryAdd(0, ls1[_j]);
                Cal(l, ref group, ref i, ref deep);
                var ds = ls.Where(p => p.GroupId == group).ToList();
                DAL.SetGroupBatch(ds, group);
            }
        }

        public void Cal(ConcurrentDictionary<int, BOMModel> ms, ref int group, ref int i, ref int deep)
        {
            //foreach (var item in ls)
            //{
            //if (item.group.HasValue)// (deep > 10)//
            //{
            //    return;
            //}
            //else
            //{
            //    deep++;
            //}
            //item.group = group;
            //DAL.SetGroup(item, group);
            //i++;
            //var d1 = ms.Select(p => p.Value.ItemNo).ToList();
            //var d2 = ms.Select(p => p.Value.PartNo).ToList();
            //Console.Write($"SetGroup2-{i}={group}\t");
            ///var subs = ls.Where(p => (p.PartNo == item.PartNo || p.ItemNo == item.ItemNo) && !p.group.HasValue).ToList();
            //var subs = ls.Where(p => (d1.Contains(p.ItemNo) || d2.Contains(p.PartNo)) && !p.group.HasValue).ToList();
            //var subs1 = (from a in ls1
            //             join b in d1 on a.ItemNo equals b into b1
            //             from bb in b1.DefaultIfEmpty()
            //             join c in d2 on a.PartNo equals c into c1
            //             from cc in c1.DefaultIfEmpty()
            //             where !a.@group.HasValue && (bb != null || cc != null)
            //             select a).ToList();
            DateTime n = DateTime.Now;
            ConcurrentDictionary<int, BOMModel> subs1 = new ConcurrentDictionary<int, BOMModel>();
            int mn = 0;
            Parallel.ForEach(ms, t =>
            {
                Parallel.ForEach(ls1, k =>
                {
                //if (!ls1[k].group.HasValue && (d1.Exists(p => p.Contains(ls1[k].ItemNo) || d2.Exists(q => q.Contains(ls1[k].PartNo)))))
                //{
                //    //if (!ls1[k].group.HasValue && (d1.Contains(ls1[k].ItemNo) || d2.Contains(ls1[k].PartNo)))
                //    subs1.TryAdd(k, ls1[k]);
                //}
                    if (!k.Value.GroupId.HasValue) 
                    {   
                    
                        if (t.Value.ItemNo == k.Value.ItemNo || t.Value.PartNo == k.Value.PartNo)
                        {
                            subs1.TryAdd(mn, k.Value);
                            mn++;
                            return;
                        }
                    }
                });
            });
            //var subs1 = ls1.Where(p => !p.Value.group.HasValue && (d1.Contains(p.Value.ItemNo) || d2.Contains(p.Value.PartNo))).ToList();
            Console.WriteLine("遍历耗时："+(DateTime.Now - n).TotalSeconds.ToString());

            //并循环 - Parallel.For(0, _files.Count(), i => { });
            //安全类型 -private static ConcurrentDictionary<string, string> RouteStrCD;

            if (subs1.Count > 0)
            {
                foreach (var sb in subs1)
                {
                    if (sb.Value.GroupId.HasValue)
                    {
                        continue;
                    }
                    sb.Value.GroupId = group;
                    //DAL.SetGroup(sb, group);
                    i++;
                    //sb.group = group;
                }
                Console.Write($"SetGroup3-{i}={group}\t");
                Cal(subs1, ref group, ref i, ref deep);
            }
            else
            {
                //group++;
                //Console.WriteLine("\nNew Group========" + group);
                //break;
                //subs1 = new List<BOMModel>();
                return;
            }
            //}
        }
    }
}
