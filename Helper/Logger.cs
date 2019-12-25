using System;
using System.IO;
using System.Text;

namespace SBS
{
    public class Logger
    {
        /// <summary>
        /// 日志文件
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="title">日志标题</param>
        public static void CreateLog(string message, string title = null)
        {
            string path = Directory.GetCurrentDirectory() + "\\log";
            string filename = path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string cont = "";
            FileInfo fileInf = new FileInfo(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (File.Exists(filename))//如何文件存在 则在文件后面累加
            {
                FileStream myFss = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamReader r = new StreamReader(myFss);
                cont = r.ReadToEnd();
                r.Close();
                myFss.Close();
            }

            FileStream myFs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter n = new StreamWriter(myFs);
            n.Write(cont);
            //n.WriteLine("------------------------------------------------------Begin-------------------------------------------------");
            if (!string.IsNullOrEmpty(title))
            {
                n.WriteLine("*****" + title + "*****");
            }
            n.WriteLine($"时间：{DateTime.Now.ToString()}\t信息：{message}");
            //n.WriteLine("-------------------------------------------------------end--------------------------------------------------");
            n.Close();
            myFs.Close();

            if (fileInf.Length >= 1024 * 1024 * 200)
            {
                string NewName = path + "log_" + DateTime.Now.ToShortDateString() + ".txt";
                File.Move(filename, NewName);
            }
        }


        /// <summary>
        /// 异常日志
        /// </summary>
        public static void WriteLogError(Exception ex)
        {
            string path = Directory.GetCurrentDirectory() + "\\error";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += "\\" + DateTime.Now.ToShortDateString() + ".log";
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.Default))
            {
                sw.WriteLine("*************************************************["
                + DateTime.Now.ToShortDateString()
                + "]**********************************************");
                if (ex != null)
                {
                    sw.WriteLine("[ErrorType]" + ex.GetType());
                    sw.WriteLine("[TargeSite]" + ex.TargetSite);
                    sw.WriteLine("[Message]" + ex.Message);
                    sw.WriteLine("[Source]" + ex.Source);
                    sw.WriteLine("[StackTrace]" + ex.StackTrace);
                }
                else
                {
                    sw.WriteLine("Exception is NULL");
                }
                sw.WriteLine();
            }
        }

        public static string ReadDT()
        {
            string dt = null;
            var txtpath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "test.txt";
            if (File.Exists(txtpath))
            {
                StreamReader sr = new StreamReader(txtpath, Encoding.UTF8);
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    dt = line;
                }
                sr.Close();
            }
            return dt;
        }
        public static void WriteDT(string dt)
        {
            var txtpath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "test.txt";
            if (File.Exists(txtpath))
            {
                FileStream stream2 = File.Open(txtpath, FileMode.OpenOrCreate, FileAccess.Write);
                stream2.Seek(0, SeekOrigin.Begin);
                stream2.SetLength(0); //清空txt文件
                StreamWriter sw = new StreamWriter(stream2);
                sw.Write(dt);
                sw.Flush();
                sw.Close();
                stream2.Close();
            }
            else
            {
                FileStream fs = new FileStream(txtpath, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(dt);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
        }
    }
}
