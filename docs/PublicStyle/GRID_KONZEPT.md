# SpinnerNet – Proportioniertes Mondrian-Grid (Konzept)

Status: Architektur- und Umsetzungskonzept für das modulare, proportionstreue Raster im bestehenden Razor/MVC-Projekt.

Ziele

- Raster basiert fix auf 4mm × 3.75mm (ratioX:ratioY = 4:3.75).
- Startkoordinate (0,0) oben links; Schema füllt den Viewport vollständig („cover“), keine weißen Ränder.
- Breakpoints: Desktop 80×60, Tablet 60×55, Mobile 40×50.
- Container („Blöcke“) sind modulare DIVs mit Hintergrundfarbe (keine Border) und einheitlicher Platzierungslogik.
- Banner/Marquee belegt immer den verbleibenden Platz neben dem Herz im oberen Streifen.
- Debug-Overlay ein-/ausblendbar per Taste „g“ und über eine sichtbare UI-Schaltfläche.
- Datenherkunft der Layout-Maps: initial JSON-Dateien in `wwwroot/config`.

Raster- und Größenberechnung

- Verhältnis: `MODULE_ASPECT = 4 / 3.75` → `moduleX = MODULE_ASPECT * moduleY`.
- Breakpoint-Logik (empfohlen):
  - `>= 1025px` → Desktop (80×60)
  - `769–1024px` → Tablet (60×55)
  - `<= 768px` → Mobile (40×50)
- Cover-Strategie (keine Ränder, ggf. Anschnitt rechts/unten):
  - `sByWidth = vw / (cols * MODULE_ASPECT)`
  - `sByHeight = vh / rows`
  - `moduleY = max(sByWidth, sByHeight)`
  - `moduleX = MODULE_ASPECT * moduleY`
  - Stage-Größe: `stageWidth = cols * moduleX`, `stageHeight = rows * moduleY`
  - Stage-Verankerung: `left=0`, `top=0` (kein Zentrieren)

Koordinatensystem und Platzierung

- Alle Block-Spezifikationen in Modulen: `{ c, r, w, h }` (Ganzzahlen).
- Pixel-Positionierung: `left=c*moduleX`, `top=r*moduleY`, `width=w*moduleX`, `height=h*moduleY`.
- Text-Ausrichtung: 1 Modul Innenabstand (Standard), Schriftgrößen als Faktor von `moduleY`.
- Marquee oben: Herz hat fixe Größe je Breakpoint; Marquee erhält `c = heart.c + heart.w`, `w = cols - c`, `r = 0`, `h = heart.h`.

Typografie

- Schrift: ROM MONO (Headline/Text). Globale Einstellungen:
  - `line-height: 1` für exakte Top-Ausrichtung.
  - `letter-spacing`: definierbar, Standard 0; optional separate Werte für Headline/Text.
  - Headline-Negativ-Einzug zur Sidebearing-Korrektur: `text-indent: -0.02em` (OK vom Design).
- Präzises Snappen an Rasterkreuzungen:
  - Links/oben mit „crisp“-Rundung (z. B. `Math.round(v)+0.5`) platzieren.
  - Optionales Glyph-Trim via Canvas-Metriken (Sidebearing links, Top-Ink) zur finalen Feinausrichtung.

Blocktypen (Beispiele)

- `heart` (SVG aus `/images/hearts/WEB_[1-9].svg`):
  - Farbe (Fill) persistiert pro Session; Block-Hintergrundfarbe kann wechseln.
  - Größe je Breakpoint fix (z. B. Desktop 12×12, Tablet 10×10, Mobile 10×8; Feinspezifikation im JSON).
- `marquee` (Banner mit „BAMBERGER SPINNEREI“):
  - Schriftgröße = `3 × moduleY`.
  - Füllt übrigen Platz des oberen Streifens neben dem Herz.
- `text` (Titel+Text oder nur Text):
  - `textScale` (z. B. 1.5) × `moduleY`.
  - Textbeginn immer bei Offset `(1,1)` innerhalb des Blocks (konfigurierbar).
- `image` (Bildcontainer):
  - `object-fit: cover` (Bildschnitt). Optional: `focusX`, `focusY` (0–1) als Fokuspunkt.

Farb- und Content-Logik

- Farbpalette (9 Farben).
- Adjazenzregel: Aus den Block-Rechtecken (Modulkoordinaten) einen Nachbarschaftsgraph bilden; greedy coloring ohne gleiche Nachbarfarben.
- Wechselintervall: alle 7000ms.
- Herz: SVG-Füllfarbe persistiert; Block-Hintergrund darf rotieren.
- Per-Block-Freeze: `persistColor: true` erlaubt, Farbe beizubehalten (Antwort „JA“ umgesetzt).
- Zitate/Text: Zufällig aus Liste für Content-Blöcke; bei Update neu ziehen.

Overlay / Debugging

- Canvas-Overlay zeichnet 1px rote Linien exakt auf Modulgrenzen (DPR-bewusst, `+0.5px` für Crispness).
- Toggle per Taste „g“ (global), zusätzlich mini-UI-Button oben rechts.
- Exportierte CSS-Variablen zu QA: `--module-x`, `--module-y`, `--grid-cols`, `--grid-rows`.

Datenmodell (JSON, initial)

Speicherort: `src/SpinnerNet.Web/wwwroot/config/layout.{desktop|tablet|mobile}.json`

```json
{
  "version": 1,
  "cols": 80,
  "rows": 60,
  "blocks": [
    { "id": "heart", "type": "heart", "c": 0,  "r": 0,  "w": 12, "h": 12, "persistColor": true },
    { "id": "marquee", "type": "marquee", "c": { "after": "heart" }, "r": 0, "h": 12, "fillRemainingCols": true, "textScale": 3 },
    { "id": "menu-vision", "type": "text", "c": 0,  "r": 12, "w": 26, "h": 8,  "text": "VISION", "textScale": 1.5 },
    { "id": "menu-zeit",   "type": "text", "c": 26, "r": 12, "w": 27, "h": 8,  "text": "ZEIT",   "textScale": 1.5 },
    { "id": "menu-raum",   "type": "text", "c": 53, "r": 12, "w": 27, "h": 8,  "text": "RAUM",   "textScale": 1.5 }
  ]
}
```

Hinweise:

- Für Tablet/Mobile identische Struktur; nur `cols`, `rows` und Blockabmessungen angepasst.
- Für `marquee` interpretiert die Engine: `c = block(after).c + block(after).w`, `w = cols - c`.
- Zusätzliche optionale Felder: `textOffset: [1,1]`, `letterSpacing`, `persistContentColor`, `imageSrc`, `focusX/Y`.

Client-Engine (GridEngine)

API-Entwurf:

- `calculateGrid(viewport): GridState`
- `drawOverlay(canvas, state, visible)`
- `placeBlock(element, spec, state)`
- `alignText(element, state, offsetCols=1, offsetRows=1, scale=1.5, trimGlyph=true)`
- `computeAdjacency(specList): Map<string, Set<string>>`
- `assignColors(blocks, palette, { persist, heartFill, previousColors }): Map<string,string>`
- `loadLayout(url): Promise<Layout>` (oder `<script type="application/json" id="layout">` als Inline-Fallback)
- `applyLayout(stage, layout, state)`

Events & Lifecycle:

- `DOMContentLoaded` → `calculateGrid` → `applyLayout` → `drawOverlay` → `assignColors`.
- `resize`/`ResizeObserver`/`matchMedia` → Recompute + Apply.
- `document.fonts.ready` → `alignText` erneut (präzise Metroik).

MVC-Integration

- Razor View Components/Partials pro Blocktyp (Heart, Marquee, Text, Image) mit gemeinsamen CSS-Klassen.
- Index.cshtml lädt `grid-engine.js` + JSON je Breakpoint (oder serverseitig eingebettet als `<script type="application/json">`).
- Später: TagHelper `<grid-block ...>` zur deklarativen Modellierung im Markup.

Tests & QA

- Playwright-Skripte prüfen:
  - Modulzählung (80×60, 60×55, 40×50) je Viewport.
  - Blockrechtecke = Vielfache der Modulgröße; Toleranz ±0.5px.
  - Textbox-Startpunkt = Rasterkreuzung (Innenoffset 1,1).
  - Screenshots mit Canvas-Overlay zur visuellen Bestätigung.

Umsetzungsplan (Schritte)

1. GridEngine als separate JS-Datei extrahieren (`wwwroot/js/grid-engine.js`).
2. Canvas-Overlay und Toggle „g“ + UI-Button implementieren.
3. JSON-Layouts (`desktop/tablet/mobile`) anlegen; Marquee-„fillRemainingCols“ auswerten.
4. View Components/Partials für Heart/Marquee/Text/Image; Beispielseite mit allen Typen.
5. Adjazenzbasierte Farbvergabe inkl. `persistColor`.
6. Typografie-Feintuning: `line-height:1`, `text-indent:-0.02em` für Headline, Letter-Spacing-Parameter.
7. Playwright-Validierung: Modulzählung, Pixel-Snapping, Screenshots.
8. Dokumentieren und Übergabe.

Optionale Erweiterungen

- Live-Konfiguration: JSON hot-reload im Dev-Modus.
- Per-Block „snapInside“/„snapOutside“ für alternative Textanker.
- Retina-/Art-Direction-Varianten für Image-Blocks (später, wenn benötigt).

