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
            DBHelper.ExecuteNonQuery($"UPDATE BOM_BOM SET GROUP_ID={group} WHERE [物料单-项目]='{b.ItemNo}' AND [零件]='{b.PartNo}'", null);
        }
        public static void SetGroupBatch(List<BOMModel> bs, int group)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in bs)
            {
                sb.AppendLine($"UPDATE BOM_BOM SET GROUP_ID={group} WHERE [物料单-项目]='{b.ItemNo}' AND [零件]='{b.PartNo}';");
            }
            if (sb.Length > 0)
                DBHelper.ExecuteNonQuery(sb.ToString(), null);
        }
        public static List<BOMModel> GetBOMTable(string sql, string where)
        {
            if (string.IsNullOrEmpty(sql))
            {
                sql = "SELECT TOP (5000000) * FROM dbo.BOM_BOM WITH(NOLOCK) ";
            }
            DataTable dt = DBHelper.ExecuteDataTable(sql + where, null);

            List<BOMModel> list = new List<BOMModel>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new BOMModel()
                {
                    ItemNo = Convert.ToString(dr["物料单-项目"]),
                    NodeTicks = Convert.ToString(dr["节点-计数器"]),
                    PartNo = Convert.ToString(dr["零件"]),
                    ValidStartDate = Convert.ToString(dr["有效起始日"]),
                    Seq = Convert.ToInt32(dr["变更顺序"]),
                    CreateTime = Convert.ToDateTime(dr["变更顺序生成时间"]),
                    GroupId = dr["group_id"] != DBNull.Value ? Convert.ToInt32(dr["group_id"]) : -1,
                });
            }

            return list;
        }
        public static BOMModel GetBom(string where)
        {
            var dr = DBHelper.ExecuteDataRow("SELECT TOP 1 * FROM dbo.BOM_BOM WITH(NOLOCK) WHERE 1=1 " + where, null);
            if (dr != null)
            {
                return new BOMModel()
                {
                    ItemNo = Convert.ToString(dr["物料单-项目"]),
                    NodeTicks = Convert.ToString(dr["节点-计数器"]),
                    PartNo = Convert.ToString(dr["零件"]),
                    ValidStartDate = Convert.ToString(dr["有效起始日"]),
                    Seq = Convert.ToInt32(dr["变更顺序"]),
                    CreateTime = Convert.ToDateTime(dr["变更顺序生成时间"]),
                    GroupId = dr["group_id"] != DBNull.Value ? Convert.ToInt32(dr["group_id"]) : -1,
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
