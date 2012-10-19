﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediaBrowser.ApiInteraction.WindowsPhone;
using MediaBrowser.WindowsPhone.Model;
using MediaBrowser.Model.DTO;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Tasks;
using ScottIsAFool.WindowsPhone;

namespace MediaBrowser.WindowsPhone.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MovieViewModel : ViewModelBase
    {
        private readonly INavigationService NavService;
        private readonly ApiClient ApiClient;
        /// <summary>
        /// Initializes a new instance of the MovieViewModel class.
        /// </summary>
        public MovieViewModel(INavigationService navService, ApiClient apiClient)
        {
            if (IsInDesignMode)
            {
                SelectedMovie = new DtoBaseItem
                                    {
                                        Id = new Guid("6536a66e10417d69105bae71d41a6e6f"),
                                        Name = "Jurassic Park",
                                        SortName = "Jurassic Park",
                                        Overview = "Lots of dinosaurs eating people!"
                                    };
            }
            else
            {
                NavService = navService;
                ApiClient = apiClient;
                WireCommands();
                WireMessages();
            }
        }

        private void WireMessages()
        {
            Messenger.Default.Register<NotificationMessage>(this, m =>
            {
                
            });
        }

        private void WireCommands()
        {
            MoviePageLoaded = new RelayCommand(async () =>
            {
                if (SelectedMovie.ProviderIds != null)
                    ImdbId = SelectedMovie.ProviderIds["Imdb"];
                if (SelectedMovie.RunTimeTicks.HasValue)
                    RunTime = TimeSpan.FromTicks(SelectedMovie.RunTimeTicks.Value).ToString();
                if (SelectedMovie != null && NavService.IsNetworkAvailable)
                {
                    ProgressIsVisible = true;
                    ProgressText = "Getting cast + crew...";

                    bool dataLoaded = await GetMovieDetails();

                    ProgressIsVisible = false;
                    ProgressText = string.Empty;
                }
            });

            PlayMovieCommand = new RelayCommand(() =>
                                                    {
                                                        var formats = new List<VideoOutputFormats>
                                                                          {
                                                                              VideoOutputFormats.Wmv,
                                                                              VideoOutputFormats.Asf,
                                                                              VideoOutputFormats.Ts
                                                                          };
                                                        var url = ApiClient.GetVideoStreamUrl(SelectedMovie.Id, formats, maxHeight: 480, maxWidth: 800, quality:StreamingQuality.Higher);
                                                        var mediaPlayerLauncher = new MediaPlayerLauncher
                                                                                      {
                                                                                          Orientation = MediaPlayerOrientation.Landscape,
                                                                                          Media = new Uri(url, UriKind.Absolute)
                                                                                      };
                                                        mediaPlayerLauncher.Show();
                                                    });
            NavigateTopage = new RelayCommand<DtoBaseItem>(NavService.NavigateToPage);
        }

        private async Task<bool> GetMovieDetails()
        {
            var result = false;

            try
            {
                var item = await ApiClient.GetItemAsync(SelectedMovie.Id, App.Settings.LoggedInUser.Id);
                CastAndCrew = Utils.GroupCastAndCrew(item.People);
                result = true;
            }
            catch
            {
                App.ShowMessage("", "Error getting extra information");
                result = false;
            }

            return result;
        }



        // UI properties
        public string ProgressText { get; set; }
        public bool ProgressIsVisible { get; set; }

        public DtoBaseItem SelectedMovie { get; set; }
        public List<Group<BaseItemPerson>> CastAndCrew { get; set; }
        public string ImdbId { get; set; }
        public string RunTime { get; set; }

        public RelayCommand<DtoBaseItem> NavigateTopage { get; set; }
        public RelayCommand MoviePageLoaded { get; set; }
        public RelayCommand PlayMovieCommand { get; set; }
    }
}