using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Rscue.Api.ViewModels
{
    public class BoatTowViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string BoatModel { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string EngineType { get; set; }

        public string RegistrationNumber { get; set; }

        public double? FuelCostPerKm { get; set; }
    }
}