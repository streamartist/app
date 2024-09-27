using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using StreamArtist.Domain;


public abstract class Animator
{
    public abstract List<RendererRequest> Animate(List<ChatMessage> chats) ;
}


public abstract class RendererRequest
{
    public string Name { get; set; }
}


