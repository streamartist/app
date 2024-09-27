using System.Collections.Generic;
using Microsoft.VisualBasic;
using StreamArtist.Domain;
using StreamArtist.Services;
using System.Threading.Tasks;

namespace StreamArtist.Services{

public class EffectsService
{
    FireAnimator FireAnimator = new FireAnimator();

    public EffectsService()
    {
        
    }

    public async Task<List<RendererRequest>> ProcessChatAnimations()
    {
        var chatService = new YouTubeChatService();
        var channelId = await chatService.GetConnectedChannelId();
        var streams = await chatService.GetLiveStreams(channelId);
        if (streams.Count > 0) {
            var chats = await chatService.GetNewChatMessages(streams[0].VideoId);
            return FireAnimator.Animate(chats);
        }

        // TODO: Fix
        return FireAnimator.Animate(null);
        
    }
}
}