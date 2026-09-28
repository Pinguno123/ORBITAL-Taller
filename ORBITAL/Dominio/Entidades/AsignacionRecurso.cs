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
        public int Id { get => id; set => id = value; }
        public Mision Mision { get => mision; set => mision = value; }
        public RecursoExploracion Recurso { get => recurso; set => recurso = value; }
        public DateTime FechaAsignacion { get => fechaAsignacion; set => fechaAsignacion = value; }
        public DateTime? FechaLiberacion { get => fechaLiberacion; set => fechaLiberacion = value; }
        public bool Activa { get => activa; set => activa = value; }

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

            // Asegurar que el recurso pase a asignado
            recurso.Asignar(mision);
        }

        public AsignacionRecurso(int id, Mision mision, RecursoExploracion recurso, DateTime fechaAsignacion, DateTime? fechaLiberacion, bool activa)
        {
            this.id = id;
            this.mision = mision;
            this.recurso = recurso;
            this.fechaAsignacion = fechaAsignacion;
            this.fechaLiberacion = fechaLiberacion;
            this.activa = activa;
        }

        // Métodos
        public void Liberar()
        {
            if (!this.activa)
                return;

            this.activa = false;
            this.fechaLiberacion = DateTime.Now;
            if (this.recurso != null)
            {
                this.recurso.Liberar();
            }
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
