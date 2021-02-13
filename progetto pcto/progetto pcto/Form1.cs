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
using System.Reflection;
using System.Collections;

namespace progetto_pcto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    int a = 0;
        //    string b = null;
        //    ArrayList ColorList = new ArrayList();
        //    Type colorType = typeof(System.Drawing.Color);
        //    PropertyInfo[] propInfoList = colorType.GetProperties(BindingFlags.Static |
        //                                  BindingFlags.DeclaredOnly | BindingFlags.Public);
        //    foreach (PropertyInfo a in propInfoList)
        //    {
        //        //this.comboBox2.Items.Add(a.Name);
        //    }
        //    List<string> Elecantanti = new List<string>();
        //    List<string> Elecanzoni = new List<string>();
        //    List<string> Elecitta = new List<string>();

        //    if (b == "")
        //    {
        //        MessageBox.Show("inserire un cantante");
        //        return;
        //    }
        //    var cantante = a;

        //    #region
        //    HttpClient clint = new HttpClient();
        //    clint.BaseAddress = new Uri("https://api.setlist.fm/docs/");
        //    HttpResponseMessage risposta = clint.GetAsync("1.0/search/artists").Result;
        //    var artista = risposta.Content.ReadAsStringAsync().Result;

        //    //comboBox1.Text = risposta.ToString();
        //    //dataGridView1.DataSource = artista;

        //    HttpClient c = new HttpClient();
        //    clint.BaseAddress = new Uri("https://api.setlist.fm/docs/");
        //    HttpResponseMessage r = clint.GetAsync("1.0/search/cities").Result;
        //    var city = risposta.Content.ReadAsStringAsync().Result;

        //    HttpClient cl = new HttpClient();
        //    clint.BaseAddress = new Uri("https://api.setlist.fm/docs/");
        //    HttpResponseMessage re = clint.GetAsync("1.0/search/setlists").Result;
        //    var song = risposta.Content.ReadAsStringAsync().Result;

        //    HttpClient clie = new HttpClient();
        //    clint.BaseAddress = new Uri("https://api.setlist.fm/docs/");
        //    HttpResponseMessage resp = clint.GetAsync("1.0/search/{mbid}/setlists").Result;
        //    var datac = risposta.Content.ReadAsStringAsync().Result;
   #endregion

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
