using backend.Models;
using backend.Services;
using Firebase.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                firebaseDatabase.Child($"Messages/{resultValidate.user.Id}").AsObservable<object>().Subscribe( data =>
                {
                    if (DateTime.UtcNow < endTime)
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
                                        if (!string.IsNullOrEmpty(last.Text) && !string.IsNullOrEmpty(last.SenderId) && last.SenderId != resultValidate.user.Id)
                                        {
                                            if (webSocket.State == WebSocketState.Open)
                                            {
                                                var serverMsg = Encoding.UTF8.GetBytes("Update");
                                                webSocket.SendAsync(serverMsg, WebSocketMessageType.Text, true, CancellationToken.None);
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
                        webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
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
