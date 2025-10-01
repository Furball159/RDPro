using System.Collections.Generic;
using ReactiveUI;
using Newtonsoft.Json;

namespace RDPro.Models
{
    public class ConnectionGroup : ReactiveObject
    {
        public string GroupId { get; set; } = System.Guid.NewGuid().ToString();

        private string _name = string.Empty;
        [JsonProperty]
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public List<string> ConnectionIds { get; set; } = new();
    }
}