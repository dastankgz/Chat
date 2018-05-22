using System;

namespace Chat.Abstract.Model
{
    public class Message
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string ChatId { get; set; }
        public DateTime CreateDate { get; set; }
        public string PostId { get; set; }
        public Content Content { get; set; }

        public override string ToString()
        {
            return
                $"[Id: {Id}; SenderId: {SenderId}; ChatId: {ChatId}; CreateDate: {CreateDate}; PostId: {PostId}; Desc: {Content?.Description}]";
        }

        public string ToString(bool withContent)
        {
            if (withContent)
                return
                    $"[Id: {Id}; SenderId: {SenderId}; ChatId: {ChatId}; CreateDate: {CreateDate}; PostId: {PostId} Content: {Content}]";

            return ToString();
        }
    }
}