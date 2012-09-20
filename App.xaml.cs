using ContosoCookbook.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ContosoCookbook.Data;
using Windows.ApplicationModel.Search;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Callisto.Controls;
using Windows.Storage;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace ContosoCookbook
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Color _background = Color.FromArgb(255, 0, 77, 96);

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                
                // Load Recipes Async
                await RecipeDataSource.LoadLocalDataAsync();

                // If the app was closed by the user the last time it ran, and if "Remember
                // "where I was" is enabled, restore the navigation state
                if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
                {
                    if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("Remember"))
                    {
                        bool remember = (bool)ApplicationData.Current.RoamingSettings.Values["Remember"];
                        if (remember)
                            await SuspensionManager.RestoreAsync();
                    }
                }

                // Register handler for SuggestionsRequested events from the search pane
                SearchPane.GetForCurrentView().SuggestionsRequested += OnSuggestionsRequested;

                // Register handler for CommandsRequested events from the settings pane
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        void OnSuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            string query = args.QueryText.ToLower();
            string[] terms = { "salt", "pepper", "water", "egg", "vinegar", "flour", "rice", "sugar", "oil" };

            foreach (var term in terms)
            {
                if (term.StartsWith(query))
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(term);
            }
        }


        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
            // event in OnWindowCreated to speed up searches once the application is already running
            // Reinitialize the app if a new instance was launched for search
            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Load recipe data
                await RecipeDataSource.LoadLocalDataAsync();

                // Register handler for SuggestionsRequested events from the search pane
                SearchPane.GetForCurrentView().SuggestionsRequested += OnSuggestionsRequested;

                // Register handler for CommandsRequested events from the settings pane
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;

            }

            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // If the app does not contain a top-level frame, it is possible that this 
            // is the initial launch of the app. Typically this method and OnLaunched 
            // in App.xaml.cs can call a common method.
            if (frame == null)
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                frame = new Frame();
                ContosoCookbook.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await ContosoCookbook.Common.SuspensionManager.RestoreAsync();
                    }
                    catch (ContosoCookbook.Common.SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }
            }

            frame.Navigate(typeof(SearchResultsPage), args.QueryText);
            Window.Current.Content = frame;

            // Ensure the current window is active
            Window.Current.Activate();
        }

        void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // Add an About command
            var about = new SettingsCommand("about", "About", (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new AboutUserControl();
                settings.HeaderBrush = new SolidColorBrush(_background);
                settings.Background = new SolidColorBrush(_background);
                settings.HeaderText = "About";
                settings.IsOpen = true;
            });

            args.Request.ApplicationCommands.Add(about);

            // Add an Preferences command
            var preferences = new SettingsCommand("preferences", "Preferences", (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new PreferencesUserControl();
                settings.HeaderBrush = new SolidColorBrush(_background);
                settings.Background = new SolidColorBrush(_background);
                settings.HeaderText = "Preferences";
                settings.IsOpen = true;
            });

            args.Request.ApplicationCommands.Add(preferences);
        }

    }
}
