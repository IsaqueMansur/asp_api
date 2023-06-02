namespace WEB_API_ASP.Resources
{
    public class TextAnnotationGoogleVision
    {
        public BoundingPoly boundingPoly { get; set; }
        public string description { get; set; }
    }

    public class BoundingPoly
    {
        public Vertex[] vertices { get; set; }
    }

    public class Vertex
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
