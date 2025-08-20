# Bamberger Spinnerei - Implementation Guide

## Übersicht der erstellten Dokumente

1. **BAMBERGER_SPINNEREI_WEBSITE.md** - Strukturelle Übersicht der Website
2. **BAMBERGER_SPINNEREI_CONTENT.md** - Detaillierte deutsche Inhalte für alle Seiten
3. **BAMBERGER_SPINNEREI_STRINGS.md** - Resource Strings für die Lokalisierung

## Implementierungsschritte

### 1. Navigation anpassen
Die Navigation sollte um folgende Punkte erweitert werden:
- Start (Homepage der Bamberger Spinnerei)
- Vision
- Projekte
  - Raum in Bamberg
  - Arbeitsmodelle
- Über uns
- Mitmachen

### 2. Neue Seiten erstellen
Folgende Razor Pages müssen erstellt werden:
- `/Pages/BambergerSpinnerei/Index.cshtml` - Hauptseite
- `/Pages/BambergerSpinnerei/Vision.cshtml` - Detaillierte Vision
- `/Pages/BambergerSpinnerei/Projekte/RaumBamberg.cshtml`
- `/Pages/BambergerSpinnerei/Projekte/Arbeitsmodelle.cshtml`
- `/Pages/BambergerSpinnerei/UeberUns.cshtml`
- `/Pages/BambergerSpinnerei/Mitmachen.cshtml`

### 3. Lokalisierung integrieren
1. Die Strings aus `BAMBERGER_SPINNEREI_STRINGS.md` in `Strings.resx` einfügen
2. Deutsche Übersetzungen sind bereits vorhanden
3. Für andere Sprachen können die Texte übersetzt werden

### 4. Design-Anpassungen
- Verwenden Sie die bestehenden CSS-Klassen aus `app.css`
- Die drei Säulen sollten die farbigen Gradient-Backgrounds nutzen:
  - Spinnen: `var(--color-alt-primary)`
  - Vermitteln: `var(--color-alt-secondary)`
  - Marktplatz: `var(--color-alt-quaternary)`

### 5. Integration mit Spinner.Net App
- Login-Button führt zur bestehenden Spinner.Net App
- Beta-Zugang kann über `/beta` angeboten werden
- Zeitwährung könnte später in die App integriert werden

## Routing-Struktur

```
/bamberger-spinnerei/                    - Hauptseite
/bamberger-spinnerei/vision              - Vision & Philosophie
/bamberger-spinnerei/projekte            - Projektübersicht
/bamberger-spinnerei/projekte/raum       - Projekt: Physischer Raum
/bamberger-spinnerei/projekte/arbeit     - Projekt: Arbeitsmodelle
/bamberger-spinnerei/ueber-uns           - Über die Initiative
/bamberger-spinnerei/mitmachen           - Mitgliedschaft & Engagement
/bamberger-spinnerei/kontakt             - Kontaktformular
/bamberger-spinnerei/newsletter          - Newsletter-Anmeldung
```

## Wichtige Features

### 1. Newsletter-Integration
- Formular für E-Mail-Anmeldung
- Integration mit bestehendem E-Mail-Service
- DSGVO-konforme Einwilligung

### 2. Mitglieder-Bereich
- Anmeldung über Azure AD
- Zugriff auf interne Dokumente
- Projektverwaltung

### 3. Zeit-Währung (Phase 2)
- Integration in Spinner.Net App
- Zeitkonto-Verwaltung
- Tauschbörse für Dienstleistungen

## Nächste Schritte

1. **Sofort umsetzbar:**
   - Basis-Seiten erstellen
   - Navigation erweitern
   - Inhalte einpflegen

2. **Kurzfristig (1-2 Monate):**
   - Newsletter-System
   - Kontaktformulare
   - SEO-Optimierung

3. **Mittelfristig (3-6 Monate):**
   - Mitglieder-Portal
   - Projekt-Einreichung
   - Event-Kalender

4. **Langfristig (6+ Monate):**
   - Zeit-Währung
   - Community-Features
   - Mobile App

## Technische Hinweise

- Nutzen Sie die bestehende `LocalizedPageModel` als Basis
- Bilder sollten im `/wwwroot/images/bamberger-spinnerei/` Ordner abgelegt werden
- Verwenden Sie die vorhandenen MudBlazor-Komponenten für konsistentes Design
- Mobile-First-Ansatz für alle neuen Seiten

## Content Management

- Regelmäßige Updates über News/Blog
- Projekt-Fortschritte dokumentieren
- Community-Stories hervorheben
- SEO-optimierte Inhalte