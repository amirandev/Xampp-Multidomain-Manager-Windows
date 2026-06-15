from PIL import Image, ImageOps
from pathlib import Path

SRC = Path("Assets/icons/256px.ico")
ICONS_OUT = Path("Assets/icons")
ASSETS_OUT = Path("Assets")

ICO_SIZES = [16, 32, 48, 64, 96]

PNG_ASSETS = {
    "LockScreenLogo.scale-200.png":                             (48, 48),
    "SplashScreen.scale-200.png":                              (1240, 600),
    "Square150x150Logo.scale-200.png":                         (300, 300),
    "Square44x44Logo.scale-200.png":                           (88, 88),
    "Square44x44Logo.targetsize-24_altform-unplated.png":      (24, 24),
    "Square44x44Logo.targetsize-48_altform-lightunplated.png": (48, 48),
    "StoreLogo.png":                                           (50, 50),
    "Wide310x150Logo.scale-200.png":                           (620, 300),
}

icon = Image.open(SRC)

for size in ICO_SIZES:
    canvas = Image.new("RGBA", (size, size), "white")
    resized = ImageOps.fit(icon, (size, size), Image.LANCZOS)
    canvas.paste(resized, (0, 0), resized)
    canvas.save(ICONS_OUT / f"{size}px.ico", format="ICO", sizes=[(size, size)])

for name, (w, h) in PNG_ASSETS.items():
    canvas = Image.new("RGBA", (w, h), "white")
    icon_size = min(w, h)
    resized = ImageOps.fit(icon, (icon_size, icon_size), Image.LANCZOS)
    x = (w - icon_size) // 2
    y = (h - icon_size) // 2
    canvas.paste(resized, (x, y), resized)
    canvas.save(ASSETS_OUT / name)

print("Done")
