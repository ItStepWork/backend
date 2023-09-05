using backend.Models;
using backend.Services;
using Firebase.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscriptionController : Controller
    {
        [Authorize]
        [HttpGet("SubscribeToMessages")]
        public async Task SubscribeToMessages()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null)
            {
                HttpContext.Response.StatusCode = 400;
            }
            else
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("client");
                var startTime = DateTime.UtcNow;
                var endTime = DateTime.UtcNow.AddMinutes(2);
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
                List<string> messages = new();
                DateTime start = DateTime.UtcNow;
                FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
                firebaseDatabase.Child($"Messages/{resultValidate.user.Id}").AsObservable<object>().Subscribe(async data =>
                    {
                        if (DateTime.UtcNow < endTime)
                        {
                            if(DateTime.UtcNow > start.AddSeconds(5))
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
                                            if (!string.IsNullOrEmpty(last.Text) && !string.IsNullOrEmpty(last.SenderId) && last.SenderId != resultValidate.user.Id)
                                            {
                                                UserBase? user = await UserService.GetUserAsync(last.SenderId);
                                                if(user != null)
                                                {
                                                    SubscriptionResponse response = new SubscriptionResponse();
                                                    response.Title = user.FirstName + " " + user.LastName;
                                                    response.AvatarUrl = user.AvatarUrl;
                                                    response.Text = last.Text;
                                                    var serverMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                                                    if(webSocket.State == WebSocketState.Open)
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
        }
        [Authorize]
        [HttpGet("SubscribeToMessagesUpdates")]
        public async Task SubscribeToMessagesUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null)
            {
                HttpContext.Response.StatusCode = 400;
            }
            else
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("client");
                var endTime = DateTime.UtcNow.AddMinutes(2);
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
                bool isSend = true;
                var startTime = DateTime.UtcNow.AddSeconds(5);
                FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
                firebaseDatabase.Child($"Messages/{resultValidate.user.Id}").AsObservable<object>().Subscribe( async data =>
                {
                    if (DateTime.UtcNow > endTime && webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    }
                    else if (DateTime.UtcNow > startTime && isSend)
                    {
                        isSend = false;
                        if (webSocket.State == WebSocketState.Open)
                        {
                            var serverMsg = Encoding.UTF8.GetBytes("Update messages");
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
        }
        [Authorize]
        [HttpGet("SubscribeToGroupsUpdates")]
        public async Task SubscribeToGroupsUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null)
            {
                HttpContext.Response.StatusCode = 400;
            }
            else
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("client");
                var endTime = DateTime.UtcNow.AddMinutes(2);
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
                bool isSend = true;
                var startTime = DateTime.UtcNow.AddSeconds(5);
                FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
                firebaseDatabase.Child($"Groups").AsObservable<object>().Subscribe(async data =>
                {
                    if(DateTime.UtcNow > endTime && webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    }
                    else if(DateTime.UtcNow > startTime && isSend)
                    {
                        isSend = false;
                        if (webSocket.State == WebSocketState.Open)
                        {
                            var serverMsg = Encoding.UTF8.GetBytes("Update groups");
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
        }
        [Authorize]
        [HttpGet("SubscribeToGroupUpdates")]
        public async Task SubscribeToGroupUpdates(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null)
            {
                HttpContext.Response.StatusCode = 400;
            }
            else
            {
                Group? group = await GroupService.GetGroupAsync(id);
                if(group == null)
                {
                    HttpContext.Response.StatusCode = 400;
                }
                else
                {
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("client");
                    var endTime = DateTime.UtcNow.AddMinutes(2);
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
                    bool isSend = true;
                    var startTime = DateTime.UtcNow.AddSeconds(5);
                    FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
                    firebaseDatabase.Child($"Groups/{id}").AsObservable<object>().Subscribe(async data =>
                    {
                        if (DateTime.UtcNow > endTime && webSocket.State == WebSocketState.Open)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                        }
                        else if (DateTime.UtcNow > startTime && isSend)
                        {
                            isSend = false;
                            if (webSocket.State == WebSocketState.Open)
                            {
                                var serverMsg = Encoding.UTF8.GetBytes("Update group");
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
            }
        }
        [Authorize]
        [HttpGet("SubscribeToFriendRequest")]
        public async Task SubscribeToFriendRequest()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null)
            {
                HttpContext.Response.StatusCode = 400;
            }
            else
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("client");
                var startTime = DateTime.UtcNow;
                var endTime = DateTime.UtcNow.AddMinutes(2);
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
                List<string> friends = new();
                DateTime start = DateTime.UtcNow.AddSeconds(5);
                FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
                firebaseDatabase.Child($"Friends/{resultValidate.user.Id}").AsObservable<object>().Subscribe(async data =>
                {
                    if (DateTime.UtcNow < endTime)
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
                                        if (!string.IsNullOrEmpty(friend.Value.SenderId) && friend.Value.SenderId != resultValidate.user.Id)
                                        {
                                            UserBase? user = await UserService.GetUserAsync(friend.Value.SenderId);
                                            if(user != null)
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
        }
        [Authorize]
        [HttpGet("SubscribeToFriendsUpdates")]
        public async Task SubscribeToFriendsUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null)
            {
                HttpContext.Response.StatusCode = 400;
            }
            else
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync("client");
                var endTime = DateTime.UtcNow.AddMinutes(2);
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
                bool isSend = true;
                var startTime = DateTime.UtcNow.AddSeconds(5);
                FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
                firebaseDatabase.Child($"Friends/{resultValidate.user.Id}").AsObservable<object>().Subscribe(async data =>
                {
                    if (DateTime.UtcNow > endTime && webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    }
                    else if (DateTime.UtcNow > startTime && isSend)
                    {
                        isSend = false;
                        if (webSocket.State == WebSocketState.Open)
                        {
                            var serverMsg = Encoding.UTF8.GetBytes("Update friends");
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
        }
    }
}
