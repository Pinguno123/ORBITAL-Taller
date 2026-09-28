using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class AsignacionRecurso
    {
        // Atributos
        private int id;
        private Mision mision;
        private RecursoExploracion recurso;
        private DateTime fechaAsignacion;
        private DateTime? fechaLiberacion;
        private bool activa;

        // Getters y Setters
        public int Id { get => id; private set => id = value; }
        public Mision Mision { get => mision; private set => mision = value; }
        public RecursoExploracion Recurso { get => recurso; private set => recurso = value; }
        public DateTime FechaAsignacion { get => fechaAsignacion; private set => fechaAsignacion = value; }
        public DateTime? FechaLiberacion { get => fechaLiberacion; private set => fechaLiberacion = value; }
        public bool Activa { get => activa; private set => activa = value; }

        // Constructor
        public AsignacionRecurso(Mision mision, RecursoExploracion recurso)
        {
            if (mision == null)
                throw new ArgumentNullException(nameof(mision));

            if (recurso == null)
                throw new ArgumentNullException(nameof(recurso));

            this.mision = mision;
            this.recurso = recurso;
            this.fechaAsignacion = DateTime.Now;
            this.fechaLiberacion = null;
            this.activa = true;
        }

        // Métodos
        public void Liberar()
        {
            if (!this.activa)
                return;

            this.activa = false;
            this.fechaLiberacion = DateTime.Now;
        }

        public bool EstaActiva()
        {
            return this.activa;
        }

        public override string ToString()
        {
            return Mision.Codigo + " - " + Recurso.Codigo + " - Activa: " + Activa;
        }
    }
}
