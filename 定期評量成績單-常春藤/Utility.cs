using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;
using FISCA.UDT;
using FISCA.Data;
using System.Data;
using System.IO;

namespace SH_jvyjhs_StudentExamScore_epost
{
    public class Utility
    {
        /// <summary>
        /// 透過日期區間取得學生缺曠統計(傳入學生系統編號、開始日期、結束日期；回傳：學生系統編號、獎懲名稱,統計值
        /// </summary>
        /// <param name="StudIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, int>> GetAttendanceCountByDate(List<StudentRecord> StudRecordList,DateTime beginDate,DateTime endDate)
        {
            Dictionary<string, Dictionary<string, int>> retVal = new Dictionary<string, Dictionary<string, int>>();

            List<PeriodMappingInfo> PeriodMappingList = PeriodMapping.SelectAll();
            // 節次>類別
            Dictionary<string,string> PeriodMappingDict = new Dictionary<string,string> ();
            foreach(PeriodMappingInfo rec in PeriodMappingList)
            {
                if(!PeriodMappingDict.ContainsKey(rec.Name))
                PeriodMappingDict.Add(rec.Name,rec.Type);
            }
                
            List<AttendanceRecord> attendList = K12.Data.Attendance.SelectByDate(StudRecordList, beginDate, endDate);

            // 計算統計資料
            foreach (AttendanceRecord rec in attendList)
            {                
                if (!retVal.ContainsKey(rec.RefStudentID))
                    retVal.Add(rec.RefStudentID, new Dictionary<string, int>());
                
                foreach (AttendancePeriod per in rec.PeriodDetail)
                {
                    if (!PeriodMappingDict.ContainsKey(per.Period))
                        continue;

                    // ex.一般:曠課
                    string key ="區間"+ PeriodMappingDict[per.Period] + "_" + per.AbsenceType;

                    if (!retVal[rec.RefStudentID].ContainsKey(key))
                        retVal[rec.RefStudentID].Add(key, 0);
                    
                        retVal[rec.RefStudentID][key]++;
                }
            }

            return retVal;
        }

        /// <summary>
        /// 透過日期區間取得獎懲資料,傳入學生ID,開始日期,結束日期,回傳：學生ID,獎懲統計名稱,統計值
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string,Dictionary<string,int>> GetDisciplineCountByDate(List<string> StudentIDList,DateTime beginDate,DateTime endDate)
        {
            Dictionary<string, Dictionary<string, int>> retVal = new Dictionary<string, Dictionary<string, int>>();

            List<string> nameList = new string[] { "大功", "小功", "嘉獎", "大過", "小過", "警告", "留校" }.ToList();

            // 取得獎懲資料
            List<DisciplineRecord> dataList = Discipline.SelectByStudentIDs(StudentIDList);

            foreach (DisciplineRecord data in dataList)
            {
                if (data.OccurDate >= beginDate && data.OccurDate <= endDate)
                {
                    // 初始化
                    if (!retVal.ContainsKey(data.RefStudentID))
                    {
                        retVal.Add(data.RefStudentID, new Dictionary<string, int>());
                        foreach (string str in nameList)
                            retVal[data.RefStudentID].Add(str, 0);
                    }

                    // 獎勵
                    if (data.MeritFlag == "1")
                    {
                        if (data.MeritA.HasValue)
                            retVal[data.RefStudentID]["大功"] += data.MeritA.Value;

                        if (data.MeritB.HasValue)
                            retVal[data.RefStudentID]["小功"] += data.MeritB.Value;

                        if (data.MeritC.HasValue)
                            retVal[data.RefStudentID]["嘉獎"] += data.MeritC.Value;
                    }
                    else if (data.MeritFlag == "0")
                    { // 懲戒
                        if (data.Cleared != "是")
                        {
                            if (data.DemeritA.HasValue)
                                retVal[data.RefStudentID]["大過"] += data.DemeritA.Value;

                            if (data.DemeritB.HasValue)
                                retVal[data.RefStudentID]["小過"] += data.DemeritB.Value;
                            
                            if (data.DemeritC.HasValue)
                                retVal[data.RefStudentID]["警告"] += data.DemeritC.Value;
                        }                    
                    }
                    else if (data.MeritFlag == "2")
                    { 
                        // 留校察看
                        retVal[data.RefStudentID]["留校"]++;
                    }
                }            
            }
            return retVal;
        }

        /// <summary>
        /// 透過學生編號取得學生服務學習時數 傳入學生編號、開始日期、結束日期,回傳：學生編號、內容、值
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, decimal>> GetServiceLearningByDate(List<string> StudentIDList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, Dictionary<string, decimal>> retVal = new Dictionary<string, Dictionary<string, decimal>>();

            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id,school_year,semester,hours from $k12.service.learning.record where ref_student_id in('"+string.Join("','",StudentIDList.ToArray())+"') and occur_date >='"+beginDate.ToShortDateString()+"' and occur_date <='"+endDate.ToShortDateString()+"'order by ref_student_id,school_year,semester;";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr[0].ToString();
                    string key2 = dr[1].ToString() + "學年度第" + dr[2].ToString() + "學期";
                    decimal hr;
                    decimal.TryParse(dr[3].ToString(), out hr);

                    if (!retVal.ContainsKey(sid))
                        retVal.Add(sid, new Dictionary<string, decimal>());

                    if (!retVal[sid].ContainsKey(key2))
                        retVal[sid].Add(key2, 0);

                    retVal[sid][key2] += hr;
                
                }
            }
            return retVal;
        }

        /// <summary>
        /// 透過學生編號、開始與結束日期，取的獎懲資料
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, List<DisciplineRecord>> GetDisciplineDetailByDate(List<string> StudentIDList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, List<DisciplineRecord>> retVal = new Dictionary<string, List<DisciplineRecord>>();

            // 取得獎懲資料
            List<DisciplineRecord> dataList = Discipline.SelectByStudentIDs(StudentIDList);
            // 依日期排序
            dataList = (from data in dataList orderby data.OccurDate select data).ToList();

            foreach (DisciplineRecord rec in dataList)
            {
                if (rec.OccurDate >= beginDate && rec.OccurDate <= endDate)
                {
                    if (!retVal.ContainsKey(rec.RefStudentID))
                        retVal.Add(rec.RefStudentID, new List<DisciplineRecord>());

                    retVal[rec.RefStudentID].Add(rec);
                }
            }

            return retVal;        
        }

        /// <summary>
        /// 透過學生編號、開始與結束日期，取得缺曠資料
        /// </summary>
        /// <param name="StudRecordList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, List<AttendanceRecord>> GetAttendanceDetailByDate(List<StudentRecord> StudRecordList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, List<AttendanceRecord>> retVal = new Dictionary<string, List<AttendanceRecord>>();
            
            // 讀取資料
            List<AttendanceRecord> attendList = K12.Data.Attendance.SelectByDate(StudRecordList, beginDate, endDate);

            // 依日期排序
            attendList = (from data in attendList orderby data.OccurDate select data).ToList();

            foreach (AttendanceRecord rec in attendList)
            {
                if (!retVal.ContainsKey(rec.RefStudentID))
                    retVal.Add(rec.RefStudentID, new List<AttendanceRecord>());
                
                retVal[rec.RefStudentID].Add(rec);
            }

            return retVal;
        }


        /// <summary>
        /// 透過學生編號、開始與結束日期，取得學習服務 DataRow
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string,List<DataRow>> GetServiceLearningDetailByDate(List<string> StudentIDList, DateTime beginDate, DateTime endDate)
        {
            Dictionary<string, List<DataRow>> retVal = new Dictionary<string, List<DataRow>>();
            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id,occur_date,reason,hours from $k12.service.learning.record where ref_student_id in('" + string.Join("','", StudentIDList.ToArray()) + "') and occur_date >='" + beginDate.ToShortDateString() + "' and occur_date <='" + endDate.ToShortDateString() + "'order by ref_student_id,occur_date;";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr[0].ToString();
                    if (!retVal.ContainsKey(sid))
                        retVal.Add(sid, new List<DataRow>());

                    retVal[sid].Add(dr);

                }
            }
            return retVal;
        }

        /// <summary>
        /// 取得缺曠對照 List,一般_曠課..
        /// </summary>
        /// <returns></returns>
        public static List<string> GetATMappingKey()
        {
            List<string> retVal = new List<string>();
            List<string> key1List = new List<string>();
            List<string> Key2List = new List<string>();
            foreach (PeriodMappingInfo data in PeriodMapping.SelectAll())
                if (!key1List.Contains(data.Type))
                    key1List.Add(data.Type);

            foreach (AbsenceMappingInfo data in AbsenceMapping.SelectAll())
                if (!Key2List.Contains(data.Name))
                    Key2List.Add(data.Name);

            // 一般_曠課
            foreach (string key1 in key1List)
                foreach (string key2 in Key2List)
                    retVal.Add(key1 + "_" + key2);
        
            return retVal;
        }

        public static void CompletedXlsCsv(string inputReportName, DataTable dt)
        {

            #region 儲存檔案
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".txt");

            if (File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Unicode);

            DataTable dataTable = dt;

            List<string> strList = new List<string>();
            foreach (DataColumn dc in dt.Columns)
                strList.Add(dc.ColumnName);

            sw.WriteLine(string.Join(",", strList.ToArray()));

            foreach (DataRow dr in dt.Rows)
            {
                List<string> subList = new List<string>();
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    subList.Add(dr[col].ToString());
                }
                sw.WriteLine(string.Join(",", subList.ToArray()));
            }

            sw.Close();
            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                try
                {
                    System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                    sd.Title = "另存新檔";
                    sd.FileName = reportName + ".txt";
                    sd.Filter = "txt檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";
                    if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        System.Diagnostics.Process.Start(sd.FileName);
                    }
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
            }
            #endregion
        }
    }
}
