﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FISCA.Data;
using System.Data;
using SmartSchool.Evaluation.WearyDogComputerHelper;
using System.Xml;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using FISCA.DSAUtil;
using SmartSchool.Evaluation.ScoreCalcRule;
using K12.Data;
using Aspose.Cells;
using System.IO;
using System.Windows.Forms;
using FISCA.Presentation.Controls;

namespace SH_SemesterScoreReport_jvyjhs
{
    class Utility
    {
        /// <summary>
        /// 取得學生分項成績(學業、體育、國防通識、健康與護理、實習科目、德行)
        /// </summary>
        /// <param name="studentIDLis"></param>
        /// <param name="SchoolYear"></param>
        /// <param name="Semester"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, decimal?>> GetStudentSemsEntryScore(List<string> studentIDLis, int SchoolYear, int Semester)
        {
            // DB XML
            //<Entry 分項="學業" 成績="81.5"/><Entry 分項="體育" 成績="90"/><Entry 分項="健康與護理" 成績="95"/><Entry 分項="國防通識" 成績="89"/><Entry 分項="實習科目" 成績="96"/></SemesterEntryScore>"
            //<SemesterEntryScore>
            //<Entry 分項="德行" 成績="83.4" 鎖定="False"/>
            //</SemesterEntryScore>
            Dictionary<string, Dictionary<string, decimal?>> retVal = new Dictionary<string, Dictionary<string, decimal?>>();

            if (studentIDLis.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string strSQL = "select ref_student_id,score_info from sems_entry_score where ref_student_id in(" + string.Join(",", studentIDLis.ToArray()) + ") and school_year=" + SchoolYear + " and semester=" + Semester + " order by ref_student_id";
                DataTable dt = qh.Select(strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr[0].ToString();

                    if (!retVal.ContainsKey(sid))
                    {
                        Dictionary<string, decimal?> val = new Dictionary<string, decimal?>();
                        retVal.Add(sid, val);
                    }

                    string strScore = dr[1].ToString();
                    if (!string.IsNullOrWhiteSpace(strScore))
                    {
                        XElement elmScore = XElement.Parse(strScore);
                        foreach (XElement elm in elmScore.Elements("Entry"))
                        {
                            if (elm.Attribute("分項") != null)
                            {
                                string name = elm.Attribute("分項").Value;
                                decimal dd;
                                retVal[sid].Add(name, null);
                                if (elm.Attribute("成績") != null)
                                    if (decimal.TryParse(elm.Attribute("成績").Value, out dd))
                                        retVal[sid][name] = dd;
                            }
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// 透過學年度學期取得學生缺曠統計(傳入學生系統編號、開始日期、結束日期；回傳：學生系統編號、獎懲名稱,統計值
        /// </summary>
        /// <param name="StudIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, int>> GetAttendanceCountBySchoolYearSemester(List<K12.Data.StudentRecord> StudRecordList, int SchoolYear, int Semester)
        {
            Dictionary<string, Dictionary<string, int>> retVal = new Dictionary<string, Dictionary<string, int>>();

            List<PeriodMappingInfo> PeriodMappingList = PeriodMapping.SelectAll();
            // 節次>類別
            Dictionary<string, string> PeriodMappingDict = new Dictionary<string, string>();
            foreach (PeriodMappingInfo rec in PeriodMappingList)
            {
                if (!PeriodMappingDict.ContainsKey(rec.Name))
                    PeriodMappingDict.Add(rec.Name, rec.Type);
            }


            List<AttendanceRecord> attendList = K12.Data.Attendance.SelectBySchoolYearAndSemester(StudRecordList, SchoolYear, Semester);

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
                    string key = PeriodMappingDict[per.Period] + "_" + per.AbsenceType;

                    if (!retVal[rec.RefStudentID].ContainsKey(key))
                        retVal[rec.RefStudentID].Add(key, 0);

                    retVal[rec.RefStudentID][key]++;
                }
            }

            // 計算統計資料， 2017/1/3 穎驊新增，因應文華ePost期末成績單 與舊常春藤 ePost 不同， 所做出更正
            //舊格式為: "一般_曠課"、"午休_曠課" ， 新格式 "曠課" 來統計所有的曠課筆數
            //雖然本次和恩正討論後，用不到了，但日後有可能有用，先留著。
            //foreach (AttendanceRecord rec in attendList)
            //{
            //    if (!retVal.ContainsKey(rec.RefStudentID))
            //        retVal.Add(rec.RefStudentID, new Dictionary<string, int>());

            //    foreach (AttendancePeriod per in rec.PeriodDetail)
            //    {               
            //        // ex :曠課
            //        string key =per.AbsenceType;

            //        if (!retVal[rec.RefStudentID].ContainsKey(key))
            //            retVal[rec.RefStudentID].Add(key, 0);

            //        retVal[rec.RefStudentID][key]++;
            //    }
            //}

            return retVal;
        }

        /// <summary>
        /// 缺曠轉換
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string ATTypeParse(string key)
        {
            string retValue = "其他假";

            if (key.Contains("事假"))
                retValue = "事假";

            if (key.Contains("病假"))
                retValue = "病假";

            if (key.Contains("公假"))
                retValue = "公假";

            if (key.Contains("喪假"))
                retValue = "喪假";

            if (key == "一般_曠課")
                retValue = "曠課";

            if (key == "一般_遲到")
                retValue = "遲到";

            if (key.Contains("曠課") || key.Contains("遲到"))
            {
                if (retValue != "曠課" || retValue != "遲到")
                    retValue = "升降旗";
            }

            return retValue;
        }

        /// <summary>
        /// 透過學年度取得學生缺曠統計(傳入學生系統編號、開始日期、結束日期；回傳：學生系統編號、獎懲名稱,統計值
        /// </summary>
        /// <param name="StudIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, int>> GetAttendanceCountBySchoolYear(List<K12.Data.StudentRecord> StudRecordList, int SchoolYear)
        {
            Dictionary<string, Dictionary<string, int>> retVal = new Dictionary<string, Dictionary<string, int>>();

            List<PeriodMappingInfo> PeriodMappingList = PeriodMapping.SelectAll();
            // 節次>類別
            Dictionary<string, string> PeriodMappingDict = new Dictionary<string, string>();
            foreach (PeriodMappingInfo rec in PeriodMappingList)
            {
                if (!PeriodMappingDict.ContainsKey(rec.Name))
                    PeriodMappingDict.Add(rec.Name, rec.Type);
            }


            List<AttendanceRecord> attendList = K12.Data.Attendance.SelectBySchoolYearAndSemester(StudRecordList, SchoolYear, null);

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
                    string key = PeriodMappingDict[per.Period] + "_" + per.AbsenceType;

                    if (!retVal[rec.RefStudentID].ContainsKey(key))
                        retVal[rec.RefStudentID].Add(key, 0);

                    retVal[rec.RefStudentID][key]++;
                }
            }

            return retVal;
        }




        /// <summary>
        /// 透過學生編號取得學生服務學習時數 傳入學生編號、學年度,回傳：學生編號、內容、值
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, decimal> GetServiceLearningBySchoolYear(List<string> StudentIDList, int SchoolYear)
        {
            Dictionary<string, decimal> retVal = new Dictionary<string, decimal>();

            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id,school_year,semester,hours from $k12.service.learning.record where ref_student_id in('" + string.Join("','", StudentIDList.ToArray()) + "') and school_year=" + SchoolYear + " order by ref_student_id,school_year,semester;";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr[0].ToString();
                    decimal hr;
                    decimal.TryParse(dr[3].ToString(), out hr);

                    if (!retVal.ContainsKey(sid))
                        retVal.Add(sid, 0);

                    retVal[sid] += hr;

                }
            }
            return retVal;
        }

        /// <summary>
        /// 透過學生編號取得學生服務學習時數 傳入學生編號、學年度,回傳：學生編號、內容、值
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static Dictionary<string, decimal> GetServiceLearningBySchoolYearSemester(List<string> StudentIDList, int SchoolYear, int Semester)
        {
            Dictionary<string, decimal> retVal = new Dictionary<string, decimal>();

            if (StudentIDList.Count > 0)
            {
                QueryHelper qh = new QueryHelper();
                string query = "select ref_student_id,school_year,semester,hours from $k12.service.learning.record where ref_student_id in('" + string.Join("','", StudentIDList.ToArray()) + "') and school_year=" + SchoolYear + " and semester=" + Semester + " order by ref_student_id,school_year,semester;";
                DataTable dt = qh.Select(query);
                foreach (DataRow dr in dt.Rows)
                {
                    string sid = dr[0].ToString();
                    decimal hr;
                    decimal.TryParse(dr[3].ToString(), out hr);

                    if (!retVal.ContainsKey(sid))
                        retVal.Add(sid, 0);

                    retVal[sid] += hr;

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
            {
                if (!Key2List.Contains(data.Name))                 
                    Key2List.Add(data.Name);

                //2017/1/3 穎驊新增，因應文華ePost 缺曠格式與舊常春藤不一樣，
                //雖然本次和恩正討論後，用不到了，但日後有可能有用，先留著。
                //if (!retVal.Contains(data.Name)) 
                //{
                //    retVal.Add(data.Name);                
                //}                                    
            }
            
                

            // 一般_曠課
            foreach (string key1 in key1List)
                foreach (string key2 in Key2List)
                    retVal.Add(key1 + "_" + key2);

            return retVal;
        }

        /// <summary>
        /// 取得學生及格與補考標準，參數用學生IDList,回傳:key:StudentID,1_及,數字
        /// </summary>
        /// <param name="StudRecList"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, decimal>> GetStudentApplyLimitDict(List<SmartSchool.Customization.Data.StudentRecord> StudRecList)
        {

            Dictionary<string, Dictionary<string, decimal>> retVal = new Dictionary<string, Dictionary<string, decimal>>();


            foreach (SmartSchool.Customization.Data.StudentRecord studRec in StudRecList)
            {
                //及格標準<年級,及格與補考標準>
                if (!retVal.ContainsKey(studRec.StudentID))
                    retVal.Add(studRec.StudentID, new Dictionary<string, decimal>());

                XmlElement scoreCalcRule = SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(studRec.StudentID) == null ? null : SmartSchool.Evaluation.ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(studRec.StudentID).ScoreCalcRuleElement;
                if (scoreCalcRule == null)
                {

                }
                else
                {

                    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                    decimal tryParseDecimal;
                    decimal tryParseDecimala;

                    foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
                    {
                        string cat = element.GetAttribute("類別");
                        bool useful = false;
                        //掃描學生的類別作比對
                        foreach (CategoryInfo catinfo in studRec.StudentCategorys)
                        {
                            if (catinfo.Name == cat || catinfo.FullName == cat)
                                useful = true;
                        }
                        //學生是指定的類別或類別為"預設"
                        if (cat == "預設" || useful)
                        {
                            for (int gyear = 1; gyear <= 4; gyear++)
                            {
                                switch (gyear)
                                {
                                    case 1:
                                        if (decimal.TryParse(element.GetAttribute("一年級及格標準"), out tryParseDecimal))
                                        {
                                            string k1s = gyear + "_及";

                                            if (!retVal[studRec.StudentID].ContainsKey(k1s))
                                                retVal[studRec.StudentID].Add(k1s, tryParseDecimal);

                                            if (retVal[studRec.StudentID][k1s] > tryParseDecimal)
                                                retVal[studRec.StudentID][k1s] = tryParseDecimal;
                                        }

                                        if (decimal.TryParse(element.GetAttribute("一年級補考標準"), out tryParseDecimala))
                                        {
                                            string k1a = gyear + "_補";

                                            if (!retVal[studRec.StudentID].ContainsKey(k1a))
                                                retVal[studRec.StudentID].Add(k1a, tryParseDecimala);

                                            if (retVal[studRec.StudentID][k1a] > tryParseDecimala)
                                                retVal[studRec.StudentID][k1a] = tryParseDecimala;
                                        }

                                        break;
                                    case 2:
                                        if (decimal.TryParse(element.GetAttribute("二年級及格標準"), out tryParseDecimal))
                                        {
                                            string k2s = gyear + "_及";

                                            if (!retVal[studRec.StudentID].ContainsKey(k2s))
                                                retVal[studRec.StudentID].Add(k2s, tryParseDecimal);

                                            if (retVal[studRec.StudentID][k2s] > tryParseDecimal)
                                                retVal[studRec.StudentID][k2s] = tryParseDecimal;
                                        }

                                        if (decimal.TryParse(element.GetAttribute("二年級補考標準"), out tryParseDecimala))
                                        {
                                            string k2a = gyear + "_補";

                                            if (!retVal[studRec.StudentID].ContainsKey(k2a))
                                                retVal[studRec.StudentID].Add(k2a, tryParseDecimala);

                                            if (retVal[studRec.StudentID][k2a] > tryParseDecimala)
                                                retVal[studRec.StudentID][k2a] = tryParseDecimala;

                                        }

                                        break;
                                    case 3:
                                        if (decimal.TryParse(element.GetAttribute("三年級及格標準"), out tryParseDecimal))
                                        {
                                            string k3s = gyear + "_及";

                                            if (!retVal[studRec.StudentID].ContainsKey(k3s))
                                                retVal[studRec.StudentID].Add(k3s, tryParseDecimal);

                                            if (retVal[studRec.StudentID][k3s] > tryParseDecimal)
                                                retVal[studRec.StudentID][k3s] = tryParseDecimal;
                                        }

                                        if (decimal.TryParse(element.GetAttribute("三年級補考標準"), out tryParseDecimala))
                                        {
                                            string k3a = gyear + "_補";

                                            if (!retVal[studRec.StudentID].ContainsKey(k3a))
                                                retVal[studRec.StudentID].Add(k3a, tryParseDecimala);

                                            if (retVal[studRec.StudentID][k3a] > tryParseDecimala)
                                                retVal[studRec.StudentID][k3a] = tryParseDecimala;
                                        }

                                        break;
                                    case 4:
                                        if (decimal.TryParse(element.GetAttribute("四年級及格標準"), out tryParseDecimal))
                                        {
                                            string k4s = gyear + "_及";

                                            if (!retVal[studRec.StudentID].ContainsKey(k4s))
                                                retVal[studRec.StudentID].Add(k4s, tryParseDecimal);

                                            if (retVal[studRec.StudentID][k4s] > tryParseDecimal)
                                                retVal[studRec.StudentID][k4s] = tryParseDecimal;
                                        }

                                        if (decimal.TryParse(element.GetAttribute("四年級補考標準"), out tryParseDecimala))
                                        {
                                            string k4a = gyear + "_補";

                                            if (!retVal[studRec.StudentID].ContainsKey(k4a))
                                                retVal[studRec.StudentID].Add(k4a, tryParseDecimala);

                                            if (retVal[studRec.StudentID][k4a] > tryParseDecimala)
                                                retVal[studRec.StudentID][k4a] = tryParseDecimala;
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }


        public static void UseAsposeToSaveCSV(string inputReportName, DataTable dt)
        {
            string reportName = inputReportName;

            DataTable dataTable = dt;

            Workbook workbook = new Workbook();
            
            workbook.Worksheets[0].Cells.ImportDataTable(dataTable, true, "A1");

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, reportName +".csv" );

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
            workbook.Save(path,SaveFormat.CSV);                 
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

            //Workbook workbook = inputXls;
            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);

            // StringBuilder sb = new StringBuilder();

            //sb.Append(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()));
            //    sb.Append(Encoding.Unicode.GetString(Encoding.Unicode.GetPreamble()));
            //if (workbook == null || workbook.Worksheets == null || workbook.Worksheets.Count == 0)
            //{
            //    MessageBox.Show("沒有資料！");
            //    return;
            //}
            //Cells cells = workbook.Worksheets[0].Cells;
            DataTable dataTable = dt;//workbook.Worksheets[0].Cells.ExportDataTable(cells.MinRow, cells.MinColumn, cells.Rows.Count, cells.Columns.Count);
            //  int row_count = 0;


            //dataTable.Rows.Cast<DataRow>().ToList().ForEach((x) =>
            //{
            //    row_count++;
            //    sb.Append(string.Join(",", x.ItemArray));
            //    if (!(row_count == dataTable.Rows.Count))
            //        sb.AppendLine();
            //});

            List<string> strList = new List<string>();
            foreach (DataColumn dc in dt.Columns)
                strList.Add(dc.ColumnName);

            sw.WriteLine(string.Join(",", strList.ToArray()));
            // sb.AppendLine(string.Join(",",strList.ToArray()));

            foreach (DataRow dr in dt.Rows)
            {
                List<string> subList = new List<string>();
                for (int col = 0; col < dt.Columns.Count; col++)
                {

                    // 2017/1/19 穎驊筆記， 經過測試，發現若原本 不特別處理 來源 value 資料，
                    // 若value其內含有 ","逗號，將會造成 CSV 檔判讀自動跳一位，最後導致錯位的問題，
                    // 與恩正討論研究後， 在其字串 前後 加 "  雙引號可以解決此問題，
                    // 又另外 若原本 value 文內 有 一個 " 雙引號 ， 將先使用 兩個 " 雙引號取代， 可以避免奇怪的 bug
                    // 此方法 顯示將不會有問題。

                    subList.Add('"'+dr[col].ToString().Replace("\"","\"\"")+'"');


                }
                sw.WriteLine(string.Join(",", subList.ToArray()));
                //sb.AppendLine(string.Join(",",subList.ToArray()));
            }

            //string out_put_string = sb.ToString();

            sw.Close();
            try
            {
                //FileStream fs = new FileStream(path, FileMode.Create);
                //fs.Write(Encoding.Unicode.GetBytes(out_put_string), 0, Encoding.Unicode.GetByteCount(out_put_string));
                //fs.Close();
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
                        //FileStream fs = new FileStream(sd.FileName, FileMode.Create);
                        //fs.Write(Encoding.Unicode.GetBytes(out_put_string), 0, Encoding.Unicode.GetByteCount(out_put_string));
                        //fs.Close();
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


            //string reportName = inputReportName;

            //string path = Path.Combine(Application.StartupPath, "Reports");
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            //path = Path.Combine(path, reportName + ".csv");

            //Workbook wb = inputXls;

            //if (File.Exists(path))
            //{
            //    int i = 1;
            //    while (true)
            //    {
            //        string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
            //        if (!File.Exists(newPath))
            //        {
            //            path = newPath;
            //            break;
            //        }
            //    }
            //}

            //try
            //{
            //    wb.Save(path, Aspose.Cells.FileFormatType.CSV);
            //    System.Diagnostics.Process.Start(path);
            //}
            //catch
            //{
            //    SaveFileDialog sd = new SaveFileDialog();
            //    sd.Title = "另存新檔";
            //    sd.FileName = reportName + ".csv";
            //    sd.Filter = "CSV檔案 (*.csv)|*.xls|所有檔案 (*.*)|*.*";
            //    if (sd.ShowDialog() == DialogResult.OK)
            //    {
            //        try
            //        {
            //            wb.Save(sd.FileName, Aspose.Cells.FileFormatType.CSV);

            //        }
            //        catch
            //        {
            //            MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            return;
            //        }
            //    }
            //}
        }


        public static void CompletedXlsCsvAnsi(string inputReportName, Workbook inputXls)
        {

            string reportName = inputReportName;

            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".csv");

            Workbook wb = inputXls;

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

            try
            {
                wb.Save(path, Aspose.Cells.FileFormatType.CSV);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".csv";
                sd.Filter = "CSV檔案 (*.csv)|*.xls|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        wb.Save(sd.FileName, Aspose.Cells.FileFormatType.CSV);

                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// 取得常春藤高中缺曠
        /// </summary>
        /// <returns></returns>
        public static List<string> GetATTypeList()
        {
            List<string> ATTypeList = new List<string>();

            ATTypeList.Add("遲到");
            ATTypeList.Add("曠課");
            ATTypeList.Add("事假");
            ATTypeList.Add("病假");
            ATTypeList.Add("公假");
            ATTypeList.Add("喪假");


            return ATTypeList;
        }

        /// <summary>
        /// Word 與ePost 欄位差異對照用
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetePostMapName1Dict()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            retVal.Add("學期學業成績", "學業成績");
            retVal.Add("學期學業成績班排名", "全班名次");
            retVal.Add("學期學業成績班排名母數", "全班人數");
            retVal.Add("本學期一般_事假", "事假");
            retVal.Add("本學期一般_病假", "病假");
            retVal.Add("本學期一般_喪假", "喪假");
            retVal.Add("本學期一般_曠課", "曠課");
            retVal.Add("本學期一般_遲到", "遲到");
            retVal.Add("本學期一般_公假", "公假");
            retVal.Add("本學期一般_早退", "早退");

            //retVal.Add("本學期一般_婚假", "婚假");
            //retVal.Add("本學期一般_生理假", "生理假");
            //retVal.Add("本學期一般_產假", "產假");

            retVal.Add("本學期升旗_升旗缺席", "升旗缺席");
            retVal.Add("本學期早修_早修缺席", "早讀");
            retVal.Add("本學期午休_午休缺席", "午休");

      
            retVal.Add("本學期修習學分數", "應得學分");
            retVal.Add("本學期取得學分數", "實得學分");
            retVal.Add("姓名", "姓名");
            retVal.Add("班級", "班級簡稱");
            retVal.Add("學年度", "學年度");
            retVal.Add("學期", "學期別");
            retVal.Add("座號", "座號");
            retVal.Add("學號", "學號");

            retVal.Add("學期學業成績加權總分", "總分");

            retVal.Add("累計取得學分數", "累計學分");

            retVal.Add("大功統計", "大功");
            retVal.Add("小功統計", "小功");
            retVal.Add("嘉獎統計", "嘉獎");
            retVal.Add("大過統計", "大過");
            retVal.Add("小過統計", "小過");
            retVal.Add("警告統計", "警告");
            retVal.Add("留校察看", "留校查看");

            retVal.Add("導師評語", "綜合評語");

            return retVal;
        }

    }
}

