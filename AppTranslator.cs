namespace LTDHelper;

/// <summary>
/// Manages multi-language translations for the LTDHelper application
/// </summary>
public static class AppTranslator
{
    // Language indices: 0 = English, 1 = Spanish, 2 = Portuguese
    
    public static string[] WelcomeMessage = new string[3] 
    { 
        "Welcome |", 
        "Bienvenidx |", 
        "Bem-vindo |" 
    };

    public static string[] FullCommandsList = new string[3] 
    { 
        "Available commands: /start /stop /force /test /exit", 
        "Comandos disponibles: /iniciar /detener /forzar /probar /salir", 
        "Comando disponíveis: /começar /parar /forçar /testar /sair" 
    };

    public static string[] ReducedCommandsList = new string[3] 
    { 
        "Available commands: /stop /exit", 
        "Comandos disponibles: /detener /salir", 
        "Comandos disponíveis: /parar /sair" 
    };

    public static string[] BuyAdvice = new string[3] 
    { 
        "It is recommended to have the catalog closed and not be in a room.", 
        "Se recomienda dejar el catalogo cerrado y no estar en una sala.", 
        "É recomendado deixar o catálogo fechado e não estar em uma sala." 
    };

    public static string[] RiskAdvice = new string[3] 
    { 
        "Use at your own risk!", 
        "Usala bajo tu propio riesgo!", 
        "Use pelo seu próprio risco!" 
    };

    public static string[] PurchaseOK = new string[3] 
    { 
        "Successfully purchased an LTD |", 
        "Adquiriste exitosamente un LTD |", 
        "Comprou com sucesso um LTD |" 
    };

    public static string[] PurchaseFailed = new string[3] 
    { 
        "Error while purchasing an LTD!", 
        "Error al comprar un LTD!", 
        "Erro ao comprar um LTD!" 
    };

    public static string[] ExitAdvice = new string[3] 
    { 
        "Use /exit to finish.", 
        "Usa /exit para finalizar.", 
        "Use /exit para finalizar." 
    };

    public static string[] StartedMessage = new string[3] 
    { 
        "I will try to buy the LTD, you can use /stop to finish.", 
        "Intentare comprar un LTD, puedes usar /detener para finalizar.", 
        "Vou tentar comprar o LTD, você pode usar /parar para finalizar." 
    };

    public static string[] StoppedMessage = new string[3] 
    { 
        "Stopped, you can use /start to try again.", 
        "Detenida, puedes usar /iniciar para reintentar.", 
        "Parado, você pode usar /começar para tentar novamente." 
    };

    public static string[] CatalogIndexLoaded = new string[3] 
    { 
        "[Catalog index loaded]", 
        "[Indice del catalogo cargado]", 
        "[Índice do catálogo carregado]" 
    };

    public static string[] SimulatingPageClick = new string[3] 
    { 
        "[Simulating page click]", 
        "[Simulando clic de pagina]", 
        "[Simulando um clique de página]" 
    };

    public static string[] TryingToBuy = new string[3] 
    { 
        "[Trying to buy]", 
        "[Intentando comprar]", 
        "[Tentando comprar]" 
    };

    public static string[] CatalogUpdateReceived = new string[3] 
    { 
        "[Catalog update received]", 
        "[Actualizacion del catalogo recibida]", 
        "[Atualização do catálogo recebida]" 
    };

    public static string[] StopFailed = new string[3] 
    { 
        "Task cannot be stopped right now!", 
        "La tarea no puede detenerse ahora!", 
        "A tarefa não pode ser parada no momento!" 
    };

    /// <summary>
    /// Gets the appropriate language index based on current culture
    /// </summary>
    public static int GetLanguageIndex()
    {
        var cultureName = System.Globalization.CultureInfo.CurrentCulture.Name.ToLower();
        
        if (cultureName.StartsWith("es"))
            return 1; // Spanish
        if (cultureName.StartsWith("pt"))
            return 2; // Portuguese
        
        return 0; // English (default)
    }
}
