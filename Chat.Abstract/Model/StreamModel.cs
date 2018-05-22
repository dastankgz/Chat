using System.ComponentModel.DataAnnotations;

namespace Chat.Abstract.Model
{
    public class StreamModel
    {
        [Required]
        public string StreamId { get; set; }

        [Required]
        public StreamUserModel User { get; set; }

        public int MembersCount { get; set; }

        public override string ToString()
        {
            return $"[StreamId: {StreamId}; User: {User.UserId}]";
        }
    }
}