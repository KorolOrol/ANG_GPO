using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGenerator.TextGenerator
{
    public interface ITextAIGenerator
    {
        public string ApiKey { set; }

        public string Endpoint { get; set; }

        public string Model { get; set; }

        public async Task<string> GenerateTextAsync(List<string> messages)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }
    }
}
