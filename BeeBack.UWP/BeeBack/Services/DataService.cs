﻿using BeeBack.Model;
using BeeBack.Services.Interfaces;
using LeSoir.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BeeBack.Services
{
    public class DataService : IDataService
    {
        private static string _username;
        private static string _password;

        public static readonly string UrlBase = "http://beeback.azurewebsites.net/";
        public static readonly string UrlActivities = "api/activities";
        public static readonly string UrlSubscriptions = "api/activities/subscribed";
        public static readonly string UrlOwnedActivities = "api/activities/owned";
        public static readonly string UrlGetActivity = "api/activities/getactivity";
        public static readonly string UrlActivitySubscribers = "api/activities/subscribed";

        public static readonly string UrlUser = "api/users";
        public static readonly string UrlLogin = "api/login";

        public void Initialize(string username, string password)
        {
            _username = username;
            _password = password;

            if (string.IsNullOrWhiteSpace(_password))
                throw new UnauthorizedAccessException("Password is empty.");

            if (string.IsNullOrWhiteSpace(_username))
                throw new UnauthorizedAccessException("Username is empty.");
        }

        public async Task<bool> CheckCredentials()
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<bool>(UrlBase + UrlLogin, new TimeSpan(0), false, false, request);
                //var content = await request.GetStringAsync(UrlBase + UrlLogin);
                //return JsonConvert.DeserializeObject<bool>(content);
            }
        }

        /// <summary>
        /// Get the list of activities to which the current user is subscribed
        /// </summary>
        /// <returns></returns>
        public async Task<List<Activity>> GetSubscribedActivities()
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<List<Activity>>(UrlBase + UrlSubscriptions, new TimeSpan(0), false, false, request);

                //var content = await request.GetStringAsync(UrlBase + UrlSubscriptions);
                //return JsonConvert.DeserializeObject<List<Activity>>(content);
            }
        }

        /// <summary>
        /// Get the list of activities created by the user.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Activity>> GetUserActivities()
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<List<Activity>>(UrlBase + UrlOwnedActivities, new TimeSpan(0), false, false, request);

                //var content = await request.GetStringAsync(UrlBase + UrlActivities);
                //return JsonConvert.DeserializeObject<List<Activity>>(content);
            }
        }

        /// <summary>
        /// Get the list of all public activities 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Activity>> GetAllPublicActivities()
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<List<Activity>>(UrlBase + UrlActivities, new TimeSpan(0), false, false, request);

                //var content = await request.GetStringAsync(UrlBase + UrlActivities);
                //return JsonConvert.DeserializeObject<List<Activity>>(content);
            }
        }
        public async Task<User> GetUser(Guid userID)
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<User>($"{UrlBase}{UrlUser}/{userID}", new TimeSpan(0, 5, 0), false, false, request);
            }
        }
        public async Task<List<User>> GetAllUsers()
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<List<User>>($"{UrlBase}{UrlUser}", new TimeSpan(0), false, false, request);
            }
        }
        public async Task<User> GetCurrentUser()
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<User>(UrlBase + UrlUser + "/me", new TimeSpan(0), false, false, request);
            }
        }

        public Task<DataItem> GetData()
        {
            // Use this to connect to the actual data service

            // Simulate by returning a DataItem
            var item = new DataItem("Welcome to MVVM Light");
            return Task.FromResult(item);
        }

        private HttpClient InitRequest()
        {
            HttpClient client = new HttpClient();

            if (string.IsNullOrWhiteSpace(_password) || string.IsNullOrWhiteSpace(_username))
            {
                throw new UnauthorizedAccessException("Credentials are missing. Initialize the service first.");
            }

            var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }

        public async Task<Activity> GetActivity(Guid iD)
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<Activity>($"{UrlBase}{UrlGetActivity}/{iD}", new TimeSpan(0), false, true, request);
            }
        }

        public async Task<List<UserActivity>> GetActivitySubscribers(Guid iD)
        {
            using (var request = InitRequest())
            {
                return await CachedFile.TryLoad<List<UserActivity>>($"{UrlBase}{UrlActivitySubscribers}/{iD}", new TimeSpan(0), false, true, request);
            }
        }
    }
}