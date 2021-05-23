using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using GlobeViewer.Classes;
using GlobeViewer.Interfaces;
using SetlistNet;
using SetlistNet.Models;

namespace SaveConcert
{
    public partial class Form1 : Form
    {
        public IGlobeViewer gv = default(IGlobeViewer);
        public SetlistApi sam;
        private bool taskRunning;

        private List<Setlists> lastSearch;
        SetListView form;

        public Form1()
        {
            InitializeComponent();

            gv = new GlobeViewer.Classes.GlobeViewer(panel1);
            gv.BindMarkerClickedEvent(delegate (object sender, string location)
            {

                Setlist item = default;

                foreach (var i in lastSearch)
                {
                   foreach (var j in i)
                    {
                        if (j.Id == location)
                        {
                            item = j;
                        }
                    }
                }

                if (item != null)
                {
                    form = new SetListView(item);
                    this.Invoke(new Action(() => form.Show()));
                }
            });

            sam = new SetlistApi("7saSG593hLsZ-onjCeYCize9zoMMP59Vf7an");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                if (!taskRunning)
                {
                   if (form != null) form.Close();
                    button1.Enabled = false;
                    textBox1.Enabled = false;
                    Task.Run(() =>
                    {
                        taskRunning = true;

                        //Set up query
                        Setlist query = new Setlist();
                        query.Artist = new Artist(textBox1.Text);
                        List<Setlists> result = sam.SearchSetlists(query);
                        lastSearch = result;

                        this.Invoke(new Action(() =>
                        {
                            textBox1.Enabled = true;
                            button1.Enabled = true;
                        }));
                        

                        //Send data to GlobeViewer
                        IList<(string, string, string, string)> locations = new List<(string MarkerName, string Name, string X, string Y)>();
                        
                        foreach (Setlists j in result)
                        {
                            foreach (Setlist i in j)
                            {
                                locations.Add((i.Id, i.Venue.City.State + " " + i.Venue.City.Name + " " + i.Venue.Name, i.Venue.City.Coords.Longitude.ToString(), i.Venue.City.Coords.Latitude.ToString()));
                            }
                        }
                       
                        try
                        {
                            gv.LoadMarkers(locations, out var _);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        taskRunning = false;

                        
                    });
                }

            }
            else
            {
                MessageBox.Show("Inserisci qualcosa nel campo");
            }

        }
    }
}
