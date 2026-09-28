using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Excepciones
{
    public class RecursoNoDisponibleException : Exception
    {
        public RecursoNoDisponibleException(string mensaje) : base(mensaje)
        {
        }
    }
}
