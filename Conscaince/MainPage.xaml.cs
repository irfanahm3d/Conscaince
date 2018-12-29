using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        UserInput input = UserInput.UserInputInstance;
        DispatcherTimer timer;
        Stopwatch stopwatch;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        public async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage_Loaded");
            this.ToggleUserButtons(false);

            coreHub.CurrentNodeChanged += OnCurrentNodeChanged;
            coreHub.CurrentNodeCompleted += OnCurrentNodeCompleted;
            timer = new DispatcherTimer();
            stopwatch = new Stopwatch();
            timer.Tick += OnTick;
            timer.Interval = new TimeSpan(0, 0, 1);
            coreHub.Initialize();            
        }

        async void OnCurrentNodeChanged(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                debugTitle.Text = sender.ToString();
            });
        }

        async void OnCurrentNodeCompleted(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                int actionCount = (int)sender;
                if (actionCount > 1)
                {
                    this.ToggleUserButtons(true);
                }
            });
        }

        async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                coreHub.userInput = yesButton.Content.ToString().ToLower();
                this.ToggleUserButtons(false);
            });
        }

        async void NoButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                coreHub.userInput = noButton.Content.ToString().ToLower();
                this.ToggleUserButtons(false);
            });
        }

        async void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                timer.Start();
                stopwatch.Start();
                coreHub.TraverseNodesAsync();
                beginButton.IsEnabled = false;
            });
        }

        async Task ToggleUserButtons(bool enable)
        {
            yesButton.IsEnabled = enable;
            noButton.IsEnabled = enable;
        }

        async void OnTick(object sender, object e)
        {
            timerText.Text = stopwatch.Elapsed.ToString("mm\\:ss");
        }
    }
}
