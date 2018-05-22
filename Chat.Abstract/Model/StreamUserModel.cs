using System.ComponentModel.DataAnnotations;

namespace Chat.Abstract.Model
{
    public class StreamUserModel
    {
        [Required]
        public string UserId { get; set; }
        public string Avatar { get; set; }
        public string NickName { get; set; }

        public override string ToString()
        {
            return $"[UserId: {UserId}; Avatar: {Avatar}; NickName: {NickName}]";
        }
    }
}