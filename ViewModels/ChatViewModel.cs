using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Azure.Data.Tables;
using ChatApp.Models;
using ChatApp.Services;
using Xamarin.KotlinX.Coroutines.Channels;

namespace ChatApp.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Message> Messages { get; private set; } = new ObservableCollection<Message>();

        public async Task Persist()
        {
            TableServiceClient tableServiceClient = new TableServiceClient(Constants.ConnectionString);
            TableClient tableClient = tableServiceClient.GetTableClient(
                tableName: "messages"
            );
            await tableClient.DeleteAsync();
            await tableClient.CreateAsync();

            foreach (Message message in this.Messages)
            {
                tableClient.AddEntity<Message>(message);
            }
        }

        public async Task Reset()
        {
            this.Messages.Clear();

            TableServiceClient tableServiceClient = new TableServiceClient(Constants.ConnectionString);
            TableClient tableClient = tableServiceClient.GetTableClient(
                tableName: "messages"
            );
            await tableClient.DeleteAsync();

            await Initialize();
        }

        public async Task Initialize()
        {
            this.Messages.Clear();

            TableServiceClient tableServiceClient = new TableServiceClient(Constants.ConnectionString);
            TableClient tableClient = tableServiceClient.GetTableClient(
                tableName: "messages"
            );
            await tableClient.CreateIfNotExistsAsync();

            var messages = tableClient.Query<Message>();

            foreach (Message message in messages)
            {
                this.Messages.Add(message);
            }

            if (this.Messages.Count == 0)
            {
                this.Messages.Add(new Message
                {
                    PartitionKey = "Messages",
                    RowKey = Guid.NewGuid().ToString(),
                    Type = MessageType.System,
                    Text = Constants.InitialPrompt
                });
            }
            if (this.Messages.Last().Type != MessageType.Assistant)
            {
                ChatService chatService = new ChatService();
                Message response = await chatService.GetChatResponse(this.Messages.ToList());

                this.Messages.Add(response);
            }
        }

        public async void AddMessage(string text)
        {
            this.Messages.Add(new Message
            {
                Type = MessageType.User,
                Text = text,
                PartitionKey = "Messages",
                RowKey = Guid.NewGuid().ToString()
            });

            ChatService chatService = new ChatService();
            Message response = await chatService.GetChatResponse(this.Messages.ToList());

            this.Messages.Add(response);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

