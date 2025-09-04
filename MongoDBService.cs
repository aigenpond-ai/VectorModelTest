using MongoDB.Driver;
using VectorModelTest.Models;

namespace VectorModelTest.Services
{
    public class MongoDBService
    {
        private MongoClient client;
        private IMongoDatabase db;
        private IMongoCollection<Venue> venueCollection;
        private IMongoCollection<VenueProduct> vpCollection;
        private IMongoCollection<ImageMatch> imCollection;


        public MongoDBService()
        {
         
            client = new MongoClient("Mongo Connections String");
            db = client.GetDatabase("Database Name");
      
            vpCollection = db.GetCollection<VenueProduct>("VenueProducts");
            imCollection = db.GetCollection<ImageMatch>("Matches");
            venueCollection = db.GetCollection<Venue>("Venues");

        }

        public List<VenueProduct> GetAllVenueProducts()
        {
            return vpCollection.AsQueryable<VenueProduct>().ToList<VenueProduct>();
        }

        public async void WriteMatch(List<Match> matches, string venueId)
        {
            List<ImageMatch> imageMatches = new List<ImageMatch>();
            foreach(Match m in matches)
            {

                ImageMatch im = new ImageMatch();
                im._id = Guid.NewGuid().ToString();
                im.CreatedDate = DateTime.Now;
                im.Venueid = venueId;
                im.VenueName = venueCollection.Find<Venue>(x => x.Id == venueId).FirstOrDefault().Name;
                im.ProductId = m.metadata.ProductId;
                im.ProductName =vpCollection.Find<VenueProduct>(x => x.ProductId == m.metadata.ProductId).FirstOrDefault().ProductName;
                im.Imageid = m.metadata.ImageName;
                im.Score = m.score.ToString();

                imageMatches.Add(im);
            }

            imCollection.InsertMany(imageMatches);
        }
    }
}
