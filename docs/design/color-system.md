# PHOENIX Leave & Office Booking Design System

## 1. Bevezetés

Ez a dokumentum a PHOENIX vállalati arculati irányelvek alapján készült, és a szabadság-, home office- és munkaállomás foglaló webalkalmazás vizuális és UI szabályrendszerét definiálja.

A cél:
- egységes, vállalati megjelenés biztosítása
- jó felhasználói élmény (UX)
- könnyen fejleszthető és skálázható UI rendszer

---

## 2. Márka és logó használat

### Logó szabályok

- A PHOENIX logó minden felületen kötelező
- Alapértelmezett használat:
  - zöld logó fehér háttéren
- Alternatív:
  - fehér logó zöld háttéren
- Tiltott:
  - színezett logó
  - mintás vagy képes háttér

### Méret és spacing

- Minimum szélesség: **20 mm**
- Védőtér:
  - bal/jobb: 1 betű szélesség
  - felül/alul: 2 betű magasság

---

## 3. Színrendszer

### Primary (Corporate Green)

| Szín | HEX | Használat |
|------|-----|----------|
| Primary 500 | `#00927B` | fő CTA, aktív elemek |
| Primary 600 | `#008872` | hover |
| Primary 700 | `#007A67` | pressed |
| Primary 200 | `#CCE9E4` | háttér highlight |

---

### Secondary (Light Green)

| Szín | HEX | Használat |
|------|-----|----------|
| Secondary 500 | `#92D400` | dekoráció, badge |
| Secondary 300 | `#C8EA80` | soft highlight |
| Secondary 200 | `#E9F7CC` | háttér |

⚠️ Nem dominálhat a UI-ban

---

### Neutral / Dark

| Szín | HEX | Használat |
|------|-----|----------|
| Dark 800 | `#535353` | fő szöveg |
| Dark 900 | `#3F3F3F` | heading |
| Dark 600 | `#8F8F8F` | secondary text |

---

### Light / Background

| Szín | HEX | Használat |
|------|-----|----------|
| Light 50 | `#FFFFFF` | card háttér |
| Light 100 | `#F8FAF9` | app háttér |
| Light 200 | `#E8EEEC` | border |
| Light 300 | `#D5DDDA` | disabled |

---

### Semantic Colors

#### Success
- `#2E9E6F` → jóváhagyott, sikeres

#### Warning
- `#E39A1C` → figyelmeztetés, pending

#### Danger
- `#D64545` → elutasított, hiba

#### Info
- `#2C8CC9` → kiválasztott állapot

---

## 4. Színek használata funkció szerint

### Kérelmek

- Pending → Warning
- Approved → Success
- Rejected → Danger

### Naptár

- Saját esemény → Primary
- Csapat esemény → Secondary
- Aktuális nap → Info
- Hétvége → Light 300

### Office foglalás

- Szabad hely → Light 200
- Foglalt → Light 400
- Kiválasztott → Info
- Saját foglalás → Primary

---

## 5. Tipográfia

### Betűtípus

- **Roboto (kötelező)**

### Használat

| Típus | Font |
|------|------|
| Heading | Roboto Medium |
| Body | Roboto Regular |

### Méretek (javasolt)

- H1: 28px
- H2: 22px
- H3: 18px
- Body: 14–16px

---

## 6. Ikonok

- Egyszínű outline ikonok
- Alapszín: Primary
- Negatív verzió használható
- Nem használható:
  - multi-color ikon
  - filled + outline mix

---

## 7. Layout rendszer

### Alap grid

- 12 oszlopos rendszer (Bootstrap)
- Spacing:
  - 8px base
  - 24px card padding
  - 32px section gap

---

### Fő layout

#### Sidebar (bal oldal)
- ikon + label
- aktív: primary háttér vagy border
- hover: light highlight

#### Topbar
- logó
- profil
- értesítések

---

## 8. Komponens szabályok

### Button

- Primary → zöld
- Secondary → outline
- Danger → piros (csak kritikus művelet)

---

### Card

- háttér: fehér
- border: light
- radius: 12–16px
- shadow: soft

---

### Table

- header: light háttér
- hover: subtle highlight
- státusz: badge

---

### Badge

- pill form
- státusz szerint színezve

---

## 9. Naptár UI szabályok

- heti/ havi nézet
- napok:
  - szám + rövid napnév
- kattintás → modal
- jelölések:
  - színes dot vagy background

---

## 10. Modal

- középre igazított
- backdrop halvány
- max-width: 500–600px

---

## 11. Office helyfoglalás UI

### Seat grid / layout

- seat shape: kocka vagy lekerekített
- spacing egységes
- állapotok:

| Állapot | Szín |
|--------|------|
| Szabad | light |
| Foglalt | szürke |
| Kiválasztott | kék |
| Saját | zöld |

---

## 12. Általános UX szabályok

- Minimalista design
- Sok whitespace
- Kevés szín
- Konzisztens komponensek

---

## 13. Tiltások

- túl sok szín használata
- logó módosítása
- nem egységes ikonok
- random font használat

---

## 14. Vizuális arány

- 70% neutral
- 20% primary/dark
- 10% accent/semantic

---

## 15. SCSS változók

```scss
$primary: #00927B;
$secondary: #92D400;

$success: #2E9E6F;
$warning: #E39A1C;
$danger: #D64545;
$info: #2C8CC9;

$body-bg: #F8FAF9;
$body-color: #535353;

$card-bg: #FFFFFF;
$border-color: #E8EEEC;