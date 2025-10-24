using ConectaBiz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConectaBiz.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Persona> Persona { get; set; }
        public DbSet<Consultor> Consultor { get; set; }
        public DbSet<Frente> Frente { get; set; }
        public DbSet<SubFrente> SubFrente { get; set; }
        public DbSet<ConsultorFrenteSubFrente> ConsultorFrenteSubFrente { get; set; }
        public DbSet<Parametro> Parametros { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketConsultorAsignacion> TicketConsultorAsignacion { get; set; }
        public DbSet<DetalleTareasConsultor> DetalleTareasConsultor { get; set; }
        public DbSet<TicketFrenteSubFrente> TicketFrenteSubFrente { get; set; }
        public DbSet<TicketHistorialEstado> TicketHistorialEstado { get; set; }
        public DbSet<Pais> Pais { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Gestor> Gestores { get; set; }
        public DbSet<GestorFrenteSubFrente> GestorFrenteSubFrente { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<RolPermisoModulo> RolPermisoModulos { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<NotificacionTicket> NotificacionTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Socio>(entity =>
            {
                entity.ToTable("Socio", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Codigo).HasMaxLength(200);
                entity.Property(e => e.Nombre).HasMaxLength(200);
                entity.Property(e => e.NombreComercial).HasMaxLength(200);
                entity.Property(e => e.NumDocContribuyente).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Telefono1).HasMaxLength(20);
                entity.Property(e => e.Telefono2).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).HasColumnType("timestamp").HasDefaultValueSql("now()");
                entity.Property(e => e.FechaModificacion).HasColumnType("timestamp");
                entity.Property(e => e.UsuarioRegistro).HasMaxLength(50);
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);

                // Relación con User
                entity.HasMany(e => e.Users)
                    .WithOne(u => u.Socio)
                    .HasForeignKey(u => u.IdSocio)
                    .OnDelete(DeleteBehavior.Restrict); // Evita que se elimine un Socio con Users
            });


            // Configuración de la entidad User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp").IsRequired();
                entity.Property(e => e.LastLogin).HasColumnType("timestamp");
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IdSocio);
                entity.HasIndex(e => e.IdRol);
                entity.HasIndex(e => e.IdPersona);
                entity.HasIndex(e => e.Activo);

                // Relación con Socio
                entity.HasOne(e => e.Socio)
                    .WithMany(s => s.Users)
                    .HasForeignKey(e => e.IdSocio)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Persona)
                    .WithMany(s => s.Users)
                    .HasForeignKey(e => e.IdPersona)
                    .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Users) 
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasColumnType("text"); 
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp without time zone").HasDefaultValueSql("now()").IsRequired();
                entity.Property(e => e.UsuarioCreacion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaModificacion).HasColumnType("timestamp without time zone");
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
            });


            // Configuración de la entidad RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshToken", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(128);
                entity.Property(e => e.ExpiryDate).HasColumnType("timestamp").IsRequired();
                entity.HasIndex(e => e.Token).IsUnique();

                // Relación con User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de la entidad Persona
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.ToTable("Persona", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombres).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApellidoPaterno).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApellidoMaterno).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NumeroDocumento).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Telefono2).HasMaxLength(20);
                entity.Property(e => e.Correo).HasMaxLength(200);
                entity.Property(e => e.FechaNacimiento).HasColumnType("timestamp without time zone");
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp without time zone").IsRequired();
                entity.Property(e => e.FechaActualizacion).HasColumnType("timestamp without time zone");
                entity.Property(e => e.UsuarioCreacion).HasMaxLength(50);
                entity.Property(e => e.UsuarioActualizacion).HasMaxLength(50);
            });

            // Configuración de la entidad Consultor
            modelBuilder.Entity<Consultor>(entity =>
            {
                entity.ToTable("Consultor", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IdNivelExperiencia);
                entity.Property(e => e.IdModalidadLaboral);
                entity.Property(e => e.IdSocio).IsRequired();
                entity.Property(e => e.IdUser).IsRequired();
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp without time zone").IsRequired();
                entity.Property(e => e.FechaActualizacion).HasColumnType("timestamp without time zone");
                entity.Property(e => e.UsuarioCreacion).IsRequired().HasMaxLength(50);
                entity.Property(g => g.UsuarioActualizacion).HasMaxLength(50);

                // Relación uno a uno con Persona
                entity.HasOne(e => e.Persona)
                    .WithOne(p => p.Consultor)
                    .HasForeignKey<Consultor>(e => e.PersonaId)
                    .OnDelete(DeleteBehavior.Restrict); // O .Cascade si quieres que al borrar Persona se borre el Consultor
                entity.HasOne(e => e.Socio)
                   .WithMany()
                   .HasForeignKey(e => e.IdSocio)
                   .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de la entidad Frente
            modelBuilder.Entity<Frente>(entity =>
            {
                entity.ToTable("Frente", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(200);
                entity.Property(e => e.Activo).HasDefaultValue(true).IsRequired();
                entity.Property(e => e.FechaRegistro).HasColumnType("timestamp").HasDefaultValueSql("now()").IsRequired();
                entity.Property(e => e.FechaModificacion).HasColumnType("timestamp");
                entity.Property(e => e.UsuarioRegistro).HasMaxLength(50);
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);

                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasIndex(e => e.Activo);
            });

            // Configuración de la entidad SubFrente
            modelBuilder.Entity<SubFrente>(entity =>
            {
                entity.ToTable("SubFrente", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(200);
                entity.Property(e => e.IdFrente).IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true).IsRequired();
                entity.Property(e => e.FechaRegistro).HasColumnType("timestamp").HasDefaultValueSql("now()").IsRequired();
                entity.Property(e => e.FechaModificacion).HasColumnType("timestamp");
                entity.Property(e => e.UsuarioRegistro).HasMaxLength(50);
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);
                entity.Property(e => e.Nivel).HasMaxLength(20);

                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasIndex(e => e.Activo);
                entity.HasIndex(e => e.IdFrente);

                // Relación con Frente
                entity.HasOne(e => e.Frente).WithMany(f => f.SubFrente).HasForeignKey(e => e.IdFrente).OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de la entidad ConsultorFrenteSubFrente
            modelBuilder.Entity<ConsultorFrenteSubFrente>(entity =>
            {
                entity.ToTable("ConsultorFrenteSubFrente", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConsultorId).IsRequired();
                entity.Property(e => e.IdFrente).IsRequired();
                entity.Property(e => e.IdSubFrente).IsRequired();
                entity.Property(e => e.IdNivelExperiencia).IsRequired();
                entity.Property(e => e.EsCertificado).HasDefaultValue(false).IsRequired();
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp").IsRequired().HasDefaultValueSql("now()");
                entity.Property(e => e.FechaActualizacion).HasColumnType("timestamp");
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);

                // Relación con Consultor
                entity.HasOne(e => e.Consultor).WithMany(c => c.ConsultorFrenteSubFrente).HasForeignKey(e => e.ConsultorId).OnDelete(DeleteBehavior.Cascade);

                // Relación con Frente
                entity.HasOne(e => e.Frente).WithMany(f => f.ConsultorFrenteSubFrente).HasForeignKey(e => e.IdFrente).OnDelete(DeleteBehavior.Restrict);

                // Relación con SubFrente
                entity.HasOne(e => e.SubFrente).WithMany(s => s.ConsultorFrenteSubFrente).HasForeignKey(e => e.IdSubFrente).OnDelete(DeleteBehavior.Restrict);

                // Índice único para evitar duplicados (un consultor no puede tener la misma combinación frente-subfrente dos veces)
                entity.HasIndex(e => new { e.ConsultorId, e.IdFrente, e.IdSubFrente }).IsUnique().HasDatabaseName("UK_ConsultorFrenteSubFrente_Unique");
            });
            // Configuración de la entidad Parametro
            modelBuilder.Entity<Parametro>(entity =>
            {
                entity.ToTable("Parametro", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(e => e.TipoParametro).HasColumnName("TipoParametro").IsRequired().HasMaxLength(30);
                entity.Property(e => e.Codigo).HasColumnName("Codigo").IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).HasColumnName("Nombre").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasColumnName("Descripcion").HasMaxLength(200);
                entity.Property(e => e.Color).HasColumnName("Color").HasMaxLength(7);
                entity.Property(e => e.Icono).HasColumnName("Icono").HasMaxLength(50);
                entity.Property(e => e.Orden).HasColumnName("Orden").IsRequired().HasDefaultValue((short)0);
                entity.Property(e => e.Valor1).HasColumnName("Valor1").HasMaxLength(50);
                entity.Property(e => e.Valor2).HasColumnName("Valor2").HasMaxLength(50);
                entity.Property(e => e.Valor3).HasColumnName("Valor3").HasMaxLength(50);
                entity.Property(e => e.Activo).HasColumnName("Activo").IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).HasColumnName("FechaRegistro").HasColumnType("timestamp").IsRequired().HasDefaultValueSql("now()");
                entity.Property(e => e.FechaModificacion).HasColumnName("FechaModificacion").HasColumnType("timestamp");
                entity.Property(e => e.UsuarioRegistro).HasColumnName("UsuarioRegistro").HasMaxLength(50);
                entity.Property(e => e.UsuarioModificacion).HasColumnName("UsuarioModificacion").HasMaxLength(50);

                // Índices
                entity.HasIndex(e => e.Activo).HasDatabaseName("IX_Parametro_Activo");
                entity.HasIndex(e => e.TipoParametro).HasDatabaseName("IX_Parametro_TipoParametro");
                entity.HasIndex(e => new { e.TipoParametro, e.Codigo }).IsUnique().HasDatabaseName("UK_Parametro_Tipo_Codigo");
            });

            // Configuración de Ticket
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket", "conectabiz");
                entity.HasKey(e => e.Id).HasName("PK_Ticket");
                entity.HasIndex(e => e.CodTicket).IsUnique().HasDatabaseName("UK_Ticket_CodTicket");
                entity.Property(e => e.CodTicketInterno).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Titulo).IsRequired();
                entity.Property(e => e.FechaSolicitud).HasColumnType("timestamp without time zone").IsRequired();
                entity.HasIndex(e => e.IdTipoTicket);
                entity.HasIndex(e => e.IdEstadoTicket);
                entity.HasIndex(e => e.IdEmpresa);
                entity.HasIndex(e => e.IdUsuarioResponsableCliente);
                entity.HasIndex(e => e.IdPrioridad);
                entity.Property(e => e.Descripcion);
                entity.Property(e => e.UrlArchivos);
                entity.Property(e => e.IdReqSgrCsti);
                entity.Property(e => e.IdGestor);
                entity.Property(e => e.IdGestorConsultoria);
                entity.Property(e => e.CodReqSgrCsti).HasMaxLength(50);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp without time zone").HasDefaultValueSql("now()").IsRequired();
                entity.Property(e => e.UsuarioCreacion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaActualizacion).HasColumnType("timestamp without time zone").HasDefaultValueSql("now()");
                entity.Property(e => e.UsuarioActualizacion).HasMaxLength(50);
                entity.Property(e => e.EsCargaMasiva).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.DatosCargaMasiva);
            });

            // Configuración de TicketConsultorAsignacion
            modelBuilder.Entity<TicketConsultorAsignacion>(entity =>
            {
                entity.ToTable("TicketConsultorAsignacion", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired();
                entity.Property(e => e.IdTicket).IsRequired();
                entity.Property(e => e.IdConsultor).IsRequired();
                entity.Property(e => e.FechaAsignacion).HasColumnType("timestamp").IsRequired();
                entity.Property(e => e.FechaDesasignacion).HasColumnType("timestamp").IsRequired();
                entity.Property(e => e.Activo).HasDefaultValue(true).IsRequired();
                entity.Property(e => e.IdTipoActividad).IsRequired();

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.ConsultorAsignaciones)
                    .HasForeignKey(d => d.IdTicket)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DetalleTareasConsultor>(entity =>
            {
                entity.ToTable("DetalleTareasConsultor", "conectabiz");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FechaInicio).HasColumnType("timestamp").IsRequired();
                entity.Property(e => e.FechaFin).HasColumnType("timestamp").IsRequired();
                entity.Property(e => e.Horas).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Activo).HasDefaultValue(true).IsRequired();

                entity.HasOne(d => d.TicketConsultorAsignacion)
                    .WithMany(p => p.DetalleTareasConsultor)
                    .HasForeignKey(d => d.IdTicketConsultorAsignacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // Configuración de TicketFrenteSubFrente
            modelBuilder.Entity<TicketFrenteSubFrente>(entity =>
            {
                entity.ToTable("TicketFrenteSubFrente", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp without time zone").HasDefaultValueSql("now()").IsRequired();
                entity.Property(e => e.UsuarioCreacion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaModificacion).HasColumnType("timestamp without time zone");
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);
                entity.Property(e => e.Activo).HasDefaultValue(true).IsRequired();
                entity.Property(e => e.Cantidad);
                entity.Property(e => e.FechaInicio).HasColumnType("timestamp without time zone");
                entity.Property(e => e.FechaFin).HasColumnType("timestamp without time zone");
                entity.Property(e => e.IdFrente).IsRequired();
                entity.Property(e => e.IdSubFrente).IsRequired();
                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.FrenteSubFrentes)
                    .HasForeignKey(d => d.IdTicket)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de TicketHistorialEstado
            modelBuilder.Entity<TicketHistorialEstado>(entity =>
            {
                entity.ToTable("TicketHistorialEstado", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaCambio).HasColumnType("timestamp").HasDefaultValueSql("now()").IsRequired();

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.TicketHistorialEstado)
                    .HasForeignKey(d => d.IdTicket)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Configuración de la entidad Pais
            modelBuilder.Entity<Pais>(entity =>
            {
                entity.ToTable("Pais", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(5).HasColumnName("Codigo");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnName("Nombre");
                entity.Property(e => e.CodigoPostal).HasMaxLength(10).HasColumnName("CodigoPostal");
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true).HasColumnName("Activo");
                entity.Property(e => e.FechaRegistro).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("now()").HasColumnName("FechaRegistro");
                entity.Property(e => e.FechaModificacion).HasColumnType("timestamp").HasColumnName("FechaModificacion");
                entity.Property(e => e.UsuarioRegistro).HasMaxLength(50).HasColumnName("UsuarioRegistro");
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50).HasColumnName("UsuarioModificacion");

                entity.HasIndex(e => e.Codigo).IsUnique().HasDatabaseName("Pais_Codigo_key");

                entity.HasMany(p => p.Empresas)
                      .WithOne(e => e.Pais)
                      .HasForeignKey(e => e.IdPais)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de la entidad Empresa
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("Empresa", "conectabiz");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20).HasColumnName("Codigo");
                entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(200).HasColumnName("RazonSocial");
                entity.Property(e => e.NombreComercial).HasMaxLength(100).HasColumnName("NombreComercial");
                entity.Property(e => e.NumDocContribuyente).HasMaxLength(20).HasColumnName("NumDocContribuyente");
                entity.Property(e => e.Direccion).HasMaxLength(200).HasColumnName("Direccion");
                entity.Property(e => e.Telefono).HasMaxLength(20).HasColumnName("Telefono");
                entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("Email");
                entity.Property(e => e.Activo).IsRequired().HasColumnName("Activo").HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).IsRequired().HasColumnName("FechaRegistro").HasColumnType("timestamp without time zone").HasDefaultValueSql("now()");
                entity.Property(e => e.FechaModificacion).HasColumnName("FechaModificacion").HasColumnType("timestamp without time zone");
                entity.Property(e => e.UsuarioRegistro).HasMaxLength(50).HasColumnName("UsuarioRegistro");
                entity.Property(e => e.UsuarioModificacion).HasMaxLength(50).HasColumnName("UsuarioModificacion");
                entity.Property(e => e.IdPais).HasColumnName("IdPais");
                entity.Property(e => e.IdGestor).HasColumnName("IdGestor");
                entity.Property(e => e.IdSocio).IsRequired().HasColumnName("IdSocio");
                entity.Property(e => e.IdPersonaResponsable).HasColumnName("IdPersonaResponsable");
                entity.Property(e => e.IdUser).HasColumnName("IdUser");
                entity.Property(e => e.CargoResponsable).HasMaxLength(100).HasColumnName("CargoResponsable");

                // Índices
                entity.HasIndex(e => e.Activo).HasDatabaseName("IX_Empresa_Activo");
                entity.HasIndex(e => e.IdGestor).HasDatabaseName("IX_Empresa_IdGestor");
                entity.HasIndex(e => e.Codigo).IsUnique().HasDatabaseName("Empresa_Codigo_key");

                // Relaciones
                entity.HasOne(e => e.Pais)
                 .WithMany(p => p.Empresas) // Especifica la propiedad de navegación inversa
                 .HasForeignKey(e => e.IdPais)
                 .HasPrincipalKey(p => p.Id)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Empresa_Pais");

                entity.HasOne(e => e.Gestor)
                     .WithMany(g => g.Empresas)
                     .HasForeignKey(e => e.IdGestor)
                     .HasPrincipalKey(g => g.Id)
                     .OnDelete(DeleteBehavior.SetNull)
                     .HasConstraintName("FK_Empresa_Gestor");

                entity.HasOne(e => e.Socio)
                    .WithMany() // No necesitas exponer lista de empresas en Socio si no quieres
                    .HasForeignKey(e => e.IdSocio)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Empresa_Socio");

                entity.HasOne(e => e.PersonaResponsable)
                   .WithMany() // No necesitas exponer lista de empresas en Persona si no quieres
                   .HasForeignKey(e => e.IdPersonaResponsable)
                   .HasPrincipalKey(p => p.Id)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Empresa_PersonaResponsable");
            });

            // Configuración de la entidad Gestor
            modelBuilder.Entity<Gestor>(entity =>
            {
                entity.ToTable("Gestor", "conectabiz");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(g => g.PersonaId).IsRequired().HasColumnName("PersonaId");
                entity.Property(g => g.IdNivelExperiencia).HasColumnName("IdNivelExperiencia");
                entity.Property(g => g.IdModalidadLaboral).HasColumnName("IdModalidadLaboral");
                entity.Property(g => g.IdSocio).HasColumnName("IdSocio").IsRequired();
                entity.Property(e => e.IdUser).HasColumnName("IdUser").IsRequired();
                entity.Property(g => g.UsuarioCreacion).IsRequired().HasMaxLength(50).HasColumnName("UsuarioCreacion");
                entity.Property(g => g.FechaCreacion).IsRequired().HasColumnName("FechaCreacion").HasColumnType("timestamp without time zone");
                entity.Property(g => g.UsuarioActualizacion).HasMaxLength(50).HasColumnName("UsuarioActualizacion");
                entity.Property(g => g.FechaActualizacion).HasColumnName("FechaActualizacion").HasColumnType("timestamp without time zone");
                entity.Property(g => g.Activo).IsRequired().HasColumnName("Activo");

                // Relaciones
                entity.HasOne(g => g.Persona)
                    .WithMany()
                    .HasForeignKey(g => g.PersonaId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_consultor_persona");

                entity.HasOne(g => g.Socio)
                    .WithMany() // o .WithMany(s => s.Gestores) si tienes navegación inversa
                    .HasForeignKey(g => g.IdSocio)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_gestor_socio");

                entity.HasMany(g => g.GestorFrenteSubFrente)
                    .WithOne()
                    .HasForeignKey(gf => gf.IdGestor)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de la entidad GestorFrenteSubFrente
            modelBuilder.Entity<GestorFrenteSubFrente>(entity =>
            {
                entity.ToTable("GestorFrenteSubFrente", "conectabiz");
                entity.HasKey(gf => gf.Id);
                entity.Property(gf => gf.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(gf => gf.IdGestor).IsRequired().HasColumnName("IdGestor");
                entity.Property(gf => gf.IdFrente).IsRequired().HasColumnName("IdFrente");
                entity.Property(gf => gf.IdSubFrente).IsRequired().HasColumnName("IdSubFrente");
                entity.Property(gf => gf.IdNivelExperiencia).HasColumnName("IdNivelExperiencia");
                entity.Property(gf => gf.EsCertificado).IsRequired().HasColumnName("EsCertificado").HasDefaultValue(false);
                entity.Property(gf => gf.FechaCreacion).IsRequired().HasColumnName("FechaCreacion").HasDefaultValueSql("now()").HasColumnType("timestamp without time zone");
                entity.Property(gf => gf.UsuarioCreacion).IsRequired().HasMaxLength(50).HasColumnName("UsuarioCreacion");
                entity.Property(gf => gf.FechaActualizacion).HasColumnName("FechaActualizacion").HasColumnType("timestamp without time zone");
                entity.Property(gf => gf.UsuarioActualizacion).HasMaxLength(50).HasColumnName("UsuarioActualizacion");
                entity.Property(gf => gf.Activo).IsRequired().HasColumnName("Activo").HasDefaultValue(true);

                // Índices
                entity.HasIndex(gf => gf.IdGestor).HasDatabaseName("IX_GFSF_IdGestor");
                entity.HasIndex(gf => gf.IdFrente).HasDatabaseName("IX_GFSF_IdFrente");
                entity.HasIndex(gf => gf.IdSubFrente).HasDatabaseName("IX_GFSF_IdSubFrente");
                entity.HasIndex(gf => new { gf.IdGestor, gf.IdFrente, gf.IdSubFrente })
                    .IsUnique()
                    .HasDatabaseName("UX_GFSF_IdGestor_Frente_SubFrente_Activo")
                    .HasFilter("\"Activo\" = true");

                // Relaciones
                entity.HasOne(gf => gf.Gestor)
                     .WithMany(g => g.GestorFrenteSubFrente)
                     .HasForeignKey(gf => gf.IdGestor)
                     .OnDelete(DeleteBehavior.Cascade)
                     .HasConstraintName("FK_GFSF_Gestor");

                entity.HasOne(gf => gf.Frente)
                    .WithMany()
                    .HasForeignKey(gf => gf.IdFrente)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_GFSF_Frente");

                entity.HasOne(gf => gf.SubFrente)
                    .WithMany()
                    .HasForeignKey(gf => gf.IdSubFrente)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_GFSF_SubFrente");

            });

            modelBuilder.Entity<Modulo>(entity =>
            {
                entity.ToTable("Modulo", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Icono).HasMaxLength(100);
                entity.Property(e => e.Ruta).HasMaxLength(150);
                entity.Property(e => e.Activo).HasDefaultValue(true).IsRequired();
            });

            modelBuilder.Entity<RolPermisoModulo>(entity =>
            {
                entity.ToTable("RolPermisoModulo", "conectabiz");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.IdRol, e.IdModulo }).IsUnique();

                entity.Property(e => e.DivsOcultos).IsRequired();
                entity.Property(e => e.DivsBloqueados).IsRequired();
                entity.Property(e => e.ControlesBloqueados).IsRequired();
                entity.Property(e => e.ControlesOcultos).IsRequired();

                entity.HasOne(e => e.Modulo)
                      .WithMany(m => m.Permisos)
                      .HasForeignKey(e => e.IdModulo)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<NotificacionTicket>(entity =>
            {
                entity.ToTable("NotificacionTicket", "conectabiz");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.IdTicket).IsRequired();
                entity.Property(e => e.IdUser).IsRequired();
                //entity.Property(e => e.TipoDestinatario).IsRequired().HasMaxLength(50);
                //entity.Property(e => e.Titulo).HasMaxLength(200);
                entity.Property(e => e.Mensaje).HasColumnType("text");
                entity.Property(e => e.Leido).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.FechaCreacion).HasColumnType("timestamp without time zone").IsRequired().HasDefaultValueSql("now()");
                entity.Property(e => e.FechaLectura).HasColumnType("timestamp without time zone");
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);

                // Índices para mejorar rendimiento
                entity.HasIndex(e => e.IdUser).HasDatabaseName("IX_NotificacionTicket_IdUser");
                entity.HasIndex(e => e.IdTicket).HasDatabaseName("IX_NotificacionTicket_IdTicket");
                entity.HasIndex(e => new { e.IdUser, e.Leido }).HasDatabaseName("IX_NotificacionTicket_User_Leido");

                // Relación con Ticket
                entity.HasOne(e => e.Ticket)
                    .WithMany()
                    .HasForeignKey(e => e.IdTicket)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NotificacionTicket_Ticket");
            });
        }
    }
}