# PLAN DE IMPLEMENTACIÓN — Zoapex

> **Documento de especificación para desarrollo asistido por agente de IA.**
> Proyecto académico — Arquitectura en N Capas con C# / WPF / Supabase (PostgreSQL).
> Curso: Desarrollo de Aplicaciones Básico — Práctica 05 (Arquitectura N Capas).

---

## 1. RESUMEN DEL PROYECTO

**Zoapex** es una aplicación de escritorio de e-commerce para una tienda de mascotas. El prototipo se centra en el **catálogo de productos y la venta de los mismos**, pero la arquitectura se diseña para ser escalable a un sistema profesional completo.

- **Tipo de aplicación:** Escritorio (WPF, .NET).
- **Patrón arquitectónico:** N Capas (4 proyectos independientes referenciados unidireccionalmente).
- **Base de datos:** PostgreSQL alojada en Supabase (la nube).
- **Desarrollador:** 1 persona.
- **Objetivo del prototipo:** Funcional y demostrable, no necesita estar 100% completo.

### Convención de nombres (IMPORTANTE)
| Elemento | Idioma | Ejemplo |
|---|---|---|
| Archivos `.cs`, `.xaml` | **Inglés** | `ProductDAL.cs`, `CartView.xaml` |
| Clases, métodos, variables, propiedades | **Inglés** | `GetAllProducts()`, `unitPrice` |
| Tablas y columnas de la base de datos | **Inglés** | `product`, `min_stock` |
| Funciones de PostgreSQL | **Inglés** | `fn_list_products()` |
| Comentarios en el código | **Español** | `// Valida que el precio sea mayor a cero` |

### Objetivo de la evaluación
Cumplir los criterios de la guía de la Práctica 05:
1. Separación estricta en 4 capas (Entities, DataAccess, Business, Presentation).
2. CRUD completo apoyado en funciones de base de datos (equivalente a stored procedures).
3. Validaciones ubicadas en la capa de Business.
4. Referencias entre proyectos unidireccionales (GUI → BL → DAL → BE).

---

## 2. STACK TECNOLÓGICO

| Componente | Tecnología | Notas |
|---|---|---|
| Lenguaje | C# | Requisito de la evaluación |
| Framework | .NET 8 o superior | El más alto disponible |
| Interfaz | WPF (Windows Presentation Foundation) | Capa de presentación |
| Base de datos | PostgreSQL (Supabase) | En la nube |
| Driver de BD | **Npgsql** (paquete NuGet) | Reemplaza a `Microsoft.Data.SqlClient` |
| Configuración | `appsettings.json` | Cadena de conexión |
| IDE | Visual Studio 2026 | — |

> **Nota clave:** Como usamos PostgreSQL en lugar de SQL Server, todos los objetos `Sql*` de ADO.NET se reemplazan por sus equivalentes `Npgsql*`:
> `SqlConnection → NpgsqlConnection`, `SqlCommand → NpgsqlCommand`, `SqlDataAdapter → NpgsqlDataAdapter`. La arquitectura en N Capas se mantiene idéntica.

---

## 3. ARQUITECTURA — LAS 4 CAPAS

```
┌────────────────────────────────────┐
│   Zoapex.Presentation (WPF)       │  ← Ventanas, vistas, interacción con el usuario
└─────────────────┬──────────────────┘
                  │ usa
┌─────────────────▼──────────────────┐
│   Zoapex.Business (BL)            │  ← Validaciones y reglas de negocio
└─────────────────┬──────────────────┘
                  │ usa
┌─────────────────▼──────────────────┐
│   Zoapex.DataAccess (DAL)         │  ← Conexión a Supabase con Npgsql
└─────────────────┬──────────────────┘
                  │ usa
┌─────────────────▼──────────────────┐
│   Zoapex.Entities (BE)            │  ← Clases POCO (núcleo, no referencia a nadie)
└────────────────────────────────────┘
```

### Reglas de referencias (estrictas)
| Proyecto | Referencia a |
|---|---|
| `Zoapex.Entities` | nadie (es el núcleo) |
| `Zoapex.DataAccess` | `Zoapex.Entities` |
| `Zoapex.Business` | `Zoapex.DataAccess`, `Zoapex.Entities` |
| `Zoapex.Presentation` | `Zoapex.Business`, `Zoapex.Entities` |

> ⚠️ La capa de Presentation **NUNCA** referencia directamente a DataAccess. Toda comunicación con la base de datos pasa por Business.

---

## 4. ESTRUCTURA DE LA SOLUCIÓN

```
Zoapex/                              (solución .sln)
│
├── Zoapex.Entities/                        (Biblioteca de clases — núcleo)
│   ├── CategoryEntity.cs                    (representa la tabla category)
│   ├── ProductEntity.cs                     (representa la tabla product)
│   ├── CustomerEntity.cs                    (representa la tabla customer)
│   ├── OrderEntity.cs                       (representa la tabla order)
│   └── OrderDetailEntity.cs                 (representa la tabla order_detail)
│
├── Zoapex.DataAccess/                      (Biblioteca de clases + NuGet Npgsql)
│   ├── DatabaseConnection.cs                (lee y entrega la cadena de conexión)
│   ├── CategoryDAL.cs                       (operaciones de base de datos para categorías)
│   ├── ProductDAL.cs                        (operaciones de base de datos para productos)
│   └── OrderDAL.cs                          (operaciones de base de datos para pedidos)
│
├── Zoapex.Business/                        (Biblioteca de clases)
│   ├── CategoryBL.cs                        (validaciones y lógica de categorías)
│   ├── ProductBL.cs                         (validaciones y lógica de productos)
│   └── OrderBL.cs                           (validaciones, cálculo de totales, lógica de pedido)
│
└── Zoapex.Presentation/                    (Aplicación WPF — proyecto de inicio)
    ├── App.xaml / App.xaml.cs
    ├── appsettings.json                      (cadena de conexión — NO subir a repositorio público)
    ├── Views/
    │   ├── MainWindow.xaml                   (ventana principal con navegación)
    │   ├── CatalogView.xaml                  (catálogo + CRUD de productos)
    │   └── CartView.xaml                     (carrito + registro de venta)
    └── ViewModels/                           (opcional — si se implementa patrón MVVM)
        ├── CatalogViewModel.cs
        └── CartViewModel.cs
```

> **Recomendación de escalabilidad:** Para la GUI se puede usar **code-behind** simple (más rápido para el prototipo) o un **MVVM ligero**. Para un proyecto profesional y escalable se recomienda MVVM, pero no es obligatorio para el prototipo. Empezar con code-behind y migrar a MVVM en una fase posterior es válido.

---

## 5. BASE DE DATOS (Supabase / PostgreSQL)

### 5.1 Tablas del prototipo

**`category`**
| Columna | Tipo | Notas |
|---|---|---|
| category_id | SERIAL PK | autonumérico |
| name | VARCHAR(50) | obligatorio |
| description | VARCHAR(150) | — |
| status | SMALLINT | 1 = activo, 0 = inactivo |

**`product`**
| Columna | Tipo | Notas |
|---|---|---|
| product_id | SERIAL PK | autonumérico |
| code | VARCHAR(10) | autogenerado tipo P001, P002... |
| name | VARCHAR(80) | obligatorio |
| description | VARCHAR(200) | — |
| price | NUMERIC(10,2) | debe ser > 0 |
| stock | INT | debe ser >= 0 |
| min_stock | INT | debe ser >= 0 |
| category_id | INT FK → category | obligatorio |
| image_url | VARCHAR(300) | URL de imagen del producto |
| status | SMALLINT | 1 = activo, 0 = inactivo |
| registered_at | TIMESTAMP | default now() |

**`customer`**
| Columna | Tipo | Notas |
|---|---|---|
| customer_id | SERIAL PK | — |
| first_name | VARCHAR(80) | obligatorio |
| last_name | VARCHAR(80) | obligatorio |
| email | VARCHAR(120) | único |
| phone | VARCHAR(15) | — |
| address | VARCHAR(200) | — |
| status | SMALLINT | 1 / 0 |

**`order`**
| Columna | Tipo | Notas |
|---|---|---|
| order_id | SERIAL PK | — |
| code | VARCHAR(12) | autogenerado tipo ORD001... |
| customer_id | INT FK → customer | — |
| order_date | TIMESTAMP | default now() |
| subtotal | NUMERIC(10,2) | — |
| tax | NUMERIC(10,2) | IGV 18% |
| total | NUMERIC(10,2) | subtotal + tax |
| status | SMALLINT | 1 = registrado |

**`order_detail`**
| Columna | Tipo | Notas |
|---|---|---|
| detail_id | SERIAL PK | — |
| order_id | INT FK → order | — |
| product_id | INT FK → product | — |
| quantity | INT | debe ser > 0 |
| unit_price | NUMERIC(10,2) | precio al momento de la venta |
| subtotal | NUMERIC(10,2) | quantity × unit_price |

### 5.2 Funciones de base de datos (equivalente a Stored Procedures)

En PostgreSQL se usan **funciones** en lugar de procedimientos almacenados. Crear las siguientes:

| Función | Propósito |
|---|---|
| `fn_list_categories()` | Lista categorías activas (para combos desplegables) |
| `fn_list_products()` | Lista todos los productos activos con el nombre de su categoría |
| `fn_get_product(p_id INT)` | Devuelve un producto específico por su ID |
| `fn_insert_product(...)` | Inserta un producto y autogenera el código P001, P002... |
| `fn_update_product(...)` | Actualiza los datos de un producto existente |
| `fn_delete_product(p_id INT)` | Eliminación lógica: pone status = 0, no borra el registro |
| `fn_register_order(...)` | Registra pedido + detalle en una transacción, descuenta stock |

> **Datos de prueba:** Insertar al menos 4 categorías (Food, Toys, Accessories, Hygiene) y 8 productos de muestra para poder demostrar el catálogo durante la evaluación.

### 5.3 Cadena de conexión (Supabase)

Supabase entrega la cadena en `Project Settings → Database`. Formato para Npgsql:

```
Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<YOUR_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true
```

Guardar esta cadena en `appsettings.json` dentro del proyecto `Zoapex.Presentation`. **No subir la contraseña a repositorios públicos — usar variables de entorno o marcador de posición.**

---

## 6. RESPONSABILIDAD DE CADA CAPA

### 6.1 Zoapex.Entities (BE)
Clases POCO: solo propiedades, sin lógica. Una clase por tabla. Son los objetos que viajan entre todas las capas.

```csharp
// Ejemplo de clase POCO — solo propiedades, sin métodos ni validaciones
public class ProductEntity
{
    public int ProductId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }  // campo de la vista JOIN
    public string ImageUrl { get; set; }
    public int Status { get; set; }
}
```

### 6.2 Zoapex.DataAccess (DAL)
- `DatabaseConnection.cs`: lee la cadena de conexión del `appsettings.json` y retorna un `NpgsqlConnection` listo para usar.
- Una clase DAL por entidad principal (`ProductDAL`, `CategoryDAL`, `OrderDAL`).
- Cada método invoca una función de PostgreSQL mediante `NpgsqlCommand`.
- **No contiene validaciones ni lógica de negocio.** Solo ejecuta y retorna datos.
- Instalar el paquete NuGet **`Npgsql`** SOLO en este proyecto.

```csharp
// Ejemplo de método DAL — solo llama a la función de base de datos
public DataTable GetAllProducts()
{
    using (NpgsqlConnection cnx = new NpgsqlConnection(_connection.GetConnectionString()))
    {
        // Invoca la función de PostgreSQL que lista todos los productos activos
        NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM fn_list_products()", cnx);
        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
        DataTable table = new DataTable();
        adapter.Fill(table);
        return table;
    }
}
```

### 6.3 Zoapex.Business (BL)
- Una clase BL por entidad. Recibe llamadas de la Presentation y delega en la DAL.
- Contiene **todas las validaciones**: campos obligatorios, precio > 0, stock no negativo, longitud de textos, cantidad del carrito > 0, stock disponible suficiente, etc.
- Calcula totales del pedido (subtotal, IGV 18%, total).

```csharp
// Ejemplo de validación en la capa de negocio
private void ValidateProduct(ProductEntity product)
{
    // Verifica que el nombre no esté vacío
    if (string.IsNullOrWhiteSpace(product.Name))
        throw new Exception("El nombre del producto es obligatorio.");

    // Verifica que el precio sea mayor a cero
    if (product.Price <= 0)
        throw new Exception("El precio debe ser mayor que cero.");

    // Verifica que el stock no sea negativo
    if (product.Stock < 0)
        throw new Exception("El stock no puede ser negativo.");

    // Verifica que se haya seleccionado una categoría
    if (product.CategoryId <= 0)
        throw new Exception("Debe seleccionar una categoría.");
}
```

### 6.4 Zoapex.Presentation (WPF)
- `MainWindow`: navegación principal entre vistas.
- `CatalogView`: muestra los productos en tarjetas o grilla; permite CRUD de productos y agregar al carrito.
- `CartView`: lista los productos seleccionados, permite modificar cantidades y registrar la venta.
- **Solo conoce la capa Business.** Nunca accede directamente a DataAccess ni a la base de datos.

---

## 7. FASES DE IMPLEMENTACIÓN

> Ejecutar en orden estricto. Compilar después de cada capa antes de pasar a la siguiente.

### Fase 0 — Preparación del entorno
- [ ] Crear proyecto en Supabase llamado `Zoapex`.
- [ ] Obtener y guardar la cadena de conexión de `Project Settings → Database`.

### Fase 1 — Base de datos
- [ ] Crear las 5 tablas en el SQL Editor de Supabase.
- [ ] Crear las 7 funciones de PostgreSQL.
- [ ] Insertar datos de prueba (categorías + productos).
- [ ] Verificar ejecutando: `SELECT * FROM fn_list_products();` — debe retornar filas.

### Fase 2 — Solución y proyectos en Visual Studio
- [ ] Crear solución en blanco `Zoapex`.
- [ ] Agregar los 3 proyectos de biblioteca de clases (`Entities`, `DataAccess`, `Business`).
- [ ] Agregar el proyecto WPF (`Presentation`) y marcarlo como **proyecto de inicio**.
- [ ] Configurar las referencias unidireccionales (ver sección 3).
- [ ] Instalar el NuGet `Npgsql` SOLO en `Zoapex.DataAccess`.

### Fase 3 — Capa Entities
- [ ] Crear las 5 clases POCO (`CategoryEntity`, `ProductEntity`, `CustomerEntity`, `OrderEntity`, `OrderDetailEntity`).
- [ ] Compilar — debe quedar sin errores antes de continuar.

### Fase 4 — Capa DataAccess
- [ ] Crear `DatabaseConnection.cs`.
- [ ] Crear `ProductDAL.cs`, `CategoryDAL.cs`, `OrderDAL.cs` con todos sus métodos.
- [ ] Compilar.

### Fase 5 — Capa Business
- [ ] Crear `ProductBL.cs`, `CategoryBL.cs`, `OrderBL.cs` con validaciones.
- [ ] Compilar.

### Fase 6 — Capa Presentation (WPF)
- [ ] Configurar `appsettings.json` con la cadena de conexión.
- [ ] Construir `MainWindow.xaml` con navegación.
- [ ] Construir `CatalogView.xaml` (listar productos + CRUD).
- [ ] Construir `CartView.xaml` (carrito + registrar venta).
- [ ] Compilar y ejecutar.

### Fase 7 — Pruebas mínimas
- [ ] El catálogo muestra los productos desde Supabase.
- [ ] Crear, editar y eliminar un producto funciona correctamente.
- [ ] Una validación falla correctamente (ej. precio en 0 → mensaje de error visible).
- [ ] Agregar productos al carrito y registrar una venta descuenta el stock en la base de datos.

---

## 8. ALCANCE: PROTOTIPO vs. VERSIÓN FUTURA

### Incluido en el prototipo (MVP)
- Gestión de categorías (listar para combos).
- CRUD completo de productos.
- Catálogo visual de productos.
- Carrito de compras.
- Registro de venta/pedido con descuento de stock automático.

### Fuera del prototipo (fases futuras — arquitectura ya preparada para ello)
- Login y roles de usuario (administrador / vendedor).
- Gestión completa de clientes.
- Reportes de ventas y dashboards.
- Citas de grooming / peluquería.
- Registro de mascotas del cliente.
- Imágenes subidas a Supabase Storage.

> La base de datos y la arquitectura ya contemplan estas extensiones (tabla `customer`, campos de auditoría, capas separadas), por lo que agregarlas no requiere rehacer lo existente.

---

## 9. CHECKLIST DE CRITERIOS DE EVALUACIÓN

- [ ] Solución con 4 proyectos separados (Entities, DataAccess, Business, Presentation).
- [ ] Referencias unidireccionales correctas.
- [ ] Base de datos con tablas y funciones PostgreSQL (equivalente a stored procedures).
- [ ] CRUD completo de productos funcionando.
- [ ] Validaciones ubicadas exclusivamente en la capa Business.
- [ ] La capa Presentation no accede directamente a la base de datos.
- [ ] El proyecto compila y ejecuta sin errores.
- [ ] Elemento diferenciador: base de datos en la nube (Supabase), interfaz WPF e inglés en el código.

---

## 10. INSTRUCCIONES DE DESARROLLO

1. Generar primero el **script SQL completo** (tablas + funciones + datos de prueba) y ejecutarlo en el SQL Editor de Supabase antes de abrir Visual Studio.
2. Crear la solución y los 4 proyectos con la estructura exacta de la sección 4.
3. Implementar capa por capa en el orden de las fases 3 a 6, compilando entre cada una.
4. Para la GUI WPF, priorizar funcionalidad sobre estética en el prototipo, pero mantener un diseño limpio y ordenado.
5. No incluir credenciales reales en el código; usar `appsettings.json` con un marcador de posición para la contraseña.
6. **Nombres de archivos, clases, métodos y variables siempre en inglés. Comentarios en el código siempre en español.**

---

*Fin del plan de implementación — Zoapex.*
