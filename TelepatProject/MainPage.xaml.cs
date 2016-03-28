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
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Telepat.GetInstance().DeviceRegistered += delegate {

                TaskHelper.RunOnUiThread(this.Dispatcher, () => {

                    button.IsEnabled = true;
                    button1.IsEnabled = true;
                    button3.IsEnabled = true;
                    button4.IsEnabled = true;
                });
            };

            Telepat.GetInstance().LoginUserSuccess += async delegate(object s, string arg) {

                pollsChannel = await Telepat
                                     .GetInstance()
                                     .Subscribe<polls>(Telepat
                                                       .GetInstance()
                                                       .GetContexts()
                                                       .Get("dda11deb-a259-43d8-b5ee-cf1010dc8a00"),
                                                       "polls",
                                                       new ChannelEventListener()
                                                       {
                                                           SubscribeComplete = OnPollsSubscribeComplete,

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
            };

            Telepat.GetInstance().LoginUserFailure += delegate (object s, string message) {
                int x = 0;
            };

            Telepat.GetInstance().LogoutSuccess += delegate {
                int x = 0;
            };

            Telepat.GetInstance().LogoutFailure += delegate {
                int x = 0;
            };

            Telepat.GetInstance().RequestPasswordResetFailure += delegate {
                int x = 0;
            };

            Telepat.GetInstance().RequestPasswordResetSuccess += delegate {
                int x = 0;
            };

            await Telepat.GetInstance().Initialize("http://devel-api.cloudapp.net:3000/",
                                                   "e10adc3949ba59abbe56e057f20f883e",
                                                   "0a686e52-36ef-4c45-8ab4-b228b57af86a",
                                                   true,
                                                   "http://devel-workers.cloudapp.net");

            Telepat.GetInstance().LoginWithUsername("dorin@veeplay.com", "parola");
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Telepat.GetInstance().Logout();
        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            var mChannel = await Telepat.GetInstance().Subscribe<answers>(Telepat
                                                                          .GetInstance()
                                                                          .GetContexts()
                                                                          .Get("9a646cc5-d27c-4545-8007-ebcea82edc47"),
                                                                          "answers");
        }

        private async void button4_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
