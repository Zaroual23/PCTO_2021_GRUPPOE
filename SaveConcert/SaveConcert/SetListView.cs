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
using System.IO;
using SpotifyAPI.Web;
using SpotifyAPI;
using SpotifyAPI.Web.Auth;
using Newtonsoft.Json;
using System.Security;
using System.Security.Permissions;

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
            labelDate.Text = "Data: " + data.EventDate;
        }

        List<string> names = new List<string>();

        private void SetListView_Load(object sender, EventArgs e)
        {

            //if (File.Exists(SpotifyClass.CredentialsPath))
            //{
            //    button2.Visible = false;
            //    client = new SpotifyClass();
            //}

            int lastY = 0;

            foreach (var i in data.Sets)
            {
                foreach (var j in i.Songs)
                {
                    ListTileForSetListView item = new ListTileForSetListView(j.Name);

                    panelForData.Controls.Add(item);

                    item.Location = new Point(item.Location.X, lastY);
                    lastY += 80;
                    names.Add(j.Name);
                }

            }


            //if (names.Count == 0)
            //{
            //    button1.Enabled = false;
            //    button2.Enabled = false;
            //}
        }

        //SpotifyClass client = null;

        //private async void button1_Click(object sender, EventArgs e)
        //{




        //    if (client != null)
        //    {

        //        var list = new List<SpotifyClass.Track>();


        //        foreach (string i in this.names) list.Add(new SpotifyClass.Track(i, data.Artist.Name));


        //        string a = data.Artist.Name + " " + data.TourName + " " + data.EventDate;

        //        await client.createPlaylist(a, list);

        //        button2.Visible = false;


        //    }

        //    else Console.WriteLine("Autenticati prima di aggiungere le canzoni");
        //}

        //private void button2_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                client = new SpotifyClass();

            }

        }

    }
    public class SpotifyClass
    {
        public const string CredentialsPath = "credentials";
        private readonly string clientId = "fa3f6d3824f6436bbe0508b85c84b39c";
        private readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

        private SpotifyClient spotify;

        public SpotifyClass()
        {
            if (File.Exists(CredentialsPath))


            {
                Start();
            }
            else
            {
                StartAuthentication();
            }

        }

        private void Start()
        {
            var json = File.ReadAllText(CredentialsPath);
            var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

            var authenticator = new PKCEAuthenticator(clientId, token);
            authenticator.TokenRefreshed += (sender, tokens) => File.WriteAllText(CredentialsPath, JsonConvert.SerializeObject(tokens));

            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);

            spotify = new SpotifyClient(config);

            _server.Dispose();
        }

        private void StartAuthentication()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            Task.WaitAll(_server.Start());

            _server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await _server.Stop();

                PKCETokenResponse dataTask = await new OAuthClient().RequestToken(
                  new PKCETokenRequest(clientId, response.Code, _server.BaseUri, verifier)
                );

                File.WriteAllText(CredentialsPath, JsonConvert.SerializeObject(dataTask));
                File.SetAttributes(CredentialsPath, FileAttributes.ReadOnly);

                Start();
            };

            var request = new LoginRequest(_server.BaseUri, clientId, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string> { SpotifyAPI.Web.Scopes.UgcImageUpload, SpotifyAPI.Web.Scopes.UserReadRecentlyPlayed, SpotifyAPI.Web.Scopes.UserReadPlaybackPosition, SpotifyAPI.Web.Scopes.UserTopRead, SpotifyAPI.Web.Scopes.UserLibraryRead, SpotifyAPI.Web.Scopes.UserLibraryModify, SpotifyAPI.Web.Scopes.PlaylistModifyPrivate, SpotifyAPI.Web.Scopes.PlaylistReadPrivate, SpotifyAPI.Web.Scopes.UserFollowRead, SpotifyAPI.Web.Scopes.PlaylistModifyPublic, SpotifyAPI.Web.Scopes.UserReadPrivate, SpotifyAPI.Web.Scopes.UserReadEmail, SpotifyAPI.Web.Scopes.AppRemoteControl, SpotifyAPI.Web.Scopes.Streaming, SpotifyAPI.Web.Scopes.UserReadCurrentlyPlaying, SpotifyAPI.Web.Scopes.UserModifyPlaybackState, SpotifyAPI.Web.Scopes.UserReadPlaybackState, SpotifyAPI.Web.Scopes.PlaylistReadCollaborative, SpotifyAPI.Web.Scopes.UserFollowModify }
            };

            Uri uri = request.ToUri();
            try
            {
                BrowserUtil.Open(uri);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to open URL, manually open: {0}", uri);
            }
        }

        public class Track
        {
            public string nome { get; }
           
            public string artista { get; }

            public Track(string p_nome, string p_artista)
            {
                if (string.IsNullOrEmpty(p_nome)) throw new ArgumentNullException("Il nome non può essere vuoto");
                if (string.IsNullOrEmpty(p_artista)) throw new ArgumentNullException("L'artista non può essere vuoto");

                this.nome = p_nome.ToLower();
                this.artista = p_artista.ToLower();
            }
        }

        public async Task<string> findTrackUri(Track item)
        {
            bool ok = false;
            string b = string.Empty;
            while (!ok)
            {
                if (spotify != null)
                {
                    var a = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, $"artist:{item.artista} track:{item.nome}"));
                    ok = true;
                    b = a.Tracks.Items[0].Uri;
                }

            }
            if (b != null)
                return b;

            return "";
        }

        public async Task createPlaylist(string name, List<Track> canzoni = null)
        {
            if (spotify == null) throw new Exception("Devi eseguire l'autenticazione prima di chiamare questo metodo");

            if (string.IsNullOrEmpty(name)) name = "default";

            var user = await spotify.UserProfile.Current();
            var playlist = await spotify.Playlists.Create(user.Id, new PlaylistCreateRequest(name));

            var songs = new List<string>();


            foreach (var i in canzoni)
            {
                var id = await findTrackUri(i);
                if (!string.IsNullOrEmpty(id)) songs.Add(id);
            }

            await spotify.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(songs));
        }
    }


}
