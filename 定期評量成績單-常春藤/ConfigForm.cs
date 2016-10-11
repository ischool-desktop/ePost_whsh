using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using K12.Data;
using System.IO;

namespace SH_jvyjhs_StudentExamScore_epost
{
    public partial class ConfigForm : FISCA.Presentation.Controls.BaseForm
    {
        private FISCA.UDT.AccessHelper _AccessHelper = new FISCA.UDT.AccessHelper();
        private Dictionary<string, List<string>> _ExamSubjects = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _ExamSubjectFull = new Dictionary<string, List<string>>();
        private List<TagConfigRecord> _TagConfigRecords = new List<TagConfigRecord>();
        private List<Configure> _Configures = new List<SH_jvyjhs_StudentExamScore_epost.Configure>();
        private string _DefalutSchoolYear = "";
        private string _DefaultSemester = "";

        // 開始日期
        private DateTime _BeginDate;
        // 結束日期
        private DateTime _EndDate;

        /// <summary>
        /// 成績校正日期
        /// </summary>
        private DateTime _ScoreCurDate;

        // 檢查是否產生學生清單
        private bool _isExportStudentList;

        // 檢查是否產生 epost
        private bool _isExportEpost;

        public ConfigForm()
        {
            InitializeComponent();
            List<ExamRecord> exams = new List<ExamRecord>();
            BackgroundWorker bkw = new BackgroundWorker();
            bkw.DoWork += delegate
            {
                bkw.ReportProgress(1);
                //預設學年度學期
                _DefalutSchoolYear = "" + K12.Data.School.DefaultSchoolYear;
                _DefaultSemester = "" + K12.Data.School.DefaultSemester;
                bkw.ReportProgress(10);
                //試別清單
                exams = K12.Data.Exam.SelectAll();
                bkw.ReportProgress(20);
                //學生類別清單
                _TagConfigRecords = K12.Data.TagConfig.SelectByCategory(TagCategory.Student);
                #region 整理所有試別對應科目
                var AEIncludeRecords = K12.Data.AEInclude.SelectAll();
                bkw.ReportProgress(30);
                var AssessmentSetupRecords = K12.Data.AssessmentSetup.SelectAll();
                bkw.ReportProgress(40);
                List<string> courseIDs = new List<string>();
                foreach (var scattentRecord in K12.Data.SCAttend.SelectByStudentIDs(K12.Presentation.NLDPanels.Student.SelectedSource))
                {
                    if (!courseIDs.Contains(scattentRecord.RefCourseID))
                        courseIDs.Add(scattentRecord.RefCourseID);
                }
                bkw.ReportProgress(60);
                foreach (var courseRecord in K12.Data.Course.SelectAll())
                {
                    foreach (var aeIncludeRecord in AEIncludeRecords)
                    {
                        if (aeIncludeRecord.RefAssessmentSetupID == courseRecord.RefAssessmentSetupID)
                        {
                            string key = courseRecord.SchoolYear + "^^" + courseRecord.Semester + "^^" + aeIncludeRecord.RefExamID;
                            if (!_ExamSubjectFull.ContainsKey(key))
                            {
                                _ExamSubjectFull.Add(key, new List<string>());
                            }
                            if (!_ExamSubjectFull[key].Contains(courseRecord.Subject))
                                _ExamSubjectFull[key].Add(courseRecord.Subject);
                            if (courseIDs.Contains(courseRecord.ID))
                            {
                                if (!_ExamSubjects.ContainsKey(key))
                                {
                                    _ExamSubjects.Add(key, new List<string>());
                                }
                                if (!_ExamSubjects[key].Contains(courseRecord.Subject))
                                    _ExamSubjects[key].Add(courseRecord.Subject);
                            }
                        }
                    }
                }
                bkw.ReportProgress(70);
                foreach (var list in _ExamSubjectFull.Values)
                {
                    #region 排序
                    list.Sort(new StringComparer("國文"
                                    , "英文"
                                    , "數學"
                                    , "理化"
                                    , "生物"
                                    , "社會"
                                    , "物理"
                                    , "化學"
                                    , "歷史"
                                    , "地理"
                                    , "公民"));
                    #endregion
                }
                #endregion
                bkw.ReportProgress(80);
                _Configures = _AccessHelper.Select<Configure>();
                bkw.ReportProgress(100);

            };
            bkw.WorkerReportsProgress = true;
            bkw.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                circularProgress1.Value = e.ProgressPercentage;
            };
            bkw.RunWorkerCompleted += delegate
            {
                cboConfigure.Items.Clear();
                foreach (var item in _Configures)
                {
                    cboConfigure.Items.Add(item);
                }
                cboConfigure.Items.Add(new Configure() { Name = "新增" });
                int i;
                if (int.TryParse(_DefalutSchoolYear, out i))
                {
                    for (int j = 0; j < 5; j++)
                    {
                        cboSchoolYear.Items.Add("" + (i - j));
                    }
                }
                cboSemester.Items.Add("1");
                cboSemester.Items.Add("2");
                cboExam.Items.Clear();
                cboRefExam.Items.Clear();
                cboExam.Items.AddRange(exams.ToArray());
                cboRefExam.Items.Add(new ExamRecord("", "", 0));
                cboRefExam.Items.AddRange(exams.ToArray());
                List<string> prefix = new List<string>();
                List<string> tag = new List<string>();
                foreach (var item in _TagConfigRecords)
                {
                    if (item.Prefix != "")
                    {
                        if (!prefix.Contains(item.Prefix))
                            prefix.Add(item.Prefix);
                    }
                    else
                    {
                        tag.Add(item.Name);
                    }
                }
                cboRankRilter.Items.Clear();
                cboTagRank1.Items.Clear();
                cboTagRank2.Items.Clear();
                cboRankRilter.Items.Add("");
                cboTagRank1.Items.Add("");
                cboTagRank2.Items.Add("");
                foreach (var s in prefix)
                {
                    cboRankRilter.Items.Add("[" + s + "]");
                    cboTagRank1.Items.Add("[" + s + "]");
                    cboTagRank2.Items.Add("[" + s + "]");
                }
                foreach (var s in tag)
                {
                    cboRankRilter.Items.Add(s);
                    cboTagRank1.Items.Add(s);
                    cboTagRank2.Items.Add(s);
                }
                circularProgress1.Hide();
                if (_Configures.Count > 0)
                {
                    cboConfigure.SelectedIndex = 0;
                }
                else
                {
                    cboConfigure.SelectedIndex = -1;
                }
            };
            bkw.RunWorkerAsync();
        }

        public Configure Configure { get; private set; }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (dtBegin.IsEmpty || dtEnd.IsEmpty)
            {
                FISCA.Presentation.Controls.MsgBox.Show("日期區間必須輸入!");
                return;
            }

            if (dtBegin.Value > dtEnd.Value)
            {
                FISCA.Presentation.Controls.MsgBox.Show("開始日期必須小於或等於結束日期!!");
                return;
            }
            SaveTemplate(null, null);
            _BeginDate = dtBegin.Value;
            _EndDate = dtEnd.Value;
            if (dtCurDate.IsEmpty)
                _ScoreCurDate = DateTime.Now;
            else
                _ScoreCurDate = dtCurDate.Value;
            _isExportStudentList = ChkExportStudList.Checked;
            _isExportEpost = chkExportEpost.Checked;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Program.AvgRd = iptRd.Value;
            this.Close();
        }

        /// <summary>
        /// 取得畫面上開始日期
        /// </summary>
        /// <returns></returns>
        public DateTime GetBeginDate()
        {
            return _BeginDate;
        }

        /// <summary>
        /// 取得畫面上結束日期
        /// </summary>
        /// <returns></returns>
        public DateTime GetEndDate()
        {
            return _EndDate;
        }

        /// <summary>
        /// 取得畫面上成績校正日期
        /// </summary>
        /// <returns></returns>
        public DateTime GetScoreCurDate()
        {
            return _ScoreCurDate;
        }

        /// <summary>
        /// 取得畫面上是否產生學生清單
        /// </summary>
        /// <returns></returns>
        public bool GetisExportStudentList()
        {
            return _isExportStudentList;
        }

        private void ExamChanged(object sender, EventArgs e)
        {
            string key = cboSchoolYear.Text + "^^" + cboSemester.Text + "^^" +
                (cboExam.SelectedItem == null ? "" : ((ExamRecord)cboExam.SelectedItem).ID);
            listViewEx1.SuspendLayout();
            listViewEx2.SuspendLayout();
            listViewEx3.SuspendLayout();
            listViewEx1.Items.Clear();
            listViewEx2.Items.Clear();
            listViewEx3.Items.Clear();
            if (_ExamSubjectFull.ContainsKey(key))
            {
                foreach (var subject in _ExamSubjectFull[key])
                {
                    var i1 = listViewEx1.Items.Add(subject);
                    var i2 = listViewEx2.Items.Add(subject);
                    var i3 = listViewEx3.Items.Add(subject);
                    if (Configure != null && Configure.PrintSubjectList.Contains(subject))
                        i1.Checked = true;
                    if (Configure != null && Configure.TagRank1SubjectList.Contains(subject))
                        i2.Checked = true;
                    if (Configure != null && Configure.TagRank2SubjectList.Contains(subject))
                        i3.Checked = true;
                    if (_ExamSubjects.ContainsKey(key) && !_ExamSubjects[key].Contains(subject))
                    {
                        i1.ForeColor = Color.DarkGray;
                        i2.ForeColor = Color.DarkGray;
                        i3.ForeColor = Color.DarkGray;
                    }
                }
            }
            listViewEx1.ResumeLayout(true);
            listViewEx2.ResumeLayout(true);
            listViewEx3.ResumeLayout(true);
        }

        private void cboConfigure_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboConfigure.SelectedIndex == cboConfigure.Items.Count - 1)
            {
                //新增
                btnSaveConfig.Enabled = btnPrint.Enabled = false;
                NewConfigure dialog = new NewConfigure();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Configure = new Configure();
                    Configure.Name = dialog.ConfigName;
                    Configure.Template = dialog.Template;
                    Configure.SubjectLimit = dialog.SubjectLimit;
                    Configure.ScoreCurDate = dialog.ScoreCurDate;
                    
                    Configure.DisciplineDetailLimit = dialog.DisciplineDetailLimit;
                    Configure.ServiceLearningDetailLimit = dialog.ServiceLearningDetailLimit;

                    Configure.SchoolYear = _DefalutSchoolYear;
                    Configure.Semester = _DefaultSemester;
                    if (cboExam.Items.Count > 0)
                        Configure.ExamRecord = (ExamRecord)cboExam.Items[0];
                    _Configures.Add(Configure);
                    cboConfigure.Items.Insert(cboConfigure.SelectedIndex, Configure);
                    cboConfigure.SelectedIndex = cboConfigure.SelectedIndex - 1;
                    Configure.Encode();
                    Configure.Save();
                }
                else
                {
                    cboConfigure.SelectedIndex = -1;
                }
            }
            else
            {
                if (cboConfigure.SelectedIndex >= 0)
                {
                    btnSaveConfig.Enabled = btnPrint.Enabled = true;
                    Configure = _Configures[cboConfigure.SelectedIndex];
                    if (Configure.Template == null)
                        Configure.Decode();
                    if (!cboSchoolYear.Items.Contains(Configure.SchoolYear))
                        cboSchoolYear.Items.Add(Configure.SchoolYear);
                    cboSchoolYear.Text = Configure.SchoolYear;
                    cboSemester.Text = Configure.Semester;
                    if (Configure.ExamRecord != null)
                    {
                        foreach (var item in cboExam.Items)
                        {
                            if (((ExamRecord)item).ID == Configure.ExamRecord.ID)
                            {
                                cboExam.SelectedIndex = cboExam.Items.IndexOf(item);
                                break;
                            }
                        }
                    }
                    cboRefExam.SelectedIndex = -1;
                    if (Configure.RefenceExamRecord != null)
                    {
                        foreach (var item in cboRefExam.Items)
                        {
                            if (((ExamRecord)item).ID == Configure.RefenceExamRecord.ID)
                            {
                                cboRefExam.SelectedIndex = cboRefExam.Items.IndexOf(item);
                                break;
                            }
                        }
                    }
                    cboRankRilter.Text = Configure.RankFilterTagName;
                    foreach (ListViewItem item in listViewEx1.Items)
                    {
                        item.Checked = Configure.PrintSubjectList.Contains(item.Text);
                    }
                    cboTagRank1.Text = Configure.TagRank1TagName;
                    foreach (ListViewItem item in listViewEx2.Items)
                    {
                        item.Checked = Configure.TagRank1SubjectList.Contains(item.Text);
                    }
                    cboTagRank2.Text = Configure.TagRank2TagName;
                    foreach (ListViewItem item in listViewEx3.Items)
                    {
                        item.Checked = Configure.TagRank2SubjectList.Contains(item.Text);
                    }

                    // 開始與結束日期
                    DateTime dtb,dte;
                    if (DateTime.TryParse(Configure.BeginDate, out dtb))
                        dtBegin.Value = dtb;
                    else
                        dtBegin.Value = DateTime.Now;

                    if (DateTime.TryParse(Configure.EndDate, out dte))
                        dtEnd.Value = dte;
                    else
                        dtEnd.Value = DateTime.Now;

                    if (Configure.ScoreCurDate != null)
                        dtCurDate.Value = Configure.ScoreCurDate;
                    else
                        dtCurDate.Value = DateTime.Now;
                    if (Configure.AvgRd.HasValue)
                        iptRd.Value = Configure.AvgRd.Value;
                    else
                        iptRd.Value = 2;

                    // 判斷是否產生勾選學生清單
                    bool bo1,bo2;
                    if (bool.TryParse(Configure.isExportStudentList, out bo1))
                        ChkExportStudList.Checked = bo1;
                    else
                        ChkExportStudList.Checked = false;

                    // 判斷是否產生epost
                    if (bool.TryParse(Configure.isExportEPost, out bo2))
                        chkExportEpost.Checked = bo2;
                    else
                        chkExportEpost.Checked = false;
                    
                }
                else
                {
                    Configure = null;
                    cboSchoolYear.SelectedIndex = -1;
                    cboSemester.SelectedIndex = -1;
                    cboExam.SelectedIndex = -1;
                    cboRefExam.SelectedIndex = -1;
                    cboRankRilter.SelectedIndex = -1;
                    cboTagRank1.SelectedIndex = -1;
                    cboTagRank2.SelectedIndex = -1;
                    foreach (ListViewItem item in listViewEx1.Items)
                    {
                        item.Checked = false;
                    }
                    foreach (ListViewItem item in listViewEx2.Items)
                    {
                        item.Checked = false;
                    }
                    foreach (ListViewItem item in listViewEx3.Items)
                    {
                        item.Checked = false;
                    }
                    // 開始與結束日期沒有預設值時給當天
                    dtCurDate.Value= dtBegin.Value= dtEnd.Value = DateTime.Now;

                    if (Configure != null)
                    {
                        // 產生學生清單
                        ChkExportStudList.Checked = false;
                        Configure.isExportStudentList = ChkExportStudList.Checked.ToString();

                        // 產生ePost
                        chkExportEpost.Checked = false;
                        Configure.isExportEPost = chkExportEpost.Checked.ToString();
                    }
                }
            }
        }
        

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // 當沒有設定檔
            if (this.Configure == null) return;
            
            #region 儲存檔案
            string inputReportName = "常春藤個人定期評量成績單樣版(" + this.Configure.Name + ")";
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

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
                
                System.IO.FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                this.Configure.Template.Save(stream, Aspose.Words.SaveFormat.Doc);
                
                stream.Flush();
                stream.Close();



                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        //document.Save(sd.FileName, Aspose.Words.SaveFormat.Doc);
                        //System.IO.FileStream stream = new FileStream(sd.FileName, FileMode.Create, FileAccess.Write);
                        //stream.Write(Properties.Resources.個人學期成績單樣板_高中_, 0, Properties.Resources.個人學期成績單樣板_高中_.Length);
                        //stream.Flush();
                        //stream.Close();

                    }
                    catch
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            #endregion
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           
            if (Configure == null) return;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "上傳樣板";
            dialog.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    this.Configure.Template = new Aspose.Words.Document(dialog.FileName);
                    List<string> fields = new List<string>(this.Configure.Template.MailMerge.GetFieldNames());
                    this.Configure.SubjectLimit = 0;
                    while (fields.Contains("科目名稱" + (this.Configure.SubjectLimit + 1)))
                    {
                        this.Configure.SubjectLimit++;
                    }

                 
                    // 缺曠區間明細
                    this.Configure.AttendanceDetailLimit = 0;
                    while(fields.Contains("缺曠區間明細日期" + (this.Configure.AttendanceDetailLimit + 1)))
                    {
                        this.Configure.AttendanceDetailLimit++;
                    }

                    // 獎懲區間明細
                    this.Configure.DisciplineDetailLimit = 0;
                    while (fields.Contains("獎懲區間明細日期" + (this.Configure.DisciplineDetailLimit + 1)))
                    {
                        this.Configure.DisciplineDetailLimit++;
                    }

                    // 學習服務區間明細
                    this.Configure.ServiceLearningDetailLimit = 0;
                    while (fields.Contains("學習服務區間明細日期" + (this.Configure.ServiceLearningDetailLimit + 1)))
                    {
                        this.Configure.ServiceLearningDetailLimit++;
                    }
                }
                catch
                {
                    MessageBox.Show("樣板開啟失敗");
                }
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Configure == null) return;
            if (MessageBox.Show("樣板刪除後將無法回復，確定刪除樣板?", "刪除樣板", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                _Configures.Remove(Configure);
                if (Configure.UID != "")
                {
                    Configure.Deleted = true;
                    Configure.Save();
                }
                var conf = Configure;
                cboConfigure.SelectedIndex = -1;
                cboConfigure.Items.Remove(conf);
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Configure == null) return;
            CloneConfigure dialog = new CloneConfigure() { ParentName = Configure.Name };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Configure conf = new Configure();
                conf.Name = dialog.NewConfigureName;
                conf.ExamRecord = Configure.ExamRecord;
                conf.PrintSubjectList.AddRange(Configure.PrintSubjectList);
                conf.RankFilterTagList.AddRange(Configure.RankFilterTagList);
                conf.RankFilterTagName = Configure.RankFilterTagName;
                conf.RefenceExamRecord = Configure.RefenceExamRecord;
                conf.SchoolYear = Configure.SchoolYear;
                conf.Semester = Configure.Semester;
                conf.SubjectLimit = Configure.SubjectLimit;
                conf.ScoreCurDate = Configure.ScoreCurDate;
                conf.TagRank1SubjectList.AddRange(Configure.TagRank1SubjectList);
                conf.TagRank1TagList.AddRange(Configure.TagRank1TagList);
                conf.TagRank1TagName = Configure.TagRank1TagName;
                conf.TagRank2SubjectList.AddRange(Configure.TagRank2SubjectList);
                conf.TagRank2TagList.AddRange(Configure.TagRank2TagList);
                conf.TagRank2TagName = Configure.TagRank2TagName;
                conf.Template = Configure.Template;
                conf.Encode();
                conf.AvgRd = Configure.AvgRd;
                conf.Save();
                _Configures.Add(conf);
                cboConfigure.Items.Insert(cboConfigure.Items.Count - 1, conf);
                cboConfigure.SelectedIndex = cboConfigure.Items.Count - 2;
            }
        }

        private void SaveTemplate(object sender, EventArgs e)
        {
            if (Configure == null) return;
            Configure.SchoolYear = cboSchoolYear.Text;
            Configure.Semester = cboSemester.Text;
            if (dtCurDate.IsEmpty)
                Configure.ScoreCurDate = DateTime.Now;
            else
                Configure.ScoreCurDate = dtCurDate.Value;

            Configure.ExamRecord = ((ExamRecord)cboExam.SelectedItem);
            Configure.RefenceExamRecord = ((ExamRecord)cboRefExam.SelectedItem);
            if (Configure.RefenceExamRecord != null && Configure.RefenceExamRecord.Name == "")
                Configure.RefenceExamRecord = null;

            Configure.AvgRd = iptRd.Value;

            foreach (ListViewItem item in listViewEx1.Items)
            {
                if (item.Checked)
                {
                    if (!Configure.PrintSubjectList.Contains(item.Text))
                        Configure.PrintSubjectList.Add(item.Text);
                }
                else
                {
                    if (Configure.PrintSubjectList.Contains(item.Text))
                        Configure.PrintSubjectList.Remove(item.Text);
                }
            }
            Configure.TagRank1TagName = cboTagRank1.Text;
            Configure.TagRank1TagList.Clear();
            foreach (var item in _TagConfigRecords)
            {
                if (item.Prefix != "")
                {
                    if (cboTagRank1.Text == "[" + item.Prefix + "]")
                        Configure.TagRank1TagList.Add(item.ID);
                }
                else
                {
                    if (cboTagRank1.Text == item.Name)
                        Configure.TagRank1TagList.Add(item.ID);
                }
            }
            foreach (ListViewItem item in listViewEx2.Items)
            {
                if (item.Checked)
                {
                    if (!Configure.TagRank1SubjectList.Contains(item.Text))
                        Configure.TagRank1SubjectList.Add(item.Text);
                }
                else
                {
                    if (Configure.TagRank1SubjectList.Contains(item.Text))
                        Configure.TagRank1SubjectList.Remove(item.Text);
                }
            }

            Configure.TagRank2TagName = cboTagRank2.Text;
            Configure.TagRank2TagList.Clear();
            foreach (var item in _TagConfigRecords)
            {
                if (item.Prefix != "")
                {
                    if (cboTagRank2.Text == "[" + item.Prefix + "]")
                        Configure.TagRank2TagList.Add(item.ID);
                }
                else
                {
                    if (cboTagRank2.Text == item.Name)
                        Configure.TagRank2TagList.Add(item.ID);
                }
            }
            foreach (ListViewItem item in listViewEx3.Items)
            {
                if (item.Checked)
                {
                    if (!Configure.TagRank2SubjectList.Contains(item.Text))
                        Configure.TagRank2SubjectList.Add(item.Text);
                }
                else
                {
                    if (Configure.TagRank2SubjectList.Contains(item.Text))
                        Configure.TagRank2SubjectList.Remove(item.Text);
                }
            }

            Configure.RankFilterTagName = cboRankRilter.Text;
            Configure.RankFilterTagList.Clear();
            foreach (var item in _TagConfigRecords)
            {
                if (item.Prefix != "")
                {
                    if (cboRankRilter.Text == "[" + item.Prefix + "]")
                        Configure.RankFilterTagList.Add(item.ID);
                }
                else
                {
                    if (cboRankRilter.Text == item.Name)
                        Configure.RankFilterTagList.Add(item.ID);
                }
            }
           
            // 儲存開始與結束日期
            Configure.BeginDate = dtBegin.Value.ToShortDateString();
            Configure.EndDate = dtEnd.Value.ToShortDateString();

            // 是否產生學生清單
            Configure.isExportStudentList = ChkExportStudList.Checked.ToString();

            // 是否產生epost
            Configure.isExportEPost = chkExportEpost.Checked.ToString();

            Configure.Encode();
            Configure.Save();           
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            #region 儲存檔案
            string inputReportName = "個人學期成績單合併欄位總表";
            string reportName = inputReportName;

            string path = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".doc");

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
                //document.Save(path, Aspose.Words.SaveFormat.Doc);
                System.IO.FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                stream.Write(Properties.Resources.歡樂的合併欄位總表, 0, Properties.Resources.歡樂的合併欄位總表.Length);
                stream.Flush();
                stream.Close();
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".doc";
                sd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        //document.Save(sd.FileName, Aspose.Words.SaveFormat.Doc);
                        System.IO.FileStream stream = new FileStream(sd.FileName, FileMode.Create, FileAccess.Write);
                        stream.Write(Properties.Resources.歡樂的合併欄位總表, 0, Properties.Resources.歡樂的合併欄位總表.Length);
                        stream.Flush();
                        stream.Close();

                    }
                    catch
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            #endregion
        }

        private void ChkExportStudList_CheckedChanged(object sender, EventArgs e)
        {

        }
               

        private void lbd01_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
                try
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(new MemoryStream(Properties.Resources.個人學期成績單樣板));
                    Configure.Template = doc;                    
                    List<string> fields = new List<string>(this.Configure.Template.MailMerge.GetFieldNames());
                    this.Configure.SubjectLimit = 0;
                    while (fields.Contains("科目名稱" + (this.Configure.SubjectLimit + 1)))
                    {
                        this.Configure.SubjectLimit++;
                    }
                }
                catch
                {
                    MessageBox.Show("樣板開啟失敗");
                }
           
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {

        }
    }
}
