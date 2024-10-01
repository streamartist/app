using System.Collections.Generic;
using StreamArtist.Domain;
using StreamArtist.Services;
using System.Threading.Tasks;

namespace StreamArtist.Services
{
    public class FlagService
    {
        static Dictionary<FlagId, Flag> Flags = new Dictionary<FlagId, Flag>
        {
            { FlagId.SimulstreamEnabled, new Flag { Id = FlagId.SimulstreamEnabled, Value = true } }
        };

        public static List<Flag> GetFlags()
        {
            return new List<Flag>(Flags.Values);
        }

        public static Flag GetFlag(FlagId Id)
        {
            return Flags.TryGetValue(Id, out var Flag) ? Flag : null;
        }
    }
}