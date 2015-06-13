using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ime;

namespace Ime.Gui {
    public partial class Main : Form {
        Api api;

        public Main() {
            InitializeComponent();
            InitApiAsync();
        }

        async void InitApiAsync() {
            slStatus.Text = "Loading Dictionary and Freq Files...";
            txtInput.Enabled = false;
            
            await Task.Run(() => {
                const string Dir = @"..\..\..\Resources\";
                this.api = new Api(Dir + "1-gram.zip", Dir + "2-gram.zip");
            });

            txtInput.Text = "";
            slStatus.Text = "Ready.";
            txtInput.Enabled = true;
            txtInput.Focus();
        }

        private void txtInput_TextChanged(object sender, EventArgs e) {
            var s = api.NormalizeInput(txtInput.Text);
            if (s.Val == "") {
                txtResult.Text = "";
                slStatus.Text = "";
                return;
            }
            else if (s.IsErr) {
                txtResult.Text = "";
                slStatus.Text = s.ErrMsg;
                return;
            }
            var wg = api.BuildWordGraph(s.Val);
            var res = api.ConvertByBigram(wg.Val);
            txtResult.Text = res.Val.ToString();
            slStatus.Text = "";
        }
    }
}
