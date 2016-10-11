using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SH_SemesterScoreReport_jvyjhs
{
    public partial class MemoForm : FISCA.Presentation.Controls.BaseForm
    {
        public MemoForm()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public void SetDateStr(string str)
        {
            txtDateStr.Text = str;
        }

        public string GetTextDateStr()
        {
            return txtDateStr.Text;
        }

        public void SetMemo(string str)
        {
            txtMemo.Text = str;
        }

        public string GetMemo()
        {
            return txtMemo.Text;
        }

    }
}
