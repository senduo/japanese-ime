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
            foreach(var c in new Control[] { txtInput, txtLam2, txtLam1, txtLam0 }) 
                c.Enabled = false;
            
            await Task.Run(() => {
                const string Dir = @"..\..\..\Resources\";
                this.api = new Api(
                    Dir + "naist-jdic-utf8.zip",
                    Dir + "unigram-freq.zip",
                    Dir + "bigram-freq.zip"
                );
            });

            txtLam2.Text = api.BigramParams["lam2"].ToString();
            txtLam1.Text = api.BigramParams["lam1"].ToString();
            txtLam0.Text = api.BigramParams["lam0"].ToString();

            txtInput.Text = "";
            slStatus.Text = "Ready.";
            foreach (var c in new Control[] { txtInput, txtLam2, txtLam1, txtLam0 })
                c.Enabled = true;
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
            api.BigramParams["lam2"] = double.Parse(txtLam2.Text);
            api.BigramParams["lam1"] = double.Parse(txtLam1.Text);
            api.BigramParams["lam0"] = double.Parse(txtLam0.Text);

            var wg = api.BuildWordGraph(s.Val);
            var res = api.ConvertByBigram(wg.Val);
            txtResult.Text = res.Val.ToString();
            slStatus.Text = "";
        }

        void HandleParamChange(TextBox tb, string paramName) {
            double x;
            if (!double.TryParse(tb.Text, out x) || x < 0.0 || x > 1.0) {
                tb.Text = api.BigramParams[paramName].ToString();
                return;
            }
            txtInput_TextChanged(null, null);
        }

        private void txtLam2_TextChanged(object sender, EventArgs e) {
            HandleParamChange(txtLam2, "lam2");
        }

        private void txtLam1_TextChanged(object sender, EventArgs e) {
            HandleParamChange(txtLam1, "lam1");
        }

        private void txtLam0_TextChanged(object sender, EventArgs e) {
            HandleParamChange(txtLam0, "lam0");
        }
    }
}
