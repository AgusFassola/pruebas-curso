using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    public class ComentariosControllerPruebas:BasePruebas
    {
        private readonly string url = "/api/v1/libros/1/comentarios";
        private string nombreBD = Guid.NewGuid().ToString();

        private async Task CrearDataDePrueba()
        {
            var context = ContruirContext(nombreBD);
            var autor = new Autor { Nombres = "Agus", Apellidos = "Fassola" };
            context.Add(autor);
            await context.SaveChangesAsync();

            var libro = new Libro { Titulo = "titulo" };
            libro.Autores.Add(new AutorLibro { Autor = autor });
            context.Add(libro);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Devuelve204_CuandoUsuarioBorraSuPropioComentario()
        {
            //preparacion
            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var token = await CrearUsuario(nombreBD, factory);

            var context = ContruirContext(nombreBD);

            var usuario = await context.Users.FirstAsync();

            var comentario = new Comentario
            {
                Cuerpo = "comentario",
                UsuarioId = usuario!.Id,
                LibroId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            //verificacion
            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Devuelve403_CuandoUsuarioIntentaBorrarElComentarioDeOtro()
        {
            //preparacion
            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var emailCreadorComentario = "creador-comentario@hotmail.com";
            await CrearUsuario(nombreBD, factory, [], emailCreadorComentario);

            var context = ContruirContext(nombreBD);

            var usuarioCreadorComentario = await context.Users.FirstAsync();

            var comentario = new Comentario
            {
                Cuerpo = "comentario",
                UsuarioId = usuarioCreadorComentario!.Id,
                LibroId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var tokenUsuarioDistinto = await CrearUsuario(nombreBD, factory, [],"usuario-distinto@hotmail.com");


            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", tokenUsuarioDistinto);
            //prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            //verificacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }
    }
}
