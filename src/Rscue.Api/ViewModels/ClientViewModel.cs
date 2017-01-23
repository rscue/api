using System;
using System.ComponentModel.DataAnnotations;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class ClientViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Debe proporcionar un email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public VehicleType VehicleType { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public HullSizeType HullSize { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string BoatModel { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string EngineType { get; set; }

        public string RegistrationNumber { get; set; }

        public Uri AvatarUri { get; set; }

        public string DeviceId { get; set; }

        public string InsuranceCompany { get; set; }

        public string PolicyNumber { get; set; }
    }
}