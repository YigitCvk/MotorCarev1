# Third-Party Assets

## Motorcycle SVG Diagrams

All motorcycle SVG files in `src/MotorCare.Web/public/assets/vehicles/motorcycles/` are sourced from
[OpenClipart](https://openclipart.org/) via [FreeSVG.org](https://freesvg.org/) under the
**CC0 1.0 Universal / Public Domain** license. No attribution is legally required, but original
sources are documented below for traceability.

### License

**CC0 1.0 Universal — Public Domain Dedication**  
<https://creativecommons.org/publicdomain/zero/1.0/>

All files may be used, modified, and distributed for any purpose, commercial or non-commercial,
without permission or attribution.

### Files

| File | OpenClipart source |
|------|--------------------|
| `scooter.svg` | <https://openclipart.org/detail/> (OpenClipart scooter) |
| `sport.svg` | <https://openclipart.org/detail/171515/sportsbike-by-crimperman-171515> |
| `sport-touring.svg` | OpenClipart motorcycle silhouette |
| `touring.svg` | OpenClipart touring motorcycle |
| `adventure.svg` | <https://openclipart.org/detail/9210/motorcycle-clipart-by-gerald_g> |
| `cruiser.svg` | <https://openclipart.org/detail/183179/Honda-Magna-by-ClayDowling> |
| `naked.svg` | OpenClipart naked bike |
| `enduro.svg` | <https://openclipart.org/detail/117211/dirtbike-by-svetislav> |
| `commuter.svg` | <https://openclipart.org/detail/4412/1960s-piaggio-vespa-125-by-flomar-4412> |
| `default-motorcycle.svg` | OpenClipart generic motorcycle icon |

### Security verification

SVG files are served as static images via `<img>` tags, not inlined into the DOM. They have been
verified to contain no `<script>` elements, no event-handler attributes (`onload`, `onclick`, etc.),
no `<foreignObject>` elements, and no external resource-loading URLs. The `http://` strings present
in these files are RDF metadata namespace declarations and attribution text embedded during Inkscape
authoring — they are not fetched by the browser.

Verification command:

```sh
grep -rniE '<script|foreignObject|onload=|onclick=|https?://' public/assets/vehicles/motorcycles
```
