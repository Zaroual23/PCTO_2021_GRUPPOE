using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;



namespace progetto_pcto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btncerca_Click(object sender, EventArgs e)
        {
            List<string> Elecantanti = new List<string>();
            List<string> Elecanzoni = new List<string>();
            List<string> Elecitta = new List<string>();

            if (comboBox1.Text == "")
            {
                MessageBox.Show("inserire un cantante");
                return;
            }
            var cantante = comboBox1.Text;

            HttpClient clint = new HttpClient();
            clint.BaseAddress = new Uri("https://api.setlist.fm/docs/");
            HttpResponseMessage risposta = clint.GetAsync("1.0/search/artists").Result;

            var artista = risposta.Content.ReadAsStringAsync().Result;

            //comboBox1.Text = risposta.ToString();
            dataGridView1.DataSource = artista;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Style.ReadOnlyEditorStyle.BorderColor = Color.Red;
            comboBox1.Style.ReadOnlyEditorStyle.ForeColor = Color.Blue;
            comboBox1.Style.ReadOnlyEditorStyle.Font = new Font("Arial", 10F, FontStyle.Bold);

        }
    }



    }
}
