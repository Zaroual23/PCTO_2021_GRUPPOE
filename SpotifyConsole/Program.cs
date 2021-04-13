using System.IO;
using System.Threading.Tasks;
using System;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using static SpotifyAPI.Web.Scopes;

namespace Example.CLI.PersistentConfig
{
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
                Scope = new List<string> { UgcImageUpload, UserReadRecentlyPlayed, UserReadPlaybackPosition, UserTopRead, UserLibraryRead, UserLibraryModify, PlaylistModifyPrivate, PlaylistReadPrivate, UserFollowRead, PlaylistModifyPublic, UserReadPrivate, UserReadEmail, AppRemoteControl, Streaming, UserReadCurrentlyPlaying, UserModifyPlaybackState, UserReadPlaybackState, PlaylistReadCollaborative, UserFollowModify }
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
            public string album { get; }
            public string artista { get; }

            public Track(string p_nome, string p_album, string p_artista)
            {
                if (string.IsNullOrEmpty(p_nome)) throw new ArgumentNullException("Il nome non può essere vuoto");
                if (string.IsNullOrEmpty(p_album)) throw new ArgumentNullException("L'album non può essere vuoto");
                if (string.IsNullOrEmpty(p_artista)) throw new ArgumentNullException("L'artista non può essere vuoto");

                this.nome = p_nome.ToLower();
                this.album = p_album.ToLower();
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
                    var a = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, $"album:{item.album} artist:{item.artista} track:{item.nome}"));
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

    public class Program
    {
        public static void Main()
        {
            var client = new SpotifyClass(); // Chiamare questa in una funzione separata perché è asincrona


            // dopo aver eseguito l'autenticazione si può chiamare questa funzione
            Task.WaitAll(client.createPlaylist("PrimaPlaylist", new List<SpotifyClass.Track>()
            {
                new SpotifyClass.Track("don't stop th party", "the beginning", "black eyed peas"),
                new SpotifyClass.Track("Godzilla", "Music to be murdered by", "Eminem")
            }));
        }
    }
}