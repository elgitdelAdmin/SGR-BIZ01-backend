
CREATE TABLE conectabiz."Pais" (
    "Id" integer NOT NULL,
    "Codigo" character varying(5) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "CodigoPostal" character varying(10),
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaRegistro" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioRegistro" character varying(50),
    "UsuarioModificacion" character varying(50)
);

CREATE TABLE conectabiz."Socio" (
    "Id" integer NOT NULL,
    "RazonSocial" character varying(200) NOT NULL,
    "Codigo" character varying(200),
    "Nombre" character varying(200),
    "NombreComercial" character varying(200),
    "NumDocContribuyente" character varying(20),
    "Direccion" character varying(200),
    "Telefono1" character varying(20),
    "Telefono2" character varying(20),
    "Email" character varying(100),
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaRegistro" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioRegistro" character varying(50),
    "UsuarioModificacion" character varying(50),
    "Logo" text
);

CREATE TABLE conectabiz."Modulo" (
    "Id" integer NOT NULL,
    "Codigo" character varying(100) NOT NULL,
    "Nombre" character varying(150) NOT NULL,
    "Icono" character varying(100),
    "Ruta" character varying(150),
    "Activo" boolean DEFAULT true NOT NULL
);

CREATE TABLE conectabiz."Rol" (
    "Id" integer NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Descripcion" text,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "UsuarioCreacion" character varying(50) NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioModificacion" character varying(50),
    "Activo" boolean DEFAULT true NOT NULL
);

CREATE TABLE conectabiz."Parametro" (
    "Id" integer NOT NULL,
    "TipoParametro" character varying(30) NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Descripcion" character varying(200),
    "Color" character varying(7),
    "Icono" character varying(50),
    "Orden" smallint DEFAULT 0 NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaRegistro" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioRegistro" character varying(50),
    "UsuarioModificacion" character varying(50),
    "Valor1" text,
    "Valor2" character varying,
    "Valor3" character varying,
    CONSTRAINT "CK_Parametro_TipoParametro" CHECK ((("TipoParametro")::text = ANY (ARRAY['Prioridad'::text, 'TipoTicket'::text, 'EstadoTicket'::text, 'TipoActividad'::text, 'TipoDocumento'::text, 'Seniority'::text, 'NivelExperiencia'::text, 'ModalidadLaboral'::text, 'TipoCargaMasiva'::text, 'Subtipos'::text, 'TipoReporte'::text])))
);

CREATE TABLE conectabiz."Feriados" (
    "Id" integer NOT NULL,
    "Fecha" date NOT NULL,
    "Nombre" character varying(150) NOT NULL,
    "Tipo" character varying(30) DEFAULT 'Nacional'::character varying NOT NULL,
    "EsFeriado" boolean DEFAULT true NOT NULL,
    "EsRecuperable" boolean DEFAULT false NOT NULL,
    "Descripcion" text,
    "FechaCreacion" timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "FechaActualizacion" timestamp without time zone
);


-- ============================================================
-- PERSONAS
-- ============================================================

CREATE TABLE conectabiz."Persona" (
    "Id" integer NOT NULL,
    "Nombres" character varying(255) NOT NULL,
    "ApellidoMaterno" character varying(255) NOT NULL,
    "ApellidoPaterno" character varying(255) NOT NULL,
    "NumeroDocumento" character varying(255),
    "TipoDocumento" integer NOT NULL,
    "Telefono" character varying(255),
    "Direccion" character varying(255),
    "FechaNacimiento" timestamp without time zone,
    "FechaCreacion" timestamp without time zone NOT NULL,
    "FechaActualizacion" timestamp without time zone,
    "Activo" boolean NOT NULL,
    "Correo" character varying,
    "Telefono2" character varying,
    "UsuarioActualizacion" character varying(50),
    "UsuarioCreacion" character varying(50) NOT NULL
);


-- ============================================================
-- PERSONAS ESPECIALIZADAS: CONSULTORES Y GESTORES
-- ============================================================

CREATE TABLE conectabiz."Consultor" (
    "Id" integer NOT NULL,
    "PersonaId" integer NOT NULL,
    "IdNivelExperiencia" integer,
    "FechaCreacion" timestamp without time zone NOT NULL,
    "FechaActualizacion" timestamp without time zone,
    "Activo" boolean NOT NULL,
    "IdModalidadLaboral" integer,
    "IdSocio" integer NOT NULL,
    "UsuarioCreacion" character varying(50),
    "UsuarioActualizacion" character varying(50),
    "IdUser" integer NOT NULL
);

CREATE TABLE conectabiz."Gestor" (
    "Id" integer NOT NULL,
    "PersonaId" integer NOT NULL,
    "IdNivelExperiencia" integer,
    "IdModalidadLaboral" integer,
    "UsuarioCreacion" character varying(50) NOT NULL,
    "FechaCreacion" timestamp without time zone NOT NULL,
    "UsuarioActualizacion" character varying(50),
    "FechaActualizacion" timestamp without time zone,
    "Activo" boolean NOT NULL,
    "IdSocio" integer NOT NULL,
    "IdUser" integer NOT NULL
);


-- ============================================================
-- ORGANIZACIÓN: EMPRESAS
-- ============================================================

CREATE TABLE conectabiz."Empresa" (
    "Id" integer NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "RazonSocial" character varying(200) NOT NULL,
    "NombreComercial" character varying(100),
    "NumDocContribuyente" character varying(20),
    "Direccion" character varying(200),
    "Telefono" character varying(20),
    "Email" character varying(100),
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaRegistro" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioRegistro" character varying(50),
    "UsuarioModificacion" character varying(50),
    "IdPais" integer,
    "IdGestor" integer,
    "IdSocio" integer NOT NULL,
    "IdPersonaResponsable" integer,
    "CargoResponsable" character varying(100),
    "CodSgrCsti" integer,
    "IdUser" integer
);

CREATE TABLE conectabiz."EmpresaGestor" (
    "Id" integer NOT NULL,
    "IdEmpresa" integer NOT NULL,
    "IdGestor" integer NOT NULL,
    "EsPrincipal" boolean DEFAULT false NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaAsignacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaDesasignacion" timestamp without time zone,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioCreacion" character varying(50),
    "UsuarioModificacion" character varying(50)
);


-- ============================================================
-- AUTENTICACIÓN Y USUARIOS
-- ============================================================

CREATE TABLE conectabiz."User" (
    "Id" integer NOT NULL,
    "Username" character varying(255) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "LastLogin" timestamp without time zone,
    "IdSocio" integer NOT NULL,
    "Activo" boolean,
    "IdPersona" integer
);

CREATE TABLE conectabiz."UserRolSocio" (
    "IdUser" integer NOT NULL,
    "IdRol" integer NOT NULL,
    "IdSocio" integer NOT NULL,
    "FechaAsignacion" timestamp without time zone DEFAULT now() NOT NULL,
    "UsuarioCreacion" character varying(50) NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL
);

CREATE TABLE conectabiz."RefreshToken" (
    "Id" integer NOT NULL,
    "Token" character varying(255) NOT NULL,
    "ExpiryDate" timestamp without time zone NOT NULL,
    "IsRevoked" boolean NOT NULL,
    "UserId" integer NOT NULL
);

CREATE TABLE conectabiz."PasswordResetToken" (
    "Id" integer NOT NULL,
    "Token" character varying(500) NOT NULL,
    "UserId" integer NOT NULL,
    "ExpiryDate" timestamp without time zone NOT NULL,
    "IsUsed" boolean DEFAULT false NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE TABLE conectabiz."EmailVerificationCode" (
    "Id" integer NOT NULL,
    "Email" character varying(100) NOT NULL,
    "Code" character varying(10) NOT NULL,
    "ExpiryDate" timestamp without time zone NOT NULL,
    "IsUsed" boolean DEFAULT false NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "UserId" integer
);

CREATE TABLE conectabiz."RolPermisoModulo" (
    "Id" integer NOT NULL,
    "IdRol" integer NOT NULL,
    "IdModulo" integer NOT NULL,
    "DivsOcultos" text,
    "ControlesBloqueados" text,
    "ControlesOcultos" text,
    "DivsBloqueados" text
);


-- ============================================================
-- ESTRUCTURA DE ESPECIALIDAD: FRENTES / SUBFRENTES
-- ============================================================

CREATE TABLE conectabiz."Frente" (
    "Id" integer NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Descripcion" character varying(200),
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaRegistro" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioRegistro" character varying(50),
    "UsuarioModificacion" character varying(50)
);

CREATE TABLE conectabiz."SubFrente" (
    "Id" integer NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Descripcion" character varying(200),
    "IdFrente" integer NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaRegistro" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioRegistro" character varying(50),
    "UsuarioModificacion" character varying(50),
    "Nivel" character varying(20),
    "Valor1" text
);

CREATE TABLE conectabiz."ConsultorFrenteSubFrente" (
    "Id" integer NOT NULL,
    "ConsultorId" integer NOT NULL,
    "IdFrente" integer NOT NULL,
    "IdSubFrente" integer NOT NULL,
    "IdNivelExperiencia" integer,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaActualizacion" timestamp without time zone,
    "Activo" boolean DEFAULT true NOT NULL,
    "EsCertificado" boolean DEFAULT false NOT NULL
);

CREATE TABLE conectabiz."GestorFrenteSubFrente" (
    "Id" integer NOT NULL,
    "IdGestor" integer NOT NULL,
    "IdFrente" integer NOT NULL,
    "IdSubFrente" integer NOT NULL,
    "IdNivelExperiencia" integer,
    "EsCertificado" boolean DEFAULT false NOT NULL,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "UsuarioCreacion" character varying(50) NOT NULL,
    "FechaActualizacion" timestamp without time zone,
    "UsuarioActualizacion" character varying(50),
    "Activo" boolean DEFAULT true NOT NULL
);


-- ============================================================
-- NÚCLEO OPERATIVO: TICKETS
-- ============================================================

CREATE TABLE conectabiz."Ticket" (
    "Id" integer NOT NULL,
    "CodTicket" character varying(50) NOT NULL,
    "CodTicketInterno" character varying(50) NOT NULL,
    "Titulo" text NOT NULL,
    "FechaSolicitud" timestamp without time zone,
    "IdTipoTicket" integer NOT NULL,
    "IdEstadoTicket" integer NOT NULL,
    "IdEmpresa" integer NOT NULL,
    "IdUsuarioResponsableCliente" integer NOT NULL,
    "IdPrioridad" integer NOT NULL,
    "Descripcion" text,
    "UrlArchivos" text,
    "IdReqSgrCsti" integer,
    "CodReqSgrCsti" character varying(50),
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaCreacion" timestamp without time zone,
    "FechaActualizacion" timestamp without time zone,
    "UsuarioCreacion" character varying(50),
    "UsuarioActualizacion" character varying(50),
    "IdGestorConsultoria" integer,
    "EsCargaMasiva" boolean DEFAULT false,
    "DatosCargaMasiva" text,
    "IdSubTipoTicket" integer,
    "Repositorios" character varying
);

CREATE TABLE conectabiz."TicketConsultorAsignacion" (
    "Id" integer NOT NULL,
    "IdTicket" integer NOT NULL,
    "IdConsultor" integer NOT NULL,
    "FechaAsignacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaDesasignacion" timestamp without time zone NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "IdTipoActividad" integer NOT NULL,
    "IdFrente" integer,
    "IdSubFrente" integer,
    "IdTicketFrenteSubFrente" integer
);

CREATE TABLE conectabiz."TicketGestorAsignacion" (
    "Id" integer NOT NULL,
    "IdTicket" integer NOT NULL,
    "IdGestor" integer NOT NULL,
    "IdGestorAsigno" integer NOT NULL,
    "IdGestorDesasigno" integer,
    "FechaAsignacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaDesasignacion" timestamp without time zone,
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioCreacion" character varying(50),
    "UsuarioModificacion" character varying(50)
);

CREATE TABLE conectabiz."TicketFrenteSubFrente" (
    "Id" integer NOT NULL,
    "IdTicket" integer NOT NULL,
    "IdFrente" integer NOT NULL,
    "IdSubFrente" integer NOT NULL,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "UsuarioCreacion" character varying(50) NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioModificacion" character varying(50),
    "Activo" boolean DEFAULT true NOT NULL,
    "Cantidad" integer,
    "FechaInicio" timestamp without time zone,
    "FechaFin" timestamp without time zone,
    "Descripcion" character varying
);

CREATE TABLE conectabiz."TicketHistorialEstado" (
    "Id" integer DEFAULT nextval('conectabiz."TicketHistorial_Id_seq"'::regclass) NOT NULL,
    "IdTicket" integer NOT NULL,
    "IdEstadoAnterior" integer,
    "IdEstadoNuevo" integer,
    "FechaCambio" timestamp without time zone DEFAULT now() NOT NULL,
    "UsuarioCambio" character varying(50)
);

CREATE TABLE conectabiz."NotificacionTicket" (
    "Id" integer NOT NULL,
    "IdTicket" integer NOT NULL,
    "IdUser" integer NOT NULL,
    "Leido" boolean DEFAULT false NOT NULL,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaLectura" timestamp without time zone,
    "Activo" boolean DEFAULT true NOT NULL,
    "Mensaje" character varying
);


-- ============================================================
-- PLANIFICACIÓN Y EJECUCIÓN DE HORAS
-- ============================================================

CREATE TABLE conectabiz."DetalleTareasConsultor" (
    "Id" integer NOT NULL,
    "IdTicketConsultorAsignacion" integer NOT NULL,
    "FechaInicio" timestamp without time zone NOT NULL,
    "FechaFin" timestamp without time zone NOT NULL,
    "Horas" numeric(18,2) NOT NULL,
    "Descripcion" character varying(500),
    "Activo" boolean DEFAULT true NOT NULL,
    "IdTipoActividad" integer NOT NULL
);

CREATE TABLE conectabiz."DetallePlanificacionConsultor" (
    "Id" integer NOT NULL,
    "FechaInicio" timestamp without time zone NOT NULL,
    "FechaFin" timestamp without time zone NOT NULL,
    "Horas" numeric(18,2) NOT NULL,
    "Descripcion" character varying(500),
    "Activo" boolean DEFAULT true NOT NULL,
    "IdTipoActividad" integer NOT NULL,
    "IdTicketFrenteSubFrente" integer,
    "IdTicketConsultorAsignacion" integer
);


-- ============================================================
-- PRIMARY KEYS
-- ============================================================

ALTER TABLE ONLY conectabiz."Consultor"
    ADD CONSTRAINT "Consultor_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."DetallePlanificacionConsultor"
    ADD CONSTRAINT "DetallePlanificacionConsultor_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."DetalleTareasConsultor"
    ADD CONSTRAINT "DetalleTareasConsultor_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."EmailVerificationCode"
    ADD CONSTRAINT "EmailVerificationCode_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."EmpresaGestor"
    ADD CONSTRAINT "EmpresaGestor_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "Empresa_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Feriados"
    ADD CONSTRAINT "Feriados_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Gestor"
    ADD CONSTRAINT "Gestor_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Modulo"
    ADD CONSTRAINT "Modulo_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."NotificacionTicket"
    ADD CONSTRAINT "NotificacionTicket_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."ConsultorFrenteSubFrente"
    ADD CONSTRAINT "PK_ConsultorFrenteSubFrente" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Frente"
    ADD CONSTRAINT "PK_Frente" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."GestorFrenteSubFrente"
    ADD CONSTRAINT "PK_GestorFrenteSubFrente" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Parametro"
    ADD CONSTRAINT "PK_Parametro" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."SubFrente"
    ADD CONSTRAINT "PK_SubFrente" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Ticket"
    ADD CONSTRAINT "PK_Ticket" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Pais"
    ADD CONSTRAINT "Pais_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."PasswordResetToken"
    ADD CONSTRAINT "PasswordResetToken_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Persona"
    ADD CONSTRAINT "Persona_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."RefreshToken"
    ADD CONSTRAINT "RefreshToken_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."RolPermisoModulo"
    ADD CONSTRAINT "RolPermisoModulo_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Rol"
    ADD CONSTRAINT "Rol_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Socio"
    ADD CONSTRAINT "Socio_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."TicketConsultorAsignacion"
    ADD CONSTRAINT "TicketConsultorAsignacion_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."TicketFrenteSubFrente"
    ADD CONSTRAINT "TicketFrenteSubFrente_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."TicketGestorAsignacion"
    ADD CONSTRAINT "TicketGestorAsignacion_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."TicketHistorialEstado"
    ADD CONSTRAINT "TicketHistorial_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."UserRolSocio"
    ADD CONSTRAINT "UserRolSocio_pkey" PRIMARY KEY ("IdUser", "IdRol", "IdSocio");

ALTER TABLE ONLY conectabiz."User"
    ADD CONSTRAINT "User_pkey" PRIMARY KEY ("Id");

-- ============================================================
-- UNIQUE CONSTRAINTS
-- ============================================================

ALTER TABLE ONLY conectabiz."Modulo"
    ADD CONSTRAINT "Modulo_Codigo_key" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."Pais"
    ADD CONSTRAINT "Pais_Codigo_key" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."Rol"
    ADD CONSTRAINT "Rol_Codigo_key" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."Frente"
    ADD CONSTRAINT "UK_Frente_Codigo" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."SubFrente"
    ADD CONSTRAINT "UK_SubFrente_Codigo" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."Ticket"
    ADD CONSTRAINT "UK_Ticket_CodTicket" UNIQUE ("CodTicket");

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "UQ_Empresa_NumDocContribuyente_IdPais_IdSocio" UNIQUE ("NumDocContribuyente", "IdPais", "IdSocio");

ALTER TABLE ONLY conectabiz."RolPermisoModulo"
    ADD CONSTRAINT uq_rol_modulo UNIQUE ("IdRol", "IdModulo");

-- ============================================================
-- FOREIGN KEYS
-- ============================================================

ALTER TABLE ONLY conectabiz."ConsultorFrenteSubFrente"
    ADD CONSTRAINT "FK_CFSF_Consultor" FOREIGN KEY ("ConsultorId") REFERENCES conectabiz."Consultor"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."ConsultorFrenteSubFrente"
    ADD CONSTRAINT "FK_CFSF_Frente" FOREIGN KEY ("IdFrente") REFERENCES conectabiz."Frente"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."ConsultorFrenteSubFrente"
    ADD CONSTRAINT "FK_CFSF_SubFrente" FOREIGN KEY ("IdSubFrente") REFERENCES conectabiz."SubFrente"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."DetallePlanificacionConsultor"
    ADD CONSTRAINT "FK_DetallePlanificacionConsultor_Asignacion" FOREIGN KEY ("IdTicketConsultorAsignacion") REFERENCES conectabiz."TicketConsultorAsignacion"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."DetalleTareasConsultor"
    ADD CONSTRAINT "FK_DetalleTareasConsultor_Asignacion" FOREIGN KEY ("IdTicketConsultorAsignacion") REFERENCES conectabiz."TicketConsultorAsignacion"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."EmpresaGestor"
    ADD CONSTRAINT "FK_EG_Empresa" FOREIGN KEY ("IdEmpresa") REFERENCES conectabiz."Empresa"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."EmpresaGestor"
    ADD CONSTRAINT "FK_EG_Gestor" FOREIGN KEY ("IdGestor") REFERENCES conectabiz."Gestor"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."EmailVerificationCode"
    ADD CONSTRAINT "FK_EmailVerificationCode_User_UserId" FOREIGN KEY ("UserId") REFERENCES conectabiz."User"("Id") ON DELETE SET NULL;

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "FK_Empresa_Gestor" FOREIGN KEY ("IdGestor") REFERENCES conectabiz."Gestor"("Id") ON DELETE SET NULL;

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "FK_Empresa_Pais" FOREIGN KEY ("IdPais") REFERENCES conectabiz."Pais"("Id");

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "FK_Empresa_PersonaResponsable" FOREIGN KEY ("IdPersonaResponsable") REFERENCES conectabiz."Persona"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "FK_Empresa_Socio" FOREIGN KEY ("IdSocio") REFERENCES conectabiz."Socio"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."GestorFrenteSubFrente"
    ADD CONSTRAINT "FK_GFSF_Frente" FOREIGN KEY ("IdFrente") REFERENCES conectabiz."Frente"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."GestorFrenteSubFrente"
    ADD CONSTRAINT "FK_GFSF_IdGestor" FOREIGN KEY ("IdGestor") REFERENCES conectabiz."Gestor"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."GestorFrenteSubFrente"
    ADD CONSTRAINT "FK_GFSF_SubFrente" FOREIGN KEY ("IdSubFrente") REFERENCES conectabiz."SubFrente"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."NotificacionTicket"
    ADD CONSTRAINT "FK_NotificacionTicket_Ticket" FOREIGN KEY ("IdTicket") REFERENCES conectabiz."Ticket"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."PasswordResetToken"
    ADD CONSTRAINT "FK_PasswordResetToken_User" FOREIGN KEY ("UserId") REFERENCES conectabiz."User"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."RefreshToken"
    ADD CONSTRAINT "FK_RefreshToken_User" FOREIGN KEY ("UserId") REFERENCES conectabiz."User"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."SubFrente"
    ADD CONSTRAINT "FK_SubFrente_Frente" FOREIGN KEY ("IdFrente") REFERENCES conectabiz."Frente"("Id");

ALTER TABLE ONLY conectabiz."TicketGestorAsignacion"
    ADD CONSTRAINT "FK_TGA_Gestor" FOREIGN KEY ("IdGestor") REFERENCES conectabiz."Gestor"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."TicketGestorAsignacion"
    ADD CONSTRAINT "FK_TGA_GestorAsigno" FOREIGN KEY ("IdGestorAsigno") REFERENCES conectabiz."Gestor"("Id");

ALTER TABLE ONLY conectabiz."TicketGestorAsignacion"
    ADD CONSTRAINT "FK_TGA_GestorDesasigno" FOREIGN KEY ("IdGestorDesasigno") REFERENCES conectabiz."Gestor"("Id");

ALTER TABLE ONLY conectabiz."TicketGestorAsignacion"
    ADD CONSTRAINT "FK_TGA_Ticket" FOREIGN KEY ("IdTicket") REFERENCES conectabiz."Ticket"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."TicketConsultorAsignacion"
    ADD CONSTRAINT "FK_TicketConsultorAsignacion_Consultor" FOREIGN KEY ("IdConsultor") REFERENCES conectabiz."Consultor"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."TicketConsultorAsignacion"
    ADD CONSTRAINT "FK_TicketConsultorAsignacion_Ticket" FOREIGN KEY ("IdTicket") REFERENCES conectabiz."Ticket"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."TicketFrenteSubFrente"
    ADD CONSTRAINT "FK_TicketFrenteSubFrente_Frente" FOREIGN KEY ("IdFrente") REFERENCES conectabiz."Frente"("Id");

ALTER TABLE ONLY conectabiz."TicketFrenteSubFrente"
    ADD CONSTRAINT "FK_TicketFrenteSubFrente_SubFrente" FOREIGN KEY ("IdSubFrente") REFERENCES conectabiz."SubFrente"("Id");

ALTER TABLE ONLY conectabiz."TicketFrenteSubFrente"
    ADD CONSTRAINT "FK_TicketFrenteSubFrente_Ticket" FOREIGN KEY ("IdTicket") REFERENCES conectabiz."Ticket"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."TicketHistorialEstado"
    ADD CONSTRAINT "FK_TicketHistorial_Ticket" FOREIGN KEY ("IdTicket") REFERENCES conectabiz."Ticket"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."UserRolSocio"
    ADD CONSTRAINT "FK_URS_Rol" FOREIGN KEY ("IdRol") REFERENCES conectabiz."Rol"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY conectabiz."UserRolSocio"
    ADD CONSTRAINT "FK_URS_Socio" FOREIGN KEY ("IdSocio") REFERENCES conectabiz."Socio"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."UserRolSocio"
    ADD CONSTRAINT "FK_URS_User" FOREIGN KEY ("IdUser") REFERENCES conectabiz."User"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."User"
    ADD CONSTRAINT "FK_User_Persona" FOREIGN KEY ("IdPersona") REFERENCES conectabiz."Persona"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY conectabiz."User"
    ADD CONSTRAINT "FK_User_Socio" FOREIGN KEY ("IdSocio") REFERENCES conectabiz."Socio"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY conectabiz."Consultor"
    ADD CONSTRAINT fk_consultor_persona FOREIGN KEY ("PersonaId") REFERENCES conectabiz."Persona"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."Gestor"
    ADD CONSTRAINT fk_consultor_persona FOREIGN KEY ("PersonaId") REFERENCES conectabiz."Persona"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."Consultor"
    ADD CONSTRAINT fk_consultor_socio FOREIGN KEY ("IdSocio") REFERENCES conectabiz."Socio"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY conectabiz."Gestor"
    ADD CONSTRAINT fk_gestor_socio FOREIGN KEY ("IdSocio") REFERENCES conectabiz."Socio"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."RolPermisoModulo"
    ADD CONSTRAINT fk_modulo FOREIGN KEY ("IdModulo") REFERENCES conectabiz."Modulo"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY conectabiz."RolPermisoModulo"
    ADD CONSTRAINT fk_rol FOREIGN KEY ("IdRol") REFERENCES conectabiz."Rol"("Id") ON DELETE RESTRICT;

-- ============================================================
-- COMENTARIOS DE COLUMNAS (documentación original de la BD)
-- ============================================================

COMMENT ON COLUMN conectabiz."Parametro"."Valor1" IS 'Código de Estado a los que puede pasar el estado actual - Tipo Parametro = EstadoTicket';
COMMENT ON COLUMN conectabiz."Parametro"."Valor2" IS 'Código de Rol de los que pueden modificar el estado actual - Tipo Parametro = EstadoTicket';
