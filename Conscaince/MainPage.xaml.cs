using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Conscaince.TrackSense;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Conscaince
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string playListUri = "ms-appx:///Assets/playlist.json";

        SpeechRecognizer speechReg;
        
        /// <summary>
        /// The audio player service instance.
        /// </summary>
        AudioPlayerService audioService = AudioPlayerService.AudioPlayerInstance;

        /// <summary>
        /// A list of items to be played for the base audio track
        /// and the interrupt track
        /// </summary>
        AudioList playList;

        JsonReader jsonPlaylist = JsonReader.JsonReaderInstance;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        public async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage_Loaded");
            
            speechReg = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);

            this.playList = new AudioList();
            await this.jsonPlaylist.LoadFromApplicationUriAsync(playListUri);
            await this.playList.CreateAudioPlaybackLists();
            await this.audioService.InitializeAudioPlayers(this.playList);

            //this.playList.BasePlaybackList.CurrentItemChanged += PlayBackItemChange;

            await this.audioService.Play(audioService.Player);
        }

        async void SpeakButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                string spokenWords = await RecordSpeechFromMicrophoneAsync();
                speechTitle.Text = spokenWords;
            });
        }

        async Task<string> RecordSpeechFromMicrophoneAsync()
        {
            string recognizedText = string.Empty;

            // the languages supported for grammars (SRGS, word lists, etc)
            //List<Language> languagesForGrammars =
            //       SpeechRecognizer.SupportedGrammarLanguages.ToList();

            using (SpeechRecognizer recognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage))
            {
                await recognizer.CompileConstraintsAsync();

                SpeechRecognitionResult result = await recognizer.RecognizeAsync();
                StringBuilder stringBuilder = new StringBuilder();

                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    if (result.Confidence == SpeechRecognitionConfidence.High)
                    {
                        stringBuilder.Append("We are confident you said '{result.Text}'");
                    }
                    else
                    {
                        IReadOnlyList<SpeechRecognitionResult> alternatives =
                        result.GetAlternates(3); // max number wanted

                        foreach (var option in alternatives)
                        {
                            stringBuilder.AppendLine("We are { option.RawConfidence * 100:N2}% confident you said '{option.Text}'");
                        }
                    }

                    recognizedText = stringBuilder.ToString();
                }
            }
            return (recognizedText);
        }
    }
}
