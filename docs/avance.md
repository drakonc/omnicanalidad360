# Avance del proyecto

Bitácora por sesión. Lo que está hecho, lo que es maqueta y lo que falta.

---

## 5 de noviembre de 2026 · Sesión del bootcamp

### Decisión del Sponsor

En la sesión del bootcamp se construye una versión local, que corre en un
computador, con la llamada, Everest y LUMIER simulados. La telefonía en Azure
y las conexiones a los ambientes de prueba no cambian: siguen el plan de 4
semanas. Quedó registrada en el PRD, en el plan de construcción.

### Hito 1 terminado · Consola y datos de prueba

Se puede atender una llamada de principio a fin:

- Entra la llamada, se reproduce el aviso de grabación y se captura el
  documento por teclado.
- La llamada se encola y suena en la consola del agente que lleva más tiempo
  disponible.
- El agente contesta, el cronómetro corre, y al colgar vuelve a quedar libre.
- Si el paciente cuelga antes, la llamada queda abandonada con sus segundos
  de espera.
- **El documento aparece en la consola sin que nadie lo digite.** Ese es el
  principio del registro único que el piloto tiene que demostrar.

Datos de prueba cargados: 5 agentes, 1 supervisor, 2 administradores, 30
pacientes inventados con EPS colombianas (26 con derechos, 4 sin derechos, 2
con un dato de contacto vacío), cupos de 3 exámenes en 2 sedes para 2 semanas
y el catálogo de tipificación. Los documentos `1000000098` y `1000000099` no
existen a propósito, para probar el error de paciente no encontrado.

**Evidencia**: 22 pruebas automáticas, todas pasando. Flujo completo recorrido
contra el backend: la llamada entró, se encoló, se asignó, se contestó con 1
segundo de espera y se finalizó.

### Tiempo real resistente a caídas

Del análisis de tráfico de uContact salió un hallazgo que cambió el diseño:
**su canal de avisos en vivo lleva meses roto en producción y nadie lo notó**,
porque el sistema seguía funcionando preguntando el estado cada poco tiempo.

De ahí la regla que adoptamos: preguntar es la base, el aviso en vivo será una
mejora encima. Cuando la plataforma no responde, la pantalla conserva el último
dato bueno y dice hace cuánto es, en vez de quedarse muda. Está explicado en
`docs/decisions/0001-tiempo-real-polling.md`.

### Diseño EverCall aplicado

Se tomó el diseño del proyecto de Stitch *EverCall Contact Center MVP*. La
paleta, la tipografía y las medidas venían escritas en su propia especificación,
así que no hubo que adivinar nada: tinta `#14213D`, fondo `#F3F5F8`, acento
`#0E6E6E`, un color por cada estado del agente, tipografía Public Sans, franja
de estado de 64 px y cronómetro de 32 px. El principio declarado es "sobrio,
calmado y funcional para jornadas de 8 horas continuas".

### Pantallas

**Con datos reales de la plataforma:**

| Ruta | Qué es |
|---|---|
| `/consola` | Mi escritorio: softphone, guion, ficha del paciente y modal de llamada entrante |
| `/supervisor` | Tablero en vivo: indicadores, distribución de agentes y monitoreo |
| `/simulador` | El teléfono del paciente, que hace de telefonía |

**Maquetas con datos de prueba, cada una con una advertencia visible:**

| Ruta | Qué es | Falta |
|---|---|---|
| `/tipificacion` | Cierre post-llamada; el catálogo de causales sí es real | Hito 3 |
| `/campanas` | Campaña saliente | Fuera del alcance del PRD |
| `/ivr` | Flujo del IVR; marca qué pasos existen hoy de verdad | Administración |

### Lo que decidimos NO hacer

- **Nivel de servicio**: el diseño lo muestra, pero su umbral en segundos está
  por validar con gerencia. Sin umbral sería un número inventado en una pantalla
  que dice "tiempo real". En su lugar hay una nota explicando por qué falta.
- **Campaña saliente**: está en el diseño, pero el PRD la excluye del piloto
  ("tienen otro flujo, con HIS y Saludtools"). Quedó como maqueta con una
  advertencia roja; ningún botón marca a nadie.

### Pendientes conocidos

- **Las pantallas no se han visto en el navegador.** Compilan y el backend
  responde, pero la comprobación visual quedó sin hacer: el computador se
  quedó sin memoria y Windows detuvo los dos servicios. Para levantarlos hacen
  falta cerca de 1,5 GB de RAM libre.
- Advertencia de seguridad en `Microsoft.OpenApi`, que vino con la plantilla de
  .NET. No se tocó porque no se cambian dependencias sin aprobación.
- Everest y LUMIER no tendrán conexión directa: el acceso es por las pantallas
  de esas plataformas. Eso pone en duda el criterio CA-3.1, que pide que el
  paciente aparezca en menos de 2 segundos al contestar. Hay que llevarlo a la
  conversación con Everest.

### Siguiente

Hito 2 · **Paciente precargado**: que al contestar aparezcan nombre, EPS, sede
y derechos, consultados en el Everest simulado con el documento que capturó el
IVR. La pantalla ya tiene el lugar reservado.
