using System.ComponentModel.DataAnnotations;

namespace Chat.Abstract.Model
{
    public class UsersJoinedModel
    {
        [Required]
        public string StreamId { get; set; }

        [Required]
        public StreamUserModel[] Users { get; set; }

        public int MembersCount { get; set; }
        public int AllMembersCount { get; set; }

        public override string ToString()
        {
            return $"[StreamId: {StreamId}; User: {Users.Length}]";
        }
    }
}