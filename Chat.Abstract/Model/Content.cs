namespace Chat.Abstract.Model
{
    public class Content
    {
        public string ContentType { get; set; }
        public string Description { get; set; }
        public string ResourceId { get; set; }
        public string MediaHost { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Duration { get; set; }
        public Screenshot Screenshot { get; set; }
        public Thumbnail Thumbnail { get; set; }

        public override string ToString()
        {
            return
                $"[ContentType: {ContentType}; Description: {Description}; ResourceId: {ResourceId}; MediaHost: {MediaHost}; Width: {Width}; Height: {Height}; Duration: {Duration}; Screenshot: {Screenshot}; Thumbnail: {Thumbnail}]";
        }
    }

    public class Screenshot
    {
        public string ResourceId { get; set; }
        public string MediaHost { get; set; }
        public Thumbnail Thumbnail { get; set; }

        public override string ToString()
        {
            return $"[ResourceId: {ResourceId}; MediaHost: {MediaHost}; Thumbnail: {Thumbnail}]";
        }
    }

    public class Thumbnail
    {
        public string ResourceId { get; set; }
        public string MediaHost { get; set; }

        public override string ToString()
        {
            return $"[ResourceId: {ResourceId}; MediaHost: {MediaHost}]";
        }
    }
}