CREATE SCHEMA conectabiz;

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

CREATE TABLE conectabiz."EmailVerificationCode" (
    "Id" integer NOT NULL,
    "Email" character varying(100) NOT NULL,
    "Code" character varying(10) NOT NULL,
    "ExpiryDate" timestamp without time zone NOT NULL,
    "IsUsed" boolean DEFAULT false NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "UserId" integer
);

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

CREATE TABLE conectabiz."EmpresaGestorTipoTicket" (
    "Id" integer NOT NULL,
    "IdEmpresaGestor" integer NOT NULL,
    "IdTipoTicket" integer NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "FechaCreacion" timestamp without time zone DEFAULT now() NOT NULL,
    "FechaModificacion" timestamp without time zone,
    "UsuarioCreacion" character varying(50),
    "UsuarioModificacion" character varying(50)
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

CREATE TABLE conectabiz."Modulo" (
    "Id" integer NOT NULL,
    "Codigo" character varying(100) NOT NULL,
    "Nombre" character varying(150) NOT NULL,
    "Icono" character varying(100),
    "Ruta" character varying(150),
    "Activo" boolean DEFAULT true NOT NULL
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

CREATE TABLE conectabiz."PasswordResetToken" (
    "Id" integer NOT NULL,
    "Token" character varying(500) NOT NULL,
    "UserId" integer NOT NULL,
    "ExpiryDate" timestamp without time zone NOT NULL,
    "IsUsed" boolean DEFAULT false NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);

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

CREATE TABLE conectabiz."RefreshToken" (
    "Id" integer NOT NULL,
    "Token" character varying(255) NOT NULL,
    "ExpiryDate" timestamp without time zone NOT NULL,
    "IsRevoked" boolean NOT NULL,
    "UserId" integer NOT NULL
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

CREATE TABLE conectabiz."RolPermisoModulo" (
    "Id" integer NOT NULL,
    "IdRol" integer NOT NULL,
    "IdModulo" integer NOT NULL,
    "DivsOcultos" text,
    "ControlesBloqueados" text,
    "ControlesOcultos" text,
    "DivsBloqueados" text
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
    "IdTicketFrenteSubFrente" integer,
    "Rechazado" bool DEFAULT false NOT NULL,
    "MotivoRechazo" varchar(5000) NULL,
    "FechaRechazo" timestamp NULL
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

CREATE TABLE conectabiz."TicketHistorialEstado" (
    "Id" integer DEFAULT nextval('conectabiz."TicketHistorial_Id_seq"'::regclass) NOT NULL,
    "IdTicket" integer NOT NULL,
    "IdEstadoAnterior" integer,
    "IdEstadoNuevo" integer,
    "FechaCambio" timestamp without time zone DEFAULT now() NOT NULL,
    "UsuarioCambio" character varying(50)
);

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

CREATE SEQUENCE conectabiz."ConsultorFrenteSubFrente_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Consultor_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."DetallePlanificacionConsultor_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."DetalleTareasConsultor_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."EmailVerificationCode_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."EmpresaGestor_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."EmpresaGestorTipoTicket_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Empresa_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Feriados_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Frente_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."GestorFrenteSubFrente_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Gestor_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Modulo_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."NotificacionTicket_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Pais_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Parametro_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."PasswordResetToken_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Persona_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."RefreshToken_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."RolPermisoModulo_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Rol_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Socio_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."SubFrente_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."TicketConsultorAsignacion_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."TicketFrenteSubFrente_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."TicketGestorAsignacion_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."TicketHistorial_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."Ticket_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE conectabiz."User_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE conectabiz."ConsultorFrenteSubFrente_Id_seq" OWNED BY conectabiz."ConsultorFrenteSubFrente"."Id";

ALTER SEQUENCE conectabiz."Consultor_Id_seq" OWNED BY conectabiz."Consultor"."Id";

ALTER SEQUENCE conectabiz."DetallePlanificacionConsultor_Id_seq" OWNED BY conectabiz."DetallePlanificacionConsultor"."Id";

ALTER SEQUENCE conectabiz."DetalleTareasConsultor_Id_seq" OWNED BY conectabiz."DetalleTareasConsultor"."Id";

ALTER SEQUENCE conectabiz."EmailVerificationCode_Id_seq" OWNED BY conectabiz."EmailVerificationCode"."Id";

ALTER SEQUENCE conectabiz."EmpresaGestor_Id_seq" OWNED BY conectabiz."EmpresaGestor"."Id";

ALTER SEQUENCE conectabiz."EmpresaGestorTipoTicket_Id_seq" OWNED BY conectabiz."EmpresaGestorTipoTicket"."Id";

ALTER SEQUENCE conectabiz."Empresa_Id_seq" OWNED BY conectabiz."Empresa"."Id";

ALTER SEQUENCE conectabiz."Feriados_Id_seq" OWNED BY conectabiz."Feriados"."Id";

ALTER SEQUENCE conectabiz."Frente_Id_seq" OWNED BY conectabiz."Frente"."Id";

ALTER SEQUENCE conectabiz."GestorFrenteSubFrente_Id_seq" OWNED BY conectabiz."GestorFrenteSubFrente"."Id";

ALTER SEQUENCE conectabiz."Gestor_Id_seq" OWNED BY conectabiz."Gestor"."Id";

ALTER SEQUENCE conectabiz."Modulo_Id_seq" OWNED BY conectabiz."Modulo"."Id";

ALTER SEQUENCE conectabiz."NotificacionTicket_Id_seq" OWNED BY conectabiz."NotificacionTicket"."Id";

ALTER SEQUENCE conectabiz."Pais_Id_seq" OWNED BY conectabiz."Pais"."Id";

ALTER SEQUENCE conectabiz."Parametro_Id_seq" OWNED BY conectabiz."Parametro"."Id";

ALTER SEQUENCE conectabiz."PasswordResetToken_Id_seq" OWNED BY conectabiz."PasswordResetToken"."Id";

ALTER SEQUENCE conectabiz."Persona_Id_seq" OWNED BY conectabiz."Persona"."Id";

ALTER SEQUENCE conectabiz."RefreshToken_Id_seq" OWNED BY conectabiz."RefreshToken"."Id";

ALTER SEQUENCE conectabiz."RolPermisoModulo_Id_seq" OWNED BY conectabiz."RolPermisoModulo"."Id";

ALTER SEQUENCE conectabiz."Rol_Id_seq" OWNED BY conectabiz."Rol"."Id";

ALTER SEQUENCE conectabiz."Socio_Id_seq" OWNED BY conectabiz."Socio"."Id";

ALTER SEQUENCE conectabiz."SubFrente_Id_seq" OWNED BY conectabiz."SubFrente"."Id";

ALTER SEQUENCE conectabiz."TicketConsultorAsignacion_Id_seq" OWNED BY conectabiz."TicketConsultorAsignacion"."Id";

ALTER SEQUENCE conectabiz."TicketFrenteSubFrente_Id_seq" OWNED BY conectabiz."TicketFrenteSubFrente"."Id";

ALTER SEQUENCE conectabiz."TicketGestorAsignacion_Id_seq" OWNED BY conectabiz."TicketGestorAsignacion"."Id";

ALTER SEQUENCE conectabiz."Ticket_Id_seq" OWNED BY conectabiz."Ticket"."Id";

ALTER SEQUENCE conectabiz."User_Id_seq" OWNED BY conectabiz."User"."Id";

ALTER TABLE ONLY conectabiz."Consultor" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Consultor_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."ConsultorFrenteSubFrente" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."ConsultorFrenteSubFrente_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."DetallePlanificacionConsultor" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."DetallePlanificacionConsultor_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."DetalleTareasConsultor" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."DetalleTareasConsultor_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."EmailVerificationCode" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."EmailVerificationCode_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Empresa" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Empresa_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."EmpresaGestor" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."EmpresaGestor_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."EmpresaGestorTipoTicket" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."EmpresaGestorTipoTicket_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Feriados" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Feriados_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Frente" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Frente_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Gestor" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Gestor_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."GestorFrenteSubFrente" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."GestorFrenteSubFrente_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Modulo" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Modulo_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."NotificacionTicket" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."NotificacionTicket_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Pais" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Pais_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Parametro" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Parametro_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."PasswordResetToken" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."PasswordResetToken_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Persona" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Persona_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."RefreshToken" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."RefreshToken_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Rol" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Rol_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."RolPermisoModulo" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."RolPermisoModulo_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Socio" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Socio_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."SubFrente" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."SubFrente_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."Ticket" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."Ticket_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."TicketConsultorAsignacion" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."TicketConsultorAsignacion_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."TicketFrenteSubFrente" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."TicketFrenteSubFrente_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."TicketGestorAsignacion" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."TicketGestorAsignacion_Id_seq"'::regclass);

ALTER TABLE ONLY conectabiz."User" ALTER COLUMN "Id" SET DEFAULT nextval('conectabiz."User_Id_seq"'::regclass);

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

ALTER TABLE ONLY conectabiz."EmpresaGestorTipoTicket"
    ADD CONSTRAINT "EmpresaGestorTipoTicket_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "Empresa_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Feriados"
    ADD CONSTRAINT "Feriados_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Gestor"
    ADD CONSTRAINT "Gestor_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."Modulo"
    ADD CONSTRAINT "Modulo_Codigo_key" UNIQUE ("Codigo");

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
    ADD CONSTRAINT "Pais_Codigo_key" UNIQUE ("Codigo");

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
    ADD CONSTRAINT "Rol_Codigo_key" UNIQUE ("Codigo");

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

ALTER TABLE ONLY conectabiz."Frente"
    ADD CONSTRAINT "UK_Frente_Codigo" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."SubFrente"
    ADD CONSTRAINT "UK_SubFrente_Codigo" UNIQUE ("Codigo");

ALTER TABLE ONLY conectabiz."Ticket"
    ADD CONSTRAINT "UK_Ticket_CodTicket" UNIQUE ("CodTicket");

ALTER TABLE ONLY conectabiz."Empresa"
    ADD CONSTRAINT "UQ_Empresa_NumDocContribuyente_IdPais_IdSocio" UNIQUE ("NumDocContribuyente", "IdPais", "IdSocio");

ALTER TABLE ONLY conectabiz."UserRolSocio"
    ADD CONSTRAINT "UserRolSocio_pkey" PRIMARY KEY ("IdUser", "IdRol", "IdSocio");

ALTER TABLE ONLY conectabiz."User"
    ADD CONSTRAINT "User_pkey" PRIMARY KEY ("Id");

ALTER TABLE ONLY conectabiz."RolPermisoModulo"
    ADD CONSTRAINT uq_rol_modulo UNIQUE ("IdRol", "IdModulo");

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

ALTER TABLE ONLY conectabiz."EmpresaGestorTipoTicket"
    ADD CONSTRAINT "FK_EGTT_EmpresaGestor" FOREIGN KEY ("IdEmpresaGestor") REFERENCES conectabiz."EmpresaGestor"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY conectabiz."EmpresaGestorTipoTicket"
    ADD CONSTRAINT "FK_EGTT_Parametro" FOREIGN KEY ("IdTipoTicket") REFERENCES conectabiz."Parametro"("Id") ON DELETE CASCADE;

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


CREATE INDEX "IX_CFSF_ConsultorId" ON conectabiz."ConsultorFrenteSubFrente" USING btree ("ConsultorId");

CREATE INDEX "IX_CFSF_IdFrente" ON conectabiz."ConsultorFrenteSubFrente" USING btree ("IdFrente");

CREATE INDEX "IX_CFSF_IdSubFrente" ON conectabiz."ConsultorFrenteSubFrente" USING btree ("IdSubFrente");

CREATE INDEX "IX_DPC_IdTFSF_FechaInicio" ON conectabiz."DetallePlanificacionConsultor" USING btree ("IdTicketFrenteSubFrente", "FechaInicio") WHERE ("Activo" = true);

CREATE INDEX "IX_DPC_IdTicketConsultorAsignacion" ON conectabiz."DetallePlanificacionConsultor" USING btree ("IdTicketConsultorAsignacion") WHERE ("Activo" = true);

CREATE INDEX "IX_DPC_IdTicketFrenteSubFrente" ON conectabiz."DetallePlanificacionConsultor" USING btree ("IdTicketFrenteSubFrente") WHERE ("Activo" = true);

CREATE INDEX "IX_DTC_IdTCA_FechaInicio" ON conectabiz."DetalleTareasConsultor" USING btree ("IdTicketConsultorAsignacion", "FechaInicio") WHERE ("Activo" = true);

CREATE INDEX "IX_DTC_IdTicketConsultorAsignacion" ON conectabiz."DetalleTareasConsultor" USING btree ("IdTicketConsultorAsignacion") WHERE ("Activo" = true);

CREATE INDEX "IX_EmailVerificationCode_Code" ON conectabiz."EmailVerificationCode" USING btree ("Code");

CREATE INDEX "IX_EmailVerificationCode_Email" ON conectabiz."EmailVerificationCode" USING btree ("Email");

CREATE INDEX "IX_Empresa_Activo" ON conectabiz."Empresa" USING btree ("Activo");

CREATE INDEX "IX_Empresa_IdGestor" ON conectabiz."Empresa" USING btree ("IdGestor");

CREATE INDEX "IX_Frente_Activo" ON conectabiz."Frente" USING btree ("Activo");

CREATE INDEX "IX_GFSF_IdFrente" ON conectabiz."GestorFrenteSubFrente" USING btree ("IdFrente");

CREATE INDEX "IX_GFSF_IdGestor" ON conectabiz."GestorFrenteSubFrente" USING btree ("IdGestor");

CREATE INDEX "IX_GFSF_IdSubFrente" ON conectabiz."GestorFrenteSubFrente" USING btree ("IdSubFrente");

CREATE INDEX "IX_NotificacionTicket_IdTicket" ON conectabiz."NotificacionTicket" USING btree ("IdTicket");

CREATE INDEX "IX_NotificacionTicket_IdUser" ON conectabiz."NotificacionTicket" USING btree ("IdUser");

CREATE INDEX "IX_NotificacionTicket_User_Leido" ON conectabiz."NotificacionTicket" USING btree ("IdUser", "Leido");

CREATE INDEX "IX_Parametro_Activo" ON conectabiz."Parametro" USING btree ("Activo");

CREATE INDEX "IX_Parametro_TipoParametro" ON conectabiz."Parametro" USING btree ("TipoParametro");

CREATE INDEX "IX_SubFrente_Activo" ON conectabiz."SubFrente" USING btree ("Activo");

CREATE INDEX "IX_SubFrente_IdFrente" ON conectabiz."SubFrente" USING btree ("IdFrente");

CREATE INDEX "IX_TCA_IdConsultor" ON conectabiz."TicketConsultorAsignacion" USING btree ("IdConsultor") WHERE ("Activo" = true);

CREATE INDEX "IX_TCA_IdTicket" ON conectabiz."TicketConsultorAsignacion" USING btree ("IdTicket") WHERE ("Activo" = true);

CREATE INDEX "IX_TCA_IdTicketFrenteSubFrente" ON conectabiz."TicketConsultorAsignacion" USING btree ("IdTicketFrenteSubFrente") WHERE ("Activo" = true);

CREATE INDEX "IX_TGA_IdGestor" ON conectabiz."TicketGestorAsignacion" USING btree ("IdGestor");

CREATE INDEX "IX_TGA_IdTicket" ON conectabiz."TicketGestorAsignacion" USING btree ("IdTicket");

CREATE INDEX "IX_Ticket_IdEmpresa" ON conectabiz."Ticket" USING btree ("IdEmpresa");

CREATE INDEX "IX_Ticket_IdEstadoTicket" ON conectabiz."Ticket" USING btree ("IdEstadoTicket");

CREATE INDEX "IX_URS_Socio" ON conectabiz."UserRolSocio" USING btree ("IdSocio");

CREATE INDEX "IX_URS_User_Socio" ON conectabiz."UserRolSocio" USING btree ("IdUser", "IdSocio");

CREATE UNIQUE INDEX "UX_CFSF_ConsultorId_Frente_SubFrente_Activo" ON conectabiz."ConsultorFrenteSubFrente" USING btree ("ConsultorId", "IdFrente", "IdSubFrente") WHERE ("Activo" = true);

CREATE UNIQUE INDEX "UX_EG_IdEmpresa_IdGestor_Activo" ON conectabiz."EmpresaGestor" USING btree ("IdEmpresa", "IdGestor") WHERE ("Activo" = true);

CREATE UNIQUE INDEX "UX_EG_IdEmpresa_Principal_Activo" ON conectabiz."EmpresaGestor" USING btree ("IdEmpresa") WHERE (("Activo" = true) AND ("EsPrincipal" = true));

CREATE UNIQUE INDEX "UX_EGTT_EmpresaGestor_TipoTicket_Activo" ON conectabiz."EmpresaGestorTipoTicket" USING btree ("IdEmpresaGestor", "IdTipoTicket") WHERE ("Activo" = true);

CREATE INDEX "IX_EGTT_IdEmpresaGestor" ON conectabiz."EmpresaGestorTipoTicket" USING btree ("IdEmpresaGestor") WHERE ("Activo" = true);

CREATE UNIQUE INDEX "UX_GFSF_IdGestor_IdFrente_IdSubFrente_Activo" ON conectabiz."GestorFrenteSubFrente" USING btree ("IdGestor", "IdFrente", "IdSubFrente") WHERE ("Activo" = true);

CREATE UNIQUE INDEX "UX_Parametro_Tipo_Codigo_Activo" ON conectabiz."Parametro" USING btree ("TipoParametro", "Codigo") WHERE ("Activo" = true);

CREATE UNIQUE INDEX "UX_TGA_IdTicket_IdGestor_Activo" ON conectabiz."TicketGestorAsignacion" USING btree ("IdTicket", "IdGestor") WHERE ("Activo" = true);