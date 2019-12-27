using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBS
{
    public class DAL
    {
        public static void SetGroup(BOMModel b, int group)
        {
            DBHelper.ExecuteNonQuery($"UPDATE dbo.[Pur_BOM_BOM] SET GROUP_ID={group} WHERE [BOMNo-ProjectID]='{b.ItemNo}' AND [PartNo]='{b.PartNo}'", null);
        }
        public static void SetGroupBatch(List<BOMModel> bs, int group)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in bs)
            {
                sb.AppendLine($"UPDATE dbo.[Pur_BOM_BOM] SET GROUP_ID={group} WHERE [BOMNo-ProjectID]='{b.ItemNo}' AND [PartNo]='{b.PartNo}';");
            }
            if (sb.Length > 0)
                DBHelper.ExecuteNonQuery(sb.ToString(), null);
        }
        public static List<BOMModel> GetBOMTable(string sql, string where)
        {
            if (string.IsNullOrEmpty(sql))
            {
                sql = "SELECT TOP (5000000) * FROM dbo.[Pur_BOM_BOM] WITH(NOLOCK) ";
            }
            DataTable dt = DBHelper.ExecuteDataTable(sql + where, null);

            List<BOMModel> list = new List<BOMModel>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new BOMModel()
                {
                    ItemNo = Convert.ToString(dr["BOMNo-ProjectID"]),
                    //NodeTicks = Convert.ToString(dr["节点-计数器"]),
                    PartNo = Convert.ToString(dr["PartNo"]),
                    ValidStartDate = Convert.ToString(dr["StartDate"]),
                    Seq = Convert.ToInt32(dr["SeqID"]),
                    CreateTime = Convert.ToDateTime(dr["SeqTime"]),
                    GroupId = ConvertUtility.ToInt32(dr["group_id"]),
                });
            }

            return list;
        }
        public static BOMModel GetBom(string where)
        {
            var dr = DBHelper.ExecuteDataRow("SELECT TOP 1 * FROM dbo.[Pur_BOM_BOM] WITH(NOLOCK) WHERE 1=1 " + where, null);
            if (dr != null)
            {
                return new BOMModel()
                {
                    ItemNo = Convert.ToString(dr["BOMNo-ProjectID"]),
                    //NodeTicks = Convert.ToString(dr["节点-计数器"]),
                    PartNo = Convert.ToString(dr["PartNo"]),
                    ValidStartDate = Convert.ToString(dr["StartDate"]),
                    Seq = Convert.ToInt32(dr["SeqID"]),
                    CreateTime = Convert.ToDateTime(dr["SeqTime"]),
                    GroupId = ConvertUtility.ToInt32(dr["group_id"]),
                };
            }
            else
            {
                return null;
            }
        }

        public static int ProcessPartSignleGroup()
        {
            var ds = DBHelper.RunProcedure("dbo.PROC_ProcessPartSignleGroup", new System.Data.SqlClient.SqlParameter[] { });
            return Convert.ToInt32(ds.Tables[0].Rows[0][0]);
        }

    }
}
