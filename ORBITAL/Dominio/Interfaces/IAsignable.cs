using ORBITAL.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Interfaces
{
    public interface IAsignable
    {
        void Asignar(Mision mision);
        void Liberar();
        bool EstaDisponible();
    }
}
