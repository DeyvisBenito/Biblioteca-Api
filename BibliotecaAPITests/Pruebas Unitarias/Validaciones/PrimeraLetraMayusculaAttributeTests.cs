using BibliotecaAPI.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.Pruebas_Unitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributeTests
    {
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        [DataRow("Hola")]
        public void IsValid_RetornaExitosa_SiLaPrimeraLetraNoEsMinuscula(string value)
        {
            //Preparacion de la prueba
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //Ejecucion de la prueba
            var resultado = primeraLetraMayuscula.GetValidationResult(value, validationContext);

            //Validacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);
        }

        [TestMethod]
        [DataRow("hola")]
        public void IsValid_RetornaError_SiLaPrimeraLetraEsMinuscula(string value)
        {
            //Preparacion de la prueba
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //Ejecucion de la prueba
            var resultado = primeraLetraMayuscula.GetValidationResult(value, validationContext);

            //Validacion
            Assert.AreEqual(expected: "La primera letra debe ser mayuscula.", actual: resultado!.ErrorMessage);
        }
    }
}
