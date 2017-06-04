namespace Rscue.Api.BindingModels
{
    using System.ComponentModel.DataAnnotations;

    public class ProviderBindingModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
