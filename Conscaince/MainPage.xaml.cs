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
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Conscaince
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// The core hub instance.
        /// </summary>
        CoreHub coreHub = CoreHub.CoreHubInstance;

        SpeechRecognizer speechReg;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        public async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage_Loaded");

            coreHub.CurrentNodeChanged += HasCurrentNodeChanged;
            await coreHub.Initialize();
            
            speechReg = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);
        }

        async void HasCurrentNodeChanged(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                debugTitle.Text = sender.ToString();
            });
        }

        async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                coreHub.userInput = yesButton.Content.ToString().ToLower();
            });
        }

        async void NoButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                coreHub.userInput = noButton.Content.ToString().ToLower();
            });
        }

        async void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await coreHub.TraverseNodesAsync();
            });
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
