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
    public class PctoSpotifyClient
    {
        public interface ITrack
        {
            string nome { get; }
            string album { get; }
            string artista { get; }
        }
        public class Track : ITrack
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

        public async Task InitializeAsync()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exiting();

            if (File.Exists(CredentialsPath))
            {
                Start();
            }
            else
            {
                await StartAuthentication();
            }
        }

        public const string CredentialsPath = "credentials";
        private readonly string clientId = "fa3f6d3824f6436bbe0508b85c84b39c";
        private readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

        private SpotifyClient spotify;

        private void Exiting() => Console.CursorVisible = true;

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

        private async Task StartAuthentication()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            await _server.Start();
            _server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await _server.Stop();
                PKCETokenResponse token = await new OAuthClient().RequestToken(
                  new PKCETokenRequest(clientId, response.Code, _server.BaseUri, verifier)
                );

                File.WriteAllText(CredentialsPath, JsonConvert.SerializeObject(token));
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

        private async Task<string> findTrackId(ITrack item)
        {
            var searchRequest = new SearchRequest(SearchRequest.Types.Track, $"album:{item.album} artist:{item.artista} track:{item.nome}");

            SearchResponse response = default;

            response = await spotify.Search.Item(searchRequest);

            if (response.Tracks.Items.Count < 1) return string.Empty;
            if (response.Tracks.Items.Count > 1)
            {
                int num = response.Tracks.Items.Count;
                Random rnd = new Random();
                num = rnd.Next(1, num + 1);

                return response.Tracks.Items[num].Id;
            }

            return string.Empty;
        }

        public async Task createPlaylist(string name, List<ITrack> canzoni = null)
        {
            if (string.IsNullOrEmpty(name)) name = "default";

            var user = await spotify.UserProfile.Current();
            var playlist = await spotify.Playlists.Create(user.Id, new PlaylistCreateRequest(name));

            if (canzoni != null)
            {
                var songs = new List<string>();

                foreach (var i in canzoni)
                {
                    var id = await findTrackId(i);
                    if (!string.IsNullOrEmpty(id)) songs.Add(id);
                }

                var addItemRequest = new PlaylistAddItemsRequest(songs);
                var a = await spotify.Playlists.AddItems(playlist.Id, addItemRequest);
            }
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var client = new PctoSpotifyClient();
            await client.InitializeAsync();

            Console.ReadKey();
        }
    }
}