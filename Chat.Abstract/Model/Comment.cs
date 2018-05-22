using System;
using System.ComponentModel.DataAnnotations;

namespace Chat.Abstract.Model
{
    public class Comment
    {
        [Required]
        public string StreamId { get; set; }

        [Required]
        public StreamUserModel User { get; set; }

        [Required]
        public string Body { get; set; }


        public DateTime CreateDate { get; set; }

        public override string ToString()
        {
            return $"[StreamId: {StreamId}; User: {User.UserId}; Body: {Body}]";
        }
    }
}