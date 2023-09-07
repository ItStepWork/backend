using backend.Models;
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
            using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync("client");
            var serverMsg = Encoding.UTF8.GetBytes(message);
            
            object endTime = Echo(webSocket);

            bool isSend = true;
            var startTime = DateTime.UtcNow.AddSeconds(5);
            SubscribeFirebase(path, async data =>
            {
                if (DateTime.UtcNow > (DateTime)endTime && webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
                else if (DateTime.UtcNow > startTime && isSend)
                {
                    isSend = false;
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    await Task.Delay(1000);
                    isSend = true;
                }
            });
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
        }
        public static async Task SubscribeToMessagesAsync(HttpContext httpContext, string path, string userId)
        {
            using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync("client");
            var startTime = DateTime.UtcNow;
            var endTime = Echo(webSocket);
            List<string> messages = new();
            DateTime start = DateTime.UtcNow;
            SubscribeFirebase(path + "/" + userId, async data => {
                if (DateTime.UtcNow < (DateTime)endTime)
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
                                            if (webSocket.State == WebSocketState.Open)
                                            {
                                                await webSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
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
                else if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            });
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
        }
        public static async Task SubscribeToFriendRequestAsync(HttpContext httpContext, string path, string userId)
        {
            using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync("client");
            var startTime = DateTime.UtcNow;
            var endTime = Echo(webSocket);
            List<string> friends = new();
            DateTime start = DateTime.UtcNow.AddSeconds(5);

            SubscribeFirebase(path + "/" + userId, async data =>
            {
                if (DateTime.UtcNow < (DateTime)endTime)
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
                                            if (webSocket.State == WebSocketState.Open)
                                            {
                                                await webSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
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
                else if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            });

            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
        }
        public static void SubscribeFirebase(string path, Action<FirebaseEvent<object>> action)
        {
            firebaseDatabase.Child(path).AsObservable<object>().Subscribe(data => {
                action(data);
            });
        }
        public static object Echo(WebSocket webSocket)
        {
            object endTime = DateTime.UtcNow.AddMinutes(2);
            _ = Task.Run(async () =>
            {
                var buffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (webSocket.State == WebSocketState.Open)
                    {
                        endTime = DateTime.UtcNow.AddMinutes(2);
                    }
                }
            });
            return endTime;
        }
    }
}
