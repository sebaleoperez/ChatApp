using System;
using Azure;
using Azure.Data.Tables;

namespace ChatApp.Models
{
	public class Message: ITableEntity
	{
        public MessageType Type { get; set; }
        public string Text { get; set; }
        public string PartitionKey { get; set; } 
        public string RowKey { get; set; } 
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
    }

    public enum MessageType { User, Assistant, System }
}

