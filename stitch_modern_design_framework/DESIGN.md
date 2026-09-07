---
name: Pacific Professional
colors:
  surface: '#f9f9ff'
  surface-dim: '#cfdaf2'
  surface-bright: '#f9f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f0f3ff'
  surface-container: '#e7eeff'
  surface-container-high: '#dee8ff'
  surface-container-highest: '#d8e3fb'
  on-surface: '#111c2d'
  on-surface-variant: '#404850'
  inverse-surface: '#263143'
  inverse-on-surface: '#ecf1ff'
  outline: '#707881'
  outline-variant: '#bfc7d1'
  surface-tint: '#006399'
  primary: '#005d90'
  on-primary: '#ffffff'
  primary-container: '#0077b6'
  on-primary-container: '#f3f7ff'
  inverse-primary: '#94ccff'
  secondary: '#315ca9'
  on-secondary: '#ffffff'
  secondary-container: '#86adff'
  on-secondary-container: '#033e8a'
  tertiary: '#864a00'
  on-tertiary: '#ffffff'
  tertiary-container: '#a95f00'
  on-tertiary-container: '#fff6f1'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#cde5ff'
  primary-fixed-dim: '#94ccff'
  on-primary-fixed: '#001d32'
  on-primary-fixed-variant: '#004b74'
  secondary-fixed: '#d8e2ff'
  secondary-fixed-dim: '#aec6ff'
  on-secondary-fixed: '#001a42'
  on-secondary-fixed-variant: '#0f4490'
  tertiary-fixed: '#ffdcc0'
  tertiary-fixed-dim: '#ffb877'
  on-tertiary-fixed: '#2e1600'
  on-tertiary-fixed-variant: '#6c3a00'
  background: '#f9f9ff'
  on-background: '#111c2d'
  surface-variant: '#d8e3fb'
  pacific-blue: '#0077b6'
  slate-deep: '#023e8a'
  surface-gray: '#f1f5f9'
  border-light: '#e2e8f0'
typography:
  display-lg:
    fontFamily: Hanken Grotesk
    fontSize: 48px
    fontWeight: '800'
    lineHeight: 56px
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '800'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Hanken Grotesk
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Hanken Grotesk
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-lg:
    fontFamily: Hanken Grotesk
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.02em
  label-sm:
    fontFamily: Hanken Grotesk
    fontSize: 12px
    fontWeight: '700'
    lineHeight: 16px
    letterSpacing: 0.05em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 8px
  gutter: 24px
  margin-desktop: 80px
  margin-mobile: 20px
  container-max: 1440px
---

## Brand & Style
The design system is defined by a high-contrast, institutional aesthetic that balances authoritative precision with modern digital clarity. It targets professional environments where trust and efficiency are paramount, moving away from soft textures toward a sharp, **Corporate Modern** look.

The visual narrative is driven by:
- **High-Contrast Professionalism:** Utilizing a stark white and deep navy foundation to establish immediate credibility.
- **Clean Minimalism:** Aggressive use of whitespace to reduce cognitive load and emphasize key data points.
- **Precision Engineering:** A shift from organic "softness" to architectural rigidity, favoring sharp corners and defined boundaries.
- **Functional Clarity:** A "no-frills" approach where every element serves a distinct navigational or informational purpose.

## Colors
The palette is anchored by "Pacific Blue" and "Slate," creating a vibrant yet sober professional environment.

- **Primary (Pacific Blue):** The core driver for interaction. It is used for primary buttons, active states, and focus indicators.
- **Secondary (Slate Deep):** Used for deep-hierarchy elements, navigation backgrounds, and high-emphasis typography to provide a grounded, structural feel.
- **Neutral:** A refined scale of Cool Grays. Text is set in a near-black Slate (`#1e293b`) to ensure maximum legibility against pure white backgrounds.
- **Backgrounds:** Primarily pure white (`#ffffff`) to facilitate high-contrast layouts, with light slate tints used sparingly for section differentiation.

The default mode is **Light**, emphasizing a clean, "paper-like" professional workspace.

## Typography
This design system utilizes **Hanken Grotesk** exclusively to maintain a cohesive, institutional, and highly modern feel. The font's geometric clarity supports the professional narrative.

- **Hierarchy:** Use heavy weights (Bold/ExtraBold) for headlines to create a strong visual anchor against the high-contrast background.
- **Body Text:** Maintained at a 1.5x line-height ratio. The typeface’s large x-height ensures readability even at smaller scales.
- **Interactive Elements:** Labels and buttons utilize SemiBold or Bold weights with slight tracking (letter-spacing) to distinguish them from static content.

## Layout & Spacing
The layout follows a **Fixed Grid** model for desktop to maintain an organized, centered workspace, transitioning to a **Fluid Grid** for mobile.

- **Desktop (1440px):** 12-column grid. Increased side margins (80px) are used to enforce a focused content area, reflecting a clean, editorial layout style.
- **Rhythm:** A strict 8px base unit governs all dimensions.
- **Whitespace:** Emphasize "intentional voids." Vertical spacing between major sections should be generous (96px+) to allow the high-contrast elements to stand out without feeling crowded.

## Elevation & Depth
In alignment with the high-contrast professional aesthetic, this design system moves away from heavy shadows and "soft" depth, favoring **Low-contrast outlines** and **Tonal Layers**.

- **Flat Hierarchy:** Depth is primarily communicated through color blocking and thin, precise borders (`1px`).
- **Surface Tiers:** Backgrounds use `#f8fafc`, while primary content containers use pure `#ffffff`.
- **Borders:** Use `#e2e8f0` (Slate-200) for standard containers. This creates "ghost borders" that define space without adding visual weight.
- **Shadows:** Reserved strictly for floating elements (modals/dropdowns). Use a sharp, "technical" shadow: `0px 4px 12px rgba(2, 62, 138, 0.08)`.

## Shapes
The shape language is **Soft** (Level 1), shifting the aesthetic toward a sharper, more rigorous professional look compared to previous iterations.

- **Components:** Standard buttons and input fields use a precise 4px (0.25rem) radius.
- **Cards & Containers:** Use a maximum of 12px (0.75rem) radius (ROUND_TWELVE). This sharpens the overall interface, making it feel more like a structured document or a precision tool.
- **Actionable Icons:** Should remain unrounded or use minimal corner smoothing to match the geometric nature of Hanken Grotesk.

## Components
- **Buttons:**
    - *Primary:* Solid Pacific Blue (`#0077b6`) with white text. 4px radius.
    - *Secondary:* Outline style using Slate Deep (`#023e8a`) with a 1px stroke.
- **Input Fields:** Sharp 4px corners. 1px border in Slate-200. On focus, the border shifts to 2px Pacific Blue with no outer glow.
- **Cards:** Pure white background with a 12px (ROUND_TWELVE) radius and a 1px Slate-200 border. No shadow is used for standard cards to maintain the clean, high-contrast look.
- **Data Tables:** High-density layout. Header rows use a Slate-50 background with `label-sm` Bold text. 1px horizontal dividers only.
- **Chips:** Rectangular with 4px radius. Use light blue backgrounds with dark navy text for a "tagging" system that feels archival and organized.
- **Navigation:** Top navigation uses a solid white background with a 1px bottom border in Slate-100. No blur or transparency.