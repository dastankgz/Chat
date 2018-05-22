namespace Chat.Abstract.Model
{
    public class SimpleMessage
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ChatId { get; set; }

        public override string ToString()
        {
            return $"[Id: {Id}; UserId: {UserId}; ChatId: {ChatId}]";
        }
    }
}