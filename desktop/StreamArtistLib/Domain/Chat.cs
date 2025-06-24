public class ChatMessage
    {
        public string AuthorName { get; set; }
        public string Message { get; set; }
        public bool IsSuperChat { get; set; }
        public bool IsChannelMembership { get; set; }
        public bool IsSuperSticker { get; set; }
        public double Amount { get; set; }
        public double USDAmount { get; set; }
        public string DisplayAmount { get; set; }
    }

    public class ChatState
    {
        public string LastPageToken { get; set; }
        public string LiveChatId { get; set; }
    }