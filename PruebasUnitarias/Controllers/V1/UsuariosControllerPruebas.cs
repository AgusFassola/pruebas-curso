using BibliotecaAPI.Controllers.V2;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class UsuariosControllerPruebas: BasePruebas
    {
        private string nombreBD = Guid.NewGuid().ToString();
        private UserManager<Usuario> userManager = null!;
        private SignInManager<Usuario> signInManager = null!;
        private UsuariosController controller = null!;
        private IServicioLlaves servicioLlaves = null!;

        [TestInitialize]
        public void Setup()
        {
            var context = ContruirContext(nombreBD);
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null);

            var miConfiguracion = new Dictionary<string, string>
            {
                {
                    "llavejwt","dsfgasdgsdagsasdg"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(miConfiguracion!)
                .Build();

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            var userClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<Usuario>>();

            signInManager = Substitute.For<SignInManager<Usuario>>(userManager,
                contextAccessor, userClaimsFactory, null, null, null, null);

            var servicioUsuarios = Substitute.For<IServiciosUsuarios>();

            var mapper = ConfigurarAutoMapper();

            servicioLlaves = Substitute.For<IServicioLlaves>();

            controller = new UsuariosController(userManager, configuration, signInManager, 
                servicioUsuarios, context, mapper, servicioLlaves);
        }

        [TestMethod]
        public async Task Registrar_DevuelveValidationProblem_CuandoNoEsExitoso()
        {
            //preparacion
            var mensajeError = "prueba";
            var credenciales = new CredencialesUsuarioDTO { Email = "prueba@hotmail.com", Password = "Agustin1998" };

            userManager.CreateAsync(Arg.Any<Usuario>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError
                {
                    Code = "prueba",
                    Description = mensajeError
                }));

            //Prueba
            var respuesta = await controller.Registrar(credenciales);

            //verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeError, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Registrar_DevuelveToken_CuandoEsExitoso()
        {
            //preparacion
            var credenciales = new CredencialesUsuarioDTO { Email = "prueba@hotmail.com", Password = "Agustin1998" };

            userManager.CreateAsync(Arg.Any<Usuario>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Prueba
            var respuesta = await controller.Registrar(credenciales);

            //verificacion

            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token);

        }

        [TestMethod]
        public async Task Login_DevuelveValidationProblem_CuandoUsuarioNoEsExiste()
        {
            //preparacion
            var credenciales = new CredencialesUsuarioDTO { Email = "prueba@hotmail.com", Password = "Agustin1998" };

            userManager.FindByEmailAsync(credenciales.Email)!.
                Returns(Task.FromResult<Usuario>(null!));

            //Prueba
            var respuesta = await controller.login(credenciales);

            //verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login incorrecto", 
                actual: problemDetails.Errors.Values.First().First());
        }
        [TestMethod]
        public async Task Login_DevuelveValidationProblem_CuandoLoginEsIncorrecto()
        {
            //preparacion
            var credenciales = new CredencialesUsuarioDTO { Email = "prueba@hotmail.com", Password = "Agustin1998" };

            var usuario = new Usuario { Email = credenciales.Email };

            userManager.FindByEmailAsync(credenciales.Email)!.
                Returns(Task.FromResult<Usuario>(usuario!));

            signInManager.CheckPasswordSignInAsync(usuario, credenciales.Password, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            //Prueba
            var respuesta = await controller.login(credenciales);

            //verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login incorrecto",
                actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_DevuelveToken_CuandoLoginEsCorrecto()
        {
            //preparacion
            var credenciales = new CredencialesUsuarioDTO { Email = "prueba@hotmail.com", Password = "Agustin1998" };

            var usuario = new Usuario { Email = credenciales.Email };

            userManager.FindByEmailAsync(credenciales.Email)!.
                Returns(Task.FromResult<Usuario>(usuario!));

            signInManager.CheckPasswordSignInAsync(usuario, credenciales.Password, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            //Prueba
            var respuesta = await controller.login(credenciales);

            //verificacion
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token);
        }

    }
}
