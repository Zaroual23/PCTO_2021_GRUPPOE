using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SetlistNet.Models;

namespace SaveConcert
{
    public partial class SetListView : Form
    {
        Setlist data;

        public SetListView(Setlist setlists)
        {
            data = setlists;
            InitializeComponent();

            labelArtist.Text = "Artista: " + data.Artist.Name;
        }

        private void SetListView_Load(object sender, EventArgs e)
        {
            int lastY = 0;

            foreach (var i in data.Sets)
            {
                foreach(var j in i.Songs)
                {
                    ListTileForSetListView item = new ListTileForSetListView(j.Name);

                    panelForData.Controls.Add(item);

                    item.Location = new Point(item.Location.X, lastY);
                    lastY += 80;
                }
               
            }
        }
    }
}
