# Clínica - Microservicios

## Descripción

Proyecto desarrollado para la asignatura **Aplicaciones Distribuidas**.

El proyecto implementa una arquitectura de microservicios para la gestión de pacientes e historiales clínicos utilizando .NET, SQL Server, RabbitMQ, Docker y un API Gateway.

El sistema permite registrar pacientes, consultar y registrar historiales clínicos asociados, y comunica ambos servicios de forma asíncrona mediante mensajería con RabbitMQ.

---

## Tecnologías utilizadas

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- RabbitMQ
- Docker
- Docker Compose
- API Gateway con YARP
- Swagger
- Visual Studio

---

## Estructura del repositorio

```text
Clinica/
│
├── ApiGatewayB/
│   └── Proyecto .NET (YARP Reverse Proxy)
│
├── Paciente/
│   └── Paciente.Api/
│       └── Proyecto .NET
│
├── HistorialClinico/
│   └── HistorialClinico.Api/
│       └── Proyecto .NET
│
├── BaseDatos/
│   ├── PacienteDB.sql
│   └── HistorialClinicoDB.sql
│
├── docker-compose.yml
│
├── README.md
│
└── .gitignore
```

---

## Instrucciones de uso

### 1. Crear las bases de datos

Abrir SQL Server Management Studio y conectarse a la instancia local de SQL Server.

Ejecutar los archivos:

```text
BaseDatos/PacienteDB.sql
BaseDatos/HistorialClinicoDB.sql
```

Los scripts crearán las bases de datos:

```text
PacienteDB
HistorialClinicoDB
```

junto con sus respectivos usuarios (`usuario_paciente` y `usuario_HistorialClinico`), permisos de lectura/escritura, tablas y los registros necesarios para realizar las pruebas.

---

### 2. Revisar la configuración de conexión

Las cadenas de conexión hacia SQL Server y RabbitMQ están definidas como variables de entorno dentro de:

```text
docker-compose.yml
```

Los usuarios, contraseñas y nombres de las bases de datos configurados ahí deben coincidir exactamente con los creados en el paso 1.

---

### 3. Abrir Docker Desktop

Asegurarse de que Docker Desktop esté abierto y corriendo antes de continuar.

---

### 4. Ejecutar el proyecto con Docker Compose

Desde la carpeta raíz del proyecto (donde está `docker-compose.yml`), ejecutar en una terminal:

```text
docker compose up --build
```

Esto construye y levanta 4 contenedores:

```text
rabbitmq          -> http://localhost:15672  (usuario: admin / clave: admin123)
paciente          -> http://localhost:8083
historialclinico  -> http://localhost:8084
apigateway        -> http://localhost:8085
```

---

### 5. Probar las operaciones de la API

Las peticiones se pueden realizar directo a cada microservicio o a través del Api Gateway.

URL directas:

```text
http://localhost:8083/api/Pacientes
http://localhost:8084/api/HistorialClinicos
```

URL a través del Api Gateway:

```text
http://localhost:8085/api/Pacientes
http://localhost:8085/api/HistorialClinicos
```

Métodos disponibles en ambos controladores:

```text
GET     /api/{recurso}
GET     /api/{recurso}/{id}
POST    /api/{recurso}
PUT     /api/{recurso}/{id}
DELETE  /api/{recurso}/{id}
```

Adicional, en `HistorialClinicosController`:

```text
GET /api/HistorialClinicos/paciente/{idPaciente}
```

---

### 6. Comprobar la mensajería con RabbitMQ

Al registrar un paciente (`POST /api/Pacientes`), `Paciente.Api` publica un evento en la cola `paciente_creado`.

`HistorialClinico.Api` escucha esa cola en segundo plano y registra la recepción del evento.

Para verificarlo, abrir:

```text
http://localhost:15672
```

e ingresar a la pestaña **Queues** → **paciente_creado**.

---

## Orden de ejecución

Para ejecutar correctamente el proyecto se recomienda seguir este orden:

```text
1. Iniciar SQL Server
        ↓
2. Ejecutar BaseDatos/PacienteDB.sql
        ↓
3. Ejecutar BaseDatos/HistorialClinicoDB.sql
        ↓
4. Revisar docker-compose.yml
        ↓
5. Abrir Docker Desktop
        ↓
6. Ejecutar: docker compose up --build
        ↓
7. Probar los endpoints (directo o vía Api Gateway)
        ↓
8. Verificar la cola paciente_creado en RabbitMQ Management
```

---
## Autor

**Estudiante:** Nombre Apellido  
**Asignatura:** Programación Web I  
**Paralelo:** Tercero A Nocturno
