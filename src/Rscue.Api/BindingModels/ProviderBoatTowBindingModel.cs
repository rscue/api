namespace Rscue.Api.BindingModels
{
    using System.ComponentModel.DataAnnotations;

    public class ProviderBoatTowBindingModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string BoatModel { get; set; }

        [Required]
        public string EngineType { get; set; }

        public string RegistrationNumber { get; set; }

        public double? FuelCostPerKm { get; set; }

    }
}
