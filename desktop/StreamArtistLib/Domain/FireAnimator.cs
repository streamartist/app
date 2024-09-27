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
            // What do we want the animator to render?
            // Fire size, amount and name.
            var list = new List<RendererRequest>();
            foreach(ChatMessage chat in chats) {
                var renderer = new FireRendererRequest {
                    Amount = chat.Amount,
                    Size = /*chat.Amount **/ 2,
                    Name = chat.AuthorName

                };
                list.Add(renderer);
            }

            return list;
        }
    }

    public class FireRendererRequest : RendererRequest
    {
        public string Amount { get; set; }

        public int Size { get; set; }

        public string Name { get; set; }
    }

}