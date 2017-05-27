using System;
using System.ComponentModel.DataAnnotations;

namespace Rscue.Api.ViewModels
{
    public class ProviderViewModel
    {
        [Required(ErrorMessage = "El campo es requerido")]
        public string Id { get; set; }

        public string Auth0Id { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string Address { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string City { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string State { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [EmailAddress(ErrorMessage = "Debe proporcionar un email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public Uri AvatarUri { get; set; }

        public string ProfilePictureUrl { get; set; }
    }
}