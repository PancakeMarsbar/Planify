# Planify

Planify er en .NET MAUI applikation designet til at håndtere maskiner (assets), siddepladser (floorplans) og opgaver via et Kanban-lignende board. Den nyeste version introducerer et loginsystem, brugerstyring med roller (Admin/User/Guest) og lokal datapersistens.

## Vigtige ændringer i denne version
* **Login System:** Appen starter nu på `LoginPage`, hvor brugere kan logge ind eller fortsætte som gæst.
* **Brugerstyring:** Administratorer har adgang til en ny `AccountsPage`, hvor de kan se, oprette, redigere og slette brugere.
* **Rollebaseret Adgang:**
    * **Admin:** Fuld adgang, inklusiv en `AccountsPage` fane og brugeradministration.
    * **User:** Adgang til Board og Floors med redigeringsrettigheder.
    * **Guest:** Begrænset "read-only" adgang til en forenklet Floor-oversigt.
* **Popups:** Integreret `CommunityToolkit.Maui` til at håndtere dialogbokse for oprettelse og redigering af brugere.

---

# Rod-filer

## App.cs
Initialiserer appen og ressourcer. Bestemmer startvinduet baseret på login-status (typisk `LoginPage`, men understøtter auto-login logik).

## AppShell.cs
Hovednavigationen efter login.
* Opsætter **FlyoutHeader** med profilbillede, brugernavn og logud-knap.
* Definerer fanerne: **Board**, **Floors**, **Accounts**.
* **Dynamisk indhold:** `AccountsPage`-fanen vises kun, hvis den loggede bruger er **Admin**.
* Indeholder sikkerhedslogik (`OnNavigating`) for at forhindre uautoriseret adgang til admin-sider.

## MauiProgram.cs
Bootstrapping af applikationen:
* Registrerer `App` og fonts (OpenSans).
* Initialiserer `MauiCommunityToolkit`.

---

# PlanifyApp/Models

## UserAccount.cs
Model for brugere indeholdende `Username`, `Password`, `IsAdmin` status og `Image`.

## Card.cs
Repræsenterer et kort på boardet (f.eks. en maskine). Indeholder data som `AssetTag`, `Model`, `PersonName`, `LocaterId`, `Status` og `SetupDeadline`. Indeholder logik (`DeadlineRed`, `DeadlineYellow`) til visuel markering af deadlines.

## FloorPlans.cs
Definerer en etage (`FloorPlan`) med egenskaber som navn, billede-sti og en liste af borde (`Tables`).

## Table.cs & Seat.cs
Definerer de fysiske områder på floorplanen. `Table` indeholder dimensioner og position, mens `Seat` repræsenterer en specifik plads med et `LocaterId`.

## Enums.cs & Tags.cs
Definerer standardiserede status-typer (`SeatStatus`, `MachineStatus`) og tags til brug i applikationen.

---

# PlanifyApp/Services

## AppRepository.cs
Singleton service der fungerer som det centrale data-lag.
* Håndterer lister af **Users**, **Cards**, **Floors** og **Lanes**.
* Styrer logik for **Login/Logout** og holder styr på den nuværende bruger og admin-rettigheder.
* Gemmer og loader data asynkront via `JsonStore`.
* Opretter seed-data (f.eks. en admin-bruger og test-kort) hvis ingen data findes.

## PasswordHasher.cs
Hjælpeklasse til hashing og verifikation af passwords ved hjælp af PBKDF2.

## JsonStore.cs & FileMutex.cs
Håndterer læsning og skrivning af JSON-filer i appens lokale data-mappe, sikret med en `FileMutex` for at undgå samtidighedsproblemer.

## AuditLog.cs
Logger brugerhandlinger (hvem, hvad, hvornår) til en lokal tekstfil.

---

# PlanifyApp/ViewModels

## AccountViewModel.cs
Styrer logikken bag `AccountsPage`.
* Loader og viser listen af brugere.
* Håndterer kommandoer for at oprette, opdatere og slette brugere, inklusiv validering (f.eks. kan man ikke slette sig selv).

## FloorViewModel.cs
Håndterer logik for visning og manipulation af floorplans.
* Styrer zoom-niveauer og valg af etage.
* Indeholder metoder til at tilføje/flytte borde og tildele kort til pladser.

## BoardViewModel.cs (V2)
Styrer Kanban-boardet.
* Organiserer kort i kolonner (`Lanes`).
* Håndterer flytning af kort mellem kolonner og synkroniserer ændringer med `AppRepository`.

---

# PlanifyApp/Pages

## LoginPage.cs
Appens indgangsside.
* Indtastning af brugernavn og adgangskode.
* Mulighed for "Login as guest", som sender brugeren videre til `FloorPageGuest`.

## AccountsPage.cs
Administrationsside hvor admins kan se en oversigt over brugere præsenteret som kort i et `FlexLayout`. Giver adgang til at redigere eller slette brugere via popups.

## BoardPage.cs
Viser opgaver i kolonner. Understøtter flytning af kort (via pile-knapper eller menu) og redigering af kort-detaljer via context-menuer.

## FloorPage.cs
Den fulde floorplan-editor for autoriserede brugere.
* Muliggør tilføjelse og flytning af borde via drag-and-drop gestures.
* Viser detaljeret information om hvem der sidder hvor.

## FloorPageGuest.cs
En begrænset version af floorplan-siden til gæster.
* Tillader visning og zoom af plantegningen, men ingen redigering.
* Indeholder en "Back" knap til login-siden.

## Pages/Popup/
Indeholder `CreateUserPopup.cs` og `UpdateUserPopup.cs`, som bruger `CommunityToolkit.Maui.Views` til at vise modale vinduer for brugerinput.

---

# GitHub
[https://github.com/PancakeMarsbar/Planify.git](https://github.com/PancakeMarsbar/Planify.git)