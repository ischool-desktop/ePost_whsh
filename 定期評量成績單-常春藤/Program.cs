using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using System.IO;
//using K12.Data;
//using SHSchool.Data;
using SmartSchool.Customization.Data;
using System.Threading;
using SmartSchool.Customization.Data.StudentExtension;
using FISCA.Data;
using FISCA.Permission;
using SmartSchool;

namespace SH_jvyjhs_StudentExamScore_epost
{
     public class Program
     {

         public static int AvgRd = 2;
          [FISCA.MainMethod]
          public static void Main()
          {
              var btn = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["報表"]["成績相關報表"]["定期評量成績單-ePost(常春藤)"];
               btn.Enable = false;
               K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
               {
                    btn.Enable = Permissions.SH_jvyjhs_StudentExamScore_epost權限 && (K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0);
               };
               btn.Click += new EventHandler(Program_Click);

               //權限設定
               Catalog permission = RoleAclSource.Instance["學生"]["功能按鈕"];
               permission.Add(new RibbonFeature(Permissions.SH_jvyjhs_StudentExamScore_epost, "定期評量成績單-ePost(常春藤)"));
          }

          // 學生清單暫存
          private static Aspose.Cells.Workbook _wbStudentList;

          static DataTable _dtEpost = new DataTable();

          private static string GetNumber(decimal? p)
          {
               if (p == null) return "";
               string levelNumber;
               switch (((int)p.Value))
               {
                    #region 對應levelNumber
                    case 0:
                         levelNumber = "";
                         break;
                    case 1:
                         levelNumber = "Ⅰ";
                         break;
                    case 2:
                         levelNumber = "Ⅱ";
                         break;
                    case 3:
                         levelNumber = "Ⅲ";
                         break;
                    case 4:
                         levelNumber = "Ⅳ";
                         break;
                    case 5:
                         levelNumber = "Ⅴ";
                         break;
                    case 6:
                         levelNumber = "Ⅵ";
                         break;
                    case 7:
                         levelNumber = "Ⅶ";
                         break;
                    case 8:
                         levelNumber = "Ⅷ";
                         break;
                    case 9:
                         levelNumber = "Ⅸ";
                         break;
                    case 10:
                         levelNumber = "Ⅹ";
                         break;
                    default:
                         levelNumber = "" + (p);
                         break;
                    #endregion
               }
               return levelNumber;
          }

          static void Program_Click(object sender_, EventArgs e_)
          {
               ConfigForm form = new ConfigForm();
               if (form.ShowDialog() == DialogResult.OK)
               {
                    AccessHelper accessHelper = new AccessHelper();
                    //return;
                    List<StudentRecord> overflowRecords = new List<StudentRecord>();
                    //取得列印設定
                    Configure conf = form.Configure;
                    //建立測試的選取學生(先期不管怎麼選就是印這些人)
                    List<string> selectedStudents = K12.Presentation.NLDPanels.Student.SelectedSource;
                    //建立合併欄位總表
                    DataTable table = new DataTable();
                    #region 所有的合併欄位
                    table.Columns.Add("學生系統編號");
                    table.Columns.Add("學生班級年級");
                    table.Columns.Add("學校名稱");
                    table.Columns.Add("學校地址");
                    table.Columns.Add("學校電話");
                    table.Columns.Add("收件人地址");
                    //«通訊地址»«通訊地址郵遞區號»«通訊地址內容»
                    //«戶籍地址»«戶籍地址郵遞區號»«戶籍地址內容»
                    //«監護人»«父親»«母親»«科別名稱»
                    table.Columns.Add("通訊地址");
                    table.Columns.Add("通訊地址郵遞區號");
                    table.Columns.Add("通訊地址內容");
                    table.Columns.Add("戶籍地址");
                    table.Columns.Add("戶籍地址郵遞區號");
                    table.Columns.Add("戶籍地址內容");
                    table.Columns.Add("監護人");
                    table.Columns.Add("父親");
                    table.Columns.Add("母親");
                    table.Columns.Add("科別名稱");
                    table.Columns.Add("試別");
                    table.Columns.Add("家長代碼");

                    table.Columns.Add("收件人");
                    table.Columns.Add("學年度");
                    table.Columns.Add("學期");
                    table.Columns.Add("班級科別名稱");
                    table.Columns.Add("班級");
                    table.Columns.Add("班導師");
                    table.Columns.Add("座號");
                    table.Columns.Add("學號");
                    table.Columns.Add("姓名");
                    table.Columns.Add("定期評量");
                    for (int subjectIndex = 1; subjectIndex <= conf.SubjectLimit; subjectIndex++)
                    {
                         table.Columns.Add("科目名稱" + subjectIndex);
                         table.Columns.Add("學分數" + subjectIndex);
                         table.Columns.Add("前次成績" + subjectIndex);
                         table.Columns.Add("科目成績" + subjectIndex);
                         table.Columns.Add("班排名" + subjectIndex);
                         table.Columns.Add("班排名母數" + subjectIndex);
                         table.Columns.Add("科排名" + subjectIndex);
                         table.Columns.Add("科排名母數" + subjectIndex);
                         table.Columns.Add("類別1排名" + subjectIndex);
                         table.Columns.Add("類別1排名母數" + subjectIndex);
                         table.Columns.Add("類別2排名" + subjectIndex);
                         table.Columns.Add("類別2排名母數" + subjectIndex);
                         table.Columns.Add("全校排名" + subjectIndex);
                         table.Columns.Add("全校排名母數" + subjectIndex);
                         #region 瘋狂的組距及分析
                         table.Columns.Add("班高標" + subjectIndex); table.Columns.Add("科高標" + subjectIndex); table.Columns.Add("校高標" + subjectIndex); table.Columns.Add("類1高標" + subjectIndex); table.Columns.Add("類2高標" + subjectIndex);
                         table.Columns.Add("班均標" + subjectIndex); table.Columns.Add("科均標" + subjectIndex); table.Columns.Add("校均標" + subjectIndex); table.Columns.Add("類1均標" + subjectIndex); table.Columns.Add("類2均標" + subjectIndex);
                         table.Columns.Add("班低標" + subjectIndex); table.Columns.Add("科低標" + subjectIndex); table.Columns.Add("校低標" + subjectIndex); table.Columns.Add("類1低標" + subjectIndex); table.Columns.Add("類2低標" + subjectIndex);
                         table.Columns.Add("班標準差" + subjectIndex); table.Columns.Add("科標準差" + subjectIndex); table.Columns.Add("校標準差" + subjectIndex); table.Columns.Add("類1標準差" + subjectIndex); table.Columns.Add("類2標準差" + subjectIndex);
                         table.Columns.Add("班組距" + subjectIndex + "count90"); table.Columns.Add("科組距" + subjectIndex + "count90"); table.Columns.Add("校組距" + subjectIndex + "count90"); table.Columns.Add("類1組距" + subjectIndex + "count90"); table.Columns.Add("類2組距" + subjectIndex + "count90");
                         table.Columns.Add("班組距" + subjectIndex + "count80"); table.Columns.Add("科組距" + subjectIndex + "count80"); table.Columns.Add("校組距" + subjectIndex + "count80"); table.Columns.Add("類1組距" + subjectIndex + "count80"); table.Columns.Add("類2組距" + subjectIndex + "count80");
                         table.Columns.Add("班組距" + subjectIndex + "count70"); table.Columns.Add("科組距" + subjectIndex + "count70"); table.Columns.Add("校組距" + subjectIndex + "count70"); table.Columns.Add("類1組距" + subjectIndex + "count70"); table.Columns.Add("類2組距" + subjectIndex + "count70");
                         table.Columns.Add("班組距" + subjectIndex + "count60"); table.Columns.Add("科組距" + subjectIndex + "count60"); table.Columns.Add("校組距" + subjectIndex + "count60"); table.Columns.Add("類1組距" + subjectIndex + "count60"); table.Columns.Add("類2組距" + subjectIndex + "count60");
                         table.Columns.Add("班組距" + subjectIndex + "count50"); table.Columns.Add("科組距" + subjectIndex + "count50"); table.Columns.Add("校組距" + subjectIndex + "count50"); table.Columns.Add("類1組距" + subjectIndex + "count50"); table.Columns.Add("類2組距" + subjectIndex + "count50");
                         table.Columns.Add("班組距" + subjectIndex + "count40"); table.Columns.Add("科組距" + subjectIndex + "count40"); table.Columns.Add("校組距" + subjectIndex + "count40"); table.Columns.Add("類1組距" + subjectIndex + "count40"); table.Columns.Add("類2組距" + subjectIndex + "count40");
                         table.Columns.Add("班組距" + subjectIndex + "count30"); table.Columns.Add("科組距" + subjectIndex + "count30"); table.Columns.Add("校組距" + subjectIndex + "count30"); table.Columns.Add("類1組距" + subjectIndex + "count30"); table.Columns.Add("類2組距" + subjectIndex + "count30");
                         table.Columns.Add("班組距" + subjectIndex + "count20"); table.Columns.Add("科組距" + subjectIndex + "count20"); table.Columns.Add("校組距" + subjectIndex + "count20"); table.Columns.Add("類1組距" + subjectIndex + "count20"); table.Columns.Add("類2組距" + subjectIndex + "count20");
                         table.Columns.Add("班組距" + subjectIndex + "count10"); table.Columns.Add("科組距" + subjectIndex + "count10"); table.Columns.Add("校組距" + subjectIndex + "count10"); table.Columns.Add("類1組距" + subjectIndex + "count10"); table.Columns.Add("類2組距" + subjectIndex + "count10");
                         table.Columns.Add("班組距" + subjectIndex + "count100Up"); table.Columns.Add("科組距" + subjectIndex + "count100Up"); table.Columns.Add("校組距" + subjectIndex + "count100Up"); table.Columns.Add("類1組距" + subjectIndex + "count100Up"); table.Columns.Add("類2組距" + subjectIndex + "count100Up");
                         table.Columns.Add("班組距" + subjectIndex + "count90Up"); table.Columns.Add("科組距" + subjectIndex + "count90Up"); table.Columns.Add("校組距" + subjectIndex + "count90Up"); table.Columns.Add("類1組距" + subjectIndex + "count90Up"); table.Columns.Add("類2組距" + subjectIndex + "count90Up");
                         table.Columns.Add("班組距" + subjectIndex + "count80Up"); table.Columns.Add("科組距" + subjectIndex + "count80Up"); table.Columns.Add("校組距" + subjectIndex + "count80Up"); table.Columns.Add("類1組距" + subjectIndex + "count80Up"); table.Columns.Add("類2組距" + subjectIndex + "count80Up");
                         table.Columns.Add("班組距" + subjectIndex + "count70Up"); table.Columns.Add("科組距" + subjectIndex + "count70Up"); table.Columns.Add("校組距" + subjectIndex + "count70Up"); table.Columns.Add("類1組距" + subjectIndex + "count70Up"); table.Columns.Add("類2組距" + subjectIndex + "count70Up");
                         table.Columns.Add("班組距" + subjectIndex + "count60Up"); table.Columns.Add("科組距" + subjectIndex + "count60Up"); table.Columns.Add("校組距" + subjectIndex + "count60Up"); table.Columns.Add("類1組距" + subjectIndex + "count60Up"); table.Columns.Add("類2組距" + subjectIndex + "count60Up");
                         table.Columns.Add("班組距" + subjectIndex + "count50Up"); table.Columns.Add("科組距" + subjectIndex + "count50Up"); table.Columns.Add("校組距" + subjectIndex + "count50Up"); table.Columns.Add("類1組距" + subjectIndex + "count50Up"); table.Columns.Add("類2組距" + subjectIndex + "count50Up");
                         table.Columns.Add("班組距" + subjectIndex + "count40Up"); table.Columns.Add("科組距" + subjectIndex + "count40Up"); table.Columns.Add("校組距" + subjectIndex + "count40Up"); table.Columns.Add("類1組距" + subjectIndex + "count40Up"); table.Columns.Add("類2組距" + subjectIndex + "count40Up");
                         table.Columns.Add("班組距" + subjectIndex + "count30Up"); table.Columns.Add("科組距" + subjectIndex + "count30Up"); table.Columns.Add("校組距" + subjectIndex + "count30Up"); table.Columns.Add("類1組距" + subjectIndex + "count30Up"); table.Columns.Add("類2組距" + subjectIndex + "count30Up");
                         table.Columns.Add("班組距" + subjectIndex + "count20Up"); table.Columns.Add("科組距" + subjectIndex + "count20Up"); table.Columns.Add("校組距" + subjectIndex + "count20Up"); table.Columns.Add("類1組距" + subjectIndex + "count20Up"); table.Columns.Add("類2組距" + subjectIndex + "count20Up");
                         table.Columns.Add("班組距" + subjectIndex + "count10Up"); table.Columns.Add("科組距" + subjectIndex + "count10Up"); table.Columns.Add("校組距" + subjectIndex + "count10Up"); table.Columns.Add("類1組距" + subjectIndex + "count10Up"); table.Columns.Add("類2組距" + subjectIndex + "count10Up");
                         table.Columns.Add("班組距" + subjectIndex + "count90Down"); table.Columns.Add("科組距" + subjectIndex + "count90Down"); table.Columns.Add("校組距" + subjectIndex + "count90Down"); table.Columns.Add("類1組距" + subjectIndex + "count90Down"); table.Columns.Add("類2組距" + subjectIndex + "count90Down");
                         table.Columns.Add("班組距" + subjectIndex + "count80Down"); table.Columns.Add("科組距" + subjectIndex + "count80Down"); table.Columns.Add("校組距" + subjectIndex + "count80Down"); table.Columns.Add("類1組距" + subjectIndex + "count80Down"); table.Columns.Add("類2組距" + subjectIndex + "count80Down");
                         table.Columns.Add("班組距" + subjectIndex + "count70Down"); table.Columns.Add("科組距" + subjectIndex + "count70Down"); table.Columns.Add("校組距" + subjectIndex + "count70Down"); table.Columns.Add("類1組距" + subjectIndex + "count70Down"); table.Columns.Add("類2組距" + subjectIndex + "count70Down");
                         table.Columns.Add("班組距" + subjectIndex + "count60Down"); table.Columns.Add("科組距" + subjectIndex + "count60Down"); table.Columns.Add("校組距" + subjectIndex + "count60Down"); table.Columns.Add("類1組距" + subjectIndex + "count60Down"); table.Columns.Add("類2組距" + subjectIndex + "count60Down");
                         table.Columns.Add("班組距" + subjectIndex + "count50Down"); table.Columns.Add("科組距" + subjectIndex + "count50Down"); table.Columns.Add("校組距" + subjectIndex + "count50Down"); table.Columns.Add("類1組距" + subjectIndex + "count50Down"); table.Columns.Add("類2組距" + subjectIndex + "count50Down");
                         table.Columns.Add("班組距" + subjectIndex + "count40Down"); table.Columns.Add("科組距" + subjectIndex + "count40Down"); table.Columns.Add("校組距" + subjectIndex + "count40Down"); table.Columns.Add("類1組距" + subjectIndex + "count40Down"); table.Columns.Add("類2組距" + subjectIndex + "count40Down");
                         table.Columns.Add("班組距" + subjectIndex + "count30Down"); table.Columns.Add("科組距" + subjectIndex + "count30Down"); table.Columns.Add("校組距" + subjectIndex + "count30Down"); table.Columns.Add("類1組距" + subjectIndex + "count30Down"); table.Columns.Add("類2組距" + subjectIndex + "count30Down");
                         table.Columns.Add("班組距" + subjectIndex + "count20Down"); table.Columns.Add("科組距" + subjectIndex + "count20Down"); table.Columns.Add("校組距" + subjectIndex + "count20Down"); table.Columns.Add("類1組距" + subjectIndex + "count20Down"); table.Columns.Add("類2組距" + subjectIndex + "count20Down");
                         table.Columns.Add("班組距" + subjectIndex + "count10Down"); table.Columns.Add("科組距" + subjectIndex + "count10Down"); table.Columns.Add("校組距" + subjectIndex + "count10Down"); table.Columns.Add("類1組距" + subjectIndex + "count10Down"); table.Columns.Add("類2組距" + subjectIndex + "count10Down");
                         #endregion
                    }
                    table.Columns.Add("總分");
                    table.Columns.Add("總分班排名");
                    table.Columns.Add("總分班排名母數");
                    table.Columns.Add("總分科排名");
                    table.Columns.Add("總分科排名母數");
                    table.Columns.Add("總分全校排名");
                    table.Columns.Add("總分全校排名母數");
                    table.Columns.Add("平均");
                    table.Columns.Add("平均班排名");
                    table.Columns.Add("平均班排名母數");
                    table.Columns.Add("平均科排名");
                    table.Columns.Add("平均科排名母數");
                    table.Columns.Add("平均全校排名");
                    table.Columns.Add("平均全校排名母數");
                    table.Columns.Add("加權總分");
                    table.Columns.Add("加權總分班排名");
                    table.Columns.Add("加權總分班排名母數");
                    table.Columns.Add("加權總分科排名");
                    table.Columns.Add("加權總分科排名母數");
                    table.Columns.Add("加權總分全校排名");
                    table.Columns.Add("加權總分全校排名母數");
                    table.Columns.Add("加權平均");
                    table.Columns.Add("加權平均班排名");
                    table.Columns.Add("加權平均班排名母數");
                    table.Columns.Add("加權平均科排名");
                    table.Columns.Add("加權平均科排名母數");
                    table.Columns.Add("加權平均全校排名");
                    table.Columns.Add("加權平均全校排名母數");

                    table.Columns.Add("類別排名1");
                    table.Columns.Add("類別1總分");
                    table.Columns.Add("類別1總分排名");
                    table.Columns.Add("類別1總分排名母數");
                    table.Columns.Add("類別1平均");
                    table.Columns.Add("類別1平均排名");
                    table.Columns.Add("類別1平均排名母數");
                    table.Columns.Add("類別1加權總分");
                    table.Columns.Add("類別1加權總分排名");
                    table.Columns.Add("類別1加權總分排名母數");
                    table.Columns.Add("類別1加權平均");
                    table.Columns.Add("類別1加權平均排名");
                    table.Columns.Add("類別1加權平均排名母數");

                    table.Columns.Add("類別排名2");
                    table.Columns.Add("類別2總分");
                    table.Columns.Add("類別2總分排名");
                    table.Columns.Add("類別2總分排名母數");
                    table.Columns.Add("類別2平均");
                    table.Columns.Add("類別2平均排名");
                    table.Columns.Add("類別2平均排名母數");
                    table.Columns.Add("類別2加權總分");
                    table.Columns.Add("類別2加權總分排名");
                    table.Columns.Add("類別2加權總分排名母數");
                    table.Columns.Add("類別2加權平均");
                    table.Columns.Add("類別2加權平均排名");
                    table.Columns.Add("類別2加權平均排名母數");
                    // 獎懲統計 --
                    table.Columns.Add("大功統計");
                    table.Columns.Add("小功統計");
                    table.Columns.Add("嘉獎統計");
                    table.Columns.Add("大過統計");
                    table.Columns.Add("小過統計");
                    table.Columns.Add("警告統計");
                    table.Columns.Add("留校察看");

                    table.Columns.Add("成績校正年");
                    table.Columns.Add("成績校正月");
                    table.Columns.Add("成績校正日");
                    table.Columns.Add("成績校正民國年");
                    table.Columns.Add("成績校正日期");

                    #region 瘋狂的組距及分析
                    table.Columns.Add("總分班高標"); table.Columns.Add("總分科高標"); table.Columns.Add("總分校高標"); table.Columns.Add("平均班高標"); table.Columns.Add("平均科高標"); table.Columns.Add("平均校高標"); table.Columns.Add("加權總分班高標"); table.Columns.Add("加權總分科高標"); table.Columns.Add("加權總分校高標"); table.Columns.Add("加權平均班高標"); table.Columns.Add("加權平均科高標"); table.Columns.Add("加權平均校高標"); table.Columns.Add("類1總分高標"); table.Columns.Add("類1平均高標"); table.Columns.Add("類1加權總分高標"); table.Columns.Add("類1加權平均高標"); table.Columns.Add("類2總分高標"); table.Columns.Add("類2平均高標"); table.Columns.Add("類2加權總分高標"); table.Columns.Add("類2加權平均高標");
                    table.Columns.Add("總分班均標"); table.Columns.Add("總分科均標"); table.Columns.Add("總分校均標"); table.Columns.Add("平均班均標"); table.Columns.Add("平均科均標"); table.Columns.Add("平均校均標"); table.Columns.Add("加權總分班均標"); table.Columns.Add("加權總分科均標"); table.Columns.Add("加權總分校均標"); table.Columns.Add("加權平均班均標"); table.Columns.Add("加權平均科均標"); table.Columns.Add("加權平均校均標"); table.Columns.Add("類1總分均標"); table.Columns.Add("類1平均均標"); table.Columns.Add("類1加權總分均標"); table.Columns.Add("類1加權平均均標"); table.Columns.Add("類2總分均標"); table.Columns.Add("類2平均均標"); table.Columns.Add("類2加權總分均標"); table.Columns.Add("類2加權平均均標");
                    table.Columns.Add("總分班低標"); table.Columns.Add("總分科低標"); table.Columns.Add("總分校低標"); table.Columns.Add("平均班低標"); table.Columns.Add("平均科低標"); table.Columns.Add("平均校低標"); table.Columns.Add("加權總分班低標"); table.Columns.Add("加權總分科低標"); table.Columns.Add("加權總分校低標"); table.Columns.Add("加權平均班低標"); table.Columns.Add("加權平均科低標"); table.Columns.Add("加權平均校低標"); table.Columns.Add("類1總分低標"); table.Columns.Add("類1平均低標"); table.Columns.Add("類1加權總分低標"); table.Columns.Add("類1加權平均低標"); table.Columns.Add("類2總分低標"); table.Columns.Add("類2平均低標"); table.Columns.Add("類2加權總分低標"); table.Columns.Add("類2加權平均低標");
                    table.Columns.Add("總分班標準差"); table.Columns.Add("總分科標準差"); table.Columns.Add("總分校標準差"); table.Columns.Add("平均班標準差"); table.Columns.Add("平均科標準差"); table.Columns.Add("平均校標準差"); table.Columns.Add("加權總分班標準差"); table.Columns.Add("加權總分科標準差"); table.Columns.Add("加權總分校標準差"); table.Columns.Add("加權平均班標準差"); table.Columns.Add("加權平均科標準差"); table.Columns.Add("加權平均校標準差"); table.Columns.Add("類1總分標準差"); table.Columns.Add("類1平均標準差"); table.Columns.Add("類1加權總分標準差"); table.Columns.Add("類1加權平均標準差"); table.Columns.Add("類2總分標準差"); table.Columns.Add("類2平均標準差"); table.Columns.Add("類2加權總分標準差"); table.Columns.Add("類2加權平均標準差");
                    table.Columns.Add("總分班組距count90"); table.Columns.Add("總分科組距count90"); table.Columns.Add("總分校組距count90"); table.Columns.Add("平均班組距count90"); table.Columns.Add("平均科組距count90"); table.Columns.Add("平均校組距count90"); table.Columns.Add("加權總分班組距count90"); table.Columns.Add("加權總分科組距count90"); table.Columns.Add("加權總分校組距count90"); table.Columns.Add("加權平均班組距count90"); table.Columns.Add("加權平均科組距count90"); table.Columns.Add("加權平均校組距count90"); table.Columns.Add("類1總分組距count90"); table.Columns.Add("類1平均組距count90"); table.Columns.Add("類1加權總分組距count90"); table.Columns.Add("類1加權平均組距count90"); table.Columns.Add("類2總分組距count90"); table.Columns.Add("類2平均組距count90"); table.Columns.Add("類2加權總分組距count90"); table.Columns.Add("類2加權平均組距count90");
                    table.Columns.Add("總分班組距count80"); table.Columns.Add("總分科組距count80"); table.Columns.Add("總分校組距count80"); table.Columns.Add("平均班組距count80"); table.Columns.Add("平均科組距count80"); table.Columns.Add("平均校組距count80"); table.Columns.Add("加權總分班組距count80"); table.Columns.Add("加權總分科組距count80"); table.Columns.Add("加權總分校組距count80"); table.Columns.Add("加權平均班組距count80"); table.Columns.Add("加權平均科組距count80"); table.Columns.Add("加權平均校組距count80"); table.Columns.Add("類1總分組距count80"); table.Columns.Add("類1平均組距count80"); table.Columns.Add("類1加權總分組距count80"); table.Columns.Add("類1加權平均組距count80"); table.Columns.Add("類2總分組距count80"); table.Columns.Add("類2平均組距count80"); table.Columns.Add("類2加權總分組距count80"); table.Columns.Add("類2加權平均組距count80");
                    table.Columns.Add("總分班組距count70"); table.Columns.Add("總分科組距count70"); table.Columns.Add("總分校組距count70"); table.Columns.Add("平均班組距count70"); table.Columns.Add("平均科組距count70"); table.Columns.Add("平均校組距count70"); table.Columns.Add("加權總分班組距count70"); table.Columns.Add("加權總分科組距count70"); table.Columns.Add("加權總分校組距count70"); table.Columns.Add("加權平均班組距count70"); table.Columns.Add("加權平均科組距count70"); table.Columns.Add("加權平均校組距count70"); table.Columns.Add("類1總分組距count70"); table.Columns.Add("類1平均組距count70"); table.Columns.Add("類1加權總分組距count70"); table.Columns.Add("類1加權平均組距count70"); table.Columns.Add("類2總分組距count70"); table.Columns.Add("類2平均組距count70"); table.Columns.Add("類2加權總分組距count70"); table.Columns.Add("類2加權平均組距count70");
                    table.Columns.Add("總分班組距count60"); table.Columns.Add("總分科組距count60"); table.Columns.Add("總分校組距count60"); table.Columns.Add("平均班組距count60"); table.Columns.Add("平均科組距count60"); table.Columns.Add("平均校組距count60"); table.Columns.Add("加權總分班組距count60"); table.Columns.Add("加權總分科組距count60"); table.Columns.Add("加權總分校組距count60"); table.Columns.Add("加權平均班組距count60"); table.Columns.Add("加權平均科組距count60"); table.Columns.Add("加權平均校組距count60"); table.Columns.Add("類1總分組距count60"); table.Columns.Add("類1平均組距count60"); table.Columns.Add("類1加權總分組距count60"); table.Columns.Add("類1加權平均組距count60"); table.Columns.Add("類2總分組距count60"); table.Columns.Add("類2平均組距count60"); table.Columns.Add("類2加權總分組距count60"); table.Columns.Add("類2加權平均組距count60");
                    table.Columns.Add("總分班組距count50"); table.Columns.Add("總分科組距count50"); table.Columns.Add("總分校組距count50"); table.Columns.Add("平均班組距count50"); table.Columns.Add("平均科組距count50"); table.Columns.Add("平均校組距count50"); table.Columns.Add("加權總分班組距count50"); table.Columns.Add("加權總分科組距count50"); table.Columns.Add("加權總分校組距count50"); table.Columns.Add("加權平均班組距count50"); table.Columns.Add("加權平均科組距count50"); table.Columns.Add("加權平均校組距count50"); table.Columns.Add("類1總分組距count50"); table.Columns.Add("類1平均組距count50"); table.Columns.Add("類1加權總分組距count50"); table.Columns.Add("類1加權平均組距count50"); table.Columns.Add("類2總分組距count50"); table.Columns.Add("類2平均組距count50"); table.Columns.Add("類2加權總分組距count50"); table.Columns.Add("類2加權平均組距count50");
                    table.Columns.Add("總分班組距count40"); table.Columns.Add("總分科組距count40"); table.Columns.Add("總分校組距count40"); table.Columns.Add("平均班組距count40"); table.Columns.Add("平均科組距count40"); table.Columns.Add("平均校組距count40"); table.Columns.Add("加權總分班組距count40"); table.Columns.Add("加權總分科組距count40"); table.Columns.Add("加權總分校組距count40"); table.Columns.Add("加權平均班組距count40"); table.Columns.Add("加權平均科組距count40"); table.Columns.Add("加權平均校組距count40"); table.Columns.Add("類1總分組距count40"); table.Columns.Add("類1平均組距count40"); table.Columns.Add("類1加權總分組距count40"); table.Columns.Add("類1加權平均組距count40"); table.Columns.Add("類2總分組距count40"); table.Columns.Add("類2平均組距count40"); table.Columns.Add("類2加權總分組距count40"); table.Columns.Add("類2加權平均組距count40");
                    table.Columns.Add("總分班組距count30"); table.Columns.Add("總分科組距count30"); table.Columns.Add("總分校組距count30"); table.Columns.Add("平均班組距count30"); table.Columns.Add("平均科組距count30"); table.Columns.Add("平均校組距count30"); table.Columns.Add("加權總分班組距count30"); table.Columns.Add("加權總分科組距count30"); table.Columns.Add("加權總分校組距count30"); table.Columns.Add("加權平均班組距count30"); table.Columns.Add("加權平均科組距count30"); table.Columns.Add("加權平均校組距count30"); table.Columns.Add("類1總分組距count30"); table.Columns.Add("類1平均組距count30"); table.Columns.Add("類1加權總分組距count30"); table.Columns.Add("類1加權平均組距count30"); table.Columns.Add("類2總分組距count30"); table.Columns.Add("類2平均組距count30"); table.Columns.Add("類2加權總分組距count30"); table.Columns.Add("類2加權平均組距count30");
                    table.Columns.Add("總分班組距count20"); table.Columns.Add("總分科組距count20"); table.Columns.Add("總分校組距count20"); table.Columns.Add("平均班組距count20"); table.Columns.Add("平均科組距count20"); table.Columns.Add("平均校組距count20"); table.Columns.Add("加權總分班組距count20"); table.Columns.Add("加權總分科組距count20"); table.Columns.Add("加權總分校組距count20"); table.Columns.Add("加權平均班組距count20"); table.Columns.Add("加權平均科組距count20"); table.Columns.Add("加權平均校組距count20"); table.Columns.Add("類1總分組距count20"); table.Columns.Add("類1平均組距count20"); table.Columns.Add("類1加權總分組距count20"); table.Columns.Add("類1加權平均組距count20"); table.Columns.Add("類2總分組距count20"); table.Columns.Add("類2平均組距count20"); table.Columns.Add("類2加權總分組距count20"); table.Columns.Add("類2加權平均組距count20");
                    table.Columns.Add("總分班組距count10"); table.Columns.Add("總分科組距count10"); table.Columns.Add("總分校組距count10"); table.Columns.Add("平均班組距count10"); table.Columns.Add("平均科組距count10"); table.Columns.Add("平均校組距count10"); table.Columns.Add("加權總分班組距count10"); table.Columns.Add("加權總分科組距count10"); table.Columns.Add("加權總分校組距count10"); table.Columns.Add("加權平均班組距count10"); table.Columns.Add("加權平均科組距count10"); table.Columns.Add("加權平均校組距count10"); table.Columns.Add("類1總分組距count10"); table.Columns.Add("類1平均組距count10"); table.Columns.Add("類1加權總分組距count10"); table.Columns.Add("類1加權平均組距count10"); table.Columns.Add("類2總分組距count10"); table.Columns.Add("類2平均組距count10"); table.Columns.Add("類2加權總分組距count10"); table.Columns.Add("類2加權平均組距count10");
                    table.Columns.Add("總分班組距count100Up"); table.Columns.Add("總分科組距count100Up"); table.Columns.Add("總分校組距count100Up"); table.Columns.Add("平均班組距count100Up"); table.Columns.Add("平均科組距count100Up"); table.Columns.Add("平均校組距count100Up"); table.Columns.Add("加權總分班組距count100Up"); table.Columns.Add("加權總分科組距count100Up"); table.Columns.Add("加權總分校組距count100Up"); table.Columns.Add("加權平均班組距count100Up"); table.Columns.Add("加權平均科組距count100Up"); table.Columns.Add("加權平均校組距count100Up"); table.Columns.Add("類1總分組距count100Up"); table.Columns.Add("類1平均組距count100Up"); table.Columns.Add("類1加權總分組距count100Up"); table.Columns.Add("類1加權平均組距count100Up"); table.Columns.Add("類2總分組距count100Up"); table.Columns.Add("類2平均組距count100Up"); table.Columns.Add("類2加權總分組距count100Up"); table.Columns.Add("類2加權平均組距count100Up");
                    table.Columns.Add("總分班組距count90Up"); table.Columns.Add("總分科組距count90Up"); table.Columns.Add("總分校組距count90Up"); table.Columns.Add("平均班組距count90Up"); table.Columns.Add("平均科組距count90Up"); table.Columns.Add("平均校組距count90Up"); table.Columns.Add("加權總分班組距count90Up"); table.Columns.Add("加權總分科組距count90Up"); table.Columns.Add("加權總分校組距count90Up"); table.Columns.Add("加權平均班組距count90Up"); table.Columns.Add("加權平均科組距count90Up"); table.Columns.Add("加權平均校組距count90Up"); table.Columns.Add("類1總分組距count90Up"); table.Columns.Add("類1平均組距count90Up"); table.Columns.Add("類1加權總分組距count90Up"); table.Columns.Add("類1加權平均組距count90Up"); table.Columns.Add("類2總分組距count90Up"); table.Columns.Add("類2平均組距count90Up"); table.Columns.Add("類2加權總分組距count90Up"); table.Columns.Add("類2加權平均組距count90Up");
                    table.Columns.Add("總分班組距count80Up"); table.Columns.Add("總分科組距count80Up"); table.Columns.Add("總分校組距count80Up"); table.Columns.Add("平均班組距count80Up"); table.Columns.Add("平均科組距count80Up"); table.Columns.Add("平均校組距count80Up"); table.Columns.Add("加權總分班組距count80Up"); table.Columns.Add("加權總分科組距count80Up"); table.Columns.Add("加權總分校組距count80Up"); table.Columns.Add("加權平均班組距count80Up"); table.Columns.Add("加權平均科組距count80Up"); table.Columns.Add("加權平均校組距count80Up"); table.Columns.Add("類1總分組距count80Up"); table.Columns.Add("類1平均組距count80Up"); table.Columns.Add("類1加權總分組距count80Up"); table.Columns.Add("類1加權平均組距count80Up"); table.Columns.Add("類2總分組距count80Up"); table.Columns.Add("類2平均組距count80Up"); table.Columns.Add("類2加權總分組距count80Up"); table.Columns.Add("類2加權平均組距count80Up");
                    table.Columns.Add("總分班組距count70Up"); table.Columns.Add("總分科組距count70Up"); table.Columns.Add("總分校組距count70Up"); table.Columns.Add("平均班組距count70Up"); table.Columns.Add("平均科組距count70Up"); table.Columns.Add("平均校組距count70Up"); table.Columns.Add("加權總分班組距count70Up"); table.Columns.Add("加權總分科組距count70Up"); table.Columns.Add("加權總分校組距count70Up"); table.Columns.Add("加權平均班組距count70Up"); table.Columns.Add("加權平均科組距count70Up"); table.Columns.Add("加權平均校組距count70Up"); table.Columns.Add("類1總分組距count70Up"); table.Columns.Add("類1平均組距count70Up"); table.Columns.Add("類1加權總分組距count70Up"); table.Columns.Add("類1加權平均組距count70Up"); table.Columns.Add("類2總分組距count70Up"); table.Columns.Add("類2平均組距count70Up"); table.Columns.Add("類2加權總分組距count70Up"); table.Columns.Add("類2加權平均組距count70Up");
                    table.Columns.Add("總分班組距count60Up"); table.Columns.Add("總分科組距count60Up"); table.Columns.Add("總分校組距count60Up"); table.Columns.Add("平均班組距count60Up"); table.Columns.Add("平均科組距count60Up"); table.Columns.Add("平均校組距count60Up"); table.Columns.Add("加權總分班組距count60Up"); table.Columns.Add("加權總分科組距count60Up"); table.Columns.Add("加權總分校組距count60Up"); table.Columns.Add("加權平均班組距count60Up"); table.Columns.Add("加權平均科組距count60Up"); table.Columns.Add("加權平均校組距count60Up"); table.Columns.Add("類1總分組距count60Up"); table.Columns.Add("類1平均組距count60Up"); table.Columns.Add("類1加權總分組距count60Up"); table.Columns.Add("類1加權平均組距count60Up"); table.Columns.Add("類2總分組距count60Up"); table.Columns.Add("類2平均組距count60Up"); table.Columns.Add("類2加權總分組距count60Up"); table.Columns.Add("類2加權平均組距count60Up");
                    table.Columns.Add("總分班組距count50Up"); table.Columns.Add("總分科組距count50Up"); table.Columns.Add("總分校組距count50Up"); table.Columns.Add("平均班組距count50Up"); table.Columns.Add("平均科組距count50Up"); table.Columns.Add("平均校組距count50Up"); table.Columns.Add("加權總分班組距count50Up"); table.Columns.Add("加權總分科組距count50Up"); table.Columns.Add("加權總分校組距count50Up"); table.Columns.Add("加權平均班組距count50Up"); table.Columns.Add("加權平均科組距count50Up"); table.Columns.Add("加權平均校組距count50Up"); table.Columns.Add("類1總分組距count50Up"); table.Columns.Add("類1平均組距count50Up"); table.Columns.Add("類1加權總分組距count50Up"); table.Columns.Add("類1加權平均組距count50Up"); table.Columns.Add("類2總分組距count50Up"); table.Columns.Add("類2平均組距count50Up"); table.Columns.Add("類2加權總分組距count50Up"); table.Columns.Add("類2加權平均組距count50Up");
                    table.Columns.Add("總分班組距count40Up"); table.Columns.Add("總分科組距count40Up"); table.Columns.Add("總分校組距count40Up"); table.Columns.Add("平均班組距count40Up"); table.Columns.Add("平均科組距count40Up"); table.Columns.Add("平均校組距count40Up"); table.Columns.Add("加權總分班組距count40Up"); table.Columns.Add("加權總分科組距count40Up"); table.Columns.Add("加權總分校組距count40Up"); table.Columns.Add("加權平均班組距count40Up"); table.Columns.Add("加權平均科組距count40Up"); table.Columns.Add("加權平均校組距count40Up"); table.Columns.Add("類1總分組距count40Up"); table.Columns.Add("類1平均組距count40Up"); table.Columns.Add("類1加權總分組距count40Up"); table.Columns.Add("類1加權平均組距count40Up"); table.Columns.Add("類2總分組距count40Up"); table.Columns.Add("類2平均組距count40Up"); table.Columns.Add("類2加權總分組距count40Up"); table.Columns.Add("類2加權平均組距count40Up");
                    table.Columns.Add("總分班組距count30Up"); table.Columns.Add("總分科組距count30Up"); table.Columns.Add("總分校組距count30Up"); table.Columns.Add("平均班組距count30Up"); table.Columns.Add("平均科組距count30Up"); table.Columns.Add("平均校組距count30Up"); table.Columns.Add("加權總分班組距count30Up"); table.Columns.Add("加權總分科組距count30Up"); table.Columns.Add("加權總分校組距count30Up"); table.Columns.Add("加權平均班組距count30Up"); table.Columns.Add("加權平均科組距count30Up"); table.Columns.Add("加權平均校組距count30Up"); table.Columns.Add("類1總分組距count30Up"); table.Columns.Add("類1平均組距count30Up"); table.Columns.Add("類1加權總分組距count30Up"); table.Columns.Add("類1加權平均組距count30Up"); table.Columns.Add("類2總分組距count30Up"); table.Columns.Add("類2平均組距count30Up"); table.Columns.Add("類2加權總分組距count30Up"); table.Columns.Add("類2加權平均組距count30Up");
                    table.Columns.Add("總分班組距count20Up"); table.Columns.Add("總分科組距count20Up"); table.Columns.Add("總分校組距count20Up"); table.Columns.Add("平均班組距count20Up"); table.Columns.Add("平均科組距count20Up"); table.Columns.Add("平均校組距count20Up"); table.Columns.Add("加權總分班組距count20Up"); table.Columns.Add("加權總分科組距count20Up"); table.Columns.Add("加權總分校組距count20Up"); table.Columns.Add("加權平均班組距count20Up"); table.Columns.Add("加權平均科組距count20Up"); table.Columns.Add("加權平均校組距count20Up"); table.Columns.Add("類1總分組距count20Up"); table.Columns.Add("類1平均組距count20Up"); table.Columns.Add("類1加權總分組距count20Up"); table.Columns.Add("類1加權平均組距count20Up"); table.Columns.Add("類2總分組距count20Up"); table.Columns.Add("類2平均組距count20Up"); table.Columns.Add("類2加權總分組距count20Up"); table.Columns.Add("類2加權平均組距count20Up");
                    table.Columns.Add("總分班組距count10Up"); table.Columns.Add("總分科組距count10Up"); table.Columns.Add("總分校組距count10Up"); table.Columns.Add("平均班組距count10Up"); table.Columns.Add("平均科組距count10Up"); table.Columns.Add("平均校組距count10Up"); table.Columns.Add("加權總分班組距count10Up"); table.Columns.Add("加權總分科組距count10Up"); table.Columns.Add("加權總分校組距count10Up"); table.Columns.Add("加權平均班組距count10Up"); table.Columns.Add("加權平均科組距count10Up"); table.Columns.Add("加權平均校組距count10Up"); table.Columns.Add("類1總分組距count10Up"); table.Columns.Add("類1平均組距count10Up"); table.Columns.Add("類1加權總分組距count10Up"); table.Columns.Add("類1加權平均組距count10Up"); table.Columns.Add("類2總分組距count10Up"); table.Columns.Add("類2平均組距count10Up"); table.Columns.Add("類2加權總分組距count10Up"); table.Columns.Add("類2加權平均組距count10Up");
                    table.Columns.Add("總分班組距count90Down"); table.Columns.Add("總分科組距count90Down"); table.Columns.Add("總分校組距count90Down"); table.Columns.Add("平均班組距count90Down"); table.Columns.Add("平均科組距count90Down"); table.Columns.Add("平均校組距count90Down"); table.Columns.Add("加權總分班組距count90Down"); table.Columns.Add("加權總分科組距count90Down"); table.Columns.Add("加權總分校組距count90Down"); table.Columns.Add("加權平均班組距count90Down"); table.Columns.Add("加權平均科組距count90Down"); table.Columns.Add("加權平均校組距count90Down"); table.Columns.Add("類1總分組距count90Down"); table.Columns.Add("類1平均組距count90Down"); table.Columns.Add("類1加權總分組距count90Down"); table.Columns.Add("類1加權平均組距count90Down"); table.Columns.Add("類2總分組距count90Down"); table.Columns.Add("類2平均組距count90Down"); table.Columns.Add("類2加權總分組距count90Down"); table.Columns.Add("類2加權平均組距count90Down");
                    table.Columns.Add("總分班組距count80Down"); table.Columns.Add("總分科組距count80Down"); table.Columns.Add("總分校組距count80Down"); table.Columns.Add("平均班組距count80Down"); table.Columns.Add("平均科組距count80Down"); table.Columns.Add("平均校組距count80Down"); table.Columns.Add("加權總分班組距count80Down"); table.Columns.Add("加權總分科組距count80Down"); table.Columns.Add("加權總分校組距count80Down"); table.Columns.Add("加權平均班組距count80Down"); table.Columns.Add("加權平均科組距count80Down"); table.Columns.Add("加權平均校組距count80Down"); table.Columns.Add("類1總分組距count80Down"); table.Columns.Add("類1平均組距count80Down"); table.Columns.Add("類1加權總分組距count80Down"); table.Columns.Add("類1加權平均組距count80Down"); table.Columns.Add("類2總分組距count80Down"); table.Columns.Add("類2平均組距count80Down"); table.Columns.Add("類2加權總分組距count80Down"); table.Columns.Add("類2加權平均組距count80Down");
                    table.Columns.Add("總分班組距count70Down"); table.Columns.Add("總分科組距count70Down"); table.Columns.Add("總分校組距count70Down"); table.Columns.Add("平均班組距count70Down"); table.Columns.Add("平均科組距count70Down"); table.Columns.Add("平均校組距count70Down"); table.Columns.Add("加權總分班組距count70Down"); table.Columns.Add("加權總分科組距count70Down"); table.Columns.Add("加權總分校組距count70Down"); table.Columns.Add("加權平均班組距count70Down"); table.Columns.Add("加權平均科組距count70Down"); table.Columns.Add("加權平均校組距count70Down"); table.Columns.Add("類1總分組距count70Down"); table.Columns.Add("類1平均組距count70Down"); table.Columns.Add("類1加權總分組距count70Down"); table.Columns.Add("類1加權平均組距count70Down"); table.Columns.Add("類2總分組距count70Down"); table.Columns.Add("類2平均組距count70Down"); table.Columns.Add("類2加權總分組距count70Down"); table.Columns.Add("類2加權平均組距count70Down");
                    table.Columns.Add("總分班組距count60Down"); table.Columns.Add("總分科組距count60Down"); table.Columns.Add("總分校組距count60Down"); table.Columns.Add("平均班組距count60Down"); table.Columns.Add("平均科組距count60Down"); table.Columns.Add("平均校組距count60Down"); table.Columns.Add("加權總分班組距count60Down"); table.Columns.Add("加權總分科組距count60Down"); table.Columns.Add("加權總分校組距count60Down"); table.Columns.Add("加權平均班組距count60Down"); table.Columns.Add("加權平均科組距count60Down"); table.Columns.Add("加權平均校組距count60Down"); table.Columns.Add("類1總分組距count60Down"); table.Columns.Add("類1平均組距count60Down"); table.Columns.Add("類1加權總分組距count60Down"); table.Columns.Add("類1加權平均組距count60Down"); table.Columns.Add("類2總分組距count60Down"); table.Columns.Add("類2平均組距count60Down"); table.Columns.Add("類2加權總分組距count60Down"); table.Columns.Add("類2加權平均組距count60Down");
                    table.Columns.Add("總分班組距count50Down"); table.Columns.Add("總分科組距count50Down"); table.Columns.Add("總分校組距count50Down"); table.Columns.Add("平均班組距count50Down"); table.Columns.Add("平均科組距count50Down"); table.Columns.Add("平均校組距count50Down"); table.Columns.Add("加權總分班組距count50Down"); table.Columns.Add("加權總分科組距count50Down"); table.Columns.Add("加權總分校組距count50Down"); table.Columns.Add("加權平均班組距count50Down"); table.Columns.Add("加權平均科組距count50Down"); table.Columns.Add("加權平均校組距count50Down"); table.Columns.Add("類1總分組距count50Down"); table.Columns.Add("類1平均組距count50Down"); table.Columns.Add("類1加權總分組距count50Down"); table.Columns.Add("類1加權平均組距count50Down"); table.Columns.Add("類2總分組距count50Down"); table.Columns.Add("類2平均組距count50Down"); table.Columns.Add("類2加權總分組距count50Down"); table.Columns.Add("類2加權平均組距count50Down");
                    table.Columns.Add("總分班組距count40Down"); table.Columns.Add("總分科組距count40Down"); table.Columns.Add("總分校組距count40Down"); table.Columns.Add("平均班組距count40Down"); table.Columns.Add("平均科組距count40Down"); table.Columns.Add("平均校組距count40Down"); table.Columns.Add("加權總分班組距count40Down"); table.Columns.Add("加權總分科組距count40Down"); table.Columns.Add("加權總分校組距count40Down"); table.Columns.Add("加權平均班組距count40Down"); table.Columns.Add("加權平均科組距count40Down"); table.Columns.Add("加權平均校組距count40Down"); table.Columns.Add("類1總分組距count40Down"); table.Columns.Add("類1平均組距count40Down"); table.Columns.Add("類1加權總分組距count40Down"); table.Columns.Add("類1加權平均組距count40Down"); table.Columns.Add("類2總分組距count40Down"); table.Columns.Add("類2平均組距count40Down"); table.Columns.Add("類2加權總分組距count40Down"); table.Columns.Add("類2加權平均組距count40Down");
                    table.Columns.Add("總分班組距count30Down"); table.Columns.Add("總分科組距count30Down"); table.Columns.Add("總分校組距count30Down"); table.Columns.Add("平均班組距count30Down"); table.Columns.Add("平均科組距count30Down"); table.Columns.Add("平均校組距count30Down"); table.Columns.Add("加權總分班組距count30Down"); table.Columns.Add("加權總分科組距count30Down"); table.Columns.Add("加權總分校組距count30Down"); table.Columns.Add("加權平均班組距count30Down"); table.Columns.Add("加權平均科組距count30Down"); table.Columns.Add("加權平均校組距count30Down"); table.Columns.Add("類1總分組距count30Down"); table.Columns.Add("類1平均組距count30Down"); table.Columns.Add("類1加權總分組距count30Down"); table.Columns.Add("類1加權平均組距count30Down"); table.Columns.Add("類2總分組距count30Down"); table.Columns.Add("類2平均組距count30Down"); table.Columns.Add("類2加權總分組距count30Down"); table.Columns.Add("類2加權平均組距count30Down");
                    table.Columns.Add("總分班組距count20Down"); table.Columns.Add("總分科組距count20Down"); table.Columns.Add("總分校組距count20Down"); table.Columns.Add("平均班組距count20Down"); table.Columns.Add("平均科組距count20Down"); table.Columns.Add("平均校組距count20Down"); table.Columns.Add("加權總分班組距count20Down"); table.Columns.Add("加權總分科組距count20Down"); table.Columns.Add("加權總分校組距count20Down"); table.Columns.Add("加權平均班組距count20Down"); table.Columns.Add("加權平均科組距count20Down"); table.Columns.Add("加權平均校組距count20Down"); table.Columns.Add("類1總分組距count20Down"); table.Columns.Add("類1平均組距count20Down"); table.Columns.Add("類1加權總分組距count20Down"); table.Columns.Add("類1加權平均組距count20Down"); table.Columns.Add("類2總分組距count20Down"); table.Columns.Add("類2平均組距count20Down"); table.Columns.Add("類2加權總分組距count20Down"); table.Columns.Add("類2加權平均組距count20Down");
                    table.Columns.Add("總分班組距count10Down"); table.Columns.Add("總分科組距count10Down"); table.Columns.Add("總分校組距count10Down"); table.Columns.Add("平均班組距count10Down"); table.Columns.Add("平均科組距count10Down"); table.Columns.Add("平均校組距count10Down"); table.Columns.Add("加權總分班組距count10Down"); table.Columns.Add("加權總分科組距count10Down"); table.Columns.Add("加權總分校組距count10Down"); table.Columns.Add("加權平均班組距count10Down"); table.Columns.Add("加權平均科組距count10Down"); table.Columns.Add("加權平均校組距count10Down"); table.Columns.Add("類1總分組距count10Down"); table.Columns.Add("類1平均組距count10Down"); table.Columns.Add("類1加權總分組距count10Down"); table.Columns.Add("類1加權平均組距count10Down"); table.Columns.Add("類2總分組距count10Down"); table.Columns.Add("類2平均組距count10Down"); table.Columns.Add("類2加權總分組距count10Down"); table.Columns.Add("類2加權平均組距count10Down");

                    // 新增 columns name
                    table.Columns.Add("開始日期");
                    table.Columns.Add("結束日期");

                    // 先固定8個
                    for (int i = 1; i <= 8; i++)
                         table.Columns.Add("學習服務區間時數" + i);

                    table.Columns.Add("大功區間統計");
                    table.Columns.Add("小功區間統計");
                    table.Columns.Add("嘉獎區間統計");
                    table.Columns.Add("大過區間統計");
                    table.Columns.Add("小過區間統計");
                    table.Columns.Add("警告區間統計");
                    table.Columns.Add("留校察看區間");

                    // 動態新增缺曠統計，使用模式一般_曠課、一般_事假..
                    foreach (string name in Utility.GetATMappingKey())
                         table.Columns.Add("區間" + name);

                    // 動態資料新增
                    for (int atIdx = 1; atIdx <= conf.AttendanceDetailLimit; atIdx++)
                    {
                         // 缺曠區間明細 A:日期,B:
                         table.Columns.Add("缺曠區間明細日期" + atIdx);
                         table.Columns.Add("缺曠區間明細內容" + atIdx);
                         table.Columns.Add("缺曠區間明細C" + atIdx);
                    }

                    // 獎懲區間明細 A:日期,B:類別支數,C:事由
                    for (int atIdx = 1; atIdx <= conf.DisciplineDetailLimit; atIdx++)
                    {
                         table.Columns.Add("獎懲區間明細日期" + atIdx);
                         table.Columns.Add("獎懲區間明細類別支數" + atIdx);
                         table.Columns.Add("獎懲區間明細事由" + atIdx);
                    }
                    for (int atIdx = 1; atIdx <= conf.ServiceLearningDetailLimit; atIdx++)
                    {
                         // 學習服務區間明細 A:日期,B:內容,C:時數
                         table.Columns.Add("學習服務區間明細日期" + atIdx);
                         table.Columns.Add("學習服務區間明細內容" + atIdx);
                         table.Columns.Add("學習服務區間明細時數" + atIdx);
                    }
                    #endregion
                    #endregion
                    //宣告產生的報表
                    Aspose.Words.Document document = new Aspose.Words.Document();
                    //用一個BackgroundWorker包起來
                    System.ComponentModel.BackgroundWorker bkw = new System.ComponentModel.BackgroundWorker();
                    bkw.WorkerReportsProgress = true;
                    System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " 個人評量成績單產生 S");
                    bkw.ProgressChanged += delegate(object sender, System.ComponentModel.ProgressChangedEventArgs e)
                    {
                         FISCA.Presentation.MotherForm.SetStatusBarMessage("個人評量成績單產生中", e.ProgressPercentage);
                         System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " 個人評量成績單產生 " + e.ProgressPercentage);
                    };
                    Exception exc = null;
                    bkw.RunWorkerCompleted += delegate
                    {
                         //#region 將 DataTable 內合併欄位產生出來
                         //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\SH_jvyjhs_StudentExamScore_epost合併欄位.txt");
                         //foreach (DataColumn dc in table.Columns)
                         //    sw.WriteLine(dc.Caption);

                         //sw.Close();
                         //#endregion


                         System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " 個人評量成績單產生 E");
                         string err = "下列學生因成績項目超過樣板支援上限，\n超出部分科目成績無法印出，建議調整樣板內容。";
                         if (overflowRecords.Count > 0)
                         {
                              foreach (var stuRec in overflowRecords)
                              {
                                   err += "\n" + (stuRec.RefClass == null ? "" : (stuRec.RefClass.ClassName + "班" + stuRec.SeatNo + "號")) + "[" + stuRec.StudentNumber + "]" + stuRec.StudentName;
                              }
                         }
                         #region 儲存檔案
                         string inputReportName = "個人評量成績單";
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
                              document.Save(path, Aspose.Words.SaveFormat.Doc);
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
                                        document.Save(sd.FileName, Aspose.Words.SaveFormat.Doc);

                                   }
                                   catch
                                   {
                                        FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                        return;
                                   }
                              }
                         }
                         #endregion

                         // 檢查是否需要產生學生清單
                         if (form.GetisExportStudentList())
                         {
                              string ExportReportName = "個人評量成績單(學生清單)";

                              string pathxls = Path.Combine(System.Windows.Forms.Application.StartupPath, "Reports");
                              if (!Directory.Exists(pathxls))
                                   Directory.CreateDirectory(pathxls);
                              pathxls = Path.Combine(pathxls, ExportReportName + ".xls");

                              if (File.Exists(pathxls))
                              {
                                   int i = 1;
                                   while (true)
                                   {
                                        string newPath = Path.GetDirectoryName(pathxls) + "\\" + Path.GetFileNameWithoutExtension(pathxls) + (i++) + Path.GetExtension(pathxls);
                                        if (!File.Exists(newPath))
                                        {
                                             pathxls = newPath;
                                             break;
                                        }
                                   }
                              }

                              try
                              {
                                   _wbStudentList.Save(pathxls, Aspose.Cells.FileFormatType.Excel2003);
                                   System.Diagnostics.Process.Start(pathxls);
                              }
                              catch
                              {
                                   System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                                   sd.Title = "另存新檔";
                                   sd.FileName = ExportReportName + ".xls";
                                   sd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                                   if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                   {
                                        try
                                        {
                                             _wbStudentList.Save(sd.FileName, Aspose.Cells.FileFormatType.Excel2003);


                                        }
                                        catch
                                        {
                                             FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                             return;
                                        }
                                   }
                              }
                         }
                                                 

                         FISCA.Presentation.MotherForm.SetStatusBarMessage("個人評量成績單產生完成。", 100);
                         if (overflowRecords.Count > 0)
                              MessageBox.Show(err);
                         if (exc != null)
                         {
                              throw new Exception("產生期末成績單發生錯誤", exc);
                         }

                         // 產生 EPost
                         bool chkEpost = false;
                         if (bool.TryParse(conf.isExportEPost, out chkEpost))
                         {
                             if (chkEpost)
                             {
                                 // 檢查是否產生 Excel
                                 Aspose.Cells.Workbook wb = new Aspose.Cells.Workbook();
                                 Utility.CompletedXlsCsv("個人評量成績單epost", _dtEpost);
                             }
                         }
                    };
                    bkw.DoWork += delegate(object sender, System.ComponentModel.DoWorkEventArgs e)
                    {
                         //取得家長代碼
                         QueryHelper q = new QueryHelper();
                         string ids = string.Join("','", selectedStudents);

                         Dictionary<string, string> parentCode = new Dictionary<string, string>();

                         DataTable response = q.Select("SELECT id,parent_code FROM student WHERE id IN ('" + ids + "')");
                         foreach (DataRow row in response.Rows)
                         {
                              string id = row["id"].ToString();
                              string code = row["parent_code"].ToString();

                              if (!parentCode.ContainsKey(id))
                                   parentCode.Add(id, code);
                         }

                         var studentRecords = accessHelper.StudentHelper.GetStudents(selectedStudents);
                         Dictionary<string, Dictionary<string, Dictionary<string, ExamScoreInfo>>> studentExamSores = new Dictionary<string, Dictionary<string, Dictionary<string, ExamScoreInfo>>>();
                         Dictionary<string, Dictionary<string, ExamScoreInfo>> studentRefExamSores = new Dictionary<string, Dictionary<string, ExamScoreInfo>>();
                         ManualResetEvent scoreReady = new ManualResetEvent(false);
                         ManualResetEvent elseReady = new ManualResetEvent(false);
                         #region 偷跑取得考試成績
                         new Thread(new ThreadStart(delegate
                         {
                              // 取得學生學期科目成績
                              int sSchoolYear, sSemester;
                              int.TryParse(conf.SchoolYear, out sSchoolYear);
                              int.TryParse(conf.Semester, out sSemester);
                              #region 整理學生定期評量成績
                              #region 篩選課程學年度、學期、科目取得有可能有需要的資料
                              List<CourseRecord> targetCourseList = new List<CourseRecord>();
                              try
                              {
                                   foreach (var courseRecord in accessHelper.CourseHelper.GetAllCourse(sSchoolYear, sSemester))
                                   {
                                        //用科目濾出可能有用到的課程
                                        if (conf.PrintSubjectList.Contains(courseRecord.Subject)
                                            || conf.TagRank1SubjectList.Contains(courseRecord.Subject)
                                            || conf.TagRank2SubjectList.Contains(courseRecord.Subject))
                                             targetCourseList.Add(courseRecord);
                                   }
                              }
                              catch (Exception exception)
                              {
                                   exc = exception;
                              }
                              #endregion
                              try
                              {
                                   if (conf.ExamRecord != null || conf.RefenceExamRecord != null)
                                   {
                                        accessHelper.CourseHelper.FillExam(targetCourseList);
                                        var tcList = new List<CourseRecord>();
                                        var totalList = new List<CourseRecord>();
                                        foreach (var courseRec in targetCourseList)
                                        {
                                             if (conf.ExamRecord != null && courseRec.ExamList.Contains(conf.ExamRecord.Name))
                                             {
                                                  tcList.Add(courseRec);
                                                  totalList.Add(courseRec);
                                             }
                                             if (tcList.Count == 180)
                                             {
                                                  accessHelper.CourseHelper.FillStudentAttend(tcList);
                                                  accessHelper.CourseHelper.FillExamScore(tcList);
                                                  tcList.Clear();
                                             }
                                        }
                                        accessHelper.CourseHelper.FillStudentAttend(tcList);
                                        accessHelper.CourseHelper.FillExamScore(tcList);
                                        foreach (var courseRecord in totalList)
                                        {
                                             #region 整理本次定期評量成績
                                             if (conf.ExamRecord != null && courseRecord.ExamList.Contains(conf.ExamRecord.Name))
                                             {
                                                  foreach (var attendStudent in courseRecord.StudentAttendList)
                                                  {
                                                       if (!studentExamSores.ContainsKey(attendStudent.StudentID)) studentExamSores.Add(attendStudent.StudentID, new Dictionary<string, Dictionary<string, ExamScoreInfo>>());
                                                       if (!studentExamSores[attendStudent.StudentID].ContainsKey(courseRecord.Subject)) studentExamSores[attendStudent.StudentID].Add(courseRecord.Subject, new Dictionary<string, ExamScoreInfo>());
                                                       studentExamSores[attendStudent.StudentID][courseRecord.Subject].Add("" + attendStudent.CourseID, null);
                                                  }
                                                  foreach (var examScoreRec in courseRecord.ExamScoreList)
                                                  {
                                                       if (examScoreRec.ExamName == conf.ExamRecord.Name)
                                                       {
                                                            studentExamSores[examScoreRec.StudentID][courseRecord.Subject]["" + examScoreRec.CourseID] = examScoreRec;
                                                       }
                                                  }
                                             }
                                             #endregion
                                             #region 整理前次定期評量成績
                                             if (conf.RefenceExamRecord != null && courseRecord.ExamList.Contains(conf.RefenceExamRecord.Name))
                                             {
                                                  foreach (var examScoreRec in courseRecord.ExamScoreList)
                                                  {
                                                       if (examScoreRec.ExamName == conf.RefenceExamRecord.Name)
                                                       {
                                                            if (!studentRefExamSores.ContainsKey(examScoreRec.StudentID))
                                                                 studentRefExamSores.Add(examScoreRec.StudentID, new Dictionary<string, ExamScoreInfo>());
                                                            studentRefExamSores[examScoreRec.StudentID].Add("" + examScoreRec.CourseID, examScoreRec);
                                                       }
                                                  }
                                             }
                                             #endregion
                                        }
                                   }
                              }
                              catch (Exception exception)
                              {
                                   exc = exception;
                              }
                              finally
                              {
                                   scoreReady.Set();
                              }
                              #endregion
                              #region 整理學生學期、學年成績
                              try
                              {
                                   accessHelper.StudentHelper.FillAttendance(studentRecords);
                                   accessHelper.StudentHelper.FillReward(studentRecords);
                              }
                              catch (Exception exception)
                              {
                                   exc = exception;
                              }
                              finally
                              {
                                   elseReady.Set();
                              }
                              #endregion
                         })).Start();
                         #endregion
                         try
                         {
                              string key = "";
                              bkw.ReportProgress(0);

                              // 產生 epost data table columns
                              // 清空epost 使用欄位
                              _dtEpost.Columns.Clear();
                              _dtEpost.Clear();
                              // 處理 epost 欄位
                              _dtEpost.Columns.Add("CN");
                              _dtEpost.Columns.Add("POSTALCODE");
                              _dtEpost.Columns.Add("POSTALADDRESS");
                              _dtEpost.Columns.Add("學年度");
                              _dtEpost.Columns.Add("學期");
                              _dtEpost.Columns.Add("試別");
                              _dtEpost.Columns.Add("班級");
                              //_dtEpost.Columns.Add("導師姓名");
                              _dtEpost.Columns.Add("學號");
                              _dtEpost.Columns.Add("座號");
                              _dtEpost.Columns.Add("姓名");

                              // 處理科目相關成績
                              for (int subjectIndex = 1; subjectIndex <= conf.SubjectLimit; subjectIndex++)
                              {
                                  _dtEpost.Columns.Add("科目名稱" + subjectIndex);
                                  _dtEpost.Columns.Add("學分數" + subjectIndex);
                                  //_dtEpost.Columns.Add("前次成績" + subjectIndex);
                                  _dtEpost.Columns.Add("科目成績" + subjectIndex);
                                  //_dtEpost.Columns.Add("班級平均" + subjectIndex);
                                  //_dtEpost.Columns.Add("班級排名" + subjectIndex);
                                  //_dtEpost.Columns.Add("科排名" + subjectIndex);
                                  //_dtEpost.Columns.Add("類組排名" + subjectIndex);
                              }

                              _dtEpost.Columns.Add("加權總分");
                              _dtEpost.Columns.Add("加權平均");
                              //_dtEpost.Columns.Add("班級加權平均");
                              _dtEpost.Columns.Add("加權總分班排名");
                              //_dtEpost.Columns.Add("加權總分科排名");
                              //_dtEpost.Columns.Add("科加權平均");
                              // _dtEpost.Columns.Add("加權總分類組排名");
                              //_dtEpost.Columns.Add("加權平均類組排名");

                              // 固定會對照
                              Dictionary<string, string> eKeyValDict = new Dictionary<string, string>();
                              eKeyValDict.Add("收件人", "CN");
                              eKeyValDict.Add("學年度", "學年度");
                              eKeyValDict.Add("學期", "學期");
                              eKeyValDict.Add("試別", "試別");
                              eKeyValDict.Add("班級", "班級");
                              //eKeyValDict.Add("班導師", "導師姓名");
                              eKeyValDict.Add("學號", "學號");
                              eKeyValDict.Add("座號", "座號");
                              eKeyValDict.Add("姓名", "姓名");
                              eKeyValDict.Add("加權總分", "加權總分");
                              eKeyValDict.Add("加權平均", "加權平均");
                              //eKeyValDict.Add("加權平均班均標", "班級加權平均");
                              eKeyValDict.Add("加權總分班排名", "加權總分班排名");
                              //eKeyValDict.Add("加權總分科排名", "加權總分科排名");
                              //eKeyValDict.Add("加權平均科均標", "科加權平均");
                              //eKeyValDict.Add("類別1加權總分排名", "加權總分類組排名");
                              //eKeyValDict.Add("類別1加權平均排名", "加權平均類組排名");
                              //eKeyValDict.Add("大功區間統計", "大功");
                              //eKeyValDict.Add("小功區間統計", "小功");
                              //eKeyValDict.Add("嘉獎區間統計", "嘉獎");
                              //eKeyValDict.Add("大過區間統計", "大過");
                              //eKeyValDict.Add("小過區間統計", "小過");
                              //eKeyValDict.Add("警告區間統計", "警告");
                              //eKeyValDict.Add("留校察看區間", "留校察看");
                              //eKeyValDict.Add("加權總分班排名母數", "班級人數");
                              //eKeyValDict.Add("加權總分科排名母數", "科人數");
                              //eKeyValDict.Add("類別1加權總分排名母數", "類組人數");
                              //eKeyValDict.Add("類別1加權平均排名母數", "類組人數");

                              #region 缺曠對照表
                              List<K12.Data.PeriodMappingInfo> periodMappingInfos = K12.Data.PeriodMapping.SelectAll();
                              Dictionary<string, string> dicPeriodMappingType = new Dictionary<string, string>();
                              List<string> periodTypes = new List<string>();
                              foreach (K12.Data.PeriodMappingInfo periodMappingInfo in periodMappingInfos)
                              {
                                   if (!dicPeriodMappingType.ContainsKey(periodMappingInfo.Name))
                                        dicPeriodMappingType.Add(periodMappingInfo.Name, periodMappingInfo.Type);

                                   if (!periodTypes.Contains(periodMappingInfo.Type))
                                        periodTypes.Add(periodMappingInfo.Type);
                              }
                              foreach (var absence in K12.Data.AbsenceMapping.SelectAll())
                              {
                                   foreach (var pt in periodTypes)
                                   {
                                        string attendanceKey = pt + "_" + absence.Name;
                                        if (!table.Columns.Contains(attendanceKey))
                                        {
                                             table.Columns.Add(attendanceKey);
                                        }
                                   }
                              }
                              #endregion
                              bkw.ReportProgress(3);
                              #region 整理學生住址
                              accessHelper.StudentHelper.FillContactInfo(studentRecords);
                              #endregion
                              #region 整理學生父母及監護人
                              accessHelper.StudentHelper.FillParentInfo(studentRecords);
                              #endregion
                              bkw.ReportProgress(10);
                              #region 整理同年級學生
                              //整理選取學生的年級
                              Dictionary<string, List<StudentRecord>> gradeyearStudents = new Dictionary<string, List<StudentRecord>>();
                              foreach (var studentRec in studentRecords)
                              {
                                   string grade = "";
                                   if (studentRec.RefClass != null)
                                        grade = "" + studentRec.RefClass.GradeYear;
                                   if (!gradeyearStudents.ContainsKey(grade))
                                        gradeyearStudents.Add(grade, new List<StudentRecord>());
                                   gradeyearStudents[grade].Add(studentRec);
                              }
                              foreach (var classRec in accessHelper.ClassHelper.GetAllClass())
                              {
                                   if (gradeyearStudents.ContainsKey("" + classRec.GradeYear))
                                   {
                                        //用班級去取出可能有相關的學生
                                        foreach (var studentRec in classRec.Students)
                                        {
                                             string grade = "";
                                             if (studentRec.RefClass != null)
                                                  grade = "" + studentRec.RefClass.GradeYear;
                                             if (!gradeyearStudents[grade].Contains(studentRec))
                                                  gradeyearStudents[grade].Add(studentRec);
                                        }
                                   }
                              }
                              #endregion
                              bkw.ReportProgress(15);
                              #region 取得學生類別
                              Dictionary<string, List<K12.Data.StudentTagRecord>> studentTags = new Dictionary<string, List<K12.Data.StudentTagRecord>>();
                              List<string> list = new List<string>();
                              foreach (var sRecs in gradeyearStudents.Values)
                              {
                                   foreach (var stuRec in sRecs)
                                   {
                                        list.Add(stuRec.StudentID);
                                   }
                              }
                              foreach (var tag in K12.Data.StudentTag.SelectByStudentIDs(list))
                              {
                                   if (!studentTags.ContainsKey(tag.RefStudentID))
                                        studentTags.Add(tag.RefStudentID, new List<K12.Data.StudentTagRecord>());
                                   studentTags[tag.RefStudentID].Add(tag);
                              }
                              #endregion
                              bkw.ReportProgress(20);
                              //等到成績載完
                              scoreReady.WaitOne();
                              bkw.ReportProgress(35);
                              int progressCount = 0;
                              #region 計算總分及各項目排名
                              Dictionary<string, string> studentTag1Group = new Dictionary<string, string>();
                              Dictionary<string, string> studentTag2Group = new Dictionary<string, string>();
                              Dictionary<string, List<decimal>> ranks = new Dictionary<string, List<decimal>>();
                              Dictionary<string, List<string>> rankStudents = new Dictionary<string, List<string>>();
                              Dictionary<string, decimal> studentPrintSubjectSum = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag1SubjectSum = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag2SubjectSum = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentPrintSubjectAvg = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag1SubjectAvg = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag2SubjectAvg = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentPrintSubjectSumW = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag1SubjectSumW = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag2SubjectSumW = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentPrintSubjectAvgW = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag1SubjectAvgW = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> studentTag2SubjectAvgW = new Dictionary<string, decimal>();
                              Dictionary<string, decimal> analytics = new Dictionary<string, decimal>();
                              int total = 0;
                              foreach (var gss in gradeyearStudents.Values)
                              {
                                   total += gss.Count;
                              }
                              bkw.ReportProgress(40);
                              foreach (string gradeyear in gradeyearStudents.Keys)
                              {
                                   //找出全年級學生
                                   foreach (var studentRec in gradeyearStudents[gradeyear])
                                   {
                                        string studentID = studentRec.StudentID;
                                        bool rank = true;
                                        string tag1ID = "";
                                        string tag2ID = "";
                                        #region 分析學生所屬類別
                                        if (studentTags.ContainsKey(studentID))
                                        {
                                             foreach (var tag in studentTags[studentID])
                                             {
                                                  #region 判斷學生是否屬於不排名類別
                                                  if (conf.RankFilterTagList.Contains(tag.RefTagID))
                                                  {
                                                       rank = false;
                                                  }
                                                  #endregion
                                                  #region 判斷學生在類別排名1中所屬的類別
                                                  if (tag1ID == "" && conf.TagRank1TagList.Contains(tag.RefTagID))
                                                  {
                                                       tag1ID = tag.RefTagID;
                                                       studentTag1Group.Add(studentID, tag1ID);
                                                  }
                                                  #endregion
                                                  #region 判斷學生在類別排名2中所屬的類別
                                                  if (tag2ID == "" && conf.TagRank2TagList.Contains(tag.RefTagID))
                                                  {
                                                       tag2ID = tag.RefTagID;
                                                       studentTag2Group.Add(studentID, tag2ID);
                                                  }
                                                  #endregion
                                             }
                                        }
                                        #endregion
                                        bool summaryRank = true;
                                        bool tag1SummaryRank = true;
                                        bool tag2SummaryRank = true;
                                        if (studentExamSores.ContainsKey(studentID))
                                        {
                                             decimal printSubjectSum = 0;
                                             int printSubjectCount = 0;
                                             decimal tag1SubjectSum = 0;
                                             int tag1SubjectCount = 0;
                                             decimal tag2SubjectSum = 0;
                                             int tag2SubjectCount = 0;
                                             decimal printSubjectSumW = 0;
                                             decimal printSubjectCreditSum = 0;
                                             decimal tag1SubjectSumW = 0;
                                             decimal tag1SubjectCreditSum = 0;
                                             decimal tag2SubjectSumW = 0;
                                             decimal tag2SubjectCreditSum = 0;
                                             foreach (var subjectName in studentExamSores[studentID].Keys)
                                             {
                                                  if (conf.PrintSubjectList.Contains(subjectName))
                                                  {
                                                       #region 是列印科目
                                                       foreach (var sceTakeRecord in studentExamSores[studentID][subjectName].Values)
                                                       {
                                                            if (sceTakeRecord != null && sceTakeRecord.SpecialCase == "")
                                                            {
                                                                 printSubjectSum += sceTakeRecord.ExamScore;//計算總分
                                                                 printSubjectCount++;
                                                                 //計算加權總分 - 2014/10/9 改為decimal
                                                                 printSubjectSumW += sceTakeRecord.ExamScore * sceTakeRecord.CreditDec();
                                                                 printSubjectCreditSum += sceTakeRecord.CreditDec();
                                                                 if (rank && sceTakeRecord.Status == "一般")//不在過濾名單且為一般生才做排名
                                                                 {
                                                                      if (sceTakeRecord.RefClass != null)
                                                                      {
                                                                           //各科目班排名
                                                                           key = "班排名" + sceTakeRecord.RefClass.ClassID + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                                           if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                                           ranks[key].Add(sceTakeRecord.ExamScore);
                                                                           rankStudents[key].Add(studentID);
                                                                      }
                                                                      if (sceTakeRecord.Department != "")
                                                                      {
                                                                           //各科目科排名
                                                                           key = "科排名" + sceTakeRecord.Department + "^^^" + gradeyear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                                           if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                                           ranks[key].Add(sceTakeRecord.ExamScore);
                                                                           rankStudents[key].Add(studentID);
                                                                      }
                                                                      //各科目全校排名
                                                                      key = "全校排名" + gradeyear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                      if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                                      if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                                      ranks[key].Add(sceTakeRecord.ExamScore);
                                                                      rankStudents[key].Add(studentID);
                                                                 }
                                                            }
                                                            else
                                                            {
                                                                 summaryRank = false;
                                                            }
                                                       }
                                                       #endregion
                                                  }
                                                  if (tag1ID != "" && conf.TagRank1SubjectList.Contains(subjectName))
                                                  {
                                                       #region 有Tag1且是排名科目
                                                       foreach (var sceTakeRecord in studentExamSores[studentID][subjectName].Values)
                                                       {
                                                            if (sceTakeRecord != null && sceTakeRecord.SpecialCase == "")
                                                            {
                                                                 tag1SubjectSum += sceTakeRecord.ExamScore;//計算總分
                                                                 tag1SubjectCount++;
                                                                 //計算加權總分
                                                                 tag1SubjectSumW += sceTakeRecord.ExamScore * sceTakeRecord.CreditDec();
                                                                 tag1SubjectCreditSum += sceTakeRecord.CreditDec();
                                                                 //各科目類別1排名
                                                                 if (rank && sceTakeRecord.Status == "一般")//不在過濾名單且為一般生才做排名
                                                                 {
                                                                      if (conf.PrintSubjectList.Contains(subjectName))//是列印科目才算科目排名                                                
                                                                      {
                                                                           key = "類別1排名" + tag1ID + "^^^" + gradeyear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                                           if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                                           ranks[key].Add(sceTakeRecord.ExamScore);
                                                                           rankStudents[key].Add(studentID);
                                                                      }
                                                                 }
                                                            }
                                                            else
                                                            {
                                                                 tag1SummaryRank = false;
                                                            }
                                                       }
                                                       #endregion
                                                  }
                                                  if (tag2ID != "" && conf.TagRank2SubjectList.Contains(subjectName))
                                                  {
                                                       #region 有Tag2且是排名科目
                                                       foreach (var sceTakeRecord in studentExamSores[studentID][subjectName].Values)
                                                       {
                                                            if (sceTakeRecord != null && sceTakeRecord.SpecialCase == "")
                                                            {
                                                                 tag2SubjectSum += sceTakeRecord.ExamScore;//計算總分
                                                                 tag2SubjectCount++;
                                                                 //計算加權總分
                                                                 tag2SubjectSumW += sceTakeRecord.ExamScore * sceTakeRecord.CreditDec();
                                                                 tag2SubjectCreditSum += sceTakeRecord.CreditDec();
                                                                 //各科目類別2排名
                                                                 if (rank && sceTakeRecord.Status == "一般")//不在過濾名單且為一般生才做排名
                                                                 {
                                                                      if (conf.PrintSubjectList.Contains(subjectName))//是列印科目才算科目排名                                                
                                                                      {
                                                                           key = "類別2排名" + tag2ID + "^^^" + gradeyear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                                           if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                                           ranks[key].Add(sceTakeRecord.ExamScore);
                                                                           rankStudents[key].Add(studentID);
                                                                      }
                                                                 }
                                                            }
                                                            else
                                                            {
                                                                 tag2SummaryRank = false;
                                                            }
                                                       }
                                                       #endregion
                                                  }

                                             }
                                             if (printSubjectCount > 0)
                                             {
                                                  #region 有列印科目處理加總成績
                                                  //總分
                                                  studentPrintSubjectSum.Add(studentID, printSubjectSum);
                                                  //平均四捨五入至小數點第二位
                                                  studentPrintSubjectAvg.Add(studentID, Math.Round(printSubjectSum / printSubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                  if (rank && studentRec.Status == "一般" && summaryRank == true)//不在過濾名單且沒有特殊成績狀況且為一般生才做排名
                                                  {
                                                       //總分班排名
                                                       key = "總分班排名" + studentRec.RefClass.ClassID;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(printSubjectSum);
                                                       rankStudents[key].Add(studentID);
                                                       //總分科排名
                                                       key = "總分科排名" + studentRec.Department + "^^^" + gradeyear;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(printSubjectSum);
                                                       rankStudents[key].Add(studentID);
                                                       //總分全校排名
                                                       key = "總分全校排名" + gradeyear;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(printSubjectSum);
                                                       rankStudents[key].Add(studentID);
                                                       //平均班排名
                                                       key = "平均班排名" + studentRec.RefClass.ClassID;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(Math.Round(printSubjectSum / printSubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                       rankStudents[key].Add(studentID);
                                                       //平均科排名
                                                       key = "平均科排名" + studentRec.Department + "^^^" + gradeyear;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(Math.Round(printSubjectSum / printSubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                       rankStudents[key].Add(studentID);
                                                       //平均全校排名
                                                       key = "平均全校排名" + gradeyear;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(Math.Round(printSubjectSum / printSubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                       rankStudents[key].Add(studentID);
                                                  }
                                                  #endregion
                                                  if (printSubjectCreditSum > 0)
                                                  {
                                                       #region 有總學分數處理加總
                                                       //加權總分
                                                       studentPrintSubjectSumW.Add(studentID, printSubjectSumW);
                                                       //加權平均四捨五入至小數點第二位
                                                       studentPrintSubjectAvgW.Add(studentID, Math.Round(printSubjectSumW / printSubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                       if (rank && studentRec.Status == "一般" && summaryRank == true)//不在過濾名單且為一般生才做排名
                                                       {
                                                            //加權總分班排名
                                                            key = "加權總分班排名" + studentRec.RefClass.ClassID;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(printSubjectSumW);
                                                            rankStudents[key].Add(studentID);
                                                            //加權總分科排名
                                                            key = "加權總分科排名" + studentRec.Department + "^^^" + gradeyear;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(printSubjectSumW);
                                                            rankStudents[key].Add(studentID);
                                                            //加權總分全校排名
                                                            key = "加權總分全校排名" + gradeyear;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(printSubjectSumW);
                                                            rankStudents[key].Add(studentID);
                                                            //加權平均班排名
                                                            key = "加權平均班排名" + studentRec.RefClass.ClassID;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(Math.Round(printSubjectSumW / printSubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                            rankStudents[key].Add(studentID);
                                                            //加權平均科排名
                                                            key = "加權平均科排名" + studentRec.Department + "^^^" + gradeyear;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(Math.Round(printSubjectSumW / printSubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                            rankStudents[key].Add(studentID);
                                                            //加權平均全校排名
                                                            key = "加權平均全校排名" + gradeyear;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(Math.Round(printSubjectSumW / printSubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                            rankStudents[key].Add(studentID);
                                                       }
                                                       #endregion
                                                  }
                                             }
                                             //類別1總分平均排名
                                             if (tag1SubjectCount > 0)
                                             {
                                                  //總分
                                                  studentTag1SubjectSum.Add(studentID, tag1SubjectSum);
                                                  //平均四捨五入至小數點第二位
                                                  studentTag1SubjectAvg.Add(studentID, Math.Round(tag1SubjectSum / tag1SubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                  if (rank && studentRec.Status == "一般" && tag1SummaryRank == true)//不在過濾名單且為一般生才做排名
                                                  {
                                                       key = "類別1總分排名" + "^^^" + gradeyear + "^^^" + tag1ID;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(tag1SubjectSum);
                                                       rankStudents[key].Add(studentID);

                                                       key = "類別1平均排名" + "^^^" + gradeyear + "^^^" + tag1ID;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(Math.Round(tag1SubjectSum / tag1SubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                       rankStudents[key].Add(studentID);
                                                  }
                                                  //類別1加權總分平均排名
                                                  if (tag1SubjectCreditSum > 0)
                                                  {
                                                       studentTag1SubjectSumW.Add(studentID, tag1SubjectSumW);
                                                       studentTag1SubjectAvgW.Add(studentID, Math.Round(tag1SubjectSumW / tag1SubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                       if (rank && studentRec.Status == "一般" && tag1SummaryRank == true)//不在過濾名單且為一般生才做排名
                                                       {
                                                            key = "類別1加權總分排名" + "^^^" + gradeyear + "^^^" + tag1ID;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(tag1SubjectSumW);
                                                            rankStudents[key].Add(studentID);

                                                            key = "類別1加權平均排名" + "^^^" + gradeyear + "^^^" + tag1ID;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(Math.Round(tag1SubjectSumW / tag1SubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                            rankStudents[key].Add(studentID);
                                                       }
                                                  }
                                             }
                                             //類別2總分平均排名
                                             if (tag2SubjectCount > 0)
                                             {
                                                  //總分
                                                  studentTag2SubjectSum.Add(studentID, tag2SubjectSum);
                                                  //平均四捨五入至小數點第二位
                                                  studentTag2SubjectAvg.Add(studentID, Math.Round(tag2SubjectSum / tag2SubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                  if (rank && studentRec.Status == "一般" && tag2SummaryRank == true)//不在過濾名單且為一般生才做排名
                                                  {
                                                       key = "類別2總分排名" + "^^^" + gradeyear + "^^^" + tag2ID;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(tag2SubjectSum);
                                                       rankStudents[key].Add(studentID);
                                                       key = "類別2平均排名" + "^^^" + gradeyear + "^^^" + tag2ID;
                                                       if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                       if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                       ranks[key].Add(Math.Round(tag2SubjectSum / tag2SubjectCount, AvgRd, MidpointRounding.AwayFromZero));
                                                       rankStudents[key].Add(studentID);
                                                  }
                                                  //類別2加權總分平均排名
                                                  if (tag2SubjectCreditSum > 0)
                                                  {
                                                       studentTag2SubjectSumW.Add(studentID, tag2SubjectSumW);
                                                       studentTag2SubjectAvgW.Add(studentID, Math.Round(tag2SubjectSumW / tag2SubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                       if (rank && studentRec.Status == "一般" && tag2SummaryRank == true)//不在過濾名單且為一般生才做排名
                                                       {
                                                            key = "類別2加權總分排名" + "^^^" + gradeyear + "^^^" + tag2ID;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(tag2SubjectSumW);
                                                            rankStudents[key].Add(studentID);

                                                            key = "類別2加權平均排名" + "^^^" + gradeyear + "^^^" + tag2ID;
                                                            if (!ranks.ContainsKey(key)) ranks.Add(key, new List<decimal>());
                                                            if (!rankStudents.ContainsKey(key)) rankStudents.Add(key, new List<string>());
                                                            ranks[key].Add(Math.Round(tag2SubjectSumW / tag2SubjectCreditSum, AvgRd, MidpointRounding.AwayFromZero));
                                                            rankStudents[key].Add(studentID);
                                                       }
                                                  }
                                             }
                                        }
                                        progressCount++;
                                        bkw.ReportProgress(40 + progressCount * 30 / total);
                                   }
                              }
                              foreach (var k in ranks.Keys)
                              {
                                   var rankscores = ranks[k];
                                   //排序
                                   rankscores.Sort();
                                   rankscores.Reverse();
                                   //高均標、組距
                                   if (rankscores.Count > 0)
                                   {
                                        #region 算高標的中點
                                        int middleIndex = 0;
                                        int count = 1;
                                        var score = rankscores[0];
                                        while (rankscores.Count > middleIndex)
                                        {
                                             if (score != rankscores[middleIndex])
                                             {
                                                  if (count * 2 >= rankscores.Count) break;
                                                  score = rankscores[middleIndex];
                                             }
                                             middleIndex++;
                                             count++;
                                        }
                                        if (rankscores.Count == middleIndex)
                                        {
                                             middleIndex--;
                                             count--;
                                        }
                                        #endregion
                                        analytics.Add(k + "^^^高標", Math.Round(rankscores.GetRange(0, count).Average(), AvgRd, MidpointRounding.AwayFromZero));
                                        analytics.Add(k + "^^^均標", Math.Round(rankscores.Average(), AvgRd, MidpointRounding.AwayFromZero));
                                        #region 算低標的中點
                                        middleIndex = rankscores.Count - 1;
                                        count = 1;
                                        score = rankscores[middleIndex];
                                        while (middleIndex >= 0)
                                        {
                                             if (score != rankscores[middleIndex])
                                             {
                                                  if (count * 2 >= rankscores.Count) break;
                                                  score = rankscores[middleIndex];
                                             }
                                             middleIndex--;
                                             count++;
                                        }
                                        if (middleIndex < 0)
                                        {
                                             middleIndex++;
                                             count--;
                                        }
                                        #endregion
                                        analytics.Add(k + "^^^低標", Math.Round(rankscores.GetRange(middleIndex, count).Average(), AvgRd, MidpointRounding.AwayFromZero));
                                        //Compute the Average      
                                        var avg = (double)rankscores.Average();
                                        //Perform the Sum of (value-avg)_2_2      
                                        var sum = (double)rankscores.Sum(d => Math.Pow((double)d - avg, 2));
                                        //Put it all together      
                                        analytics.Add(k + "^^^標準差", Math.Round((decimal)Math.Sqrt((sum) / rankscores.Count()), AvgRd, MidpointRounding.AwayFromZero));
                                   }
                                   #region 計算級距
                                   int count90 = 0, count80 = 0, count70 = 0, count60 = 0, count50 = 0, count40 = 0, count30 = 0, count20 = 0, count10 = 0;
                                   int count100Up = 0, count90Up = 0, count80Up = 0, count70Up = 0, count60Up = 0, count50Up = 0, count40Up = 0, count30Up = 0, count20Up = 0, count10Up = 0;
                                   int count90Down = 0, count80Down = 0, count70Down = 0, count60Down = 0, count50Down = 0, count40Down = 0, count30Down = 0, count20Down = 0, count10Down = 0;
                                   foreach (var score in rankscores)
                                   {
                                        if (score >= 100)
                                             count100Up++;
                                        else if (score >= 90)
                                             count90++;
                                        else if (score >= 80)
                                             count80++;
                                        else if (score >= 70)
                                             count70++;
                                        else if (score >= 60)
                                             count60++;
                                        else if (score >= 50)
                                             count50++;
                                        else if (score >= 40)
                                             count40++;
                                        else if (score >= 30)
                                             count30++;
                                        else if (score >= 20)
                                             count20++;
                                        else if (score >= 10)
                                             count10++;
                                        else
                                             count10Down++;
                                   }
                                   count90Up = count100Up + count90;
                                   count80Up = count90Up + count80;
                                   count70Up = count80Up + count70;
                                   count60Up = count70Up + count60;
                                   count50Up = count60Up + count50;
                                   count40Up = count50Up + count40;
                                   count30Up = count40Up + count30;
                                   count20Up = count30Up + count20;
                                   count10Up = count20Up + count10;

                                   count20Down = count10Down + count10;
                                   count30Down = count20Down + count20;
                                   count40Down = count30Down + count30;
                                   count50Down = count40Down + count40;
                                   count60Down = count50Down + count50;
                                   count70Down = count60Down + count60;
                                   count80Down = count70Down + count70;
                                   count90Down = count80Down + count80;

                                   analytics.Add(k + "^^^count90", count90);
                                   analytics.Add(k + "^^^count80", count80);
                                   analytics.Add(k + "^^^count70", count70);
                                   analytics.Add(k + "^^^count60", count60);
                                   analytics.Add(k + "^^^count50", count50);
                                   analytics.Add(k + "^^^count40", count40);
                                   analytics.Add(k + "^^^count30", count30);
                                   analytics.Add(k + "^^^count20", count20);
                                   analytics.Add(k + "^^^count10", count10);
                                   analytics.Add(k + "^^^count100Up", count100Up);
                                   analytics.Add(k + "^^^count90Up", count90Up);
                                   analytics.Add(k + "^^^count80Up", count80Up);
                                   analytics.Add(k + "^^^count70Up", count70Up);
                                   analytics.Add(k + "^^^count60Up", count60Up);
                                   analytics.Add(k + "^^^count50Up", count50Up);
                                   analytics.Add(k + "^^^count40Up", count40Up);
                                   analytics.Add(k + "^^^count30Up", count30Up);
                                   analytics.Add(k + "^^^count20Up", count20Up);
                                   analytics.Add(k + "^^^count10Up", count10Up);
                                   analytics.Add(k + "^^^count90Down", count90Down);
                                   analytics.Add(k + "^^^count80Down", count80Down);
                                   analytics.Add(k + "^^^count70Down", count70Down);
                                   analytics.Add(k + "^^^count60Down", count60Down);
                                   analytics.Add(k + "^^^count50Down", count50Down);
                                   analytics.Add(k + "^^^count40Down", count40Down);
                                   analytics.Add(k + "^^^count30Down", count30Down);
                                   analytics.Add(k + "^^^count20Down", count20Down);
                                   analytics.Add(k + "^^^count10Down", count10Down);
                                   #endregion
                              }
                              #endregion

                              // 先取得 K12 StudentRec,因為後面透過 k12.data 取資料有的傳入ID,有的傳入 Record 有點亂
                              List<K12.Data.StudentRecord> StudRecList = new List<K12.Data.StudentRecord>();
                              List<string> StudIDList = (from data in studentRecords select data.StudentID).ToList();
                              StudRecList = K12.Data.Student.SelectByIDs(StudIDList);

                              // 取得暫存資料 學習服務區間時數
                              Dictionary<string, Dictionary<string, decimal>> ServiceLearningByDateDict = Utility.GetServiceLearningByDate(StudIDList, form.GetBeginDate(), form.GetEndDate());
                              Dictionary<string, List<DataRow>> ServiceLearningDetailByDateDict = Utility.GetServiceLearningDetailByDate(StudIDList, form.GetBeginDate(), form.GetEndDate());

                              // 取得缺曠
                              Dictionary<string, Dictionary<string, int>> AttendanceCountDict = Utility.GetAttendanceCountByDate(StudRecList, form.GetBeginDate(), form.GetEndDate());
                              Dictionary<string, List<K12.Data.AttendanceRecord>> AttendanceDetailDict = Utility.GetAttendanceDetailByDate(StudRecList, form.GetBeginDate(), form.GetEndDate());

                              // 取得獎懲
                              Dictionary<string, Dictionary<string, int>> DisciplineCountDict = Utility.GetDisciplineCountByDate(StudIDList, form.GetBeginDate(), form.GetEndDate());
                              Dictionary<string, List<K12.Data.DisciplineRecord>> DisciplinedetailDict = Utility.GetDisciplineDetailByDate(StudIDList, form.GetBeginDate(), form.GetEndDate());

                              List<K12.Data.PeriodMappingInfo> PeriodMappingList = K12.Data.PeriodMapping.SelectAll();
                              // 節次>類別
                              Dictionary<string, string> PeriodMappingDict = new Dictionary<string, string>();
                              foreach (K12.Data.PeriodMappingInfo rec in PeriodMappingList)
                              {
                                   if (!PeriodMappingDict.ContainsKey(rec.Name))
                                        PeriodMappingDict.Add(rec.Name, rec.Type);
                              }

                              //// 其它epost
                              //_dtEpost.Columns.Add("大功");
                              //_dtEpost.Columns.Add("小功");
                              //_dtEpost.Columns.Add("嘉獎");
                              //_dtEpost.Columns.Add("大過");
                              //_dtEpost.Columns.Add("小過");
                              //_dtEpost.Columns.Add("警告");
                              //_dtEpost.Columns.Add("留校察看");
                              //_dtEpost.Columns.Add("班級人數");
                              //_dtEpost.Columns.Add("科人數");
                              //_dtEpost.Columns.Add("類組人數");
                              //_dtEpost.Columns.Add("缺曠獎懲統計期間");


                              bkw.ReportProgress(70);
                              elseReady.WaitOne();
                              progressCount = 0;
                              #region 填入資料表
                              foreach (var stuRec in studentRecords)
                              {
                                   string studentID = stuRec.StudentID;
                                   string gradeYear = (stuRec.RefClass == null ? "" : "" + stuRec.RefClass.GradeYear);
                                   DataRow row = table.NewRow();

                                   // 這區段是新增功能資料
                                   // 畫面上開始結束日期
                                   row["開始日期"] = form.GetBeginDate().ToShortDateString();
                                   row["結束日期"] = form.GetEndDate().ToShortDateString();

                                   if (ServiceLearningByDateDict.ContainsKey(studentID))
                                   {
                                        // 處理學生學習服務時數
                                        int idx = 1;
                                        foreach (KeyValuePair<string, decimal> data in ServiceLearningByDateDict[studentID])
                                        {
                                             row["學習服務區間時數" + idx] = data.Key + " 時數：" + data.Value;
                                             idx++;
                                        }
                                   }
                                   // 處理獎懲
                                   if (DisciplineCountDict.ContainsKey(studentID))
                                   {
                                        foreach (KeyValuePair<string, int> data in DisciplineCountDict[studentID])
                                        {
                                             switch (data.Key)
                                             {
                                                  case "大功": row["大功區間統計"] = data.Value; break;
                                                  case "小功": row["小功區間統計"] = data.Value; break;
                                                  case "嘉獎": row["嘉獎區間統計"] = data.Value; break;
                                                  case "大過": row["大過區間統計"] = data.Value; break;
                                                  case "小過": row["小過區間統計"] = data.Value; break;
                                                  case "警告": row["警告區間統計"] = data.Value; break;

                                                  case "留校":
                                                       if (data.Value > 0)
                                                            row["留校察看區間"] = "是";
                                                       else
                                                            row["留校察看區間"] = "";
                                                       break;
                                             }
                                        }
                                   }

                                   // 處理缺曠
                                   if (AttendanceCountDict.ContainsKey(studentID))
                                   {
                                        foreach (KeyValuePair<string, int> data in AttendanceCountDict[studentID])
                                        {
                                             if (table.Columns.Contains(data.Key))
                                                  row[data.Key] = data.Value;
                                        }
                                   }

                                   // 處理缺曠區間明細
                                   if (AttendanceDetailDict.ContainsKey(studentID))
                                   {
                                        int idx = 1;
                                        foreach (K12.Data.AttendanceRecord rec in AttendanceDetailDict[studentID])
                                        {

                                             foreach (K12.Data.AttendancePeriod per in rec.PeriodDetail)
                                             {
                                                  if (PeriodMappingDict.ContainsKey(per.Period))
                                                  {
                                                       if (idx <= conf.AttendanceDetailLimit)
                                                       {

                                                            row["缺曠區間明細日期" + idx] = rec.OccurDate.ToShortDateString();
                                                            row["缺曠區間明細內容" + idx] = PeriodMappingDict[per.Period] + ":" + per.AbsenceType + " (節次：" + per.Period + ")";
                                                            //row["缺曠區間明細C" + idx] = rec.                                        
                                                            idx++;
                                                       }
                                                  }
                                             }

                                        }


                                   }

                                   // 處理獎懲區間明細
                                   if (DisciplinedetailDict.ContainsKey(studentID))
                                   {
                                        int idx = 1;

                                        foreach (K12.Data.DisciplineRecord data in DisciplinedetailDict[studentID])
                                        {
                                             if (idx <= conf.DisciplineDetailLimit)
                                             {
                                                  // 獎懲區間明細 A:日期,B:類別支數,C:事由
                                                  row["獎懲區間明細日期" + idx] = data.OccurDate.ToShortDateString();

                                                  List<string> strTypeList = new List<string>();
                                                  if (data.MeritFlag == "1")
                                                  {
                                                       if (data.MeritA.HasValue)
                                                            strTypeList.Add("大功：" + data.MeritA.Value);

                                                       if (data.MeritB.HasValue)
                                                            strTypeList.Add("小功：" + data.MeritB.Value);

                                                       if (data.MeritC.HasValue)
                                                            strTypeList.Add("嘉獎：" + data.MeritC.Value);

                                                  }
                                                  if (data.MeritFlag == "0")
                                                  {
                                                       if (data.Cleared != "是")
                                                       {
                                                            if (data.DemeritA.HasValue)
                                                                 strTypeList.Add("大過：" + data.DemeritA.Value);

                                                            if (data.DemeritB.HasValue)
                                                                 strTypeList.Add("小過：" + data.DemeritB.Value);

                                                            if (data.DemeritC.HasValue)
                                                                 strTypeList.Add("警告：" + data.DemeritC.Value);
                                                       }
                                                  }
                                                  if (data.MeritFlag == "2")
                                                       strTypeList.Add("留校察看：是");

                                                  row["獎懲區間明細類別支數" + idx] = string.Join(",", strTypeList.ToArray());
                                                  row["獎懲區間明細事由" + idx] = data.Reason;
                                                  idx++;
                                             }
                                        }

                                   }

                                   // 處理學習服務
                                   if (ServiceLearningDetailByDateDict.ContainsKey(studentID))
                                   {
                                        int idx = 1;
                                        foreach (DataRow dr in ServiceLearningDetailByDateDict[studentID])
                                        {
                                             if (idx <= conf.ServiceLearningDetailLimit)
                                             {
                                                  //  學習服務區間明細 A:日期,B:內容,C:時數
                                                  DateTime dt;
                                                  decimal hr;
                                                  string cont = dr["reason"].ToString();

                                                  if (DateTime.TryParse(dr["occur_date"].ToString(), out dt))
                                                       row["學習服務區間明細日期" + idx] = dt.ToShortDateString();
                                                  else
                                                       row["學習服務區間明細日期" + idx] = "";
                                                  row["學習服務區間明細內容" + idx] = cont;

                                                  if (decimal.TryParse(dr["hours"].ToString(), out hr))
                                                       row["學習服務區間明細時數" + idx] = hr;
                                                  else
                                                       row["學習服務區間明細時數" + idx] = "";
                                                  idx++;
                                             }
                                        }
                                   }

                                   #region 基本資料
                                   row["學校名稱"] = SmartSchool.Customization.Data.SystemInformation.SchoolChineseName;
                                   row["學校地址"] = SmartSchool.Customization.Data.SystemInformation.Address;
                                   row["學校電話"] = SmartSchool.Customization.Data.SystemInformation.Telephone;
                                   row["收件人地址"] = stuRec.ContactInfo.MailingAddress.FullAddress != "" ?
                                                       stuRec.ContactInfo.MailingAddress.FullAddress : stuRec.ContactInfo.PermanentAddress.FullAddress;
                                   row["收件人"] = stuRec.ParentInfo.CustodianName != "" ? stuRec.ParentInfo.CustodianName :
                                                       (stuRec.ParentInfo.FatherName != "" ? stuRec.ParentInfo.FatherName :
                                                           (stuRec.ParentInfo.FatherName != "" ? stuRec.ParentInfo.MotherName : stuRec.StudentName));
                                   //«通訊地址»«通訊地址郵遞區號»«通訊地址內容»
                                   //«戶籍地址»«戶籍地址郵遞區號»«戶籍地址內容»
                                   //«監護人»«父親»«母親»«科別名稱»
                                   row["通訊地址"] = stuRec.ContactInfo.MailingAddress.FullAddress;
                                   row["通訊地址郵遞區號"] = stuRec.ContactInfo.MailingAddress.ZipCode;
                                   row["通訊地址內容"] = stuRec.ContactInfo.MailingAddress.County + stuRec.ContactInfo.MailingAddress.Town + stuRec.ContactInfo.MailingAddress.DetailAddress;
                                   row["戶籍地址"] = stuRec.ContactInfo.PermanentAddress.FullAddress;
                                   row["戶籍地址郵遞區號"] = stuRec.ContactInfo.PermanentAddress.ZipCode;
                                   row["戶籍地址內容"] = stuRec.ContactInfo.PermanentAddress.County + stuRec.ContactInfo.PermanentAddress.Town + stuRec.ContactInfo.PermanentAddress.DetailAddress;
                                   row["監護人"] = stuRec.ParentInfo.CustodianName;
                                   row["父親"] = stuRec.ParentInfo.FatherName;
                                   row["母親"] = stuRec.ParentInfo.MotherName;
                                   row["科別名稱"] = stuRec.Department;
                                   row["試別"] = conf.ExamRecord.Name;
                                   row["家長代碼"] = parentCode.ContainsKey(studentID) ? parentCode[studentID] : "";

                                   row["學年度"] = conf.SchoolYear;
                                   row["學期"] = conf.Semester;
                                   row["班級科別名稱"] = stuRec.RefClass == null ? "" : stuRec.RefClass.Department;
                                   row["班級"] = stuRec.RefClass == null ? "" : stuRec.RefClass.ClassName;
                                   row["班導師"] = (stuRec.RefClass == null || stuRec.RefClass.RefTeacher == null) ? "" : stuRec.RefClass.RefTeacher.TeacherName;
                                   row["座號"] = stuRec.SeatNo;
                                   row["學號"] = stuRec.StudentNumber;
                                   row["姓名"] = stuRec.StudentName;
                                   row["定期評量"] = conf.ExamRecord.Name;

                                   if (conf.ScoreCurDate != null)
                                   {
                                   row["成績校正年"] = conf.ScoreCurDate.Year;
                                   row["成績校正月"] = conf.ScoreCurDate.Month;
                                   row["成績校正日"] = conf.ScoreCurDate.Day;
                                   row["成績校正民國年"] = (conf.ScoreCurDate.Year - 1911);
                                   row["成績校正日期"] = conf.ScoreCurDate.ToShortDateString();
                                   }

                                   #endregion
                                   #region 成績資料
                                   #region 各科成績資料
                                   #region 整理科目順序
                                   List<string> subjectNameList = new List<string>();
                                   if (studentExamSores.ContainsKey(stuRec.StudentID))
                                   {
                                        foreach (var subjectName in studentExamSores[studentID].Keys)
                                        {
                                             foreach (var courseID in studentExamSores[studentID][subjectName].Keys)
                                             {
                                                  if (conf.PrintSubjectList.Contains(subjectName))
                                                  {
                                                       subjectNameList.Add(subjectName);
                                                  }
                                             }
                                        }
                                   }
                                   subjectNameList.Sort(new StringComparer("國文"
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
                                   int subjectIndex = 1;
                                   // 學期科目與定期評量
                                   foreach (string subjectName in subjectNameList)
                                   {
                                        if (subjectIndex <= conf.SubjectLimit)
                                        {
                                             decimal? subjectNumber = null;
                                             // 檢查畫面上定期評量列印科目
                                             if (conf.PrintSubjectList.Contains(subjectName))
                                             {
                                                  if (studentExamSores.ContainsKey(studentID))
                                                  {
                                                       if (studentExamSores[studentID].ContainsKey(subjectName))
                                                       {
                                                            foreach (var courseID in studentExamSores[studentID][subjectName].Keys)
                                                            {
                                                                 #region 評量成績
                                                                 var sceTakeRecord = studentExamSores[studentID][subjectName][courseID];
                                                                 if (sceTakeRecord != null)
                                                                 {//有輸入
                                                                      decimal level;
                                                                      subjectNumber = decimal.TryParse(sceTakeRecord.SubjectLevel, out level) ? (decimal?)level : null;
                                                                      row["科目名稱" + subjectIndex] = sceTakeRecord.Subject + GetNumber(subjectNumber);
                                                                      row["學分數" + subjectIndex] = sceTakeRecord.CreditDec();
                                                                      row["科目成績" + subjectIndex] = sceTakeRecord.SpecialCase == "" ? ("" + sceTakeRecord.ExamScore) : sceTakeRecord.SpecialCase;
                                                                      #region 班排名及落點分析
                                                                      if (stuRec.RefClass != null)
                                                                      {
                                                                           key = "班排名" + stuRec.RefClass.ClassID + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                                                           {
                                                                                row["班排名" + subjectIndex] = ranks[key].IndexOf(sceTakeRecord.ExamScore) + 1;
                                                                                row["班排名母數" + subjectIndex] = ranks[key].Count;
                                                                           }
                                                                           if (rankStudents.ContainsKey(key))
                                                                           {
                                                                                row["班高標" + subjectIndex] = analytics[key + "^^^高標"];
                                                                                row["班均標" + subjectIndex] = analytics[key + "^^^均標"];
                                                                                row["班低標" + subjectIndex] = analytics[key + "^^^低標"];
                                                                                row["班標準差" + subjectIndex] = analytics[key + "^^^標準差"];
                                                                                row["班組距" + subjectIndex + "count90"] = analytics[key + "^^^count90"];
                                                                                row["班組距" + subjectIndex + "count80"] = analytics[key + "^^^count80"];
                                                                                row["班組距" + subjectIndex + "count70"] = analytics[key + "^^^count70"];
                                                                                row["班組距" + subjectIndex + "count60"] = analytics[key + "^^^count60"];
                                                                                row["班組距" + subjectIndex + "count50"] = analytics[key + "^^^count50"];
                                                                                row["班組距" + subjectIndex + "count40"] = analytics[key + "^^^count40"];
                                                                                row["班組距" + subjectIndex + "count30"] = analytics[key + "^^^count30"];
                                                                                row["班組距" + subjectIndex + "count20"] = analytics[key + "^^^count20"];
                                                                                row["班組距" + subjectIndex + "count10"] = analytics[key + "^^^count10"];
                                                                                row["班組距" + subjectIndex + "count100Up"] = analytics[key + "^^^count100Up"];
                                                                                row["班組距" + subjectIndex + "count90Up"] = analytics[key + "^^^count90Up"];
                                                                                row["班組距" + subjectIndex + "count80Up"] = analytics[key + "^^^count80Up"];
                                                                                row["班組距" + subjectIndex + "count70Up"] = analytics[key + "^^^count70Up"];
                                                                                row["班組距" + subjectIndex + "count60Up"] = analytics[key + "^^^count60Up"];
                                                                                row["班組距" + subjectIndex + "count50Up"] = analytics[key + "^^^count50Up"];
                                                                                row["班組距" + subjectIndex + "count40Up"] = analytics[key + "^^^count40Up"];
                                                                                row["班組距" + subjectIndex + "count30Up"] = analytics[key + "^^^count30Up"];
                                                                                row["班組距" + subjectIndex + "count20Up"] = analytics[key + "^^^count20Up"];
                                                                                row["班組距" + subjectIndex + "count10Up"] = analytics[key + "^^^count10Up"];
                                                                                row["班組距" + subjectIndex + "count90Down"] = analytics[key + "^^^count90Down"];
                                                                                row["班組距" + subjectIndex + "count80Down"] = analytics[key + "^^^count80Down"];
                                                                                row["班組距" + subjectIndex + "count70Down"] = analytics[key + "^^^count70Down"];
                                                                                row["班組距" + subjectIndex + "count60Down"] = analytics[key + "^^^count60Down"];
                                                                                row["班組距" + subjectIndex + "count50Down"] = analytics[key + "^^^count50Down"];
                                                                                row["班組距" + subjectIndex + "count40Down"] = analytics[key + "^^^count40Down"];
                                                                                row["班組距" + subjectIndex + "count30Down"] = analytics[key + "^^^count30Down"];
                                                                                row["班組距" + subjectIndex + "count20Down"] = analytics[key + "^^^count20Down"];
                                                                                row["班組距" + subjectIndex + "count10Down"] = analytics[key + "^^^count10Down"];
                                                                           }
                                                                      }
                                                                      #endregion
                                                                      #region 科排名及落點分析
                                                                      if (stuRec.Department != "")
                                                                      {
                                                                           key = "科排名" + stuRec.Department + "^^^" + gradeYear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                                                           {
                                                                                row["科排名" + subjectIndex] = ranks[key].IndexOf(sceTakeRecord.ExamScore) + 1;
                                                                                row["科排名母數" + subjectIndex] = ranks[key].Count;
                                                                           }
                                                                           if (rankStudents.ContainsKey(key))
                                                                           {
                                                                                row["科高標" + subjectIndex] = analytics[key + "^^^高標"];
                                                                                row["科均標" + subjectIndex] = analytics[key + "^^^均標"];
                                                                                row["科低標" + subjectIndex] = analytics[key + "^^^低標"];
                                                                                row["科標準差" + subjectIndex] = analytics[key + "^^^標準差"];
                                                                                row["科組距" + subjectIndex + "count90"] = analytics[key + "^^^count90"];
                                                                                row["科組距" + subjectIndex + "count80"] = analytics[key + "^^^count80"];
                                                                                row["科組距" + subjectIndex + "count70"] = analytics[key + "^^^count70"];
                                                                                row["科組距" + subjectIndex + "count60"] = analytics[key + "^^^count60"];
                                                                                row["科組距" + subjectIndex + "count50"] = analytics[key + "^^^count50"];
                                                                                row["科組距" + subjectIndex + "count40"] = analytics[key + "^^^count40"];
                                                                                row["科組距" + subjectIndex + "count30"] = analytics[key + "^^^count30"];
                                                                                row["科組距" + subjectIndex + "count20"] = analytics[key + "^^^count20"];
                                                                                row["科組距" + subjectIndex + "count10"] = analytics[key + "^^^count10"];
                                                                                row["科組距" + subjectIndex + "count100Up"] = analytics[key + "^^^count100Up"];
                                                                                row["科組距" + subjectIndex + "count90Up"] = analytics[key + "^^^count90Up"];
                                                                                row["科組距" + subjectIndex + "count80Up"] = analytics[key + "^^^count80Up"];
                                                                                row["科組距" + subjectIndex + "count70Up"] = analytics[key + "^^^count70Up"];
                                                                                row["科組距" + subjectIndex + "count60Up"] = analytics[key + "^^^count60Up"];
                                                                                row["科組距" + subjectIndex + "count50Up"] = analytics[key + "^^^count50Up"];
                                                                                row["科組距" + subjectIndex + "count40Up"] = analytics[key + "^^^count40Up"];
                                                                                row["科組距" + subjectIndex + "count30Up"] = analytics[key + "^^^count30Up"];
                                                                                row["科組距" + subjectIndex + "count20Up"] = analytics[key + "^^^count20Up"];
                                                                                row["科組距" + subjectIndex + "count10Up"] = analytics[key + "^^^count10Up"];
                                                                                row["科組距" + subjectIndex + "count90Down"] = analytics[key + "^^^count90Down"];
                                                                                row["科組距" + subjectIndex + "count80Down"] = analytics[key + "^^^count80Down"];
                                                                                row["科組距" + subjectIndex + "count70Down"] = analytics[key + "^^^count70Down"];
                                                                                row["科組距" + subjectIndex + "count60Down"] = analytics[key + "^^^count60Down"];
                                                                                row["科組距" + subjectIndex + "count50Down"] = analytics[key + "^^^count50Down"];
                                                                                row["科組距" + subjectIndex + "count40Down"] = analytics[key + "^^^count40Down"];
                                                                                row["科組距" + subjectIndex + "count30Down"] = analytics[key + "^^^count30Down"];
                                                                                row["科組距" + subjectIndex + "count20Down"] = analytics[key + "^^^count20Down"];
                                                                                row["科組距" + subjectIndex + "count10Down"] = analytics[key + "^^^count10Down"];
                                                                           }
                                                                      }
                                                                      #endregion
                                                                      #region 全校排名及落點分析
                                                                      key = "全校排名" + gradeYear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                      if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                                                      {
                                                                           row["全校排名" + subjectIndex] = ranks[key].IndexOf(sceTakeRecord.ExamScore) + 1;
                                                                           row["全校排名母數" + subjectIndex] = ranks[key].Count;
                                                                      }
                                                                      if (rankStudents.ContainsKey(key))
                                                                      {
                                                                           row["校高標" + subjectIndex] = analytics[key + "^^^高標"];
                                                                           row["校均標" + subjectIndex] = analytics[key + "^^^均標"];
                                                                           row["校低標" + subjectIndex] = analytics[key + "^^^低標"];
                                                                           row["校標準差" + subjectIndex] = analytics[key + "^^^標準差"];
                                                                           row["校組距" + subjectIndex + "count90"] = analytics[key + "^^^count90"];
                                                                           row["校組距" + subjectIndex + "count80"] = analytics[key + "^^^count80"];
                                                                           row["校組距" + subjectIndex + "count70"] = analytics[key + "^^^count70"];
                                                                           row["校組距" + subjectIndex + "count60"] = analytics[key + "^^^count60"];
                                                                           row["校組距" + subjectIndex + "count50"] = analytics[key + "^^^count50"];
                                                                           row["校組距" + subjectIndex + "count40"] = analytics[key + "^^^count40"];
                                                                           row["校組距" + subjectIndex + "count30"] = analytics[key + "^^^count30"];
                                                                           row["校組距" + subjectIndex + "count20"] = analytics[key + "^^^count20"];
                                                                           row["校組距" + subjectIndex + "count10"] = analytics[key + "^^^count10"];
                                                                           row["校組距" + subjectIndex + "count100Up"] = analytics[key + "^^^count100Up"];
                                                                           row["校組距" + subjectIndex + "count90Up"] = analytics[key + "^^^count90Up"];
                                                                           row["校組距" + subjectIndex + "count80Up"] = analytics[key + "^^^count80Up"];
                                                                           row["校組距" + subjectIndex + "count70Up"] = analytics[key + "^^^count70Up"];
                                                                           row["校組距" + subjectIndex + "count60Up"] = analytics[key + "^^^count60Up"];
                                                                           row["校組距" + subjectIndex + "count50Up"] = analytics[key + "^^^count50Up"];
                                                                           row["校組距" + subjectIndex + "count40Up"] = analytics[key + "^^^count40Up"];
                                                                           row["校組距" + subjectIndex + "count30Up"] = analytics[key + "^^^count30Up"];
                                                                           row["校組距" + subjectIndex + "count20Up"] = analytics[key + "^^^count20Up"];
                                                                           row["校組距" + subjectIndex + "count10Up"] = analytics[key + "^^^count10Up"];
                                                                           row["校組距" + subjectIndex + "count90Down"] = analytics[key + "^^^count90Down"];
                                                                           row["校組距" + subjectIndex + "count80Down"] = analytics[key + "^^^count80Down"];
                                                                           row["校組距" + subjectIndex + "count70Down"] = analytics[key + "^^^count70Down"];
                                                                           row["校組距" + subjectIndex + "count60Down"] = analytics[key + "^^^count60Down"];
                                                                           row["校組距" + subjectIndex + "count50Down"] = analytics[key + "^^^count50Down"];
                                                                           row["校組距" + subjectIndex + "count40Down"] = analytics[key + "^^^count40Down"];
                                                                           row["校組距" + subjectIndex + "count30Down"] = analytics[key + "^^^count30Down"];
                                                                           row["校組距" + subjectIndex + "count20Down"] = analytics[key + "^^^count20Down"];
                                                                           row["校組距" + subjectIndex + "count10Down"] = analytics[key + "^^^count10Down"];
                                                                      }
                                                                      #endregion
                                                                      #region 類別1排名及落點分析
                                                                      if (studentTag1Group.ContainsKey(studentID) && conf.TagRank1SubjectList.Contains(subjectName))
                                                                      {
                                                                           key = "類別1排名" + studentTag1Group[studentID] + "^^^" + gradeYear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                                                           {
                                                                                row["類別1排名" + subjectIndex] = ranks[key].IndexOf(sceTakeRecord.ExamScore) + 1;
                                                                                row["類別1排名母數" + subjectIndex] = ranks[key].Count;
                                                                           }
                                                                           if (rankStudents.ContainsKey(key))
                                                                           {
                                                                                row["類1高標" + subjectIndex] = analytics[key + "^^^高標"];
                                                                                row["類1均標" + subjectIndex] = analytics[key + "^^^均標"];
                                                                                row["類1低標" + subjectIndex] = analytics[key + "^^^低標"];
                                                                                row["類1標準差" + subjectIndex] = analytics[key + "^^^標準差"];
                                                                                row["類1組距" + subjectIndex + "count90"] = analytics[key + "^^^count90"];
                                                                                row["類1組距" + subjectIndex + "count80"] = analytics[key + "^^^count80"];
                                                                                row["類1組距" + subjectIndex + "count70"] = analytics[key + "^^^count70"];
                                                                                row["類1組距" + subjectIndex + "count60"] = analytics[key + "^^^count60"];
                                                                                row["類1組距" + subjectIndex + "count50"] = analytics[key + "^^^count50"];
                                                                                row["類1組距" + subjectIndex + "count40"] = analytics[key + "^^^count40"];
                                                                                row["類1組距" + subjectIndex + "count30"] = analytics[key + "^^^count30"];
                                                                                row["類1組距" + subjectIndex + "count20"] = analytics[key + "^^^count20"];
                                                                                row["類1組距" + subjectIndex + "count10"] = analytics[key + "^^^count10"];
                                                                                row["類1組距" + subjectIndex + "count100Up"] = analytics[key + "^^^count100Up"];
                                                                                row["類1組距" + subjectIndex + "count90Up"] = analytics[key + "^^^count90Up"];
                                                                                row["類1組距" + subjectIndex + "count80Up"] = analytics[key + "^^^count80Up"];
                                                                                row["類1組距" + subjectIndex + "count70Up"] = analytics[key + "^^^count70Up"];
                                                                                row["類1組距" + subjectIndex + "count60Up"] = analytics[key + "^^^count60Up"];
                                                                                row["類1組距" + subjectIndex + "count50Up"] = analytics[key + "^^^count50Up"];
                                                                                row["類1組距" + subjectIndex + "count40Up"] = analytics[key + "^^^count40Up"];
                                                                                row["類1組距" + subjectIndex + "count30Up"] = analytics[key + "^^^count30Up"];
                                                                                row["類1組距" + subjectIndex + "count20Up"] = analytics[key + "^^^count20Up"];
                                                                                row["類1組距" + subjectIndex + "count10Up"] = analytics[key + "^^^count10Up"];
                                                                                row["類1組距" + subjectIndex + "count90Down"] = analytics[key + "^^^count90Down"];
                                                                                row["類1組距" + subjectIndex + "count80Down"] = analytics[key + "^^^count80Down"];
                                                                                row["類1組距" + subjectIndex + "count70Down"] = analytics[key + "^^^count70Down"];
                                                                                row["類1組距" + subjectIndex + "count60Down"] = analytics[key + "^^^count60Down"];
                                                                                row["類1組距" + subjectIndex + "count50Down"] = analytics[key + "^^^count50Down"];
                                                                                row["類1組距" + subjectIndex + "count40Down"] = analytics[key + "^^^count40Down"];
                                                                                row["類1組距" + subjectIndex + "count30Down"] = analytics[key + "^^^count30Down"];
                                                                                row["類1組距" + subjectIndex + "count20Down"] = analytics[key + "^^^count20Down"];
                                                                                row["類1組距" + subjectIndex + "count10Down"] = analytics[key + "^^^count10Down"];
                                                                           }
                                                                      }
                                                                      #endregion
                                                                      #region 類別2排名及落點分析
                                                                      if (studentTag2Group.ContainsKey(studentID) && conf.TagRank2SubjectList.Contains(subjectName))
                                                                      {
                                                                           key = "類別2排名" + studentTag2Group[studentID] + "^^^" + gradeYear + "^^^" + sceTakeRecord.Subject + "^^^" + sceTakeRecord.SubjectLevel;
                                                                           if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                                                           {
                                                                                row["類別2排名" + subjectIndex] = ranks[key].IndexOf(sceTakeRecord.ExamScore) + 1;
                                                                                row["類別2排名母數" + subjectIndex] = ranks[key].Count;
                                                                           }
                                                                           if (rankStudents.ContainsKey(key))
                                                                           {
                                                                                row["類2高標" + subjectIndex] = analytics[key + "^^^高標"];
                                                                                row["類2均標" + subjectIndex] = analytics[key + "^^^均標"];
                                                                                row["類2低標" + subjectIndex] = analytics[key + "^^^低標"];
                                                                                row["類2標準差" + subjectIndex] = analytics[key + "^^^標準差"];
                                                                                row["類2組距" + subjectIndex + "count90"] = analytics[key + "^^^count90"];
                                                                                row["類2組距" + subjectIndex + "count80"] = analytics[key + "^^^count80"];
                                                                                row["類2組距" + subjectIndex + "count70"] = analytics[key + "^^^count70"];
                                                                                row["類2組距" + subjectIndex + "count60"] = analytics[key + "^^^count60"];
                                                                                row["類2組距" + subjectIndex + "count50"] = analytics[key + "^^^count50"];
                                                                                row["類2組距" + subjectIndex + "count40"] = analytics[key + "^^^count40"];
                                                                                row["類2組距" + subjectIndex + "count30"] = analytics[key + "^^^count30"];
                                                                                row["類2組距" + subjectIndex + "count20"] = analytics[key + "^^^count20"];
                                                                                row["類2組距" + subjectIndex + "count10"] = analytics[key + "^^^count10"];
                                                                                row["類2組距" + subjectIndex + "count100Up"] = analytics[key + "^^^count100Up"];
                                                                                row["類2組距" + subjectIndex + "count90Up"] = analytics[key + "^^^count90Up"];
                                                                                row["類2組距" + subjectIndex + "count80Up"] = analytics[key + "^^^count80Up"];
                                                                                row["類2組距" + subjectIndex + "count70Up"] = analytics[key + "^^^count70Up"];
                                                                                row["類2組距" + subjectIndex + "count60Up"] = analytics[key + "^^^count60Up"];
                                                                                row["類2組距" + subjectIndex + "count50Up"] = analytics[key + "^^^count50Up"];
                                                                                row["類2組距" + subjectIndex + "count40Up"] = analytics[key + "^^^count40Up"];
                                                                                row["類2組距" + subjectIndex + "count30Up"] = analytics[key + "^^^count30Up"];
                                                                                row["類2組距" + subjectIndex + "count20Up"] = analytics[key + "^^^count20Up"];
                                                                                row["類2組距" + subjectIndex + "count10Up"] = analytics[key + "^^^count10Up"];
                                                                                row["類2組距" + subjectIndex + "count90Down"] = analytics[key + "^^^count90Down"];
                                                                                row["類2組距" + subjectIndex + "count80Down"] = analytics[key + "^^^count80Down"];
                                                                                row["類2組距" + subjectIndex + "count70Down"] = analytics[key + "^^^count70Down"];
                                                                                row["類2組距" + subjectIndex + "count60Down"] = analytics[key + "^^^count60Down"];
                                                                                row["類2組距" + subjectIndex + "count50Down"] = analytics[key + "^^^count50Down"];
                                                                                row["類2組距" + subjectIndex + "count40Down"] = analytics[key + "^^^count40Down"];
                                                                                row["類2組距" + subjectIndex + "count30Down"] = analytics[key + "^^^count30Down"];
                                                                                row["類2組距" + subjectIndex + "count20Down"] = analytics[key + "^^^count20Down"];
                                                                                row["類2組距" + subjectIndex + "count10Down"] = analytics[key + "^^^count10Down"];
                                                                           }
                                                                      }
                                                                      #endregion
                                                                 }
                                                                 else
                                                                 {//修課有該考試但沒有成績資料
                                                                      var courseRecs = accessHelper.CourseHelper.GetCourse(courseID);
                                                                      if (courseRecs.Count > 0)
                                                                      {
                                                                           var courseRec = courseRecs[0];
                                                                           decimal level;
                                                                           subjectNumber = decimal.TryParse(courseRec.SubjectLevel, out level) ? (decimal?)level : null;
                                                                           row["科目名稱" + subjectIndex] = courseRec.Subject + GetNumber(subjectNumber);
                                                                           row["學分數" + subjectIndex] = courseRec.CreditDec();
                                                                           row["科目成績" + subjectIndex] = "未輸入";
                                                                      }
                                                                 }
                                                                 #endregion
                                                                 #region 參考成績
                                                                 if (studentRefExamSores.ContainsKey(studentID) && studentRefExamSores[studentID].ContainsKey(courseID))
                                                                 {
                                                                      row["前次成績" + subjectIndex] =
                                                                          studentRefExamSores[studentID][courseID].SpecialCase == ""
                                                                          ? ("" + studentRefExamSores[studentID][courseID].ExamScore)
                                                                          : studentRefExamSores[studentID][courseID].SpecialCase;
                                                                 }
                                                                 #endregion
                                                                 studentExamSores[studentID][subjectName].Remove(courseID);
                                                                 break;
                                                            }
                                                       }
                                                  }
                                             }
                                             subjectIndex++;
                                        }
                                        else
                                        {
                                             //重要!!發現資料在樣板中印不下時一定要記錄起來，否則使用者自己不會去發現的
                                             if (!overflowRecords.Contains(stuRec))
                                                  overflowRecords.Add(stuRec);
                                        }
                                   }
                                   #endregion
                                   #region 總分
                                   if (studentPrintSubjectSum.ContainsKey(studentID))
                                   {
                                        row["總分"] = studentPrintSubjectSum[studentID];
                                        //總分班排名                                
                                        key = "總分班排名" + stuRec.RefClass.ClassID;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["總分班排名"] = ranks[key].IndexOf(studentPrintSubjectSum[studentID]) + 1;
                                             row["總分班排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["總分班高標"] = analytics[key + "^^^高標"];
                                             row["總分班均標"] = analytics[key + "^^^均標"];
                                             row["總分班低標"] = analytics[key + "^^^低標"];
                                             row["總分班標準差"] = analytics[key + "^^^標準差"];
                                             row["總分班組距count90"] = analytics[key + "^^^count90"];
                                             row["總分班組距count80"] = analytics[key + "^^^count80"];
                                             row["總分班組距count70"] = analytics[key + "^^^count70"];
                                             row["總分班組距count60"] = analytics[key + "^^^count60"];
                                             row["總分班組距count50"] = analytics[key + "^^^count50"];
                                             row["總分班組距count40"] = analytics[key + "^^^count40"];
                                             row["總分班組距count30"] = analytics[key + "^^^count30"];
                                             row["總分班組距count20"] = analytics[key + "^^^count20"];
                                             row["總分班組距count10"] = analytics[key + "^^^count10"];
                                             row["總分班組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["總分班組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["總分班組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["總分班組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["總分班組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["總分班組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["總分班組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["總分班組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["總分班組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["總分班組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["總分班組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["總分班組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["總分班組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["總分班組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["總分班組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["總分班組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["總分班組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["總分班組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["總分班組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        //總分科排名
                                        key = "總分科排名" + stuRec.Department + "^^^" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["總分科排名"] = ranks[key].IndexOf(studentPrintSubjectSum[studentID]) + 1;
                                             row["總分科排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["總分科高標"] = analytics[key + "^^^高標"];
                                             row["總分科均標"] = analytics[key + "^^^均標"];
                                             row["總分科低標"] = analytics[key + "^^^低標"];
                                             row["總分科標準差"] = analytics[key + "^^^標準差"];
                                             row["總分科組距count90"] = analytics[key + "^^^count90"];
                                             row["總分科組距count80"] = analytics[key + "^^^count80"];
                                             row["總分科組距count70"] = analytics[key + "^^^count70"];
                                             row["總分科組距count60"] = analytics[key + "^^^count60"];
                                             row["總分科組距count50"] = analytics[key + "^^^count50"];
                                             row["總分科組距count40"] = analytics[key + "^^^count40"];
                                             row["總分科組距count30"] = analytics[key + "^^^count30"];
                                             row["總分科組距count20"] = analytics[key + "^^^count20"];
                                             row["總分科組距count10"] = analytics[key + "^^^count10"];
                                             row["總分科組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["總分科組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["總分科組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["總分科組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["總分科組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["總分科組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["總分科組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["總分科組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["總分科組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["總分科組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["總分科組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["總分科組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["總分科組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["總分科組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["總分科組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["總分科組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["總分科組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["總分科組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["總分科組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        //總分全校排名
                                        key = "總分全校排名" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["總分全校排名"] = ranks[key].IndexOf(studentPrintSubjectSum[studentID]) + 1;
                                             row["總分全校排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["總分校高標"] = analytics[key + "^^^高標"];
                                             row["總分校均標"] = analytics[key + "^^^均標"];
                                             row["總分校低標"] = analytics[key + "^^^低標"];
                                             row["總分校標準差"] = analytics[key + "^^^標準差"];
                                             row["總分校組距count90"] = analytics[key + "^^^count90"];
                                             row["總分校組距count80"] = analytics[key + "^^^count80"];
                                             row["總分校組距count70"] = analytics[key + "^^^count70"];
                                             row["總分校組距count60"] = analytics[key + "^^^count60"];
                                             row["總分校組距count50"] = analytics[key + "^^^count50"];
                                             row["總分校組距count40"] = analytics[key + "^^^count40"];
                                             row["總分校組距count30"] = analytics[key + "^^^count30"];
                                             row["總分校組距count20"] = analytics[key + "^^^count20"];
                                             row["總分校組距count10"] = analytics[key + "^^^count10"];
                                             row["總分校組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["總分校組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["總分校組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["總分校組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["總分校組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["總分校組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["總分校組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["總分校組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["總分校組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["總分校組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["總分校組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["總分校組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["總分校組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["總分校組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["總分校組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["總分校組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["總分校組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["總分校組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["總分校組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                   }
                                   #endregion
                                   #region 平均
                                   if (studentPrintSubjectAvg.ContainsKey(studentID))
                                   {
                                        row["平均"] = studentPrintSubjectAvg[studentID];
                                        key = "平均班排名" + stuRec.RefClass.ClassID;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["平均班排名"] = ranks[key].IndexOf(studentPrintSubjectAvg[studentID]) + 1;
                                             row["平均班排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["平均班高標"] = analytics[key + "^^^高標"];
                                             row["平均班均標"] = analytics[key + "^^^均標"];
                                             row["平均班低標"] = analytics[key + "^^^低標"];
                                             row["平均班標準差"] = analytics[key + "^^^標準差"];
                                             row["平均班組距count90"] = analytics[key + "^^^count90"];
                                             row["平均班組距count80"] = analytics[key + "^^^count80"];
                                             row["平均班組距count70"] = analytics[key + "^^^count70"];
                                             row["平均班組距count60"] = analytics[key + "^^^count60"];
                                             row["平均班組距count50"] = analytics[key + "^^^count50"];
                                             row["平均班組距count40"] = analytics[key + "^^^count40"];
                                             row["平均班組距count30"] = analytics[key + "^^^count30"];
                                             row["平均班組距count20"] = analytics[key + "^^^count20"];
                                             row["平均班組距count10"] = analytics[key + "^^^count10"];
                                             row["平均班組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["平均班組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["平均班組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["平均班組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["平均班組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["平均班組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["平均班組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["平均班組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["平均班組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["平均班組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["平均班組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["平均班組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["平均班組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["平均班組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["平均班組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["平均班組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["平均班組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["平均班組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["平均班組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        key = "平均科排名" + stuRec.Department + "^^^" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["平均科排名"] = ranks[key].IndexOf(studentPrintSubjectAvg[studentID]) + 1;
                                             row["平均科排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["平均科高標"] = analytics[key + "^^^高標"];
                                             row["平均科均標"] = analytics[key + "^^^均標"];
                                             row["平均科低標"] = analytics[key + "^^^低標"];
                                             row["平均科標準差"] = analytics[key + "^^^標準差"];
                                             row["平均科組距count90"] = analytics[key + "^^^count90"];
                                             row["平均科組距count80"] = analytics[key + "^^^count80"];
                                             row["平均科組距count70"] = analytics[key + "^^^count70"];
                                             row["平均科組距count60"] = analytics[key + "^^^count60"];
                                             row["平均科組距count50"] = analytics[key + "^^^count50"];
                                             row["平均科組距count40"] = analytics[key + "^^^count40"];
                                             row["平均科組距count30"] = analytics[key + "^^^count30"];
                                             row["平均科組距count20"] = analytics[key + "^^^count20"];
                                             row["平均科組距count10"] = analytics[key + "^^^count10"];
                                             row["平均科組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["平均科組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["平均科組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["平均科組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["平均科組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["平均科組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["平均科組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["平均科組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["平均科組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["平均科組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["平均科組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["平均科組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["平均科組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["平均科組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["平均科組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["平均科組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["平均科組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["平均科組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["平均科組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        key = "平均全校排名" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["平均全校排名"] = ranks[key].IndexOf(studentPrintSubjectAvg[studentID]) + 1;
                                             row["平均全校排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["平均校高標"] = analytics[key + "^^^高標"];
                                             row["平均校均標"] = analytics[key + "^^^均標"];
                                             row["平均校低標"] = analytics[key + "^^^低標"];
                                             row["平均校標準差"] = analytics[key + "^^^標準差"];
                                             row["平均校組距count90"] = analytics[key + "^^^count90"];
                                             row["平均校組距count80"] = analytics[key + "^^^count80"];
                                             row["平均校組距count70"] = analytics[key + "^^^count70"];
                                             row["平均校組距count60"] = analytics[key + "^^^count60"];
                                             row["平均校組距count50"] = analytics[key + "^^^count50"];
                                             row["平均校組距count40"] = analytics[key + "^^^count40"];
                                             row["平均校組距count30"] = analytics[key + "^^^count30"];
                                             row["平均校組距count20"] = analytics[key + "^^^count20"];
                                             row["平均校組距count10"] = analytics[key + "^^^count10"];
                                             row["平均校組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["平均校組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["平均校組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["平均校組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["平均校組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["平均校組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["平均校組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["平均校組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["平均校組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["平均校組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["平均校組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["平均校組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["平均校組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["平均校組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["平均校組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["平均校組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["平均校組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["平均校組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["平均校組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                   }
                                   #endregion
                                   #region 加權總分
                                   if (studentPrintSubjectSumW.ContainsKey(studentID))
                                   {
                                        row["加權總分"] = studentPrintSubjectSumW[studentID];
                                        key = "加權總分班排名" + stuRec.RefClass.ClassID;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["加權總分班排名"] = ranks[key].IndexOf(studentPrintSubjectSumW[studentID]) + 1;
                                             row["加權總分班排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["加權總分班高標"] = analytics[key + "^^^高標"];
                                             row["加權總分班均標"] = analytics[key + "^^^均標"];
                                             row["加權總分班低標"] = analytics[key + "^^^低標"];
                                             row["加權總分班標準差"] = analytics[key + "^^^標準差"];
                                             row["加權總分班組距count90"] = analytics[key + "^^^count90"];
                                             row["加權總分班組距count80"] = analytics[key + "^^^count80"];
                                             row["加權總分班組距count70"] = analytics[key + "^^^count70"];
                                             row["加權總分班組距count60"] = analytics[key + "^^^count60"];
                                             row["加權總分班組距count50"] = analytics[key + "^^^count50"];
                                             row["加權總分班組距count40"] = analytics[key + "^^^count40"];
                                             row["加權總分班組距count30"] = analytics[key + "^^^count30"];
                                             row["加權總分班組距count20"] = analytics[key + "^^^count20"];
                                             row["加權總分班組距count10"] = analytics[key + "^^^count10"];
                                             row["加權總分班組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["加權總分班組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["加權總分班組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["加權總分班組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["加權總分班組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["加權總分班組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["加權總分班組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["加權總分班組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["加權總分班組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["加權總分班組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["加權總分班組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["加權總分班組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["加權總分班組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["加權總分班組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["加權總分班組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["加權總分班組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["加權總分班組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["加權總分班組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["加權總分班組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        key = "加權總分科排名" + stuRec.Department + "^^^" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["加權總分科排名"] = ranks[key].IndexOf(studentPrintSubjectSumW[studentID]) + 1;
                                             row["加權總分科排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["加權總分科高標"] = analytics[key + "^^^高標"];
                                             row["加權總分科均標"] = analytics[key + "^^^均標"];
                                             row["加權總分科低標"] = analytics[key + "^^^低標"];
                                             row["加權總分科標準差"] = analytics[key + "^^^標準差"];
                                             row["加權總分科組距count90"] = analytics[key + "^^^count90"];
                                             row["加權總分科組距count80"] = analytics[key + "^^^count80"];
                                             row["加權總分科組距count70"] = analytics[key + "^^^count70"];
                                             row["加權總分科組距count60"] = analytics[key + "^^^count60"];
                                             row["加權總分科組距count50"] = analytics[key + "^^^count50"];
                                             row["加權總分科組距count40"] = analytics[key + "^^^count40"];
                                             row["加權總分科組距count30"] = analytics[key + "^^^count30"];
                                             row["加權總分科組距count20"] = analytics[key + "^^^count20"];
                                             row["加權總分科組距count10"] = analytics[key + "^^^count10"];
                                             row["加權總分科組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["加權總分科組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["加權總分科組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["加權總分科組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["加權總分科組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["加權總分科組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["加權總分科組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["加權總分科組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["加權總分科組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["加權總分科組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["加權總分科組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["加權總分科組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["加權總分科組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["加權總分科組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["加權總分科組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["加權總分科組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["加權總分科組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["加權總分科組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["加權總分科組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        key = "加權總分全校排名" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["加權總分全校排名"] = ranks[key].IndexOf(studentPrintSubjectSumW[studentID]) + 1;
                                             row["加權總分全校排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["加權總分校高標"] = analytics[key + "^^^高標"];
                                             row["加權總分校均標"] = analytics[key + "^^^均標"];
                                             row["加權總分校低標"] = analytics[key + "^^^低標"];
                                             row["加權總分校標準差"] = analytics[key + "^^^標準差"];
                                             row["加權總分校組距count90"] = analytics[key + "^^^count90"];
                                             row["加權總分校組距count80"] = analytics[key + "^^^count80"];
                                             row["加權總分校組距count70"] = analytics[key + "^^^count70"];
                                             row["加權總分校組距count60"] = analytics[key + "^^^count60"];
                                             row["加權總分校組距count50"] = analytics[key + "^^^count50"];
                                             row["加權總分校組距count40"] = analytics[key + "^^^count40"];
                                             row["加權總分校組距count30"] = analytics[key + "^^^count30"];
                                             row["加權總分校組距count20"] = analytics[key + "^^^count20"];
                                             row["加權總分校組距count10"] = analytics[key + "^^^count10"];
                                             row["加權總分校組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["加權總分校組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["加權總分校組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["加權總分校組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["加權總分校組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["加權總分校組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["加權總分校組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["加權總分校組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["加權總分校組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["加權總分校組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["加權總分校組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["加權總分校組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["加權總分校組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["加權總分校組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["加權總分校組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["加權總分校組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["加權總分校組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["加權總分校組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["加權總分校組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                   }
                                   #endregion
                                   #region 加權平均
                                   if (studentPrintSubjectAvgW.ContainsKey(studentID))
                                   {
                                        row["加權平均"] = studentPrintSubjectAvgW[studentID];
                                        key = "加權平均班排名" + stuRec.RefClass.ClassID;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["加權平均班排名"] = ranks[key].IndexOf(studentPrintSubjectAvgW[studentID]) + 1;
                                             row["加權平均班排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["加權平均班高標"] = analytics[key + "^^^高標"];
                                             row["加權平均班均標"] = analytics[key + "^^^均標"];
                                             row["加權平均班低標"] = analytics[key + "^^^低標"];
                                             row["加權平均班標準差"] = analytics[key + "^^^標準差"];
                                             row["加權平均班組距count90"] = analytics[key + "^^^count90"];
                                             row["加權平均班組距count80"] = analytics[key + "^^^count80"];
                                             row["加權平均班組距count70"] = analytics[key + "^^^count70"];
                                             row["加權平均班組距count60"] = analytics[key + "^^^count60"];
                                             row["加權平均班組距count50"] = analytics[key + "^^^count50"];
                                             row["加權平均班組距count40"] = analytics[key + "^^^count40"];
                                             row["加權平均班組距count30"] = analytics[key + "^^^count30"];
                                             row["加權平均班組距count20"] = analytics[key + "^^^count20"];
                                             row["加權平均班組距count10"] = analytics[key + "^^^count10"];
                                             row["加權平均班組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["加權平均班組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["加權平均班組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["加權平均班組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["加權平均班組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["加權平均班組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["加權平均班組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["加權平均班組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["加權平均班組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["加權平均班組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["加權平均班組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["加權平均班組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["加權平均班組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["加權平均班組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["加權平均班組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["加權平均班組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["加權平均班組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["加權平均班組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["加權平均班組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        key = "加權平均科排名" + stuRec.Department + "^^^" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["加權平均科排名"] = ranks[key].IndexOf(studentPrintSubjectAvgW[studentID]) + 1;
                                             row["加權平均科排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["加權平均科高標"] = analytics[key + "^^^高標"];
                                             row["加權平均科均標"] = analytics[key + "^^^均標"];
                                             row["加權平均科低標"] = analytics[key + "^^^低標"];
                                             row["加權平均科標準差"] = analytics[key + "^^^標準差"];
                                             row["加權平均科組距count90"] = analytics[key + "^^^count90"];
                                             row["加權平均科組距count80"] = analytics[key + "^^^count80"];
                                             row["加權平均科組距count70"] = analytics[key + "^^^count70"];
                                             row["加權平均科組距count60"] = analytics[key + "^^^count60"];
                                             row["加權平均科組距count50"] = analytics[key + "^^^count50"];
                                             row["加權平均科組距count40"] = analytics[key + "^^^count40"];
                                             row["加權平均科組距count30"] = analytics[key + "^^^count30"];
                                             row["加權平均科組距count20"] = analytics[key + "^^^count20"];
                                             row["加權平均科組距count10"] = analytics[key + "^^^count10"];
                                             row["加權平均科組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["加權平均科組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["加權平均科組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["加權平均科組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["加權平均科組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["加權平均科組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["加權平均科組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["加權平均科組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["加權平均科組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["加權平均科組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["加權平均科組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["加權平均科組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["加權平均科組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["加權平均科組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["加權平均科組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["加權平均科組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["加權平均科組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["加權平均科組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["加權平均科組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                        key = "加權平均全校排名" + gradeYear;
                                        if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))//明確判斷學生是否參與排名
                                        {
                                             row["加權平均全校排名"] = ranks[key].IndexOf(studentPrintSubjectAvgW[studentID]) + 1;
                                             row["加權平均全校排名母數"] = ranks[key].Count;
                                        }
                                        if (rankStudents.ContainsKey(key))
                                        {
                                             row["加權平均校高標"] = analytics[key + "^^^高標"];
                                             row["加權平均校均標"] = analytics[key + "^^^均標"];
                                             row["加權平均校低標"] = analytics[key + "^^^低標"];
                                             row["加權平均校標準差"] = analytics[key + "^^^標準差"];
                                             row["加權平均校組距count90"] = analytics[key + "^^^count90"];
                                             row["加權平均校組距count80"] = analytics[key + "^^^count80"];
                                             row["加權平均校組距count70"] = analytics[key + "^^^count70"];
                                             row["加權平均校組距count60"] = analytics[key + "^^^count60"];
                                             row["加權平均校組距count50"] = analytics[key + "^^^count50"];
                                             row["加權平均校組距count40"] = analytics[key + "^^^count40"];
                                             row["加權平均校組距count30"] = analytics[key + "^^^count30"];
                                             row["加權平均校組距count20"] = analytics[key + "^^^count20"];
                                             row["加權平均校組距count10"] = analytics[key + "^^^count10"];
                                             row["加權平均校組距count100Up"] = analytics[key + "^^^count100Up"];
                                             row["加權平均校組距count90Up"] = analytics[key + "^^^count90Up"];
                                             row["加權平均校組距count80Up"] = analytics[key + "^^^count80Up"];
                                             row["加權平均校組距count70Up"] = analytics[key + "^^^count70Up"];
                                             row["加權平均校組距count60Up"] = analytics[key + "^^^count60Up"];
                                             row["加權平均校組距count50Up"] = analytics[key + "^^^count50Up"];
                                             row["加權平均校組距count40Up"] = analytics[key + "^^^count40Up"];
                                             row["加權平均校組距count30Up"] = analytics[key + "^^^count30Up"];
                                             row["加權平均校組距count20Up"] = analytics[key + "^^^count20Up"];
                                             row["加權平均校組距count10Up"] = analytics[key + "^^^count10Up"];
                                             row["加權平均校組距count90Down"] = analytics[key + "^^^count90Down"];
                                             row["加權平均校組距count80Down"] = analytics[key + "^^^count80Down"];
                                             row["加權平均校組距count70Down"] = analytics[key + "^^^count70Down"];
                                             row["加權平均校組距count60Down"] = analytics[key + "^^^count60Down"];
                                             row["加權平均校組距count50Down"] = analytics[key + "^^^count50Down"];
                                             row["加權平均校組距count40Down"] = analytics[key + "^^^count40Down"];
                                             row["加權平均校組距count30Down"] = analytics[key + "^^^count30Down"];
                                             row["加權平均校組距count20Down"] = analytics[key + "^^^count20Down"];
                                             row["加權平均校組距count10Down"] = analytics[key + "^^^count10Down"];
                                        }
                                   }
                                   #endregion
                                   #region 類別1綜合成績
                                   if (studentTag1Group.ContainsKey(studentID))
                                   {
                                        foreach (var tag in studentTags[studentID])
                                        {
                                             if (tag.RefTagID == studentTag1Group[studentID])
                                             {
                                                  row["類別排名1"] = tag.Name;
                                             }
                                        }
                                        if (studentTag1SubjectSum.ContainsKey(studentID))
                                        {
                                             row["類別1總分"] = studentTag1SubjectSum[studentID];
                                             key = "類別1總分排名" + "^^^" + gradeYear + "^^^" + studentTag1Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別1總分排名"] = ranks[key].IndexOf(studentTag1SubjectSum[studentID]) + 1;
                                                  row["類別1總分排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類1總分高標"] = analytics[key + "^^^高標"];
                                                  row["類1總分均標"] = analytics[key + "^^^均標"];
                                                  row["類1總分低標"] = analytics[key + "^^^低標"];
                                                  row["類1總分標準差"] = analytics[key + "^^^標準差"];
                                                  row["類1總分組距count90"] = analytics[key + "^^^count90"];
                                                  row["類1總分組距count80"] = analytics[key + "^^^count80"];
                                                  row["類1總分組距count70"] = analytics[key + "^^^count70"];
                                                  row["類1總分組距count60"] = analytics[key + "^^^count60"];
                                                  row["類1總分組距count50"] = analytics[key + "^^^count50"];
                                                  row["類1總分組距count40"] = analytics[key + "^^^count40"];
                                                  row["類1總分組距count30"] = analytics[key + "^^^count30"];
                                                  row["類1總分組距count20"] = analytics[key + "^^^count20"];
                                                  row["類1總分組距count10"] = analytics[key + "^^^count10"];
                                                  row["類1總分組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類1總分組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類1總分組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類1總分組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類1總分組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類1總分組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類1總分組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類1總分組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類1總分組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類1總分組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類1總分組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類1總分組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類1總分組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類1總分組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類1總分組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類1總分組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類1總分組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類1總分組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類1總分組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                        if (studentTag1SubjectAvg.ContainsKey(studentID))
                                        {
                                             row["類別1平均"] = studentTag1SubjectAvg[studentID];
                                             key = "類別1平均排名" + "^^^" + gradeYear + "^^^" + studentTag1Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別1平均排名"] = ranks[key].IndexOf(studentTag1SubjectAvg[studentID]) + 1; ;
                                                  row["類別1平均排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類1平均高標"] = analytics[key + "^^^高標"];
                                                  row["類1平均均標"] = analytics[key + "^^^均標"];
                                                  row["類1平均低標"] = analytics[key + "^^^低標"];
                                                  row["類1平均標準差"] = analytics[key + "^^^標準差"];
                                                  row["類1平均組距count90"] = analytics[key + "^^^count90"];
                                                  row["類1平均組距count80"] = analytics[key + "^^^count80"];
                                                  row["類1平均組距count70"] = analytics[key + "^^^count70"];
                                                  row["類1平均組距count60"] = analytics[key + "^^^count60"];
                                                  row["類1平均組距count50"] = analytics[key + "^^^count50"];
                                                  row["類1平均組距count40"] = analytics[key + "^^^count40"];
                                                  row["類1平均組距count30"] = analytics[key + "^^^count30"];
                                                  row["類1平均組距count20"] = analytics[key + "^^^count20"];
                                                  row["類1平均組距count10"] = analytics[key + "^^^count10"];
                                                  row["類1平均組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類1平均組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類1平均組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類1平均組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類1平均組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類1平均組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類1平均組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類1平均組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類1平均組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類1平均組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類1平均組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類1平均組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類1平均組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類1平均組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類1平均組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類1平均組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類1平均組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類1平均組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類1平均組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                        if (studentTag1SubjectSumW.ContainsKey(studentID))
                                        {
                                             row["類別1加權總分"] = studentTag1SubjectSumW[studentID];
                                             key = "類別1加權總分排名" + "^^^" + gradeYear + "^^^" + studentTag1Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別1加權總分排名"] = ranks[key].IndexOf(studentTag1SubjectSumW[studentID]) + 1; ;
                                                  row["類別1加權總分排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類1加權總分高標"] = analytics[key + "^^^高標"];
                                                  row["類1加權總分均標"] = analytics[key + "^^^均標"];
                                                  row["類1加權總分低標"] = analytics[key + "^^^低標"];
                                                  row["類1加權總分標準差"] = analytics[key + "^^^標準差"];
                                                  row["類1加權總分組距count90"] = analytics[key + "^^^count90"];
                                                  row["類1加權總分組距count80"] = analytics[key + "^^^count80"];
                                                  row["類1加權總分組距count70"] = analytics[key + "^^^count70"];
                                                  row["類1加權總分組距count60"] = analytics[key + "^^^count60"];
                                                  row["類1加權總分組距count50"] = analytics[key + "^^^count50"];
                                                  row["類1加權總分組距count40"] = analytics[key + "^^^count40"];
                                                  row["類1加權總分組距count30"] = analytics[key + "^^^count30"];
                                                  row["類1加權總分組距count20"] = analytics[key + "^^^count20"];
                                                  row["類1加權總分組距count10"] = analytics[key + "^^^count10"];
                                                  row["類1加權總分組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類1加權總分組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類1加權總分組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類1加權總分組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類1加權總分組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類1加權總分組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類1加權總分組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類1加權總分組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類1加權總分組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類1加權總分組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類1加權總分組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類1加權總分組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類1加權總分組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類1加權總分組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類1加權總分組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類1加權總分組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類1加權總分組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類1加權總分組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類1加權總分組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                        if (studentTag1SubjectAvgW.ContainsKey(studentID))
                                        {
                                             row["類別1加權平均"] = studentTag1SubjectAvgW[studentID];
                                             key = "類別1加權平均排名" + "^^^" + gradeYear + "^^^" + studentTag1Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別1加權平均排名"] = ranks[key].IndexOf(studentTag1SubjectAvgW[studentID]) + 1; ;
                                                  row["類別1加權平均排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類1加權平均高標"] = analytics[key + "^^^高標"];
                                                  row["類1加權平均均標"] = analytics[key + "^^^均標"];
                                                  row["類1加權平均低標"] = analytics[key + "^^^低標"];
                                                  row["類1加權平均標準差"] = analytics[key + "^^^標準差"];
                                                  row["類1加權平均組距count90"] = analytics[key + "^^^count90"];
                                                  row["類1加權平均組距count80"] = analytics[key + "^^^count80"];
                                                  row["類1加權平均組距count70"] = analytics[key + "^^^count70"];
                                                  row["類1加權平均組距count60"] = analytics[key + "^^^count60"];
                                                  row["類1加權平均組距count50"] = analytics[key + "^^^count50"];
                                                  row["類1加權平均組距count40"] = analytics[key + "^^^count40"];
                                                  row["類1加權平均組距count30"] = analytics[key + "^^^count30"];
                                                  row["類1加權平均組距count20"] = analytics[key + "^^^count20"];
                                                  row["類1加權平均組距count10"] = analytics[key + "^^^count10"];
                                                  row["類1加權平均組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類1加權平均組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類1加權平均組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類1加權平均組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類1加權平均組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類1加權平均組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類1加權平均組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類1加權平均組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類1加權平均組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類1加權平均組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類1加權平均組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類1加權平均組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類1加權平均組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類1加權平均組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類1加權平均組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類1加權平均組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類1加權平均組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類1加權平均組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類1加權平均組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                   }
                                   #endregion
                                   #region 類別2綜合成績
                                   if (studentTag2Group.ContainsKey(studentID))
                                   {
                                        foreach (var tag in studentTags[studentID])
                                        {
                                             if (tag.RefTagID == studentTag2Group[studentID])
                                             {
                                                  row["類別排名2"] = tag.Name;
                                             }
                                        }
                                        if (studentTag2SubjectSum.ContainsKey(studentID))
                                        {
                                             row["類別2總分"] = studentTag2SubjectSum[studentID];
                                             key = "類別2總分排名" + "^^^" + gradeYear + "^^^" + studentTag2Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別2總分排名"] = ranks[key].IndexOf(studentTag2SubjectSum[studentID]) + 1;
                                                  row["類別2總分排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類2總分高標"] = analytics[key + "^^^高標"];
                                                  row["類2總分均標"] = analytics[key + "^^^均標"];
                                                  row["類2總分低標"] = analytics[key + "^^^低標"];
                                                  row["類2總分標準差"] = analytics[key + "^^^標準差"];
                                                  row["類2總分組距count90"] = analytics[key + "^^^count90"];
                                                  row["類2總分組距count80"] = analytics[key + "^^^count80"];
                                                  row["類2總分組距count70"] = analytics[key + "^^^count70"];
                                                  row["類2總分組距count60"] = analytics[key + "^^^count60"];
                                                  row["類2總分組距count50"] = analytics[key + "^^^count50"];
                                                  row["類2總分組距count40"] = analytics[key + "^^^count40"];
                                                  row["類2總分組距count30"] = analytics[key + "^^^count30"];
                                                  row["類2總分組距count20"] = analytics[key + "^^^count20"];
                                                  row["類2總分組距count10"] = analytics[key + "^^^count10"];
                                                  row["類2總分組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類2總分組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類2總分組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類2總分組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類2總分組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類2總分組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類2總分組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類2總分組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類2總分組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類2總分組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類2總分組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類2總分組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類2總分組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類2總分組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類2總分組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類2總分組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類2總分組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類2總分組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類2總分組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                        if (studentTag2SubjectAvg.ContainsKey(studentID))
                                        {
                                             row["類別2平均"] = studentTag2SubjectAvg[studentID];
                                             key = "類別2平均排名" + "^^^" + gradeYear + "^^^" + studentTag2Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別2平均排名"] = ranks[key].IndexOf(studentTag2SubjectAvg[studentID]) + 1; ;
                                                  row["類別2平均排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類2平均高標"] = analytics[key + "^^^高標"];
                                                  row["類2平均均標"] = analytics[key + "^^^均標"];
                                                  row["類2平均低標"] = analytics[key + "^^^低標"];
                                                  row["類2平均標準差"] = analytics[key + "^^^標準差"];
                                                  row["類2平均組距count90"] = analytics[key + "^^^count90"];
                                                  row["類2平均組距count80"] = analytics[key + "^^^count80"];
                                                  row["類2平均組距count70"] = analytics[key + "^^^count70"];
                                                  row["類2平均組距count60"] = analytics[key + "^^^count60"];
                                                  row["類2平均組距count50"] = analytics[key + "^^^count50"];
                                                  row["類2平均組距count40"] = analytics[key + "^^^count40"];
                                                  row["類2平均組距count30"] = analytics[key + "^^^count30"];
                                                  row["類2平均組距count20"] = analytics[key + "^^^count20"];
                                                  row["類2平均組距count10"] = analytics[key + "^^^count10"];
                                                  row["類2平均組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類2平均組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類2平均組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類2平均組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類2平均組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類2平均組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類2平均組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類2平均組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類2平均組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類2平均組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類2平均組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類2平均組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類2平均組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類2平均組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類2平均組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類2平均組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類2平均組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類2平均組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類2平均組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                        if (studentTag2SubjectSumW.ContainsKey(studentID))
                                        {
                                             row["類別2加權總分"] = studentTag2SubjectSumW[studentID];
                                             key = "類別2加權總分排名" + "^^^" + gradeYear + "^^^" + studentTag2Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別2加權總分排名"] = ranks[key].IndexOf(studentTag2SubjectSumW[studentID]) + 1; ;
                                                  row["類別2加權總分排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類2加權總分高標"] = analytics[key + "^^^高標"];
                                                  row["類2加權總分均標"] = analytics[key + "^^^均標"];
                                                  row["類2加權總分低標"] = analytics[key + "^^^低標"];
                                                  row["類2加權總分標準差"] = analytics[key + "^^^標準差"];
                                                  row["類2加權總分組距count90"] = analytics[key + "^^^count90"];
                                                  row["類2加權總分組距count80"] = analytics[key + "^^^count80"];
                                                  row["類2加權總分組距count70"] = analytics[key + "^^^count70"];
                                                  row["類2加權總分組距count60"] = analytics[key + "^^^count60"];
                                                  row["類2加權總分組距count50"] = analytics[key + "^^^count50"];
                                                  row["類2加權總分組距count40"] = analytics[key + "^^^count40"];
                                                  row["類2加權總分組距count30"] = analytics[key + "^^^count30"];
                                                  row["類2加權總分組距count20"] = analytics[key + "^^^count20"];
                                                  row["類2加權總分組距count10"] = analytics[key + "^^^count10"];
                                                  row["類2加權總分組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類2加權總分組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類2加權總分組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類2加權總分組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類2加權總分組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類2加權總分組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類2加權總分組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類2加權總分組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類2加權總分組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類2加權總分組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類2加權總分組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類2加權總分組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類2加權總分組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類2加權總分組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類2加權總分組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類2加權總分組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類2加權總分組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類2加權總分組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類2加權總分組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                        if (studentTag2SubjectAvgW.ContainsKey(studentID))
                                        {
                                             row["類別2加權平均"] = studentTag2SubjectAvgW[studentID];
                                             key = "類別2加權平均排名" + "^^^" + gradeYear + "^^^" + studentTag2Group[studentID];
                                             if (rankStudents.ContainsKey(key) && rankStudents[key].Contains(studentID))
                                             {
                                                  row["類別2加權平均排名"] = ranks[key].IndexOf(studentTag2SubjectAvgW[studentID]) + 1; ;
                                                  row["類別2加權平均排名母數"] = ranks[key].Count;
                                             }
                                             if (rankStudents.ContainsKey(key))
                                             {
                                                  row["類2加權平均高標"] = analytics[key + "^^^高標"];
                                                  row["類2加權平均均標"] = analytics[key + "^^^均標"];
                                                  row["類2加權平均低標"] = analytics[key + "^^^低標"];
                                                  row["類2加權平均標準差"] = analytics[key + "^^^標準差"];
                                                  row["類2加權平均組距count90"] = analytics[key + "^^^count90"];
                                                  row["類2加權平均組距count80"] = analytics[key + "^^^count80"];
                                                  row["類2加權平均組距count70"] = analytics[key + "^^^count70"];
                                                  row["類2加權平均組距count60"] = analytics[key + "^^^count60"];
                                                  row["類2加權平均組距count50"] = analytics[key + "^^^count50"];
                                                  row["類2加權平均組距count40"] = analytics[key + "^^^count40"];
                                                  row["類2加權平均組距count30"] = analytics[key + "^^^count30"];
                                                  row["類2加權平均組距count20"] = analytics[key + "^^^count20"];
                                                  row["類2加權平均組距count10"] = analytics[key + "^^^count10"];
                                                  row["類2加權平均組距count100Up"] = analytics[key + "^^^count100Up"];
                                                  row["類2加權平均組距count90Up"] = analytics[key + "^^^count90Up"];
                                                  row["類2加權平均組距count80Up"] = analytics[key + "^^^count80Up"];
                                                  row["類2加權平均組距count70Up"] = analytics[key + "^^^count70Up"];
                                                  row["類2加權平均組距count60Up"] = analytics[key + "^^^count60Up"];
                                                  row["類2加權平均組距count50Up"] = analytics[key + "^^^count50Up"];
                                                  row["類2加權平均組距count40Up"] = analytics[key + "^^^count40Up"];
                                                  row["類2加權平均組距count30Up"] = analytics[key + "^^^count30Up"];
                                                  row["類2加權平均組距count20Up"] = analytics[key + "^^^count20Up"];
                                                  row["類2加權平均組距count10Up"] = analytics[key + "^^^count10Up"];
                                                  row["類2加權平均組距count90Down"] = analytics[key + "^^^count90Down"];
                                                  row["類2加權平均組距count80Down"] = analytics[key + "^^^count80Down"];
                                                  row["類2加權平均組距count70Down"] = analytics[key + "^^^count70Down"];
                                                  row["類2加權平均組距count60Down"] = analytics[key + "^^^count60Down"];
                                                  row["類2加權平均組距count50Down"] = analytics[key + "^^^count50Down"];
                                                  row["類2加權平均組距count40Down"] = analytics[key + "^^^count40Down"];
                                                  row["類2加權平均組距count30Down"] = analytics[key + "^^^count30Down"];
                                                  row["類2加權平均組距count20Down"] = analytics[key + "^^^count20Down"];
                                                  row["類2加權平均組距count10Down"] = analytics[key + "^^^count10Down"];
                                             }
                                        }
                                   }
                                   #endregion
                                   #endregion
                                   #region 學務資料
                                   #region 獎懲統計
                                   int 大功 = 0;
                                   int 小功 = 0;
                                   int 嘉獎 = 0;
                                   int 大過 = 0;
                                   int 小過 = 0;
                                   int 警告 = 0;
                                   bool 留校察看 = false;
                                   foreach (RewardInfo info in stuRec.RewardList)
                                   {
                                        if (("" + info.Semester) == conf.Semester && ("" + info.SchoolYear) == conf.SchoolYear)
                                        {
                                             大功 += info.AwardA;
                                             小功 += info.AwardB;
                                             嘉獎 += info.AwardC;
                                             if (!info.Cleared)
                                             {
                                                  大過 += info.FaultA;
                                                  小過 += info.FaultB;
                                                  警告 += info.FaultC;
                                             }
                                             if (info.UltimateAdmonition)
                                                  留校察看 = true;
                                        }
                                   }
                                   row["大功統計"] = 大功 == 0 ? "" : ("" + 大功);
                                   row["小功統計"] = 小功 == 0 ? "" : ("" + 小功);
                                   row["嘉獎統計"] = 嘉獎 == 0 ? "" : ("" + 嘉獎);
                                   row["大過統計"] = 大過 == 0 ? "" : ("" + 大過);
                                   row["小過統計"] = 小過 == 0 ? "" : ("" + 小過);
                                   row["警告統計"] = 警告 == 0 ? "" : ("" + 警告);
                                   row["留校察看"] = 留校察看 ? "是" : "";
                                   #endregion
                                   #region 缺曠統計
                                   Dictionary<string, int> 缺曠項目統計 = new Dictionary<string, int>();
                                   foreach (AttendanceInfo info in stuRec.AttendanceList)
                                   {
                                        if (("" + info.Semester) == conf.Semester && ("" + info.SchoolYear) == conf.SchoolYear)
                                        {
                                             string infoType = "";
                                             if (dicPeriodMappingType.ContainsKey(info.Period))
                                                  infoType = dicPeriodMappingType[info.Period];
                                             else
                                                  infoType = "";
                                             string attendanceKey = "" + infoType + "_" + info.Absence;
                                             if (!缺曠項目統計.ContainsKey(attendanceKey))
                                                  缺曠項目統計.Add(attendanceKey, 0);
                                             缺曠項目統計[attendanceKey]++;
                                        }
                                   }
                                   foreach (string attendanceKey in 缺曠項目統計.Keys)
                                   {
                                        row[attendanceKey] = 缺曠項目統計[attendanceKey] == 0 ? "" : ("" + 缺曠項目統計[attendanceKey]);
                                   }
                                   #endregion
                                   #endregion
                                   table.Rows.Add(row);
                                   progressCount++;
                                   bkw.ReportProgress(70 + progressCount * 20 / selectedStudents.Count);
                              }
                              #endregion
                              bkw.ReportProgress(90);

                              // 收集學生清單資料並產生學生清單
                              _wbStudentList = new Aspose.Cells.Workbook();
                              _wbStudentList.Open(new MemoryStream(Properties.Resources.個人學期成績單_學生清單_));

                              int rowIdx = 1;
                              foreach (DataRow dr in table.Rows)
                              {
                                   _wbStudentList.Worksheets[0].Cells[rowIdx, 0].PutValue(dr["班級"].ToString());
                                   _wbStudentList.Worksheets[0].Cells[rowIdx, 1].PutValue(dr["座號"].ToString());
                                   _wbStudentList.Worksheets[0].Cells[rowIdx, 2].PutValue(dr["學號"].ToString());
                                   _wbStudentList.Worksheets[0].Cells[rowIdx, 3].PutValue(dr["姓名"].ToString());
                                   _wbStudentList.Worksheets[0].Cells[rowIdx, 4].PutValue(dr["收件人"].ToString());
                                   _wbStudentList.Worksheets[0].Cells[rowIdx, 5].PutValue(dr["收件人地址"].ToString());
                                   rowIdx++;
                              }

                              // 處理 epost
                              foreach (DataRow dr in table.Rows)
                              {
                                  DataRow data = _dtEpost.NewRow();

                                  // 取得學生及格與補考標準
                                  string studID = dr["學生系統編號"].ToString();
                                  int grYear;
                                  int.TryParse(dr["學生班級年級"].ToString(), out grYear);

                                  // 先取通訊地址，沒有再取戶籍地址
                                  string zip1 = dr["通訊地址郵遞區號"].ToString();
                                  string zip2 = dr["戶籍地址郵遞區號"].ToString();
                                  string Address1 = dr["通訊地址內容"].ToString();
                                  string Address2 = dr["戶籍地址內容"].ToString();

                                  // POSTALADDRESS
                                  if (string.IsNullOrWhiteSpace(zip1))
                                      data["POSTALCODE"] = zip2;
                                  else
                                      data["POSTALCODE"] = zip1;

                                  if (string.IsNullOrWhiteSpace(Address1))
                                      data["POSTALADDRESS"] = Address2;
                                  else
                                      data["POSTALADDRESS"] = Address1;                             
                                  
                                  //data["缺曠獎懲統計期間"] = dr["開始日期"] + "～" + dr["結束日期"];


                                  //data["總成績名次"] = dr["學期學業成績班排名"].ToString();

                                  // 處理固定對照
                                  foreach (DataColumn dc in table.Columns)
                                  {
                                      if (eKeyValDict.ContainsKey(dc.Caption))
                                          data[eKeyValDict[dc.Caption]] = dr[dc.Caption];
                                  }

                                  // 處理科目成績
                                  for (int subjectIndex = 1; subjectIndex <= conf.SubjectLimit; subjectIndex++)
                                  {
                                      if (dr["科目名稱" + subjectIndex].ToString() != "")
                                      {
                                          data["科目名稱" + subjectIndex] = dr["科目名稱" + subjectIndex];
                                          data["學分數" + subjectIndex] = dr["學分數" + subjectIndex];
                                  //        data["前次成績" + subjectIndex] = dr["前次成績" + subjectIndex];
                                           data["科目成績" + subjectIndex] = dr["科目成績" + subjectIndex];
                                  //        data["成績" + subjectIndex] = dr["科目成績" + subjectIndex];
                                  //        data["班級平均" + subjectIndex] = dr["班均標" + subjectIndex];
                                  //        data["班級排名" + subjectIndex] = dr["班排名" + subjectIndex];
                                  //        data["科排名" + subjectIndex] = dr["科排名" + subjectIndex];
                                  //        data["類組排名" + subjectIndex] = dr["類別1排名" + subjectIndex];
                                      }
                                  }

                                  //data["導師評語"] = @"""" + data["導師評語"].ToString() + @"""";
                                  _dtEpost.Rows.Add(data);
                              }

                              document = conf.Template;
                              document.MailMerge.Execute(table);
                         }
                         catch (Exception exception)
                         {
                              exc = exception;
                         }
                    };
                    bkw.RunWorkerAsync();
               }
          }


          internal static void CreateFieldTemplate()
          {
               #region 產生欄位表
               Aspose.Words.Document doc = new Aspose.Words.Document(new System.IO.MemoryStream(Properties.Resources.Template));
               Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder(doc);
               int maxNum = 30;

               #region 科目學期學年評量成績
               builder.Writeln("成績");
               builder.StartTable();
               builder.InsertCell();
               builder.Write("科目名稱");
               builder.InsertCell();
               builder.Write("學分數");
               builder.InsertCell();
               builder.Write("評量參考成績");
               builder.InsertCell();
               builder.Write("評量成績");
               builder.InsertCell();
               builder.Write("學年成績");
               builder.InsertCell();
               builder.Write("學期成績");
               builder.InsertCell();
               builder.Write("上學期成績");

               builder.InsertCell();
               builder.Write("學期科目原始成績");
               builder.InsertCell();
               builder.Write("學期科目補考成績");
               builder.InsertCell();
               builder.Write("學期科目重修成績");
               builder.InsertCell();
               builder.Write("學期科目手動調整成績");
               builder.InsertCell();
               builder.Write("學期科目學年調整成績");

               builder.InsertCell();
               builder.Write("上學期科目原始成績");
               builder.InsertCell();
               builder.Write("上學期科目補考成績");
               builder.InsertCell();
               builder.Write("上學期科目重修成績");
               builder.InsertCell();
               builder.Write("上學期科目手動調整成績");
               builder.InsertCell();
               builder.Write("上學期科目學年調整成績");
               builder.EndRow();
               for (int i = 1; i <= maxNum; i++)
               {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 科目名稱" + i + " \\* MERGEFORMAT ", "«N" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學分數" + i + " \\* MERGEFORMAT ", "«C" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 前次成績" + i + " \\* MERGEFORMAT ", "«SP" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 科目成績" + i + " \\* MERGEFORMAT ", "«S" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學年科目成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 上學期科目成績" + i + " \\* MERGEFORMAT ", "«R»");

                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目原始成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目補考成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目重修成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目手動調整成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目學年調整成績" + i + " \\* MERGEFORMAT ", "«R»");

                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 上學期科目原始成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 上學期科目補考成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 上學期科目重修成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 上學期科目手動調整成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 上學期科目學年調整成績" + i + " \\* MERGEFORMAT ", "«R»");

                    builder.EndRow();
               }

               builder.EndTable();
               builder.Write("固定變數");
               builder.StartTable();
               builder.InsertCell();
               builder.Write("項目");
               builder.InsertCell();
               builder.Write("變數");
               builder.EndRow();
               foreach (string key in new string[]{
                    "學期學業成績"
                    ,"學期體育成績"
                    ,"學期國防通識成績"
                    ,"學期健康與護理成績"
                    ,"學期實習科目成績"
                    ,"學期德行成績"
                    ,"學期學業成績班排名"
                    ,"上學期學業成績"
                    ,"上學期體育成績"
                    ,"上學期國防通識成績"
                    ,"上學期健康與護理成績"
                    ,"上學期實習科目成績"
                    ,"上學期德行成績"
                    ,"學年學業成績"
                    ,"學年體育成績"
                    ,"學年國防通識成績"
                    ,"學年健康與護理成績"
                    ,"學年實習科目成績"
                    ,"學年德行成績"
                    ,"學年學業成績班排名"
                    ,"導師評語"
                    ,"大功統計"
                    ,"小功統計"
                    ,"嘉獎統計"
                    ,"大過統計"
                    ,"小過統計"
                    ,"警告統計"
                    ,"留校察看"
                    ,"班導師"
                })
               {
                    builder.InsertCell();
                    builder.Write(key);
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD " + key + " \\* MERGEFORMAT ", "«" + key + "»");
                    builder.EndRow();
               }
               foreach (var key in new string[] { "班", "科", "類別1", "類別2", "校" })
               {
                    builder.InsertCell();
                    builder.Write("學期學業成績" + key + "排名");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期學業成績" + key + "排名 \\* MERGEFORMAT ", "«學期學業成績" + key + "排名»");
                    builder.InsertField("MERGEFIELD 學期學業成績" + key + "排名母數 \\b /  \\* MERGEFORMAT ", "/«學期學業成績" + key + "排名母數»");
                    builder.EndRow();
               }
               builder.EndTable();
               #endregion

               #region 學期科目成績排名
               builder.Writeln("學期科目成績排名");
               builder.StartTable();
               builder.InsertCell();
               builder.Write("科目名稱");
               builder.InsertCell();
               builder.Write("學期科目排名成績");
               builder.InsertCell();
               builder.Write("學期科目班排名");
               builder.InsertCell();
               builder.Write("學期科目科排名");
               builder.InsertCell();
               builder.Write("學期科目類別1排名");
               builder.InsertCell();
               builder.Write("學期科目類別2排名");
               builder.InsertCell();
               builder.Write("學期科目全校排名");
               builder.EndRow();
               for (int i = 1; i <= maxNum; i++)
               {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 科目名稱" + i + " \\* MERGEFORMAT ", "«N" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目排名成績" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目班排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 學期科目班排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目科排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 學期科目科排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目類別1排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 學期科目類別1排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目類別2排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 學期科目類別2排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學期科目全校排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 學期科目全校排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.EndRow();
               }
               builder.EndTable();
               #endregion

               #region 科目成績及排名
               builder.Writeln("科目成績及排名");
               builder.StartTable();
               builder.InsertCell();
               builder.Write("科目名稱");
               builder.InsertCell();
               builder.Write("學分數");
               builder.InsertCell();
               builder.Write("前次成績");
               builder.InsertCell();
               builder.Write("科目成績");
               builder.InsertCell();
               builder.Write("班排名");
               builder.InsertCell();
               builder.Write("科排名");
               builder.InsertCell();
               builder.Write("類別1排名");
               builder.InsertCell();
               builder.Write("類別2排名");
               builder.InsertCell();
               builder.Write("全校排名");
               builder.EndRow();
               for (int i = 1; i <= maxNum; i++)
               {
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 科目名稱" + i + " \\* MERGEFORMAT ", "«N" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 學分數" + i + " \\* MERGEFORMAT ", "«C" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 前次成績" + i + " \\* MERGEFORMAT ", "«SP" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 科目成績" + i + " \\* MERGEFORMAT ", "«S" + i + "»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 班排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 班排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 科排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 科排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 類別1排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 類別1排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 類別2排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 類別2排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.InsertCell();
                    builder.InsertField("MERGEFIELD 全校排名" + i + " \\* MERGEFORMAT ", "«R»");
                    builder.InsertField("MERGEFIELD 全校排名母數" + i + " \\b /  \\* MERGEFORMAT ", "/«T»");
                    builder.EndRow();
               }

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("項目");
               builder.InsertCell();
               builder.Write("成績");
               builder.InsertCell();
               builder.Write("班排名");
               builder.InsertCell();
               builder.Write("科排名");
               builder.InsertCell();
               builder.Write("類別1排名");
               builder.InsertCell();
               builder.Write("類別2排名");
               builder.InsertCell();
               builder.Write("全校排名");
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("總分");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 總分 \\* MERGEFORMAT ", "«總分»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 總分班排名 \\* MERGEFORMAT ", "«RS»");
               builder.InsertField("MERGEFIELD 總分班排名母數 \\b /  \\* MERGEFORMAT ", "/«TS»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 總分科排名 \\* MERGEFORMAT ", "«RS»");
               builder.InsertField("MERGEFIELD 總分科排名母數 \\b /  \\* MERGEFORMAT ", "/«TS»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 總分全校排名 \\* MERGEFORMAT ", "«RS»");
               builder.InsertField("MERGEFIELD 總分全校排名母數 \\b /  \\* MERGEFORMAT ", "/«TS»");
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("平均");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 平均 \\* MERGEFORMAT ", "«平均»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 平均班排名 \\* MERGEFORMAT ", "«RA»");
               builder.InsertField("MERGEFIELD 平均班排名母數 \\b /  \\* MERGEFORMAT ", "/«TA»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 平均科排名 \\* MERGEFORMAT ", "«RA»");
               builder.InsertField("MERGEFIELD 平均科排名母數 \\b /  \\* MERGEFORMAT ", "/«TA»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 平均全校排名 \\* MERGEFORMAT ", "«RA»");
               builder.InsertField("MERGEFIELD 平均全校排名母數 \\b /  \\* MERGEFORMAT ", "/«TA»");
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("加權總分");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權總分 \\* MERGEFORMAT ", "«加權總»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權總分班排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 加權總分班排名母數 \\b /  \\* MERGEFORMAT ", "/«TP»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權總分科排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 加權總分科排名母數 \\b /  \\* MERGEFORMAT ", "/«TP»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權總分全校排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 加權總分全校排名母數 \\b /  \\* MERGEFORMAT ", "/«TP»");
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("加權平均");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權平均 \\* MERGEFORMAT ", "«加權均»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權平均班排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 加權平均班排名母數 \\b /  \\* MERGEFORMAT ", "/«TP»");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權平均科排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 加權平均科排名母數 \\b /  \\* MERGEFORMAT ", "/«TP»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 加權平均全校排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 加權平均全校排名母數 \\b /  \\* MERGEFORMAT ", "/«TP»");
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類1總分");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1總分 \\* MERGEFORMAT ", "«類1總»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1總分排名 \\* MERGEFORMAT ", "«RS»");
               builder.InsertField("MERGEFIELD 類別1總分排名母數 \\b /  \\* MERGEFORMAT ", "«/TS»");
               builder.InsertCell();
               builder.InsertCell();
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類1平均");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1平均 \\* MERGEFORMAT ", "«類1均»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1平均排名 \\* MERGEFORMAT ", "«RA»");
               builder.InsertField("MERGEFIELD 類別1平均排名母數 \\b /  \\* MERGEFORMAT ", "«/TA»");
               builder.InsertCell();
               builder.InsertCell();
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類1加權總分");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1加權總分 \\* MERGEFORMAT ", "«類1加總»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1加權總分排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 類別1加權總分排名母數 \\b /  \\* MERGEFORMAT ", "«/TP»");
               builder.InsertCell();
               builder.InsertCell();
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類1加權平均");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1加權平均 \\* MERGEFORMAT ", "«類1加均»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別1加權平均排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 類別1加權平均排名母數 \\b /  \\* MERGEFORMAT ", "«/TP»");
               builder.InsertCell();
               builder.InsertCell();
               builder.EndRow();


               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類2總分");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2總分 \\* MERGEFORMAT ", "«類2總»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2總分排名 \\* MERGEFORMAT ", "«RS»");
               builder.InsertField("MERGEFIELD 類別2總分排名母數 \\b /  \\* MERGEFORMAT ", "«/TS»");
               builder.InsertCell();
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類2平均");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2平均 \\* MERGEFORMAT ", "«類2均»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2平均排名 \\* MERGEFORMAT ", "«RA»");
               builder.InsertField("MERGEFIELD 類別2平均排名母數 \\b /  \\* MERGEFORMAT ", "«/TA»");
               builder.InsertCell();
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類2加權總分");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2加權總分 \\* MERGEFORMAT ", "«類2加總»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2加權總分排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 類別2加權總分排名母數 \\b /  \\* MERGEFORMAT ", "«/TP»");
               builder.InsertCell();
               builder.EndRow();

               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.Write("類2加權平均");
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2加權平均 \\* MERGEFORMAT ", "«類2加均»");
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertCell();
               builder.InsertField("MERGEFIELD 類別2加權平均排名 \\* MERGEFORMAT ", "«RP»");
               builder.InsertField("MERGEFIELD 類別2加權平均排名母數 \\b /  \\* MERGEFORMAT ", "«/TP»");
               builder.InsertCell();
               builder.EndRow();

               builder.EndTable();
               #endregion

               #region 各項科目成績分析
               foreach (string key in new string[] { "班", "科", "校", "類1", "類2" })
               {
                    builder.InsertBreak(Aspose.Words.BreakType.PageBreak);

                    builder.Writeln(key + "成績分析及組距");

                    builder.StartTable();
                    builder.InsertCell(); builder.Write("科目名稱");
                    builder.InsertCell(); builder.Write("高標");
                    builder.InsertCell(); builder.Write("均標");
                    builder.InsertCell(); builder.Write("低標");
                    builder.InsertCell(); builder.Write("標準差");
                    builder.InsertCell(); builder.Write("100以上");
                    builder.InsertCell(); builder.Write("90以上");
                    builder.InsertCell(); builder.Write("80以上");
                    builder.InsertCell(); builder.Write("70以上");
                    builder.InsertCell(); builder.Write("60以上");
                    builder.InsertCell(); builder.Write("50以上");
                    builder.InsertCell(); builder.Write("40以上");
                    builder.InsertCell(); builder.Write("30以上");
                    builder.InsertCell(); builder.Write("20以上");
                    builder.InsertCell(); builder.Write("10以上");
                    builder.EndRow();
                    for (int subjectIndex = 1; subjectIndex <= maxNum; subjectIndex++)
                    {
                         builder.InsertCell(); builder.InsertField("MERGEFIELD 科目名稱" + subjectIndex + " \\* MERGEFORMAT ", "«N" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "高標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "均標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "低標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "標準差" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count100Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count90Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count80Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count70Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count60Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count50Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count40Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count30Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count20Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count10Up \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.EndRow();
                    }
                    builder.EndTable();

                    builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
                    builder.StartTable();
                    builder.InsertCell(); builder.Write("科目名稱");
                    builder.InsertCell(); builder.Write("高標");
                    builder.InsertCell(); builder.Write("均標");
                    builder.InsertCell(); builder.Write("低標");
                    builder.InsertCell(); builder.Write("標準差");
                    builder.InsertCell(); builder.Write("90以上小於100");
                    builder.InsertCell(); builder.Write("80以上小於90");
                    builder.InsertCell(); builder.Write("70以上小於80");
                    builder.InsertCell(); builder.Write("60以上小於70");
                    builder.InsertCell(); builder.Write("50以上小於60");
                    builder.InsertCell(); builder.Write("40以上小於50");
                    builder.InsertCell(); builder.Write("30以上小於40");
                    builder.InsertCell(); builder.Write("20以上小於30");
                    builder.InsertCell(); builder.Write("10以上小於20");
                    builder.EndRow();
                    for (int subjectIndex = 1; subjectIndex <= maxNum; subjectIndex++)
                    {
                         builder.InsertCell(); builder.InsertField("MERGEFIELD 科目名稱" + subjectIndex + " \\* MERGEFORMAT ", "«N" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "高標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "均標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "低標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "標準差" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count90 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count80 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count70 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count60 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count50 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count40 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count30 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count20 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count10 \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.EndRow();
                    }
                    builder.EndTable();

                    builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
                    builder.StartTable();
                    builder.InsertCell(); builder.Write("科目名稱");
                    builder.InsertCell(); builder.Write("高標");
                    builder.InsertCell(); builder.Write("均標");
                    builder.InsertCell(); builder.Write("低標");
                    builder.InsertCell(); builder.Write("標準差");
                    builder.InsertCell(); builder.Write("小於90");
                    builder.InsertCell(); builder.Write("小於80");
                    builder.InsertCell(); builder.Write("小於70");
                    builder.InsertCell(); builder.Write("小於60");
                    builder.InsertCell(); builder.Write("小於50");
                    builder.InsertCell(); builder.Write("小於40");
                    builder.InsertCell(); builder.Write("小於30");
                    builder.InsertCell(); builder.Write("小於20");
                    builder.InsertCell(); builder.Write("小於10");
                    builder.EndRow();
                    for (int subjectIndex = 1; subjectIndex <= maxNum; subjectIndex++)
                    {
                         builder.InsertCell(); builder.InsertField("MERGEFIELD 科目名稱" + subjectIndex + " \\* MERGEFORMAT ", "«N" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "高標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "均標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "低標" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "標準差" + subjectIndex + " \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count90Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count80Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count70Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count60Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count50Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count40Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count30Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count20Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距" + subjectIndex + "count10Down \\* MERGEFORMAT ", "«C" + subjectIndex + "»");
                         builder.EndRow();
                    }
                    builder.EndTable();
               }
               #endregion

               #region 加總成績分析
               builder.Writeln("加總成績分析及組距");

               builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
               builder.StartTable();
               builder.InsertCell(); builder.Write("項目");
               builder.InsertCell(); builder.Write("高標");
               builder.InsertCell(); builder.Write("均標");
               builder.InsertCell(); builder.Write("低標");
               builder.InsertCell(); builder.Write("標準差");
               builder.InsertCell(); builder.Write("100以上");
               builder.InsertCell(); builder.Write("90以上");
               builder.InsertCell(); builder.Write("80以上");
               builder.InsertCell(); builder.Write("70以上");
               builder.InsertCell(); builder.Write("60以上");
               builder.InsertCell(); builder.Write("50以上");
               builder.InsertCell(); builder.Write("40以上");
               builder.InsertCell(); builder.Write("30以上");
               builder.InsertCell(); builder.Write("20以上");
               builder.InsertCell(); builder.Write("10以上");
               builder.EndRow();
               foreach (string key in new string[] { "總分班", "總分科", "總分校", "平均班", "平均科", "平均校", "加權總分班", "加權總分科", "加權總分校", "加權平均班", "加權平均科", "加權平均校", "類1總分", "類1平均", "類1加權總分", "類1加權平均", "類2總分", "類2平均", "類2加權總分", "類2加權平均" })
               {
                    builder.InsertCell(); builder.Write(key);
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "高標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "均標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "低標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "標準差 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count100Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count90Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count80Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count70Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count60Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count50Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count40Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count30Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count20Up \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count10Up \\* MERGEFORMAT ", "«C»");
                    builder.EndRow();
               }
               builder.EndTable();
               builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
               builder.StartTable();
               builder.InsertCell(); builder.Write("項目");
               builder.InsertCell(); builder.Write("高標");
               builder.InsertCell(); builder.Write("均標");
               builder.InsertCell(); builder.Write("低標");
               builder.InsertCell(); builder.Write("標準差");
               builder.InsertCell(); builder.Write("90以上小於100");
               builder.InsertCell(); builder.Write("80以上小於90");
               builder.InsertCell(); builder.Write("70以上小於80");
               builder.InsertCell(); builder.Write("60以上小於70");
               builder.InsertCell(); builder.Write("50以上小於60");
               builder.InsertCell(); builder.Write("40以上小於50");
               builder.InsertCell(); builder.Write("30以上小於40");
               builder.InsertCell(); builder.Write("20以上小於30");
               builder.InsertCell(); builder.Write("10以上小於20");
               builder.EndRow();
               foreach (string key in new string[] { "總分班", "總分科", "總分校", "平均班", "平均科", "平均校", "加權總分班", "加權總分科", "加權總分校", "加權平均班", "加權平均科", "加權平均校", "類1總分", "類1平均", "類1加權總分", "類1加權平均", "類2總分", "類2平均", "類2加權總分", "類2加權平均" })
               {
                    builder.InsertCell(); builder.Write(key);
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "高標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "均標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "低標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "標準差 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count90 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count80 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count70 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count60 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count50 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count40 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count30 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count20 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count10 \\* MERGEFORMAT ", "«C»");
                    builder.EndRow();
               }
               builder.EndTable();
               builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
               builder.StartTable();
               builder.InsertCell(); builder.Write("項目");
               builder.InsertCell(); builder.Write("高標");
               builder.InsertCell(); builder.Write("均標");
               builder.InsertCell(); builder.Write("低標");
               builder.InsertCell(); builder.Write("標準差");
               builder.InsertCell(); builder.Write("小於90");
               builder.InsertCell(); builder.Write("小於80");
               builder.InsertCell(); builder.Write("小於70");
               builder.InsertCell(); builder.Write("小於60");
               builder.InsertCell(); builder.Write("小於50");
               builder.InsertCell(); builder.Write("小於40");
               builder.InsertCell(); builder.Write("小於30");
               builder.InsertCell(); builder.Write("小於20");
               builder.InsertCell(); builder.Write("小於10");
               builder.EndRow();
               foreach (string key in new string[] { "總分班", "總分科", "總分校", "平均班", "平均科", "平均校", "加權總分班", "加權總分科", "加權總分校", "加權平均班", "加權平均科", "加權平均校", "類1總分", "類1平均", "類1加權總分", "類1加權平均", "類2總分", "類2平均", "類2加權總分", "類2加權平均" })
               {
                    builder.InsertCell(); builder.Write(key);
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "高標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "均標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "低標 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "標準差 \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count90Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count80Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count70Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count60Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count50Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count40Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count30Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count20Down \\* MERGEFORMAT ", "«C»");
                    builder.InsertCell(); builder.InsertField("MERGEFIELD " + key + "組距count10Down \\* MERGEFORMAT ", "«C»");
                    builder.EndRow();
               }
               builder.EndTable();
               #endregion
               #endregion

               #region 儲存檔案
               string inputReportName = "個人評量成績單合併欄位總表";
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
                    doc.Save(path, Aspose.Words.SaveFormat.Doc);
                    System.Diagnostics.Process.Start(path);
               }
               catch
               {
                    System.Windows.Forms.SaveFileDialog sd = new System.Windows.Forms.SaveFileDialog();
                    sd.Title = "另存新檔";
                    sd.FileName = reportName + ".doc";
                    sd.Filter = "Excel檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                    if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                         try
                         {
                              doc.Save(path, Aspose.Words.SaveFormat.Doc);
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
     }
}


