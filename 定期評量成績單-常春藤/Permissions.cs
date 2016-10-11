using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SH_jvyjhs_StudentExamScore_epost
{
    class Permissions
    {
        public static string SH_jvyjhs_StudentExamScore_epost { get { return "SH_jvyjhs_StudentExamScore_epost.D04E7F02-89C1-4412-81FA-8D87B96BF847"; } }

        public static bool SH_jvyjhs_StudentExamScore_epost權限
        {
            get { return FISCA.Permission.UserAcl.Current[SH_jvyjhs_StudentExamScore_epost].Executable; }
        }
    }
}
