namespace Chat.Abstract.Model
{
    public class RemoveFromChatModel
    {
        public string UserId { get; set; }
        public string UserIdToRemove { get; set; }
        public string ChatId { get; set; }
    }
}