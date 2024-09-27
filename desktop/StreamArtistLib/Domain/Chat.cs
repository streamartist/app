public class ChatMessage
    {
        public string AuthorName { get; set; }
        public string Message { get; set; }
        public bool IsSuperChat { get; set; }
        public string Amount { get; set; }
    }

    public class ChatState
    {
        public string LastPageToken { get; set; }
        public string LiveChatId { get; set; }
    }