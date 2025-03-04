using Azure;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using BibliotecaAPITests.Utilidades.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas: BasePruebas
    {
        IAlmacenadorArchivos almacenadorArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServicioAutores servicioAutores = null!;
        private string nombreBD = Guid.NewGuid().ToString();
        private AutoresController controller = null!;

        [TestInitialize]
        public void Setup()
        {
            
            var context = ContruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            almacenadorArchivos = Substitute.For<IAlmacenadorArchivos>();
            logger = Substitute.For<ILogger<AutoresController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();

            controller = new AutoresController(context, mapper, almacenadorArchivos,
                logger, outputCacheStore, servicioAutores);
        }

        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {

            //prueba
            var respuesta = await controller.Get(1);

            //verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            //preparacion
            var context = ContruirContext(nombreBD);

            context.Autores.Add(new Autor { Nombres = "Agus", Apellidos = "Fassola" });
            context.Autores.Add(new Autor { Nombres = "Lara", Apellidos = "Valdez" });

            await context.SaveChangesAsync();

            //prueba
            var respuesta = await controller.Get(1);

            //verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }

        [TestMethod]
        public async Task Get_RetornaAutorConLibros_CuandoAutorTieneLibros()
        {
            //preparacion 

            var context = ContruirContext(nombreBD);
            var libro1 = new Libro { Titulo = "libro 1" };
            var libro2 = new Libro { Titulo = "libro 2" };
            var autor = new Autor()
            {
                Nombres = "agus",
                Apellidos = "fass",
                Libros = new List<AutorLibro>
                {
                    new AutorLibro{Libro = libro1},
                    new AutorLibro{Libro = libro2}
                }
            };
            context.Add(autor);
            await context.SaveChangesAsync();

            //prueba
            var respuesta = await controller.Get(1);

            //verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.Libros.Count);
        }

        [TestMethod]
        public async Task Get_DebeLlamarGetDelServicioAutores()
        {
            //preparacion 
            var paginacionDTO = new PaginacionDTO(2, 3);

            //prueba
            await controller.Get(paginacionDTO);

            //verificacion
            await servicioAutores.Received(1).Get(paginacionDTO);
        }

        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            //preparacion 
            var context = ContruirContext(nombreBD);
            var nuevoAutor = new AutorCreacionDTO{ Nombres = "nuevo", Apellidos = "autor"};

            //prueba
            var respuesta = await controller.Post(nuevoAutor);

            //verificacion
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ContruirContext(nombreBD);
            var cantidadd = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidadd);

        }
        [TestMethod]
        public async Task Put_Retorna404_CuandoAutorNoExiste()
        {
            //prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: null!);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

        }
        private const string contenedor = "autores";
        private const string cache = "autores-obtener";

        [TestMethod]
        public async Task Put_ActualizarAutor_CuandoEnviamosAutorSinFoto()
        {
            //preparacion 
            var context = ContruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres="mateo",
                Apellidos="fassola",
                Identificacion ="Id"
            });

            await context.SaveChangesAsync();

            var autorCreacionDTO = new AutorCreacionDTOConFoto
            {
                Nombres = "mateo2",
                Apellidos = "fassola2",
                Identificacion = "Id2"
            };

            //prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

            var context3 = ContruirContext(nombreBD);
            var AutorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "mateo2", actual: AutorActualizado.Nombres);
            Assert.AreEqual(expected: "fassola2", actual: AutorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: AutorActualizado.Identificacion);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Editar(default, default!, default!);
        }

        [TestMethod]
        public async Task Put_ActualizarAutor_CuandoEnviamosAutorConFoto()
        {
            //preparacion 
            var context = ContruirContext(nombreBD);

            var urlAnterior = "URL-1";
            var urlNueva = "URL-2";
            almacenadorArchivos.Editar(default, default!, default!).ReturnsForAnyArgs(urlNueva);

            context.Autores.Add(new Autor
            {
                Nombres = "mateo",
                Apellidos = "fassola",
                Identificacion = "Id",
                Foto = urlAnterior
            });

            await context.SaveChangesAsync();

            var formFIle = Substitute.For<IFormFile>();

            var autorCreacionDTO = new AutorCreacionDTOConFoto
            {
                Nombres = "mateo2",
                Apellidos = "fassola2",
                Identificacion = "Id2",
                Foto = formFIle
            };

            //prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

            var context3 = ContruirContext(nombreBD);
            var AutorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "mateo2", actual: AutorActualizado.Nombres);
            Assert.AreEqual(expected: "fassola2", actual: AutorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: AutorActualizado.Identificacion);
            Assert.AreEqual(expected: urlNueva, actual: AutorActualizado.Foto);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Editar(urlAnterior, contenedor, formFIle);
        }

        [TestMethod]
        public async Task Patch_Retorna400_CuandoPatchDocEsNulo()
        {
            //prueba
            var respuesta = await controller.Patch(1, patchDoc: null!);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(400, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            //preparacion
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            //preparacion
            var context = ContruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Agus",
                Apellidos = "Fassola",
                Identificacion = "123"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeError = "mensaje de error";
            controller.ModelState.AddModelError("", mensajeError);

            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //verificacion
            var resultado = respuesta as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeError, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Patch_ActualizarUnCAmpo_CuandoSeLeEnviaUnaOperacion()
        {
            //preparacion
            var context = ContruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Agus",
                Apellidos = "fassola",
                Identificacion = "123",
                Foto="URL-1"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;


            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            patchDoc.Operations.Add(new Microsoft.AspNetCore.JsonPatch.Operations.Operation<AutorPatchDTO>("replace", "/nombres", null, "mateo2"));

            //prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = ContruirContext(nombreBD);
            var autorBD = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "mateo2", autorBD.Nombres);
            Assert.AreEqual(expected: "fassola", autorBD.Apellidos);
            Assert.AreEqual(expected: "123", autorBD.Identificacion);
            Assert.AreEqual(expected: "URL-1", autorBD.Foto);
        }

        [TestMethod]
        public async Task Delete_Retornar404_CuandoAutorNoExiste()
        {
            //prueba
            var respuesta = await controller.Delete(1);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_RBorraAutor_CuandoAutorExiste()
        {
            //preparacion
            var urlFoto = "URL-1";
            var context = ContruirContext(nombreBD);

            context.Autores.Add(new Autor
            {
                Nombres = "autor1",
                Apellidos = "autor1",
                Foto = urlFoto
            });
            context.Autores.Add(new Autor
            {
                Nombres = "autor2",
                Apellidos = "autor2",
            });

            await context.SaveChangesAsync();

            //prueba
            var respuesta = await controller.Delete(1);

            //verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context2 = ContruirContext(nombreBD);
            var cantidadAutores = await context2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidadAutores);

            var autor2Existe = await context2.Autores.AnyAsync(x => x.Nombres == "Autor2");
            Assert.IsTrue(autor2Existe);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Borrar(urlFoto, contenedor);

        }
    }
}
