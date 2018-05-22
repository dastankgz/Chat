namespace Chat.Abstract.Model
{
    public class JoinToChatModel
    {
        public string UserId { get; set; }
        public string UserIdToJoin { get; set; }
        public string ChatId { get; set; }

        public override string ToString()
        {
            return $"[UserId: {UserId}; UserIdToJoin: {UserIdToJoin}; ChatId: {ChatId}]";
        }
    }
}