using BibliotecaAPI.Validaciones;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Autor: IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos.")]
        //[PrimeraLetraMayuscula]
        public required string Nombres { get; set; }
        [PrimeraLetraMayuscula]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(150, ErrorMessage ="El campo {0} debe tener menos de {1} caracteres")]
        public required string Apellidos { get; set; }
        [StringLength(20, ErrorMessage ="El campo {0} debe tener como maximo {1} caracteres")]
        public string? Identificacion { get; set; }
        [Unicode(false)]
        public string? Foto { get; set; }
        public List<AutorLibro> Libros { get; set; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Nombres))
            {
                var primeraLetra = Nombres[0].ToString();
                if(primeraLetra != primeraLetra.ToUpper())
                {
                    yield return new ValidationResult("La primera letra debe ser mayuscula - (Modelo)", new string[] { nameof(Nombres) });
                }
            }
        }
    }
}
