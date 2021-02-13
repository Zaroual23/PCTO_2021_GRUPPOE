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

using System.Net.Http.Headers;



namespace progetto_pcto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
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
    }
}
        public class DataObject
        {
            public string Name { get; set; }
        }

        public class Class1
        {
            private const string URL = "https://api.setlist.fm/docs/";
            private string urlParameters = "?api_key=123";

            static void Main(string[] args)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(URL);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                    foreach (var d in dataObjects)
                    {
                        Console.WriteLine("{0}", d.Name);
                    }
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }

                // Make any other calls using HttpClient here.

                // Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
                client.Dispose();
            }
        }
    }

}

