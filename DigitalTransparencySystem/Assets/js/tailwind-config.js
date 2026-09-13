tailwind.config = {
            darkMode: "class",
            theme: {
                extend: {
                    colors: {
                        "pacific-blue": "#b08948",
                        "slate-deep": "#0e2238",
                        "surface-gray": "#e9dfcc",
                        "border-light": "#d8cbb3",

                        "tertiary": "rgb(var(--dt-tertiary-rgb) / <alpha-value>)",
                        "on-tertiary": "rgb(var(--dt-on-tertiary-rgb) / <alpha-value>)",
                        "tertiary-container": "rgb(var(--dt-tertiary-container-rgb) / <alpha-value>)",
                        "on-tertiary-container": "rgb(var(--dt-on-tertiary-container-rgb) / <alpha-value>)",
                        "tertiary-fixed": "rgb(var(--dt-tertiary-fixed-rgb) / <alpha-value>)",
                        "on-tertiary-fixed": "rgb(var(--dt-on-tertiary-fixed-rgb) / <alpha-value>)",
                        "tertiary-fixed-dim": "rgb(var(--dt-tertiary-fixed-dim-rgb) / <alpha-value>)",
                        "on-tertiary-fixed-variant": "rgb(var(--dt-on-tertiary-fixed-variant-rgb) / <alpha-value>)",

                        "secondary": "rgb(var(--dt-secondary-rgb) / <alpha-value>)",
                        "on-secondary": "rgb(var(--dt-on-secondary-rgb) / <alpha-value>)",
                        "secondary-container": "rgb(var(--dt-secondary-container-rgb) / <alpha-value>)",
                        "on-secondary-container": "rgb(var(--dt-on-secondary-container-rgb) / <alpha-value>)",
                        "secondary-fixed": "rgb(var(--dt-secondary-fixed-rgb) / <alpha-value>)",
                        "on-secondary-fixed": "rgb(var(--dt-on-secondary-fixed-rgb) / <alpha-value>)",
                        "secondary-fixed-dim": "rgb(var(--dt-secondary-fixed-dim-rgb) / <alpha-value>)",
                        "on-secondary-fixed-variant": "rgb(var(--dt-on-secondary-fixed-variant-rgb) / <alpha-value>)",

                        "primary": "rgb(var(--dt-primary-rgb) / <alpha-value>)",
                        "on-primary": "rgb(var(--dt-on-primary-rgb) / <alpha-value>)",
                        "primary-container": "rgb(var(--dt-primary-container-rgb) / <alpha-value>)",
                        "on-primary-container": "rgb(var(--dt-on-primary-container-rgb) / <alpha-value>)",
                        "primary-fixed": "rgb(var(--dt-primary-fixed-rgb) / <alpha-value>)",
                        "on-primary-fixed": "rgb(var(--dt-on-primary-fixed-rgb) / <alpha-value>)",
                        "primary-fixed-dim": "rgb(var(--dt-primary-fixed-dim-rgb) / <alpha-value>)",
                        "on-primary-fixed-variant": "rgb(var(--dt-on-primary-fixed-variant-rgb) / <alpha-value>)",
                        "inverse-primary": "rgb(var(--dt-inverse-primary-rgb) / <alpha-value>)",

                        "background": "rgb(var(--dt-background-rgb) / <alpha-value>)",
                        "on-background": "rgb(var(--dt-on-background-rgb) / <alpha-value>)",
                        "surface": "rgb(var(--dt-surface-rgb) / <alpha-value>)",
                        "on-surface": "rgb(var(--dt-on-surface-rgb) / <alpha-value>)",
                        "surface-variant": "rgb(var(--dt-surface-variant-rgb) / <alpha-value>)",
                        "on-surface-variant": "rgb(var(--dt-on-surface-variant-rgb) / <alpha-value>)",
                        "inverse-surface": "rgb(var(--dt-inverse-surface-rgb) / <alpha-value>)",
                        "inverse-on-surface": "rgb(var(--dt-inverse-on-surface-rgb) / <alpha-value>)",
                        "surface-tint": "rgb(var(--dt-surface-tint-rgb) / <alpha-value>)",
                        "surface-dim": "rgb(var(--dt-surface-dim-rgb) / <alpha-value>)",
                        "surface-bright": "rgb(var(--dt-surface-bright-rgb) / <alpha-value>)",
                        "surface-container-lowest": "rgb(var(--dt-surface-container-lowest-rgb) / <alpha-value>)",
                        "surface-container-low": "rgb(var(--dt-surface-container-low-rgb) / <alpha-value>)",
                        "surface-container": "rgb(var(--dt-surface-container-rgb) / <alpha-value>)",
                        "surface-container-high": "rgb(var(--dt-surface-container-high-rgb) / <alpha-value>)",
                        "surface-container-highest": "rgb(var(--dt-surface-container-highest-rgb) / <alpha-value>)",

                        "outline": "rgb(var(--dt-outline-rgb) / <alpha-value>)",
                        "outline-variant": "rgb(var(--dt-outline-variant-rgb) / <alpha-value>)",

                        "error": "rgb(var(--dt-error-rgb) / <alpha-value>)",
                        "on-error": "rgb(var(--dt-on-error-rgb) / <alpha-value>)",
                        "error-container": "rgb(var(--dt-error-container-rgb) / <alpha-value>)",
                        "on-error-container": "rgb(var(--dt-on-error-container-rgb) / <alpha-value>)",
                    },
                    borderRadius: {
                        "DEFAULT": "0.25rem",
                        "lg": "0.25rem",
                        "xl": "0.5rem",
                        "2xl": "0.75rem",
                        "3xl": "1rem",
                        "full": "9999px"
                    },
                    spacing: {
                        "gutter": "24px",
                        "container-padding-mobile": "20px",
                        "base": "8px",
                        "container-padding-desktop": "64px",
                        "section-gap": "48px"
                    },
                    maxWidth: {
                        "7xl": "1400px"
                    },
                    backgroundImage: {
                        "gradient-colorful": "linear-gradient(135deg, #023e8a 0%, #0077b6 40%, #48cae4 75%, #90e0ef 100%)",
                        "gradient-hero": "linear-gradient(135deg, #001d32 0%, #023e8a 55%, #0077b6 100%)",
                    },
                    fontFamily: {
                        "badge-cap": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "display-lg": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "title-lg": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "body-lg": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "headline-lg": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "headline-lg-mobile": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "label-md": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "label-sm": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "label-lg": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "body-md": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"],
                        "headline-md": ["\"Hanken Grotesk\"", "ui-sans-serif", "system-ui", "sans-serif"]
                    },
                    fontSize: {
                        "badge-cap": ["11px", { lineHeight: "16px", letterSpacing: "0.05em", fontWeight: "700" }],
                        "display-lg": ["48px", { lineHeight: "56px", letterSpacing: "-0.02em", fontWeight: "800" }],
                        "display-lg-mobile": ["36px", { lineHeight: "44px", letterSpacing: "-0.01em", fontWeight: "800" }],
                        "title-lg": ["20px", { lineHeight: "28px", fontWeight: "700" }],
                        "body-lg": ["18px", { lineHeight: "28px", fontWeight: "400" }],
                        "headline-lg": ["32px", { lineHeight: "40px", letterSpacing: "-0.01em", fontWeight: "700" }],
                        "headline-lg-mobile": ["28px", { lineHeight: "36px", fontWeight: "700" }],
                        "label-md": ["14px", { lineHeight: "20px", fontWeight: "600" }],
                        "label-sm": ["12px", { lineHeight: "16px", letterSpacing: "0.05em", fontWeight: "700" }],
                        "label-lg": ["14px", { lineHeight: "20px", fontWeight: "600" }],
                        "body-md": ["16px", { lineHeight: "24px", fontWeight: "400" }],
                        "headline-md": ["24px", { lineHeight: "32px", letterSpacing: "-0.01em", fontWeight: "600" }]
                    }
                }
            }
        }
