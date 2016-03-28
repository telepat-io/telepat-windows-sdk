using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TelepatSDK;
using Windows.ApplicationModel;
using TelepatProject.Utils;
using TelepatSDK.Models;
using TelepatProject.Data;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TelepatProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Channel pollsChannel;
        Channel answersChannel;

        public MainPage()
        {
            this.InitializeComponent();

            Telepat.GetInstance().LoginUserSuccess += OnLoginUserSuccess;
            Telepat.GetInstance().LoginUserFailure += OnLoginUserFailure;

            Telepat.GetInstance().LogoutSuccess += OnLogoutSuccess;
            Telepat.GetInstance().LogoutFailure += OnLogoutFailure;
        }

        private void OnLoginUserSuccess(object sender, string args) {
            TaskHelper.RunOnUiThread(this.Dispatcher, async () =>
            {
                var dialog = new MessageDialog("User " + args + " logged in!");
                await dialog.ShowAsync();
            });
            // subscribe to polls
            //pollsChannel = await Telepat
            //                     .GetInstance()
            //                     .Subscribe<polls>(Telepat
            //                                       .GetInstance()
            //                                       .GetContexts()
            //                                       .Get("dda11deb-a259-43d8-b5ee-cf1010dc8a00"),
            //                                       "polls",
            //                                       new ChannelEventListener()
            //                                       {
            //                                           SubscribeComplete = OnPollsSubscribeComplete,

            //                                           ObjectCreateSuccess = delegate
            //                                           {
            //                                               int x = 0;
            //                                           },
            //                                           ObjectAdded = delegate
            //                                           {
            //                                               int x = 0;
            //                                           },
            //                                           ObjectModified = delegate
            //                                           {
            //                                               int x = 0;
            //                                           },
            //                                           ObjectRemoved = delegate
            //                                           {
            //                                               int x = 0;
            //                                           },
            //                                           Error = delegate
            //                                           {
            //                                               int x = 0;
            //                                           }
            //                                       });
        }

        private void OnLoginUserFailure(object sender, string args) {
            TaskHelper.RunOnUiThread(this.Dispatcher, async () =>
            {
                var dialog = new MessageDialog("Unable to login: " + args);
                await dialog.ShowAsync();
            });
        }

        private void OnLogoutSuccess(object sender, object args) {
            TaskHelper.RunOnUiThread(this.Dispatcher, async () =>
            {
                var dialog = new MessageDialog("dorin@appscend.com logged out!");
                await dialog.ShowAsync();
            });
        }

        private void OnLogoutFailure(object sender, object args) {
            TaskHelper.RunOnUiThread(this.Dispatcher, async () =>
            {
                var dialog = new MessageDialog("Unable to logout: " + args);
                await dialog.ShowAsync();
            });
        }

        async void OnPollsSubscribeComplete(object sender, EventArgs args)
        {
            var answersChannel = await Telepat
                                       .GetInstance()
                                       .Subscribe<polls>(Telepat
                                                        .GetInstance()
                                                        .GetContexts()
                                                        .Get("dda11deb-a259-43d8-b5ee-cf1010dc8a00"),
                                                        "answers",
                                                        new ChannelEventListener()
                                                        {
                                                            SubscribeComplete = OnAnswersSubscribeComplete,

                                                            ObjectCreateSuccess = delegate
                                                            {
                                                                int x = 0;
                                                            },
                                                            ObjectAdded = delegate
                                                            {
                                                                int x = 0;
                                                            },
                                                            ObjectModified = delegate
                                                            {
                                                                int x = 0;
                                                            },
                                                            ObjectRemoved = delegate
                                                            {
                                                                int x = 0;
                                                            },
                                                            Error = delegate
                                                            {
                                                                int x = 0;
                                                            }
                                                       });
        }

        async void OnAnswersSubscribeComplete(object sender, EventArgs args)
        {
            var polls = await Telepat
                              .GetInstance()
                              .GetDBInstance()
                              .GetChannelObjects<polls>(pollsChannel.GetSubscriptionIdentifier());

            var answers = await Telepat
                                .GetInstance()
                                .GetDBInstance()
                                .GetChannelObjects<answers>((sender as Channel).GetSubscriptionIdentifier());


            foreach (var answer in answers)
            {
                (sender as Channel).Listen(answer);
                answer.option_index = 1;
            }

            answers = await Telepat
                            .GetInstance()
                            .GetDBInstance()
                            .GetChannelObjects<answers>((sender as Channel).GetSubscriptionIdentifier());
        }


        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Telepat.GetInstance().DeviceRegistered += OnDeviceRegistered;

            // init telepat
            await Telepat.GetInstance().Initialize("http://devel-api.cloudapp.net:3000/",
                                                   "e10adc3949ba59abbe56e057f20f883e",
                                                   "0a686e52-36ef-4c45-8ab4-b228b57af86a",
                                                   true,
                                                   "http://devel-workers.cloudapp.net");
        }

        private void OnDeviceRegistered(object sender, object args) {
            TaskHelper.RunOnUiThread(this.Dispatcher, () => {

                // activate UI buttons
                foreach (var item in buttonContainer.Children)
                {
                    if (item is Button) (item as Button).IsEnabled = true;
                }
            });
        }

        private void register_Click(object sender, RoutedEventArgs e)
        {
            Telepat.GetInstance().CreateUserSuccess += delegate {
                TaskHelper.RunOnUiThread(this.Dispatcher, () =>
                {
                    register.IsEnabled = false;
                    var dialog = new MessageDialog("User created succesfully!");
                    var task = dialog.ShowAsync();
                });
            };

            Telepat.GetInstance().CreateUserFailure += delegate (object s, string args) {
                TaskHelper.RunOnUiThread(this.Dispatcher, () => {

                    var dialog = new MessageDialog("Unable to create user: " + args);
                    var task = dialog.ShowAsync();
                });
            };

            Telepat.GetInstance().CreateUser("dorin@appscend.com", "parola", "Dorin");
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            Telepat.GetInstance().LoginWithUsername("dorin@appscend.com", "parola");
        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            Telepat.GetInstance().Logout();
        }

        private async void contexts_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
