# Planify
# Rod-filer

## App.cs
Opretter app’en, definerer simple styles og bruger `CreateWindow(...)` til at sætte `AppShell` som roden (ny, anbefalet MAUI-måde).

## AppShell.cs
Topnavigation med faner: **Board**, **Floors**, **Settings**.  
Hver fane loader sin side via C# (ingen XAML).

## MauiProgram.cs
Bootstrapping: registrerer `App`, sætter fonte (**OpenSans**), og aktiverer debug-logging.

---

# PlanifyApp/Models

## Enums.cs
Alle status-typer: board-kolonner, maskinestatus, seatstatus.  
Holder forretningslogik konsekvent.

## Tags.cs
Standardiserede tags (**Wipe**, **Remote**, **Ekstra skærm**, **Klippeprog version**) + plads til brugerdefinerede.  
Bruges på `Card`.

## Card.cs
“Maskine-kortet”.  
Felter:  
- LOCATER-ID  
- Personnavn (fri tekst)  
- Stilling  
- iMac/PC nr  
- Serienr.  
- Model  
- Setup-deadline  
- Status  
- Noter  
- Tags  
- Kolonne & assignee  

Afledte felter: `DeadlineRed` / `DeadlineYellow` til rød/gul labels.

## Floor.cs
En etage (firma, bygning, level).  
Har referencer til `Tables` (områder på tegningen).

## Table.cs
Et område på planen med koordinater/størrelse og en liste af `Seats`.

## Seat.cs
En fysisk plads.  
Indeholder position (pixels), LOCATER-ID (etage.rum.plads), evt. rolle/label.  
Flere maskiner kan være på samme seat – de vises via opslag i repo.

---

# PlanifyApp/Services

## JsonStore.cs
Simpel gem/indlæs som JSON under `AppData/Planify`.  
Bruger MAUI’s `FileSystem.AppDataDirectory`.

## FileMutex.cs
Enkel fil-lås (mutex på tværs af processer), så samtidige brugere ikke skriver samtidig.

## AuditLog.cs
Append-logger ændringer (hvem/hvad/hvornår) til `audit.log`.  
Bruges fra repository.

## AppRepository.cs
Hjertet af data.  
Holder `Cards` og `Floors`, loader/saver JSON via `JsonStore`, og gemmer seed første gang.  

  

Hjælper-metoder:  
`CardsAtLocater(...)` samt seed-data (et par cards + en dummy-floor med 2 seats).

---

# PlanifyApp/ViewModels

## BaseViewModel.cs
Standard `INotifyPropertyChanged` base til databinding.

## BoardViewModel.cs
Serverer kolonnerne (`ObservableCollections`).  
`InitAsync()` loader data og starter auto-refresh hver 5. sek.  
`Move(...)` flytter kort mellem kolonner med regler (fx check før *I brug*).  
Har en `Alert`-callback så UI kan vise dialog uden deprecated API.

## FloorViewModel.cs
Loader `CurrentFloor`, eksponerer `Seats` og metoder til at finde maskiner for en plads og tildele person (sætter *To-Wipe* efter din regel).

---

# PlanifyApp/Pages

## MainPage.cs
Enkel velkomstside (forklarer fanerne).  
Kan fjernes, men god til test.

## BoardPage.cs
Trello-lignende tavle i ren C#: fem kolonner (*Setup Queue → David → Done → I brug → Lager*).  
Hver “card” vises i en `Border` (med stroke).  
Rød/gul deadline markeres både med badge og lys baggrund.  
Pil-knap → flytter kortet til næste kolonne og bruger VM’s regler.

## FloorPage.cs
Viser en dummy-floormap (`Grid`) med en `AbsoluteLayout`-layer.  
Hver seat rendres i en `Border` (stroke+baggrund) med LOCATER-ID, rolle, liste over maskiner på pladsen (splittet visning).  
Tryk på en seat → prompt for personnavn → VM sætter *To-Wipe* på relevante maskiner (advarsel-men-tillad).

## SettingsPage.cs
Placeholder til ADMIN/USER, CSV-eksport, kolonner/tags-konfig – så du ved hvor det kommer ind.

---

# GitHub
[https://github.com/PancakeMarsbar/Planify.git](https://github.com/PancakeMarsbar/Planify.git)
