using System;
using System.Runtime.Intrinsics.Arm;
using Azure;
using Azure.AI.OpenAI;
using ChatApp.Models;

namespace ChatApp.Services
{
    public class ChatService
	{
        string apiBase = "https://azureoaiprueba.openai.azure.com/";
        string apiKey = "568641b4fc3f4c69b396907da7be279a";
        string deploymentId = "prueba";

        public async Task<Message> GetChatResponse(List<Message> history)
		{
            OpenAIClient client = new OpenAIClient(new Uri(apiBase), new AzureKeyCredential(apiKey!));

            IList<ChatMessage> chatMessages = new List<ChatMessage>();
            foreach(Message message in history)
            {
                if (message.Type == MessageType.System) chatMessages.Add(new ChatMessage(ChatRole.System, message.Text));
                else if (message.Type == MessageType.Assistant) chatMessages.Add(new ChatMessage(ChatRole.Assistant, message.Text));
                else if (message.Type == MessageType.User) chatMessages.Add(new ChatMessage(ChatRole.User, message.Text));
            }

            Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
                deploymentId,
                new ChatCompletionsOptions(chatMessages));

            ChatCompletions completions = responseWithoutStream.Value;

            return new Message(){
                Type = MessageType.Assistant,
                Text = completions.Choices[0].Message.Content,
                PartitionKey = "Messages",
                RowKey = Guid.NewGuid().ToString()
            };
        }
	}
}

