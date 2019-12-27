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
            int c = 0;
            Console.WriteLine("Get top 1 from db order by group_id desc\n");
            var exbom = DAL.GetBom("AND GROUP_ID>0 ORDER BY group_id DESC");
            if (exbom != null)
            {
                Logger.CreateLog("已获得之前最大的组号：" + exbom.GroupId);
                Console.WriteLine($"There already exists the data with group_id, now begin to add.\n");
                Console.WriteLine("Begin to sync group id with part_no\n");
                //已经有分组存在，得走追加模式
                c = DBHelper.ExecuteNonQuery("update a set a.group_id=b.group_id from dbo.[Pur_BOM_BOM] a inner join dbo.[Pur_BOM_BOM] b on a.[PartNo]=b.[PartNo] where a.group_id is null and b.group_id is not null;");
                Logger.CreateLog("根据已有组号的零件同步组号，共同步" + c + "条记录");
                Console.WriteLine("Begin to sync group id with item no\n");
                c = DBHelper.ExecuteNonQuery("update a set a.group_id=b.group_id from dbo.[Pur_BOM_BOM] a inner join dbo.[Pur_BOM_BOM] b on a.[BOMNo-ProjectID]=b.[BOMNo-ProjectID] where a.group_id is null and b.group_id is not null;");
                Logger.CreateLog("根据已有组号的物料单号同步组号，共同步" + c + "条记录");
                Console.WriteLine("Now, begin to init the group id left.\n");
                this.InitGroup(exbom.GroupId.Value);


                c = DBHelper.ExecuteNonQuery("update a set a.group_id=b.group_id from dbo.[Pur_BOM_BOM] a inner join dbo.[Pur_BOM_BOM] b on a.[PartNo]=b.[PartNo] where a.group_id is null and b.group_id is not null;");
                Logger.CreateLog("再次根据已有组号的零件同步组号，共同步" + c + "条记录");
            }
            else
            {
                Console.WriteLine($"There is none data yet, begin to init group id.\n");
                InitGroup();
            }
        }
        private void InitGroup(int g = 0)
        {
            Logger.CreateLog("开始从上次组号" + g + "重新组织数据组号");
            var ls = DAL.GetBOMTable(@"select * from dbo.[Pur_BOM_BOM] with(nolock) where group_id is null and [BOMNo-ProjectID] IN (
                            select[BOMNo-ProjectID] from dbo.[Pur_BOM_BOM] with(nolock) where group_id is null group by [BOMNo-ProjectID] having count(1) > 1
                        )", null);
            Logger.CreateLog("重新组织的数据量行数：" + ls.Count);
            int group = g, i = 0;
            foreach (var item in ls)
            {
                if (item.GroupId.HasValue)
                {
                    continue;
                }
                group++;
                Console.Write($"StartNewGroup={group}\n");
                Cal(new List<BOMModel> { item }, ref ls, ref group, ref i);
                var ds = ls.Where(p => p.GroupId == group).ToList();
                DAL.SetGroupBatch(ds, group);
            }

            int c = DAL.ProcessPartSignleGroup();
            Logger.CreateLog("已完成存储过程同步剩余未处理的数据行数：" + c);

            //DateTime? mt = ls.Max(p => p.CreateTime);
            //if (mt.HasValue)
            //{
            //    Logger.WriteDT(mt.Value.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            //}
        }

        public void Cal(List<BOMModel> ms, ref List<BOMModel> ls, ref int group, ref int i)
        {
            DateTime n = DateTime.Now;
            
            var d1 = ms.Select(p => p.ItemNo).ToList();
            var d2 = ms.Select(p => p.PartNo).ToList();
            Console.Write($"SetGroup{group}-{i}\n");
            
            var subs = ls.Where(p => !p.GroupId.HasValue && (d1.Contains(p.ItemNo) || d2.Contains(p.PartNo))).ToList();
            Console.WriteLine($"\tQuery item from items:{String.Join(",", d1)}; parts:{String.Join(",", d2)} duration(sec):{(DateTime.Now - n).TotalSeconds.ToString()}\t");

            if (subs.Count > 0)
            {
                foreach (var sb in subs)
                {
                    if (sb.GroupId.HasValue)
                    {
                        continue;
                    }
                    sb.GroupId = group;
                    i++;
                    Console.WriteLine($"To set item {sb.ItemNo},{sb.PartNo} group_id to {group}\t");
                }
                Console.Write($"Begin a new recursion{group}-{i}\t");
                Cal(subs, ref ls, ref group, ref i);
            }
            else
            {
                subs = new List<BOMModel>();
                return;
            }
        }
    }
}
