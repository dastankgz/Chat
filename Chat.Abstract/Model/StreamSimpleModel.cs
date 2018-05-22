using System.ComponentModel.DataAnnotations;

namespace Chat.Abstract.Model
{
    public class StreamSimpleModel
    {
        [Required]
        public string StreamId { get; set; }

        public override string ToString()
        {
            return $"[StreamId: {StreamId}]";
        }
    }
}