using Microsoft.EntityFrameworkCore;

namespace Game.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(GameDbContext context)
    {
        // Avoid duplicate seeds if data is already present
        if (await context.Words.AnyAsync()) return;

        var seedWords = new List<Word>();

        // =========================================================================
        // 1. ENGLISH (50 Beginner, 80 Intermediate per topic)
        // =========================================================================
        
        // --- Car Parts: Beginner (50) ---
        var enCarBeg = new[] {
            "WHEEL", "BRAKE", "GEAR", "DOOR", "SEAT", "TIRE", "HOOD", "ROOF", "PIPE", "TANK",
            "LAMP", "TYRE", "BOOT", "WIND", "LOCK", "FUEL", "KEYS", "PUMP", "BELT", "RING",
            "JACK", "HOSE", "PLUG", "WIRE", "FUSE", "VENT", "DASH", "DIAL", "GRILLE", "BODY",
            "DECAL", "MATS", "RACK", "HORN", "BOLT", "NUT", "CAP", "RIM", "VALVE", "STEM",
            "AXLE", "SEAL", "CLIP", "BAR", "ROD", "BEAM", "LINER", "GLASS", "MIRROR", "TINT"
        };
        foreach (var w in enCarBeg) seedWords.Add(new Word { Language = "english", Difficulty = "beginner", Topic = "car-parts", Text = w });

        // --- Car Parts: Intermediate (80) ---
        var enCarInt = new[] {
            "ENGINE", "BATTERY", "MUFFLER", "RADIATOR", "CLUTCH", "PISTON", "GASKET", "SPARK",
            "EXHAUST", "CHASSIS", "BUMPER", "FENDER", "SPOILER", "CARBURETOR", "ALTERNATOR", "IGNITION",
            "STARTER", "FLYWHEEL", "CALIPER", "ROTOR", "SUSPENSION", "SPRING", "STRUT", "MANIFOLD",
            "BEARING", "BUSHING", "CAMSHAFT", "CRANKSHAFT", "INJECTOR", "THROTTLE", "SUPERCHARGER", "TURBOCHARGER",
            "DIPSTICK", "TAILGATE", "SUNROOF", "HEADLIGHT", "TAILLIGHT", "INDICATOR", "SPEEDOMETER", "ODOMETER",
            "TACHOMETER", "TRANSMISSION", "DIFFERENTIAL", "DRIVESHAFT", "SATELLITE", "NAVIGATION", "AIRBAG", "ANTENNA",
            "WIPER", "WASHER", "FLUID", "COOLANT", "ANTIFREEZE", "CONDENSER", "EVAPORATOR", "COMPRESSOR",
            "DISTRIBUTOR", "SOLENOID", "ACTUATOR", "SENSOR", "HARNESS", "TERMINAL", "BRACKET", "HOUSING",
            "RESERVOIR", "DIFFUSER", "SPLITTER", "INTERCOOLER", "SCAVENGE", "SILENCER", "CROSSMEMBER", "STABILIZER",
            "SUBFRAME", "TIE-ROD", "KNUCKLE", "BALL-JOINT", "LINKAGE", "FLANGE", "BEZEL", "STRAP"
        };
        foreach (var w in enCarInt) seedWords.Add(new Word { Language = "english", Difficulty = "intermediate", Topic = "car-parts", Text = w });

        // --- Beverages: Beginner (50) ---
        var enBevBeg = new[] {
            "WATER", "MILK", "TEA", "COFFEE", "JUICE", "SODA", "COKE", "BEER", "WINE", "ALE",
            "POP", "CHAI", "DRINK", "MUG", "CUP", "ICE", "MINT", "LIME", "PEACH", "APPLE",
            "BERRY", "FRUIT", "SWEET", "COLD", "HOT", "WARM", "PURE", "FRESH", "TONIC", "FIZZ",
            "BREW", "SHOT", "CIDER", "SHAKE", "SMOOTHIE", "LATTE", "MOCHA", "BOBA", "ROOT", "FLOAT",
            "ZEST", "SYRUP", "CREAM", "FOAM", "MALT", "HOPS", "GRAPE", "LEMON", "PUNCH", "SLUSH"
        };
        foreach (var w in enBevBeg) seedWords.Add(new Word { Language = "english", Difficulty = "beginner", Topic = "beverages", Text = w });

        // --- Beverages: Intermediate (80) ---
        var enBevInt = new[] {
            "MACCHIATO", "ESPRESSO", "CAPPUCCINO", "AMERICANO", "RISTRETTO", "CORTADO", "AFFOGATO", "FRAPPUCCINO",
            "KOMBUCHA", "LEMONADE", "MILKSHAKE", "COCKTAIL", "MOCKTAIL", "CHAMPAGNE", "PROSECCO", "VERMOUTH",
            "RIESLING", "CHARDONNAY", "MERLOT", "CABERNET", "ZINFANDEL", "BORDEAUX", "SHIRAZ", "WHISKEY",
            "BOURBON", "SCOTCH", "VODKA", "TEQUILA", "LIQUEUR", "COGNAC", "BRANDY", "ABSINTHE",
            "GINGER-ALE", "MOJITO", "MARGARITA", "MARTINI", "DAIQUIRI", "NEGRONI", "SANGRIA", "HEINEKEN",
            "PILSNER", "LAGER", "PORTER", "STOUT", "MATCHA", "OOLONG", "CHAMOMILE", "HIBISCUS",
            "PEPPERMINT", "EARL-GREY", "DARJEELING", "ROOIBOS", "INFUSION", "DECAF", "PROTEIN", "ELECTROLYTE",
            "SELTZER", "SPARKLING", "MINERAL", "CARBONATED", "DISTILLED", "CORDIAL", "NECTAR", "SQUASH",
            "SHRUB", "TODDY", "EGGNOG", "BITTERS", "GRENADINE", "AMARETTO", "KAHLUA", "BAILEYS",
            "SCHNAPPS", "MEAD", "SAKE", "GUINNESS", "SHANDY", "PERRY"
        };
        foreach (var w in enBevInt) seedWords.Add(new Word { Language = "english", Difficulty = "intermediate", Topic = "beverages", Text = w });

        // =========================================================================
        // 2. DUTCH (50 Beginner, 80 Intermediate per topic)
        // =========================================================================
        
        // --- Car Parts: Beginner (50) ---
        var nlCarBeg = new[] {
            "REM", "WIEL", "DEUR", "STOEL", "BAND", "KAP", "DAK", "PIJP", "TANK", "LAMP",
            "SLOT", "RAAM", "STUUR", "ACCU", "OLIE", "MOER", "BOUT", "KLEP", "AS", "RING",
            "SLANG", "KABEL", "POELIE", "VEER", "STANG", "LIJM", "STRIP", "GRILL", "BODEM", "HOES",
            "LOGO", "MAT", "REK", "TOETER", "KOP", "VELG", "STIFT", "BUIS", "PIN", "BUS",
            "KLEM", "PLAAT", "STREEP", "LAK", "VERF", "GLAS", "SPIEGEL", "TINT", "RELAIS", "ZEKERING"
        };
        foreach (var w in nlCarBeg) seedWords.Add(new Word { Language = "dutch", Difficulty = "beginner", Topic = "car-parts", Text = w });

        // --- Car Parts: Intermediate (80) ---
        var nlCarInt = new[] {
            "MOTOR", "UITLAAT", "CHASSIS", "BUMPER", "SPOILER", "BOEGIE", "KOPPELING", "ZUIGER",
            "PAKKING", "DYNAMO", "STARTMOTOR", "VLIEGWIEL", "REMKLAUW", "REMSCHIJF", "OPHANGING", "SCHOKBREKER",
            "INJECTOR", "GASPEDAAL", "TURBO", "COMPRESSOR", "CONDENSOR", "VERDAMPER", "PEILSTOK", "ACHTERKLEP",
            "DAKRAAM", "KOPLAMP", "REMLAMP", "KNIPPERLICHT", "SNELHEIDSMETER", "KILOMETERMETER", "TOERENTELLER", "VERSNELLINGSBAK",
            "DIFFERENTIEEL", "AANDRIJFAS", "NAVIGATIE", "AIRBAG", "ANTENNE", "RUITENWISSER", "SPROEIER", "VLOEISTOF",
            "KOELVLOEISTOF", "ANTIVRIES", "BOORDCOMPUTER", "STUURBEKRACHTIGING", "REMBEKRACHTIGING", "VERGENDELING", "ALARM", "SENSORS",
            "DRAADBOOM", "AANSLUITING", "BEUGEL", "BEHUIZING", "RESERVOIR", "STABILISATOR", "SUBFRAME", "STUURKOGEL",
            "KOGELLAGER", "WIELNAAF", "KOPPELINGSPEDAAL", "REMPEDAAL", "KROMAS", "NEKKENAS", "KLEPPENDEKSEL", "CARTERPAN",
            "DISTRIBUTIERIEM", "VRENAAT", "THERMOSTAAT", "RADIATEUR", "KOELFEN", "STUURHUIS", "AANDRIJVING", "KREUKELZONE",
            "DORPEL", "SPIEGELGLAS", "HANDGREEP", "SLOTBOUT", "WIELBOUT", "REMSLANG", "STARTING", "SPRAK"
        };
        foreach (var w in nlCarInt) seedWords.Add(new Word { Language = "dutch", Difficulty = "intermediate", Topic = "car-parts", Text = w });

        // --- Beverages: Beginner (50) ---
        var nlBevBeg = new[] {
            "WATER", "MELK", "THEE", "KOFFIE", "SAP", "FRIS", "COLA", "BIER", "WIJN", "MOUT",
            "DRANK", "MOK", "KOP", "IJS", "MUNT", "LIME", "PERZIK", "APPEL", "BES", "FRUIT",
            "ZOET", "KOUD", "HEET", "WARM", "PUUR", "VERS", "TONIC", "BRUIS", "KOM", "GLAS",
            "SODA", "SHAKE", "LATTE", "MOCHA", "BOBA", "STROOP", "ROOM", "SCHUIM", "DRUIF", "CITROEN",
            "PUNCH", "IJSTHEE", "CHOCO", "ANANAS", "MANGO", "BANAAN", "KERS", "BESSIES", "KORN", "PILS"
        };
        foreach (var w in nlBevBeg) seedWords.Add(new Word { Language = "dutch", Difficulty = "beginner", Topic = "beverages", Text = w });

        // --- Beverages: Intermediate (80) ---
        var nlBevInt = new[] {
            "MACCHIATO", "ESPRESSO", "CAPPUCCINO", "AMERICANO", "RISTRETTO", "CORTADO", "AFFOGATO", "FRAPPUCCINO",
            "KOMBUCHA", "LEMONADE", "MILKSHAKE", "SMOOTHIE", "COCKTAIL", "MOCKTAIL", "CHAMPAGNE", "PROSECCO",
            "VERMOUTH", "RIESLING", "CHARDONNAY", "MERLOT", "CABERNET", "ZINFANDEL", "BORDEAUX", "SHIRAZ",
            "WHISKEY", "BOURBON", "COGNAC", "BRANDEWRIJN", "ABSINT", "JENEVER", "VODKA", "TEQUILA",
            "LIKEUR", "GINGER-ALE", "MOJITO", "MARGARITA", "MARTINI", "DAIQUIRI", "NEGRONI", "SANGRIA",
            "HEINEKEN", "PILSNER", "LAGER", "PORTER", "STOUT", "MATCHA", "OOLONG", "KAMILLE",
            "HIBISCUS", "MUNTTHEE", "EARL-GREY", "DARJEELING", "ROOIBOS", "INFUSIE", "DECAF", "SPORTDRANK",
            "EIWITSHAKE", "ELECTROLYT", "BRUISWATER", "MINERAALWATER", "BRONWATER", "NEKTAR", "SIROOP", "TODDY",
            "EIERDORST", "BITTERS", "GRENADINE", "AMARETTO", "KAHLUA", "BAILEYS", "SCHNAPPS", "MEDE",
            "SAKE", "RADLER", "SPECIAALBIER", "WITBIER", "BOCKBIER", "TRAPPIST", "PORT", "SHERRY"
        };
        foreach (var w in nlBevInt) seedWords.Add(new Word { Language = "dutch", Difficulty = "intermediate", Topic = "beverages", Text = w });

        // =========================================================================
        // 3. INDONESIAN (50 Beginner, 80 Intermediate per topic)
        // =========================================================================
        
        // --- Car Parts: Beginner (50) ---
        var idCarBeg = new[] {
            "RODA", "REM", "PINTU", "KURSI", "BAN", "KAP", "ATAP", "PIPA", "TANGKI", "LAMPU",
            "KUNCI", "KACA", "SETIR", "AKI", "OLI", "BAUT", "MUR", "KATUP", "AS", "CINCIN",
            "SELANG", "KABEL", "SABUK", "PER", "BATANG", "LEM", "STRIP", "GRIL", "BODI", "SARUNG",
            "LOGO", "ALAS", "RAK", "KLAKSON", "BORD", "PELG", "PENA", "TABUNG", "PIN", "BOS",
            "KLIP", "PLAT", "GARIS", "CAT", "TINTA", "CERMIN", "RELAI", "SEKERING", "BUSA", "KARET"
        };
        foreach (var w in idCarBeg) seedWords.Add(new Word { Language = "indonesian", Difficulty = "beginner", Topic = "car-parts", Text = w });

        // --- Car Parts: Intermediate (80) ---
        var idCarInt = new[] {
            "MESIN", "KNALPOT", "SPOILER", "BUSI", "KOPLING", "PISTON", "PAKING", "DINAMO",
            "STARTER", "KREKAS", "REMKALIPER", "REMDISK", "SUSPENSI", "SHOCKBREKER", "GASKET", "INJEKTOR",
            "PEDALGAS", "TURBO", "KOMPRESOR", "KONDENSOR", "EVAPORATOR", "DIPSTICK", "PINTUBELAKANG", "SUNROOF",
            "LAMPUDEPAN", "LAMPUREM", "LAMPUSEN", "SPEEDOMETER", "ODOMETER", "TACHOMETER", "TRANSMISI", "DIFERENSIAL",
            "POROSAANDR", "NAVIGASI", "AIRBAG", "ANTENA", "WIPER", "AIRWIPER", "CAIRAN", "AIRRADIOATOR",
            "ANTIFREEZE", "KOMPUTER", "STEERPOWER", "REMPOWER", "PENGUNCIAN", "ALARM", "SENSOR", "KABELBODY",
            "TERMINAL", "BRACKET", "RUMAHAN", "RESERVOIR", "STABILISATOR", "SUBFRAME", "TIEROD", "BEARING",
            "WIELNAAF", "PEDALKOPLING", "PEDALREM", "POROSENGKOL", "POROSBUBUNG", "TUTUPKATUP", "CARTERPAN", "RANTAIKAMPRAT",
            "THERMOSTAT", "RADIATOR", "KIPASRADIATOR", "RUMAHSETIR", "PENGGERAK", "BUMPER", "SPAKBOR", "HANDGREEP",
            "BAUTRODA", "SELANGREM", "CHASSIS", "VENTILASI", "DASBOR", "INDIKATOR", "AKSELBRATOR", "MANIFOLD"
        };
        foreach (var w in idCarInt) seedWords.Add(new Word { Language = "indonesian", Difficulty = "intermediate", Topic = "car-parts", Text = w });

        // --- Beverages: Beginner (50) ---
        var idBevBeg = new[] {
            "AIR", "SUSU", "TEH", "KOPI", "JUS", "SODA", "COLA", "BIR", "ANGGUR", "ARAK",
            "MINUMAN", "CANGKIR", "GELAS", "ES", "MINT", "LEMON", "NANGKA", "APEL", "MANGGA", "BUAH",
            "MANIS", "DINGIN", "PANAS", "HANGAT", "MURNI", "SEGAR", "TONIK", "BUSA", "MANGKUK", "MADU",
            "SHAKE", "LATTE", "MOKA", "BOBA", "SIRUP", "KRIM", "UNTU", "JERUK", "PUNCH", "ESTEH",
            "COKELAT", "NANAS", "PISANG", "KERI", "MELON", "JAMBU", "ALPUKAT", "SARI", "PILS", "TAPE"
        };
        foreach (var w in idBevBeg) seedWords.Add(new Word { Language = "indonesian", Difficulty = "beginner", Topic = "beverages", Text = w });

        // --- Beverages: Intermediate (80) ---
        var idBevInt = new[] {
            "MACCHIATO", "ESPRESSO", "CAPPUCCINO", "AMERICANO", "RISTRETTO", "CORTADO", "AFFOGATO", "FRAPPUCCINO",
            "KOMBUCHA", "LEMONADE", "MILKSHAKE", "SMOOTHIE", "COCKTAIL", "MOCKTAIL", "CHAMPAGNE", "PROSECCO",
            "VERMOUTH", "RIESLING", "CHARDONNAY", "MERLOT", "CABERNET", "ZINFANDEL", "BORDEAUX", "SHIRAZ",
            "WHISKEY", "BOURBON", "COGNAC", "BRANDY", "ABSINTHE", "VODKA", "TEQUILA", "LIQUEUR",
            "GINGER-ALE", "MOJITO", "MARGARITA", "MARTINI", "DAIQUIRI", "NEGRONI", "SANGRIA", "BEERALE",
            "PILSNER", "LAGER", "PORTER", "STOUT", "MATCHA", "OOLONG", "KAMOMIL", "KRISAN",
            "HIBISCUS", "TEHHIDU", "EARLGREY", "DARJEELING", "ROOIBOS", "INFUSI", "DECAF", "MINUMANSPORT",
            "PROTEINSHAKE", "ELEKTROLIT", "AIRSODA", "MINERAL", "AIRALAMI", "NEKTAR", "SIROP", "TODDY",
            "EGGNOG", "BITTERS", "GRENADINE", "AMARETTO", "KAHLUA", "BAILEYS", "SCHNAPPS", "MEAD",
            "SAKE", "RADLER", "TUAK", "CENDOL", "DAWET", "BAJIGUR", "BANDREK", "WEDANGJUH"
        };
        foreach (var w in idBevInt) seedWords.Add(new Word { Language = "indonesian", Difficulty = "intermediate", Topic = "beverages", Text = w });

        context.Words.AddRange(seedWords);
        await context.SaveChangesAsync();
    }
}
