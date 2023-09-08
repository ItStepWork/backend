﻿using backend.Models;
using Firebase.Database;
using Firebase.Database.Streaming;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace backend.Services
{
    public static class SubscriptionService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task SubscribeUpdatesAsync(HttpContext httpContext, string path, string message)
        {
            var serverMsg = Encoding.UTF8.GetBytes(message);

            Subscription subscription = new();
            subscription.EndTime = DateTime.UtcNow.AddMinutes(2);
            subscription.WebSocket = await httpContext.WebSockets.AcceptWebSocketAsync("client");
            Echo(subscription);

            bool isSend = true;
            var startTime = DateTime.UtcNow.AddSeconds(5);
            var result = SubscribeFirebase(path, async data =>
            {
                try
                {
                    bool check = false;
                    lock (subscription)
                    {
                        if (DateTime.UtcNow < subscription.EndTime) check = true;
                    }
                    if (check)
                    {
                        if (DateTime.UtcNow > startTime && isSend)
                        {
                            isSend = false;
                            if (subscription.WebSocket.State == WebSocketState.Open)
                            {
                                await subscription.WebSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            await Task.Delay(1000);
                            isSend = true;
                        }
                    }
                    else if (subscription.WebSocket.State == WebSocketState.Open)
                    {
                        await subscription.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception SubscribeUpdatesAsync");
                    Console.WriteLine(ex.ToString());
                }
            });
            Console.WriteLine("Subscribe\t id: " + subscription.WebSocket.GetHashCode());
            while (subscription.WebSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
            if (result != null)result.Dispose();
            Console.WriteLine("Unsubscribe\t id: " + subscription.WebSocket.GetHashCode());
        }
        public static async Task SubscribeToMessagesAsync(HttpContext httpContext, string path, string userId)
        {
            var startTime = DateTime.UtcNow;
            Subscription subscription = new();
            subscription.EndTime = DateTime.UtcNow.AddMinutes(2);
            subscription.WebSocket = await httpContext.WebSockets.AcceptWebSocketAsync("client");
            Echo(subscription);
            List<string> messages = new();
            DateTime start = DateTime.UtcNow;
            var result = SubscribeFirebase(path + "/" + userId, async data => {
                try
                {
                    bool check = false;
                    lock (subscription)
                    {
                        if (DateTime.UtcNow < subscription.EndTime) check = true;
                    }
                    if (check)
                    {
                        if (DateTime.UtcNow > start.AddSeconds(5))
                        {
                            string json = JsonConvert.SerializeObject(data.Object);
                            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, Message>>(json);
                            if (dictionary != null)
                            {
                                var last = dictionary.LastOrDefault().Value;
                                if (last.Id != null)
                                {
                                    if (!messages.Contains(last.Id))
                                    {
                                        messages.Add(last.Id);
                                        if (!string.IsNullOrEmpty(last.Text) && !string.IsNullOrEmpty(last.SenderId) && last.SenderId != userId)
                                        {
                                            UserBase? user = await UserService.GetUserAsync(last.SenderId);
                                            if (user != null)
                                            {
                                                SubscriptionResponse response = new SubscriptionResponse();
                                                response.Title = user.FirstName + " " + user.LastName;
                                                response.AvatarUrl = user.AvatarUrl;
                                                response.Text = last.Text;
                                                var serverMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                                if (subscription.WebSocket.State == WebSocketState.Open)
                                                {
                                                    await subscription.WebSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(data.Object);
                            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, Message>>(json);
                            if (dictionary != null)
                            {
                                var last = dictionary.LastOrDefault().Value;
                                if (last.Id != null)
                                {
                                    if (!messages.Contains(last.Id))
                                    {
                                        messages.Add(last.Id);
                                    }
                                }
                            }
                        }
                    }
                    else if (subscription.WebSocket.State == WebSocketState.Open)
                    {
                        await subscription.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception SubscribeToMessagesAsync");
                    Console.WriteLine(ex.ToString());
                }
                
            });
            Console.WriteLine("Subscribe\t id: " + subscription.WebSocket.GetHashCode());
            while (subscription.WebSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
            if (result != null) result.Dispose();
            Console.WriteLine("Unsubscribe\t id: " + subscription.WebSocket.GetHashCode());
        }
        public static async Task SubscribeToFriendRequestAsync(HttpContext httpContext, string path, string userId)
        {
            var startTime = DateTime.UtcNow;
            Subscription subscription = new();
            subscription.EndTime = DateTime.UtcNow.AddMinutes(2);
            subscription.WebSocket = await httpContext.WebSockets.AcceptWebSocketAsync("client");
            Echo(subscription);
            List<string> friends = new();
            DateTime start = DateTime.UtcNow.AddSeconds(5);

            var result = SubscribeFirebase(path + "/" + userId, async data =>
            {
                try
                {
                    bool check = false;
                    lock (subscription)
                    {
                        if (DateTime.UtcNow < subscription.EndTime) check = true;
                    }
                    if (check)
                    {
                        if (DateTime.UtcNow > start)
                        {
                            string json = JsonConvert.SerializeObject(data.Object);
                            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, FriendRequest>>(json);
                            if (dictionary != null)
                            {
                                foreach (var friend in dictionary)
                                {
                                    if (!friends.Contains(friend.Key))
                                    {
                                        friends.Add(friend.Key);
                                        if (!string.IsNullOrEmpty(friend.Value.SenderId) && friend.Value.SenderId != userId)
                                        {
                                            UserBase? user = await UserService.GetUserAsync(friend.Value.SenderId);
                                            if (user != null)
                                            {
                                                SubscriptionResponse response = new SubscriptionResponse();
                                                response.Title = "Запрос в друзья";
                                                response.AvatarUrl = user.AvatarUrl;
                                                response.Text = user.FirstName + " " + user.LastName;
                                                var serverMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                                if (subscription.WebSocket.State == WebSocketState.Open)
                                                {
                                                    await subscription.WebSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(data.Object);
                            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, FriendRequest>>(json);
                            if (dictionary != null)
                            {
                                foreach (var item in dictionary)
                                {
                                    friends.Add(item.Key);
                                }
                            }
                        }
                    }
                    else if (subscription.WebSocket.State == WebSocketState.Open)
                    {
                        await subscription.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception SubscribeToFriendRequestAsync");
                    Console.WriteLine(ex.ToString());
                }
            });

            Console.WriteLine("Subscribe\t id: " + subscription.WebSocket.GetHashCode());
            while (subscription.WebSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
            if (result != null) result.Dispose();
            Console.WriteLine("Unsubscribe\t id: " + subscription.WebSocket.GetHashCode());
        }
        public static IDisposable? SubscribeFirebase(string path, Action<FirebaseEvent<object>> action)
        {
            try
            {
                var result = firebaseDatabase.Child(path).AsObservable<object>().Subscribe(data => {
                    action(data);
                });
                return result;
            }
            catch (Exception ex) {
                Console.WriteLine("Exception SubscribeFirebase");
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
        public static void Echo(Subscription subscription)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var buffer = new byte[1024];
                    while (true)
                    {
                        var receiveResult = await subscription.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        lock (subscription)
                        {
                            subscription.EndTime = DateTime.UtcNow.AddMinutes(2);
                            if (subscription.WebSocket.State != WebSocketState.Open) break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Exception Echo");
                    Console.WriteLine(ex.ToString());
                }
            });
        }
    }
}
