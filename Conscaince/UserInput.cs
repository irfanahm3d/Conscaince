using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace Conscaince
{
    class UserInput
    {
        IReadOnlyList<string> acceptedUserInput = new List<string>
        {
            "yes",
            "no",
            "sure",
            "not yet",
            "heck yeah",
            "yes why not",
            "nope",
            "naw",
            "maybe"
        };

        static UserInput userInputInstance;

        public static UserInput UserInputInstance
        {
            get
            {
                if (userInputInstance == null)
                    userInputInstance = new UserInput();

                return userInputInstance;
            }
        }

        public async Task<string> RecordSpeechFromMicrophoneAsync()
        {
            string recognizedText = string.Empty;
            
            using (SpeechRecognizer recognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage))
            {
                recognizer.Constraints.Add(new SpeechRecognitionListConstraint(acceptedUserInput));
                await recognizer.CompileConstraintsAsync();

                SpeechRecognitionResult result = await recognizer.RecognizeAsync();
                StringBuilder stringBuilder = new StringBuilder();

                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    if (result.Confidence == SpeechRecognitionConfidence.High)
                    {
                        stringBuilder.Append(result.Text);
                    }
                    else
                    {
                        IReadOnlyList<SpeechRecognitionResult> alternatives =
                            result.GetAlternates(1);

                        if (alternatives.First().RawConfidence > 0.5)
                        {
                            stringBuilder.Append(alternatives.First().Text);
                        }
                    }

                    recognizedText = stringBuilder.ToString();
                }
            }
            return (recognizedText);
        }
    }
}
