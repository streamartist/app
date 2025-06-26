using System.Text.Json;
//using Microsoft.Maui.Devices;
//using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StreamArtist.Repositories
{
    public class LocalChatRepository
    {
        private static LocalChatRepository _instance;
        private static readonly object _lock = new object();

        private Queue<ChatMessage> ChatQueue;

        private LocalChatRepository()
        {
            ChatQueue = new Queue<ChatMessage>();
        }

        public static LocalChatRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LocalChatRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        public void AddChat(ChatMessage Chat)
        {
            ChatQueue.Enqueue(Chat);
        }

        public ChatMessage GetOldestChat()
        {
            return ChatQueue.Count > 0 ? ChatQueue.Peek() : null;
        }

        public ChatMessage RemoveOldestChat()
        {
            return ChatQueue.Count > 0 ? ChatQueue.Dequeue() : null;
        }

        public int GetChatCount()
        {
            return ChatQueue.Count;
        }

        public List<ChatMessage> GetAndRemoveAllChats()
        {
            List<ChatMessage> AllChats = new List<ChatMessage>(ChatQueue);
            ChatQueue.Clear();
            return AllChats;
        }

        // Add other methods and properties here
    }
}