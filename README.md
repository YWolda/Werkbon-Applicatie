# Uren Registratie

Een Blazor WebAssembly PWA om je werkdag te starten/stoppen per opdracht, en je uren
per opdracht te overzien. Werkt volledig offline (data wordt lokaal in de browser
opgeslagen) en is te installeren als app-icoon op je iPhone.

## Blijft mijn data bewaard tussen herstarts?

Ja. Je opdrachten, uren en werkbonnen staan niet "in" het programma, maar in de
localStorage van je browser, gekoppeld aan het webadres waarop de app draait
(bijv. `https://localhost:7080`). Zolang je de app altijd op datzelfde adres
opent, blijft alles gewoon staan — ook na het herstarten van Visual Studio, je
laptop, of je telefoon.

Daarom heeft dit project een vast poortnummer ingesteld (zie
`Properties/launchSettings.json`), zodat Visual Studio niet steeds een ander
adres kiest. Kies in Visual Studio bovenin gewoon steeds hetzelfde opstartprofiel
("https" is de standaard).

**Wanneer raak je de data wél kwijt?**
- Als je in je browser handmatig "site-data wissen" gebruikt voor deze site.
- Als je de app via een ander adres/poort opent dan normaal.
- Als je de app straks online zet (Azure) op een nieuw webadres — dat is dan
  een "nieuwe" locatie voor de browser, dus begin je daar met een lege lijst.
  Zorg dat je testdata dan niet toevallig belangrijk is.

## Openen in Visual Studio

1. Zorg dat je de **.NET 8 SDK** en de workload **ASP.NET en webontwikkeling**
   geïnstalleerd hebt (via Visual Studio Installer als dat nog niet zo is).
2. Dubbelklik op `UrenRegistratie.csproj` om het project in Visual Studio te openen.
3. Visual Studio herstelt automatisch de benodigde NuGet-packages.
4. Druk op **F5** (of de groene "play"-knop) om de app te starten. Hij opent in je
   standaardbrowser.

## Testen op je eigen laptop

Gewoon F5 in Visual Studio — de app draait dan op `https://localhost:xxxx`.

## Testen op je iPhone (zelfde wifi-netwerk als je laptop)

1. Start de app in Visual Studio, maar in plaats van `localhost` moet je iPhone het
   IP-adres van je laptop gebruiken. Zoek het IP-adres van je laptop op (Instellingen
   netwerk, of `ipconfig` in een terminal).
2. Zorg dat de app in Visual Studio luistert op alle netwerkadressen: open
   `Properties/launchSettings.json` en zet `"applicationUrl"` op iets als
   `https://0.0.0.0:5001` (of gebruik `dotnet run --urls=https://0.0.0.0:5001`).
   Let op: voor een zelfondertekend HTTPS-certificaat moet je mogelijk de
   beveiligingswaarschuwing in Safari accepteren.
3. Open Safari op je iPhone en ga naar `https://<ip-van-laptop>:5001`.
4. Tik op het deel-icoon (vierkantje met pijl omhoog) → **"Zet op beginscherm"**.
   Nu heb je een app-icoon dat aanvoelt als een echte app.

**Let op:** dit werkt alleen zolang je laptop aan staat en op hetzelfde netwerk zit
als je iPhone. Voor echt gebruik onderweg bij een klant (fase 2) ga je de app hosten
op een online server, zodat je 'm overal kan openen en de data straks ook kan
synchroniseren tussen meerdere gebruikers.

## Belangrijk: waar wordt de data bewaard?

De uren en opdrachten worden opgeslagen in de **localStorage van de browser** op het
apparaat waarop je de app opent. Dit betekent:

- ✅ Werkt volledig offline, ook zonder wifi/data bij een klant.
- ⚠️ De data van je iPhone en je laptop zijn (nog) niet met elkaar verbonden — het
  zijn twee aparte "kopieën". Als je alleen op je iPhone werkt, is dat geen
  probleem.
- ⚠️ Als je de app-data in Safari handmatig wist (of de app van je beginscherm
  verwijdert en opnieuw toevoegt na een tijdje), kan de data verloren gaan. Voor
  belangrijke uren is het slim om regelmatig het Overzicht te bekijken/noteren tot
  fase 2 (met een echte database) klaar is.

## Projectstructuur

```
UrenRegistratie/
├── Models/              → Project.cs, WorkSession.cs (datamodellen)
├── Services/
│   ├── LocalStorageService.cs   → leest/schrijft data naar de browser
│   └── TimeTrackingService.cs   → alle logica (starten, stoppen, totalen berekenen)
├── Pages/
│   ├── Index.razor       → Start/stop werkdag
│   ├── Opdrachten.razor  → Opdrachten beheren
│   └── Overzicht.razor   → Uren overzicht
├── Layout/               → Navigatiebalk en paginalayout
└── wwwroot/              → HTML, CSS, PWA-manifest en icoon
```

## Volgende stappen (fase 2 — later)

Wanneer je collega's mee willen doen, vervang je `LocalStorageService` door een
service die praat met een echte backend (ASP.NET Core Web API + database). Omdat
alle schermen alleen met `TimeTrackingService` praten (en niet direct met opslag),
hoef je de Razor-pagina's dan nauwelijks aan te passen.

## Ontbrekend: app-icoon

Voeg zelf een `icon-512.png` (512x512 pixels) toe aan de `wwwroot`-map voor een
mooi icoon op je beginscherm. Zonder dit bestand werkt de app gewoon, maar toont
Safari een standaard schermafbeelding als icoon.

