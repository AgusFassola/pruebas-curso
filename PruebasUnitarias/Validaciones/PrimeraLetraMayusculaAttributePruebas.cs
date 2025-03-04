using BibliotecaAPI.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributePruebas
    {
        [TestMethod]
        //datarow para correr varias veces la prueba con diferentes valores
        [DataRow("")]
        [DataRow(null)]
        [DataRow("Felipe")]

        public void IsValid_RetornaExitoso_SiValueNoTieneLaPrimerLEtraMinuscula(string value)
        {
            //preparacion
            var primeraLetraMatusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //prueba
            var resultado = primeraLetraMatusculaAttribute.GetValidationResult(value, validationContext);

            //verificacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);
        }

        [TestMethod]
        [DataRow("felipe")]

        public void IsValid_RetornaErroro_SiValueNoTieneLaPrimerLEtraMinuscula(string value)
        {
            //preparacion
            var primeraLetraMatusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //prueba
            var resultado = primeraLetraMatusculaAttribute.GetValidationResult(value, validationContext);

            //verificacion
            Assert.AreEqual(expected: "La primera letra debe ser mayuscula", actual: resultado!.ErrorMessage);
        }
    }
}
