using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    public class LibroControllerPruebas:BasePruebas
    {
        [TestMethod]
        public async Task Get_RetornarCeroLibros_CuandoNoHayLibros()
        {
            //preparacion
            var nombreBD = Guid.NewGuid().ToString();
            var context = ContruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IOutputCacheStore outputCacheStore = null!;

            var controller = new LibrosController(context, mapper, outputCacheStore);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var paginacionDTO = new PaginacionDTO(1, 1);

            //prueba
            var respuesta = await controller.Get(paginacionDTO);

            //verificacion
            Assert.AreEqual(expected: 0, actual: respuesta.Count());

        }
    }
}
