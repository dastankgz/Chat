using System.Collections.Generic;

namespace Chat.Abstract.Model
{
    public class CreateChatModel
    {
        public List<string> Users { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            var str = "[" + string.Join(", ", Users) + "]";
            return $"[Users: {str}; Name: {Name}; Type: {Type}]";
        }
    }
}