using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Principal;
using System.Text;
using System;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Clients;
using Twilio.Http;
using Twilio.TwiML.Messaging;
using Twilio.TwiML;
using System.Net.WebSockets;
using OpenAI_API;
using OpenAI_API.Completions;

namespace WhatsApp_ChatGPT_Itegration.Controllers
{
    public class MyTwilioController :TwilioController
    {
        private readonly IConfiguration _config;
        public MyTwilioController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        public async Task<TwiMLResult> Index(SmsRequest request)
        {
            var twilio = new TwilioRestClient(_config["Twilio:AccountSid"], _config["Twilio:AuthToken"]);
            var response = new MessagingResponse();

            // Get the user's message
            var userMessage = request.Body;

            // Process the user's message using ChatGPT
            var chatGptResponse = await ProcessUserMessage(userMessage);

            // Send the response back to the user
            var message = new Message(chatGptResponse);
            response.Append(message);

            return TwiML(response);
        }
        private async Task<string> ProcessUserMessage(string userMessage)
        {
            var openAI =new OpenAIAPI(_config["OpenAI:Key"]);
            CompletionRequest completionRequest = new CompletionRequest();
            completionRequest.Prompt = userMessage;
            completionRequest.Model = OpenAI_API.Models.Model.DavinciText;
            var completions = await openAI.Completions.CreateCompletionAsync(completionRequest);
            List<string> response = new();
            foreach (var completer in completions.Completions) 
            {
                response.Add(completer.Text);
            }
            return string.Join("\n", response);  
        }
    }
}
