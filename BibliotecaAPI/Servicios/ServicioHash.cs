using BibliotecaAPI.DTOs;
using BibliotecaAPI.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace BibliotecaAPI.Servicios
{
    public class ServicioHash: IServicioHash
    {

        public HashDTO Hash(string texto)
        {
            var sal = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sal);
            }

            return Hash(texto, sal);
        }

        public HashDTO Hash(string texto, byte[] sal)
        {
            string hassed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: texto,
                salt: sal,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10_000,
                numBytesRequested: 256 / 8
                ));

            return new HashDTO { Hash = hassed, Sal = sal };
        }
    }
}
