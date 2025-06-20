using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Text.Json.Serialization;

namespace StreamArtist.Domain
{
    public enum FlagId
    {
        SimulstreamEnabled,
        ArEffectsEnabled,
        OverlayEffectsEnabled
    }

    public class Flag
    {
        public FlagId Id { get; set; }
        public string StringId { get {
            return Id.ToString();
        }}
        public bool Value { get; set; }
    }
}