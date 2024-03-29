﻿using BeeBack.Messages;
using BeeBack.Model;
using BeeBack.Pages;
using BeeBack.Services.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BeeBack.ViewModel
{
    public class ActivityViewModel : ViewModelBase
    {
        private IDataService _dataService;
        private Activity _activity;
        private readonly ICustomNavigationService _customNavigationService;
        private DispatcherTimer _timer;


        public bool _iSubscribed;
        public bool IsSubscribed
        {
            get { return _iSubscribed; }
            set
            {
                _iSubscribed = value;
                RaisePropertyChanged(() => IsSubscribed);
            }
        }

        public ICommand LoadedCommand { get; set; }
        public ICommand UserTappedCommand { get; set; }
        public ICommand SetSubscribtionCommand { get; set; }
        public ICommand SubscriberSelectionChangedCommand { get; set; }
        public ICommand UnLoadedCommand { get; set; }

        public ObservableCollection<User> Subscribers { get; set; }
        public Activity Activity
        {
            get { return _activity; }
            set
            {
                _activity = value;
                RaisePropertyChanged(() => Activity);
            }
        }

        public ActivityViewModel(IDataService dataService, ICustomNavigationService customNavigationService)
        {
            _dataService = dataService;
            _customNavigationService = customNavigationService;

            LoadedCommand = new RelayCommand(OnLoaded);
            UserTappedCommand = new RelayCommand<User>(OnUserTapped);
            SetSubscribtionCommand = new RelayCommand(OnSetSubscribtion);
            UnLoadedCommand = new RelayCommand(OnUnloaded);
            SubscriberSelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(OnSubscriberSelectionChanged);

            SimpleIoc.Default.GetInstance<UserViewModel>().User = null; //hack ?
            Subscribers = new ObservableCollection<User>();

            if (IsInDesignModeStatic)
                CreateDummyActivity();

        }

        private void OnUnloaded()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= RefreshActivity;

                Subscribers = new ObservableCollection<User>();
            }
        }

        private void OnSubscriberSelectionChanged(SelectionChangedEventArgs obj)
        {
            if (obj.AddedItems.Count > 0)
            {
                OnUserTapped(obj.AddedItems.First() as User);
            }
        }

        private void OnSetSubscribtion()
        {
            Activity.IsSubscribed = !Activity.IsSubscribed;
            IsSubscribed = Activity.IsSubscribed;
        }

        private void OnUserTapped(User user)
        {
            Messenger.Default.Send(new SetUserModelMessage { User = user });
            _customNavigationService.Navigate(typeof(UserPage));
        }

        private void CreateDummyActivity()
        {
            _activity.ID = Guid.NewGuid();
            _activity.Description = "description flkjds mlf jdsqmlkdslkfj smlk jflks jflkdsjf  fds jflksj flk jsflkds jlkf jsmlf jmls jfmlsq jfmlkds jflkds jflkds jfmlkds jfmlkds jf lkdsjfmlqs";
            _activity.Title = "Titre de l'activité";
            User u;
            for (int j = 0; j < 10; j++)
            {
                u = new User();
                u.Lastname = $"{j}Smith";
                u.Firstname = "John";
                u.PictureUrl = "John";
                if (j == 0)
                {
                    _activity.Owner = u;
                }
                _activity.Members.Add(u);
                Subscribers.Add(u);
            }
        }

        private async void OnLoaded()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += RefreshActivity;
            _timer.Start();

            await CheckMembers();

            Messenger.Default.Send(new ActivityMapCoordinateMessage { Latitude = Activity.Location.Latitude, Longitude = Activity.Location.Longitude });
        }

        private async void RefreshActivity(object sender, object e)
        {
            try
            {
                var activity = await _dataService.GetActivity(Activity.ID);

                if (activity != null && activity.DriverId.HasValue)
                {
                    Activity.DriverId = activity.DriverId;
                    await CheckMembers();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Can't catch activity : {ex.Message}");
            }
        }

        private async Task RefreshActivity()
        {
            try
            {
                var activity = await _dataService.GetActivity(Activity.ID);

                if (activity != null && activity.DriverId.HasValue)
                {
                    Activity.DriverId = activity.DriverId;
                    await CheckMembers();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Can't catch activity : {ex.Message}");
            }
        }

        private async Task CheckMembers()
        {
            try
            {
                if (!string.IsNullOrEmpty(Activity.Owner.Email))
                {
                    if (Activity.DriverId.HasValue)
                    {
                        Activity.Driver = await _dataService.GetUser(Activity.DriverId.Value);
                        RaisePropertyChanged(() => Activity.Driver);
                    }
                }

                Activity.Owner = await _dataService.GetUser(Guid.Parse(Activity.UserId));

                if (Subscribers == null || Subscribers.Count == 0)
                {
                    var useractivities = await _dataService.GetActivitySubscribers(Activity.ID);
                    foreach (var userActivity in useractivities)
                    {
                        if (userActivity.UserId != Activity.Owner.ID)
                        {
                            var user = await _dataService.GetUser(userActivity.UserId);
                            if (!Subscribers.Contains(user))
                                Subscribers.Add(user);
                        }
                    }
                }
            }
            catch
            {
                //PAS BIEN
            }

        }
    }
}
