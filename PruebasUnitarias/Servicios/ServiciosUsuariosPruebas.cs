using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Servicios
{
    [TestClass]
    public class ServiciosUsuariosPruebas
    {
        private UserManager<Usuario> userManager = null!;
        private IHttpContextAccessor contextAccessor = null!;
        private ServiciosUsuarios serviciosUsuarios = null!;

        [TestInitialize]
        public void Setup()
        {
            userManager = Substitute.For<UserManager < Usuario >> (
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null);
            contextAccessor = Substitute.For<IHttpContextAccessor>();
            serviciosUsuarios = new ServiciosUsuarios(userManager, contextAccessor);

        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornarNulo_CuandoNoHayClaimEmail()
        {
            //preparacion
            var httpContext = new DefaultHttpContext();
            contextAccessor.HttpContext.Returns(httpContext);

            //prueba
            var usuario = await serviciosUsuarios.ObtenerUsuario();

            //verificacion
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornarUsuario_CuandoHayClaimEmail()
        {
            //preparacion
            var email = "prueba@hotmail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(usuarioEsperado));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext();
            contextAccessor.HttpContext.Returns(httpContext);

            //prueba
            var usuario = await serviciosUsuarios.ObtenerUsuario();

            //verificacion
            Assert.IsNotNull(usuario);
            Assert.AreEqual(expected: email, actual: usuario.Email);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornarNulo_CuandoUsuarioNoExiste()
        {
            //preparacion
            var email = "prueba@hotmail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult<Usuario>(null!));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext();
            contextAccessor.HttpContext.Returns(httpContext);

            //prueba
            var usuario = await serviciosUsuarios.ObtenerUsuario();

            //verificacion
            Assert.IsNull(usuario);
        }
    }
}
