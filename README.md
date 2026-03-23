# LTDHelper v2.0

**Limited Auto-Buyer for Habbo using G-Earth/Geode**

## 📋 Descripción

LTDHelper es una extensión de G-Earth que compra automáticamente 2 items limitados en Habbo. Una vez comprados los 2 limitados, la extensión se bloquea automáticamente.

## ✨ Características

- ✅ Compra automáticamente **2 LTDs**
- ✅ Se bloquea después de la 2da compra
- ✅ Multiidioma: **Inglés**, **Español**, **Portugués**
- ✅ Integración completa con **Geode/G-Earth**
- ✅ Solo permite una instancia a la vez
- ✅ Código moderno (.NET 8)

## 🎮 Comandos

| Comando | Alias | Descripción |
|---------|-------|-------------|
| `/start` | `/iniciar`, `/começar` | Inicia la compra automática |
| `/stop` | `/detener`, `/parar` | Detiene la compra |
| `/force` | `/forzar`, `/forçar` | Fuerza compra inmediata |
| `/test` | `/probar`, `/testar` | Modo test (compra 1) |
| `/exit` | `/salir`, `/sair` | Cierra la extensión |

## 📦 Requisitos

- **Windows 10/11**
- **.NET 8 Runtime** o superior
- **G-Earth** instalado
- **Geode.dll** disponible

## 🚀 Instalación

1. Clona el repositorio:
```bash
git clone https://github.com/Bastiian787/LTDHelper.git
cd LTDHelper
```

2. Restaura los paquetes:
```bash
dotnet restore
```

3. Compila el proyecto:
```bash
dotnet build
```

4. Ejecuta:
```bash
dotnet run
```

## 📖 Cómo usar

1. Abre Habbo y conecta G-Earth
2. Ejecuta LTDHelper
3. Abre el chat y usa `/start` para iniciar
4. La extensión comprará automáticamente 2 LTDs
5. Después del 2do LTD, usa `/exit` para cerrar

## ⚠️ Advertencias

- **Úsalo bajo tu propio riesgo**
- Se recomienda tener el catálogo cerrado
- No estés en una sala cuando compres
- Habbo puede considerar esto contra sus términos

## 🔧 Estructura del Proyecto

```
LTDHelper/
├── Program.cs              # Entry point con Mutex
├── Application.xaml        # Aplicación WPF
├── Application.xaml.cs     # Code-behind
├── MainWindow.xaml         # Interfaz gráfica
├── MainWindow.xaml.cs      # Lógica principal
├── AppTranslator.cs        # Traducciones
├── LTDHelper.csproj        # Configuración del proyecto
├── .gitignore              # Archivos ignorados
└── README.md               # Este archivo
```

## 📝 Changelog

### v2.0.0
- ✅ Reescrito en .NET 8
- ✅ Soporte para compra de 2 LTDs
- ✅ Bloqueo automático después de 2 compras
- ✅ Interfaz mejorada
- ✅ Mejor manejo de errores

## 👨‍💻 Autor

**Bastiian787**

## 📄 Licencia

Este proyecto está bajo licencia MIT. Ver archivo `LICENSE` para más detalles.

---

⭐ Si te fue útil, déjale una estrella al repositorio
