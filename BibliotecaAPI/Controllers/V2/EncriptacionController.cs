using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/encriptacion")]
    public class EncriptacionController: ControllerBase
    {
        private IDataProtector protector;
        private ITimeLimitedDataProtector protectorConTiempo;
        private readonly IServicioHash servicioHash;

        public EncriptacionController(IDataProtectionProvider dataProtection, IServicioHash servicioHash)
        {
            protector = dataProtection.CreateProtector("EncriptacionController");
            protectorConTiempo = dataProtection.CreateProtector("EncriptacionController").ToTimeLimitedDataProtector();
            this.servicioHash = servicioHash;
        }


        [HttpGet("hash")]
        [OutputCache]
        public ActionResult Hash(string textoPlano)
        {
            var hash1 = servicioHash.Hash(textoPlano);
            var hash2 = servicioHash.Hash(textoPlano);
            var hash3 = servicioHash.Hash(textoPlano, hash2.Sal);

            var resultado = new { textoPlano, hash1, hash2, hash3 };
            return Ok(resultado);
        }

        [HttpGet("encriptar")]
        [OutputCache]
        public ActionResult Encriptar(string textoPlano)
        {
            var textoEncriptado = protector.Protect(textoPlano);
            return Ok(new { textoEncriptado });
        }

        [HttpGet("encriptarConTiempo")]
        [OutputCache]
        public ActionResult EncriptarConTiempo(string textoPlano)
        {
            var textoEncriptado = protectorConTiempo.Protect(textoPlano, TimeSpan.FromSeconds(45));
            return Ok(new { textoEncriptado });
        }

        [HttpGet("desencriptar")]
        [OutputCache]
        public ActionResult Desencriptar(string textoEncriptado)
        {
            var textoPlano = protector.Unprotect(textoEncriptado);
            return Ok(new { textoPlano });
        }

        [HttpGet("desencriptarConTiempo")]
        [OutputCache]
        public ActionResult DesencriptarConTiempo(string textoEncriptado)
        {
            var textoPlano = protectorConTiempo.Unprotect(textoEncriptado);
            return Ok(new { textoPlano });
        }
    }
}
