using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Text.Json.Serialization;

namespace StreamArtist.Domain
{
    public class FireAnimator : Animator
    {
        public override List<RendererRequest> Animate(List<ChatMessage> chats)
        {
            var list = new List<RendererRequest>();
            foreach(ChatMessage chat in chats) {
                int MultiplierFactor = (int)Math.Ceiling(chat.USDAmount / 5);
                var renderer = new FireRendererRequest {
                    Amount = chat.Amount,
                    DisplayAmount = chat.DisplayAmount,
                    Size = /*chat.Amount **/ 1,
                    TTL = 5, // TODO: Restore: Math.Min(5,(int) chat.USDAmount),
                    Num = Math.Min(10,(int) chat.USDAmount/5),
                    Name = chat.AuthorName
                };
                list.Add(renderer);
            }

            return list;
        }
    }

    public class FireRendererRequest : RendererRequest
    {
        public double Amount { get; set; }

        public string DisplayAmount {get;set;}

        public int Size { get; set; }

        public int Num {get;set;}

        public int TTL { get; set; }

        public string Name { get; set; }
        public string Message { get; set; }
    }

}