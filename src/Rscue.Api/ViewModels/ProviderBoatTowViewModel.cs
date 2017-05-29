namespace Rscue.Api.ViewModels
{
    public class ProviderBoatTowViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string BoatModel { get; set; }

        public string EngineType { get; set; }

        public string RegistrationNumber { get; set; }

        public double? FuelCostPerKm { get; set; }
    }
}