using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net;
using BibliotecaAPI.DTOs;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    public class LibrosControllerPruebas:BasePruebas
    {
        private readonly string url = "/api/v1/libros";
        private string nombreBD = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Post_Devuelve400_CuandoAutoresIdsEsVacio()
        {
            //preparacion
            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();
            var libroCreacionDTO = new LibroCreacionDTO { Titulo = "Titulo" };

            //prueba
            var respuesta = await cliente.PostAsJsonAsync(url, libroCreacionDTO);

            //verificacion
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: respuesta.StatusCode);
        }        
    }
}
