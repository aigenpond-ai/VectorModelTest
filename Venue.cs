using MongoDB.Driver.GeoJsonObjectModel;


namespace VectorModelTest.Models
{
    [Serializable]
    public class Venue
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public string VenueUrl { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        public GeoJsonPolygon<GeoJson2DGeographicCoordinates> Polygon { get; set; }
    }
}

