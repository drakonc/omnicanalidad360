# PRD — Omnicanalidad 360 · Consola de atención con registro único · Piloto NEPS Imagenología

Versión 1.0 · 3 de noviembre de 2026 · Linkai y Everest · Proyecto Bootcamp Grupo 1A

## 1. Resumen

Se va a construir una plataforma de atención telefónica propia, desplegada en Azure, para la subcampaña NEPS Imagenología de Linkai. La plataforma recibe la llamada sin pasar por uContact, captura el documento del paciente en el IVR, lo valida automáticamente en Everest, permite al agente agendar en LUMIER desde la misma consola y registra la gestión una sola vez. Es para los 5 agentes fijos de la línea y su supervisor. El objetivo es demostrar dos cosas en menos de un mes: que el doble registro y la redigitación desaparecen, y que Linkai puede atender una línea sin la telefonía de uContact.

## 2. Problema y línea base

Hoy cada llamada con agente se registra dos veces: en el sistema del cliente y en el CRM de uContact. El documento que captura el IVR o el bot se vuelve a digitar en Everest. uContact no se conecta con los sistemas de los clientes, y Linkai le paga $44.188.371 al mes en licencias. En septiembre de 2026, NEPS Imagenología recibió 14.258 llamadas, atendidas por agentes que comparten las demás subcampañas de Imagenología.

| Indicador | Valor actual | Fuente | Fecha de corte | Meta |
|---|---|---|---|---|
| Segundos de doble registro y redigitación por llamada | 45 [EST] | Estimación validada por gerencia; sin medición de calidad | Septiembre 2026 | 0 |
| Tiempo entre colgar y gestión registrada | Por medir | — | — | ≤ 3 segundos en el 100% de las llamadas |
| Llamadas de NEPS Imagenología | 14.258 al mes | Reporte de gerencia | Septiembre 2026 | Sin cambio |
| TMO de Imagenología | 3,9 minutos | Productividad general de septiembre | Septiembre 2026 | Por debajo de 3,9 minutos |
| Nivel de servicio de Linkai | 45% | Productividad general de septiembre, hoja CONSOLIDADO | Septiembre 2026 | Mínimo 80% |
| Costo del doble registro en la línea | $3.234.784 al mes [EST] | 14.258 × 45 s × $18.150 por hora | Septiembre 2026 | $0 |

## 3. Usuarios y roles

- **Agente (5)**: agentes fijos de NEPS Imagenología, con computador, navegador, micrófono y audífonos. Atiende las llamadas desde el navegador, ve los datos del paciente ya cargados, agenda en LUMIER, tipifica y cierra la llamada. Necesita no digitar nada que el sistema ya tenga.
- **Supervisor (1)**: ve en tiempo real quién está en llamada, cuántas llamadas hay en cola, las atendidas y abandonadas, y el nivel de servicio del día. Puede consultar gestiones, escuchar grabaciones y corregir una tipificación dejando rastro.
- **Administrador técnico (1 a 2, equipo de Everest)**: configura usuarios, catálogo de tipificación, mensajes del IVR y credenciales de las conexiones.

## 4. Arquetipo de la solución

- **Núcleo: RPA, como registro único con reglas fijas.** El trabajo que elimina el dolor es mover datos entre sistemas siguiendo reglas que se pueden escribir completas: el documento del IVR va a Everest, la cita de LUMIER y la tipificación van a la gestión, sin que nadie digite dos veces.
- **Superficie: web.** Una consola del agente con teléfono integrado y una vista de supervisor, porque varias personas trabajan sobre la misma información todos los días y cada gestión tiene que quedar guardada y consultable.
- **Empaque: ninguno.** Las reglas caben en tablas: el catálogo de tipificación y el flujo de la línea.

Justificación con las preguntas de diagnóstico:

1. **Variabilidad de la entrada:** cada llamada llega con el mismo dato, el documento capturado por el IVR. Apunta a RPA.
2. **Tipo de dato:** campos ordenados (documento, derechos, sede, EPS, cita, tipificación). No hay texto que interpretar. Apunta a RPA.
3. **Quién decide:** las decisiones son reglas (tiene derechos o no) o las toma el agente humano en la conversación. La solución no razona. Apunta a RPA.
4. **Quién consume el resultado:** varias personas, todos los días. Apunta a una superficie web.

La telefonía (recibir la llamada, IVR, cola, consola con teléfono y grabación) no es un arquetipo aparte: es parte de la superficie web y se ensambla con componentes existentes, no se programa desde cero.

## 5. Alcance

**MVP**: atender de principio a fin una llamada al número de prueba de NEPS Imagenología fuera de uContact, con un solo registro.

**Ventana de construcción**: un poco menos de 4 semanas. Construyen en conjunto el equipo de Everest y el grupo del bootcamp.

### Sí incluye (v1)

- Número de prueba con IVR que reproduce el aviso de grabación y pide el documento por teclado.
- Cola de llamadas y asignación al primer agente disponible de los 5.
- Consola web del agente con teléfono integrado en el navegador y datos del paciente precargados.
- Validación automática de derechos y consulta de nombre, sede, EPS, correo y teléfono alternativo en el ambiente de pruebas de Everest.
- Consulta de cupos y agendamiento en el ambiente de pruebas de LUMIER desde la consola.
- Registro único de la gestión con tipificación de Nivel 1 y Nivel 2, TMO calculado y grabación asociada, a más tardar 3 segundos después de colgar.
- Vista de supervisor con estado de agentes, cola, atendidas, abandonadas y nivel de servicio del día.
- Exportación de gestiones en CSV con los campos de la tipificación actual.

### No incluye (después)

- Tráfico real de la línea — se pasa después del prototipo, con desborde a uContact, porque 5 agentes cubren cerca del 72% de la carga [EST].
- Otras subcampañas de Imagenología y la campaña Sura — el piloto es una sola línea; Sura no expone conexión con Agenda Web.
- Llamadas salientes — tienen otro flujo, con HIS y Saludtools.
- Transferencia a la Línea con la gerencia — flujo secundario que se agrega en la siguiente versión.
- Chat, WhatsApp y voicebot con IA — no hacen parte del dolor medido.
- Uso por varias empresas para vender a terceros — primero hay que probar que funciona en Linkai.
- Reportería histórica y tableros de inteligencia de negocio — para el piloto basta el día en curso y la exportación.

## 6. Mapa funcional · Extracción de 4 capas

### Objetos de información

- **Llamada**: cada llamada que entra a la línea, atendida o abandonada.
- **Paciente**: los datos de la persona que llama, tal como vienen de Everest.
- **Gestión**: el registro único de lo que pasó en una llamada atendida.
- **Cita**: la cita agendada en LUMIER durante la llamada.
- **Tipificación**: el catálogo de causales de la llamada, en dos niveles.
- **Grabación**: el audio de la llamada.
- **Usuario**: agentes, supervisor y administradores.

### Trabajo

- Recibir la llamada y reproducir el aviso de grabación *(determinista)*.
- Capturar el documento por teclado *(determinista)*.
- Encolar y asignar la llamada a un agente disponible *(determinista)*.
- Consultar paciente y derechos en Everest con el documento *(determinista)*.
- Mostrar los datos al agente al contestar *(determinista)*.
- Conversar con el paciente y confirmar sus datos *(persona)*.
- Consultar cupos y agendar en LUMIER *(persona, con apoyo determinista)*.
- Elegir la tipificación *(persona)*.
- Calcular el TMO y asociar la grabación *(determinista)*.
- Registrar la gestión *(determinista)*.
- Calcular el nivel de servicio *(determinista)*.

### Momentos de intervención humana

1. **Contestar** — el agente ve el nombre, documento, sede, EPS, derechos, correo y teléfono alternativo antes de saludar.
2. **Confirmar o corregir datos** — el agente confirma con el paciente y, si un dato está mal, lo corrige con un motivo.
3. **Agendar** — el agente elige examen, sede, fecha y hora entre los cupos de LUMIER.
4. **Tipificar** — el agente elige la causal de Nivel 1 y Nivel 2 antes de colgar o en los 3 segundos siguientes.
5. **Corregir una gestión** — el supervisor cambia una tipificación mal puesta y escribe el motivo.

### Decisiones

| Decisión | Quién decide | Criterio | Qué pasa sin certeza |
|---|---|---|---|
| ¿El paciente tiene derechos vigentes? | Everest, como fuente | Respuesta de Everest | Si Everest no responde, la consola muestra "Derechos sin validar" y no permite agendar; el agente informa al paciente y tipifica |
| ¿Qué cita se asigna? | Agente | Necesidad del paciente y cupos de LUMIER | Si LUMIER no responde, la gestión se registra sin cita con la causal correspondiente |
| ¿Cuál es la causal de la llamada? | Agente | Catálogo de tipificación | La consola no deja cerrar la gestión sin Nivel 1 |
| ¿A qué agente va la llamada? | Plataforma | Primer agente en estado Disponible, el que lleva más tiempo libre | Si no hay agentes disponibles, la llamada espera en cola con música |

## 7. Glosario canónico

| Término | Significado | Nombre técnico sugerido |
|---|---|---|
| Línea | La subcampaña NEPS Imagenología, única en el piloto | `linea` |
| Llamada | Una comunicación telefónica que entra a la línea | `llamada` |
| Gestión | El registro único de una llamada atendida | `gestion` |
| Paciente | La persona que llama, identificada por tipo y número de documento | `paciente` |
| Validación de derechos | La consulta a Everest que dice si el paciente puede ser atendido | `validacion_derechos` |
| Cita | El turno asignado en LUMIER | `cita` |
| Tipificación | La causal de la llamada, en Nivel 1 y Nivel 2 | `tipificacion` |
| TMO | Tiempo medio de operación: segundos desde que el agente contesta hasta que cuelga | `tmo_segundos` |
| Grabación | El audio de la llamada | `grabacion` |
| Nivel de servicio | Porcentaje de llamadas ofrecidas que se contestan dentro del umbral | `nivel_servicio` |
| Agente | Usuario que atiende llamadas | `agente` |

En todo el documento y en el código se dice "gestión", nunca "registro", "caso" ni "ticket".

## 8. Modelo de información

Todas las fechas y horas en ISO 8601 con zona horaria de Bogotá, por ejemplo `2026-11-20T09:14:05-05:00`. Todos los ejemplos son ficticios.

### Llamada

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo | Notas |
|---|---|---|---|---|---|
| id_llamada | texto | Sí | UUID | `7f3c2a10-5b1e-4c8e-9a2d-1e6f0b9c4d21` | Lo genera la plataforma |
| linea | texto | Sí | `NEPS_IMAGENOLOGIA` | `NEPS_IMAGENOLOGIA` | Único valor en v1 |
| telefono_origen | texto | Sí | E.164 | `+573001234567` | |
| documento_ivr | texto | No | 5 a 12 dígitos | `1000000001` | Vacío si el paciente no marcó |
| hora_entrada | fecha-hora | Sí | ISO 8601 | `2026-11-20T09:14:05-05:00` | |
| hora_contesta | fecha-hora | No | ISO 8601 | `2026-11-20T09:14:31-05:00` | Vacío si se abandona |
| hora_fin | fecha-hora | Sí | ISO 8601 | `2026-11-20T09:18:22-05:00` | |
| segundos_espera | entero | Sí | ≥ 0 | `26` | hora_contesta − hora_entrada, o hasta el abandono |
| estado | texto | Sí | Ver tabla de estados | `finalizada` | |
| id_agente | texto | No | Código de usuario | `AG-03` | |

**Estados de la llamada**

| Estado | Cómo se llega | Quién lo cambia | A qué estados puede pasar |
|---|---|---|---|
| `en_ivr` | Entra la llamada | Plataforma | `en_cola`, `abandonada` |
| `en_cola` | Termina el IVR | Plataforma | `en_curso`, `abandonada` |
| `en_curso` | El agente contesta | Plataforma | `finalizada` |
| `finalizada` | Alguien cuelga | Plataforma | Ninguno |
| `abandonada` | El paciente cuelga antes de que conteste un agente | Plataforma | Ninguno |

### Paciente

Se guarda una copia de lo que respondió Everest en el momento de la llamada. Everest sigue siendo la fuente.

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo | Notas |
|---|---|---|---|---|---|
| tipo_documento | texto | Sí | `CC`, `TI`, `CE`, `RC`, `PA`, `PPT` | `CC` | Valores por validar con Everest |
| numero_documento | texto | Sí | 5 a 12 dígitos | `1000000001` | |
| nombre_completo | texto | Sí | Hasta 120 caracteres | `María Fernanda Ruiz Pérez` | |
| correo | texto | No | Correo válido | `mfruiz@correo.com` | |
| telefono_alterno | texto | No | 10 dígitos | `3104567890` | |
| sede | texto | No | Sedes de Everest | `Sede Norte` | |
| eps | texto | Sí | Texto de Everest | `Nueva EPS` | |
| derechos | texto | Sí | `vigentes`, `no_vigentes`, `sin_validar` | `vigentes` | `sin_validar` si Everest no respondió |
| consultado_en | fecha-hora | Sí | ISO 8601 | `2026-11-20T09:14:06-05:00` | |

### Gestión

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo | Notas |
|---|---|---|---|---|---|
| id_gestion | texto | Sí | UUID | `c1d9e2f3-…` | |
| id_llamada | texto | Sí | UUID de una llamada `finalizada` | `7f3c2a10-…` | Una sola gestión por llamada |
| tipo_documento, numero_documento | texto | Sí | Igual que Paciente | `CC`, `1000000001` | |
| nombre_completo | texto | Sí | Igual que Paciente | `María Fernanda Ruiz Pérez` | |
| correo | texto | No | Correo válido | `mfruiz@correo.com` | |
| telefono_alterno | texto | No | 10 dígitos | `3104567890` | |
| tipificacion_n1 | texto | Sí | Catálogo | `ASIGNACION` | |
| tipificacion_n2 | texto | Según N1 | Catálogo | `CITA ASIGNADA` | |
| id_cita | texto | No | Código de LUMIER | `LUM-884213` | Solo si se agendó |
| tmo_segundos | entero | Sí | ≥ 0 | `231` | hora_fin − hora_contesta; no editable |
| id_grabacion | texto | Sí | Identificador de archivo | `rec-7f3c2a10.wav` | |
| datos_corregidos | lista | No | Campo, valor anterior, valor nuevo, motivo | `[{"campo":"correo","anterior":"mfruiz@coreo.com","nuevo":"mfruiz@correo.com","motivo":"Error de digitación en Everest"}]` | |
| registrada_en | fecha-hora | Sí | ISO 8601 | `2026-11-20T09:18:24-05:00` | ≤ 3 s después de hora_fin |
| estado | texto | Sí | `borrador`, `registrada` | `registrada` | |
| historial | lista | No | Autor, hora, cambio, motivo | Ver RN-10 | Correcciones del supervisor |

**Estados de la gestión**

| Estado | Cómo se llega | Quién lo cambia | A qué estados puede pasar |
|---|---|---|---|
| `borrador` | El agente contesta | Plataforma | `registrada` |
| `registrada` | El agente tipifica y la llamada termina | Plataforma | Ninguno; las correcciones se agregan al historial |

### Cita

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo | Notas |
|---|---|---|---|---|---|
| id_cita | texto | Sí | Código que devuelve LUMIER | `LUM-884213` | |
| examen | texto | Sí | Nombre del examen en LUMIER | `Ecografía abdominal total` | Estructura exacta por validar con LUMIER |
| codigo_cups | texto | No | 6 caracteres | `881302` | Si LUMIER lo expone |
| sede | texto | Sí | Sedes de LUMIER | `Sede Norte` | |
| fecha_hora | fecha-hora | Sí | ISO 8601 | `2026-11-27T07:40:00-05:00` | |

### Tipificación

Catálogo cargado por el administrador. Nivel 1 de partida, tomado de la tipificación de septiembre: `ASIGNACION`, `INFORMACION`, `ESCALAMIENTO - NO CITA`, `CANCELACION`, `CONFIRMACION`, `PQR`, `NO CONTACTO`, `OTROS`. El Nivel 2 de NEPS Imagenología está por validar con el supervisor.

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo |
|---|---|---|---|---|
| nivel_1 | texto | Sí | Mayúsculas | `ASIGNACION` |
| nivel_2 | texto | No | Mayúsculas | `CITA ASIGNADA` |
| exige_cita | booleano | Sí | `true`, `false` | `true` |
| activa | booleano | Sí | `true`, `false` | `true` |

### Grabación

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo |
|---|---|---|---|---|
| id_grabacion | texto | Sí | Identificador de archivo | `rec-7f3c2a10.wav` |
| id_llamada | texto | Sí | UUID | `7f3c2a10-…` |
| duracion_segundos | entero | Sí | ≥ 0 | `231` |
| ubicacion | texto | Sí | Ruta en almacenamiento de Azure con acceso restringido | `grabaciones/2026/11/20/rec-7f3c2a10.wav` |

### Usuario

| Campo | Tipo | Obligatorio | Valores permitidos / formato | Ejemplo |
|---|---|---|---|---|
| id_usuario | texto | Sí | Código | `AG-03` |
| nombre | texto | Sí | Hasta 80 caracteres | `Agente Prueba Tres` |
| rol | texto | Sí | `agente`, `supervisor`, `administrador` | `agente` |
| estado_agente | texto | Solo agentes | `disponible`, `en_llamada`, `pausa`, `desconectado` | `disponible` |

**Relaciones**: una Llamada tiene cero o una Gestión y cero o una Grabación; una Gestión pertenece a una Llamada, tiene una Tipificación y cero o una Cita; un Usuario agente tiene muchas Llamadas.

## 9. Reglas de negocio

- **RN-1**: Toda llamada en estado `finalizada` tiene exactamente una gestión.
- **RN-2**: La gestión queda en estado `registrada` a más tardar 3 segundos después de `hora_fin`. Si no, la plataforma deja un registro de incumplimiento visible para el supervisor.
- **RN-3**: El documento capturado por el IVR llega precargado; el agente no lo vuelve a digitar. Solo puede corregirlo, y toda corrección queda en `datos_corregidos` con motivo.
- **RN-4**: No se puede agendar una cita si `derechos` es distinto de `vigentes`.
- **RN-5**: Ninguna gestión queda `registrada` sin `tipificacion_n1`. Si la causal elegida tiene `exige_cita = true`, la gestión necesita `id_cita`.
- **RN-6**: Toda llamada que llega a un agente pasó antes por el aviso de grabación del IVR.
- **RN-7**: Si Everest o LUMIER no responden, la plataforma no completa datos por su cuenta: deja los campos vacíos y marca `derechos = sin_validar`.
- **RN-8**: `tmo_segundos` y `segundos_espera` los calcula la plataforma y nadie los puede editar.
- **RN-9**: Una gestión `registrada` no se borra. Las correcciones se agregan al historial con autor, hora, valor anterior, valor nuevo y motivo.
- **RN-10**: Un agente solo ve las gestiones que él atendió; las grabaciones solo las escuchan el supervisor y el administrador.
- **RN-11**: El nivel de servicio del día es igual a llamadas contestadas dentro del umbral sobre llamadas ofrecidas, contando las abandonadas. El umbral en segundos está por validar con gerencia.
- **RN-12**: En el prototipo solo se usan pacientes de los ambientes de prueba de Everest y LUMIER. Ningún dato real de pacientes entra a la plataforma.

## 10. Capacidades y criterios de aceptación

### C1 · Recibir la llamada en el IVR

**Qué permite hacer**: recibir la llamada, avisar la grabación y capturar el documento.
**Actor**: plataforma. **Prioridad**: MVP.
**Entrada**: llamada al número de prueba. **Salida**: Llamada en estado `en_cola` con `documento_ivr`.

- [ ] **CA-1.1** Dado que entra una llamada al número de prueba, cuando el paciente marca su documento y la tecla numeral, entonces la llamada pasa a `en_cola` con `documento_ivr` igual a los dígitos marcados.
- [ ] **CA-1.2** Dado que el paciente no marca nada en 10 segundos, cuando se agotan 2 intentos, entonces la llamada pasa a `en_cola` con `documento_ivr` vacío.
- [ ] **CA-1.3** Dado cualquier llamada, cuando llega a un agente, entonces el registro de la llamada muestra que se reprodujo el aviso de grabación.

### C2 · Asignar la llamada a un agente

**Qué permite hacer**: entregar cada llamada al agente disponible que lleva más tiempo libre.
**Actor**: plataforma. **Prioridad**: MVP.
**Entrada**: Llamada `en_cola`. **Salida**: Llamada `en_curso` con `id_agente`.

- [ ] **CA-2.1** Dado que AG-01 lleva 40 segundos disponible y AG-02 lleva 10, cuando entra una llamada, entonces suena primero en la consola de AG-01.
- [ ] **CA-2.2** Dado que ningún agente está disponible, cuando entra una llamada, entonces espera en cola con música; si el paciente cuelga, la llamada queda `abandonada` con sus segundos de espera.

### C3 · Mostrar el paciente al contestar

**Qué permite hacer**: que el agente vea los datos del paciente sin digitar el documento.
**Actor**: agente. **Prioridad**: MVP.
**Entrada**: `documento_ivr`. **Salida**: datos del Paciente en la consola.

- [ ] **CA-3.1** Dado un documento de prueba con derechos vigentes en Everest, cuando el agente contesta, entonces la consola muestra en menos de 2 segundos nombre, documento, sede, EPS, correo, teléfono alternativo y "Derechos vigentes".
- [ ] **CA-3.2** Dado un documento que no existe en Everest, cuando el agente contesta, entonces la consola muestra "Paciente no encontrado" y un campo para escribir el documento correcto; al escribirlo, consulta de nuevo.
- [ ] **CA-3.3** Dado que Everest no responde en 4 segundos tras 2 reintentos, cuando el agente contesta, entonces la consola muestra "Derechos sin validar", bloquea el agendamiento y permite tipificar.

### C4 · Agendar en LUMIER

**Qué permite hacer**: consultar cupos y crear la cita desde la consola.
**Actor**: agente. **Prioridad**: MVP.
**Entrada**: examen, sede y fecha preferida. **Salida**: Cita con `id_cita`.

- [ ] **CA-4.1** Dado un paciente con derechos vigentes, cuando el agente elige examen y sede, entonces la consola lista los cupos disponibles de LUMIER y, al elegir uno, muestra el `id_cita` devuelto por LUMIER.
- [ ] **CA-4.2** Dado un paciente con derechos `no_vigentes`, cuando el agente intenta agendar, entonces el botón está deshabilitado y la consola muestra "No se puede agendar: derechos no vigentes; remitir a la EPS".
- [ ] **CA-4.3** Dado que LUMIER no responde, cuando el agente consulta cupos, entonces la consola muestra "LUMIER no disponible" y permite cerrar la gestión con la causal `ESCALAMIENTO - NO CITA`.

### C5 · Registrar la gestión una sola vez

**Qué permite hacer**: dejar la gestión completa sin digitarla en otro sistema.
**Actor**: agente y plataforma. **Prioridad**: MVP.
**Entrada**: datos del paciente, cita, tipificación, tiempos y grabación. **Salida**: Gestión `registrada`.

- [ ] **CA-5.1** Dado que el agente eligió la tipificación, cuando la llamada termina, entonces la gestión queda `registrada` en ≤ 3 segundos con todos los campos de la sección 8.
- [ ] **CA-5.2** Dado que el agente no eligió tipificación de Nivel 1, cuando intenta cerrar, entonces la consola no cierra y resalta el campo.
- [ ] **CA-5.3** Dado que la base de datos falla al guardar, cuando la plataforma reintenta 3 veces sin éxito, entonces guarda la gestión en una cola local, avisa al supervisor y no crea un duplicado al recuperarse.

### C6 · Grabar la llamada

**Qué permite hacer**: guardar el audio y asociarlo a la gestión.
**Actor**: plataforma. **Prioridad**: MVP.
**Entrada**: audio de la llamada. **Salida**: Grabación con `id_grabacion` en la gestión.

- [ ] **CA-6.1** Dado una llamada atendida, cuando termina, entonces la gestión tiene un `id_grabacion` cuya duración coincide con `tmo_segundos` con diferencia máxima de 2 segundos.
- [ ] **CA-6.2** Dado un usuario con rol `agente`, cuando intenta abrir una grabación, entonces la plataforma responde "Sin permiso".

### C7 · Vista de supervisor

**Qué permite hacer**: ver la operación del día y corregir tipificaciones.
**Actor**: supervisor. **Prioridad**: MVP.
**Entrada**: llamadas y gestiones del día. **Salida**: indicadores y correcciones.

- [ ] **CA-7.1** Dado que hay llamadas en el día, cuando el supervisor abre la vista, entonces ve el estado de cada agente, las llamadas en cola, atendidas, abandonadas y el nivel de servicio, actualizados cada 5 segundos.
- [ ] **CA-7.2** Dado que no hay llamadas en el día, cuando abre la vista, entonces ve "Sin llamadas hoy" y los indicadores en cero.
- [ ] **CA-7.3** Dado una gestión registrada, cuando el supervisor cambia la tipificación sin escribir motivo, entonces la plataforma no guarda el cambio.

### C8 · Exportar gestiones

**Qué permite hacer**: descargar las gestiones de un rango de fechas.
**Actor**: supervisor. **Prioridad**: MVP.
**Entrada**: fecha inicial y final. **Salida**: archivo CSV.

- [ ] **CA-8.1** Dado un rango con gestiones, cuando el supervisor exporta, entonces descarga un CSV en UTF-8 con una fila por gestión y las columnas: fecha, agente, documento, nombre, correo, teléfono alternativo, tipificación N1, tipificación N2, id_cita, TMO.
- [ ] **CA-8.2** Dado un rango sin gestiones, cuando exporta, entonces la plataforma muestra "No hay gestiones en ese rango" y no descarga un archivo vacío.

## 11. Módulos de arquetipo

### Módulo · RPA

#### Sistemas y accesos

| Sistema | Para qué se usa | Tipo de acceso | Disponible en v1 | Si no, qué se simula |
|---|---|---|---|---|
| Everest (ambiente de pruebas) | Paciente y validación de derechos | API, si existe; si no, automatización sobre pantallas [Por validar con Everest] | Sí | Tabla con los campos de Paciente |
| LUMIER (ambiente de pruebas del cliente) | Cupos y creación de cita | Por validar con el administrador de LUMIER | Sí, con acceso de prueba | Tabla de cupos con los campos de Cita, mientras llega el acceso |
| Telefonía de prueba | Número, IVR, cola y grabación | Componente en Azure, ver sección 13 | Sí | — |
| uContact | — | No se usa en la línea piloto | No aplica | — |

#### Disparador

Cada evento de la llamada: entra al IVR, el agente contesta y la llamada termina. Funciona mientras la línea esté abierta. Si la plataforma está caída, la línea de prueba no recibe llamadas y la línea real sigue en uContact.

#### Secuencia

| # | Sistema | Acción | Dato que usa | Validación posterior |
|---|---|---|---|---|
| 1 | Telefonía | Captura el documento | Dígitos marcados | 5 a 12 dígitos; si no, `documento_ivr` vacío |
| 2 | Everest | Consulta paciente y derechos | `documento_ivr` | La respuesta trae `derechos`; si no, `sin_validar` |
| 3 | Plataforma | Prepara la gestión en `borrador` | Respuesta de Everest | Gestión creada con `id_llamada` |
| 4 | LUMIER | Consulta cupos | Examen y sede | Lista de cupos o mensaje de no disponible |
| 5 | LUMIER | Crea la cita | Cupo elegido y documento | LUMIER devuelve `id_cita` |
| 6 | Plataforma | Registra la gestión | Todos los campos | Estado `registrada` y `registrada_en` ≤ 3 s después de `hora_fin` |

#### Excepciones

| Excepción | Cómo se detecta | Qué hace la plataforma | A quién notifica |
|---|---|---|---|
| Documento vacío o inválido | Validación del paso 1 | Pide el documento en la consola | Agente |
| Paciente no encontrado | Everest responde sin datos | "Paciente no encontrado" y campo para corregir | Agente |
| Everest no responde | Sin respuesta en 4 s tras 2 reintentos | `derechos = sin_validar` y bloquea agendamiento | Agente y supervisor |
| LUMIER no responde | Sin respuesta en 4 s tras 2 reintentos | Permite cerrar con `ESCALAMIENTO - NO CITA` | Agente y supervisor |
| Cupo tomado por otro | LUMIER rechaza la creación | Recarga los cupos | Agente |
| Falla al guardar la gestión | Error de base de datos | 3 reintentos y cola local | Supervisor |

#### Reglas de ejecución

- **Idempotencia**: la gestión se identifica por `id_llamada`. Si el registro corre dos veces, actualiza la misma gestión y no crea otra. La creación de cita en LUMIER se hace una sola vez por clic y el botón se deshabilita hasta recibir respuesta.
- **Reintentos**: 2 reintentos con 1 segundo de espera para Everest y LUMIER; 3 reintentos para guardar la gestión. Después, se rinde y aplica la excepción.
- **Evidencia**: por cada llamada queda un registro técnico con la hora de cada paso, la respuesta de cada sistema y los errores, guardado en Azure.

### Módulo · Aplicación web

#### Mapa de pantallas

| Ruta | Pantalla | Propósito | Roles con acceso |
|---|---|---|---|
| `/ingreso` | Ingreso | Iniciar sesión | Todos |
| `/consola` | Consola del agente | Atender, agendar y tipificar | Agente |
| `/supervisor` | Vista de supervisor | Ver la operación del día | Supervisor |
| `/gestiones` | Gestiones | Buscar, corregir y exportar | Supervisor; agente solo las suyas |
| `/administracion` | Administración | Usuarios, catálogo y mensajes del IVR | Administrador |

#### Detalle por pantalla

**`/consola`**
- **Datos que muestra**: estado del agente; en llamada, el teléfono de origen, el cronómetro y los datos del Paciente; el panel de cupos de LUMIER; la lista de tipificación.
- **Acciones**: cambiar estado (disponible o pausa), contestar, colgar, corregir un dato con motivo, consultar cupos, agendar, tipificar.
- **Estado vacío**: "Esperando llamadas" con el estado del agente.
- **Estado de error**: mensajes de las excepciones del módulo RPA en una franja visible, sin cerrar la llamada.
- **Validaciones**: tipificación de Nivel 1 obligatoria; motivo obligatorio al corregir; cita obligatoria si la causal la exige.

**`/supervisor`**
- **Datos que muestra**: agentes con su estado y tiempo en estado; llamadas en cola y la espera más larga; atendidas, abandonadas y nivel de servicio del día; gestiones que incumplieron los 3 segundos.
- **Acciones**: abrir una gestión.
- **Estado vacío**: "Sin llamadas hoy".
- **Estado de error**: "No se pudo actualizar; último dato hace N segundos".

**`/gestiones`**
- **Datos que muestra**: tabla con fecha, agente, documento, nombre, tipificación, cita y TMO; detalle con grabación e historial.
- **Acciones**: filtrar por fecha, agente y tipificación; corregir la tipificación con motivo; escuchar la grabación; exportar.
- **Estado vacío**: "No hay gestiones con esos filtros".
- **Estado de error**: "No se pudieron cargar las gestiones; intenta de nuevo".

**`/administracion`**
- **Datos que muestra**: usuarios, catálogo de tipificación y textos del IVR.
- **Acciones**: crear o desactivar usuarios, editar el catálogo, cambiar los textos del IVR.
- **Estado vacío**: "No hay usuarios creados".
- **Estado de error**: mensaje del campo que no pasó la validación.

#### Permisos

| Pantalla o acción | Agente | Supervisor | Administrador |
|---|---|---|---|
| Consola y atender llamadas | Sí | No | No |
| Ver vista de supervisor | No | Sí | Sí |
| Ver gestiones propias | Sí | Sí | Sí |
| Ver gestiones de otros | No | Sí | Sí |
| Corregir tipificación registrada | No | Sí | No |
| Escuchar grabaciones | No | Sí | Sí |
| Exportar | No | Sí | Sí |
| Administración | No | No | Sí |

#### Datos semilla

Los descritos en la sección 12, cargados antes de la primera prueba para que la consola y la vista de supervisor se puedan recorrer desde el primer minuto.

## 12. Datos, datos semilla y gobierno de la decisión

| Dato o documento | Origen | Sensibilidad | Qué se usa en v1 |
|---|---|---|---|
| Datos de paciente | Everest, ambiente de pruebas | Alta: datos de salud, Ley 1581 | Solo pacientes de prueba |
| Citas y cupos | LUMIER, ambiente de pruebas | Alta | Solo cupos de prueba |
| Grabaciones | Plataforma | Alta | Solo llamadas de prueba |
| Catálogo de tipificación | Tipificación de septiembre de 2026 | Baja | Nivel 1 completo; Nivel 2 por validar |
| Usuarios | Linkai | Media | 5 agentes, 1 supervisor, 2 administradores |

**Datos semilla**:
- 30 pacientes en el ambiente de prueba de Everest [Por validar con Everest]: 24 con derechos vigentes, 4 sin derechos y 2 con correo o teléfono alternativo vacío. Además, 2 documentos que no existen en Everest para probar el error.
- Cupos de prueba en LUMIER para al menos 3 exámenes y 2 sedes durante las 2 semanas siguientes.
- El catálogo de tipificación de la sección 8.
- 8 usuarios: `AG-01` a `AG-05`, `SUP-01`, `ADM-01` y `ADM-02`.
- 20 gestiones de ejemplo de días anteriores, para que la búsqueda y la exportación tengan contenido.

**Gobierno**:
- **Qué decide la solución**: a qué agente va la llamada y si se puede agendar, según los derechos que responde Everest.
- **Qué decide una persona**: la cita, la tipificación y la corrección de datos (agente); la corrección de tipificaciones registradas (supervisor).
- **Cómo queda registrada la decisión humana**: cada gestión guarda el agente que la registró; cada corrección guarda autor, hora, valor anterior, valor nuevo y motivo.
- **Qué pasa sin certeza**: si Everest no confirma derechos, no se agenda y la gestión queda con `derechos = sin_validar`.
- **Cómo se revierte un error**: el supervisor corrige la tipificación con motivo; una cita mal creada se cancela en LUMIER y se registra como corrección.

## 13. Restricciones de construcción

- **Dónde corre**: servidor en Azure; los agentes y el supervisor usan un navegador con micrófono y audífonos.
- **Persistencia**: base de datos administrada en Azure; motor a elección del equipo. Grabaciones en almacenamiento de Azure con acceso restringido por rol.
- **Telefonía**: componentes existentes desplegados en Azure, sin programar una central desde cero. Opciones a decidir en la semana 1 con Everest: Asterisk o FreeSWITCH en una máquina de Azure con troncal SIP de prueba, o Azure Communication Services con número de prueba [Por validar disponibilidad de números para Colombia]. Teléfono del agente en el navegador con WebRTC.
- **Integraciones permitidas en v1**: telefonía de prueba, Everest en pruebas y LUMIER en pruebas. Ninguna con uContact.
- **Lo que se simula**: nada, si los accesos llegan a tiempo. Si LUMIER tarda, una tabla de cupos con los campos de Cita, reemplazable sin cambiar la consola.
- **Idioma**: interfaz, mensajes del IVR y datos en español.
- **Restricciones del equipo**: nube Azure; lenguaje sin restricción; construyen Everest y el grupo del bootcamp.

## 14. Plan de construcción y guion de demo

**Decisión del Sponsor (5 de noviembre de 2026)**: en la sesión del bootcamp se construye una versión local, que corre en un computador, con la llamada, Everest y LUMIER simulados. Sirve para demostrar el registro único de principio a fin: el paciente aparece solo al contestar, la cita se agenda desde la consola y la gestión se guarda una sola vez. La telefonía en Azure y las conexiones a los ambientes de prueba de Everest y LUMIER no cambian: siguen el plan de 4 semanas de la tabla de abajo. Cada sistema simulado se construye por separado, para reemplazarlo luego por el real sin tocar la consola.

| # | Hito | Qué queda funcionando | Criterios que cubre |
|---|---|---|---|
| 1 (semana 1) | Llamada a la consola | Número de prueba, aviso de grabación, captura del documento, cola y llamada sonando en la consola de un agente | CA-1.1 a 1.3, CA-2.1, CA-2.2 |
| 2 (semana 2) | Paciente precargado | Consulta a Everest en pruebas y datos visibles al contestar; gestión en `borrador` | CA-3.1 a 3.3 |
| 3 (semana 3) | Registro único | Cupos y cita en LUMIER, tipificación, TMO, grabación y gestión `registrada` en ≤ 3 s | CA-4.1 a 4.3, CA-5.1 a 5.3, CA-6.1, CA-6.2 |
| 4 (semana 4) | Supervisor y prueba con agentes | Vista de supervisor, gestiones, exportación; los 5 agentes hacen llamadas de prueba y se miden los segundos | CA-7.1 a 7.3, CA-8.1, CA-8.2 |

**Guion de demo**:
1. Una persona llama al número de prueba → escucha el aviso de grabación y marca el documento `1000000001`.
2. Suena la consola de AG-01 → al contestar aparecen nombre, sede, EPS, correo, teléfono alternativo y "Derechos vigentes", sin digitar nada.
3. El agente elige "Ecografía abdominal total" y "Sede Norte" → aparecen los cupos de LUMIER; elige uno y aparece el `id_cita`.
4. El agente tipifica `ASIGNACION / CITA ASIGNADA` y cuelga → en la vista de supervisor aparece la gestión registrada antes de 3 segundos, con TMO y grabación.
5. Caso de error: llama alguien con un documento de prueba sin derechos → la consola muestra "Derechos no vigentes", el botón de agendar está deshabilitado y el agente tipifica la remisión a la EPS.
6. Caso de error: se desconecta el ambiente de Everest → la consola muestra "Derechos sin validar" y la gestión igual se registra.
7. El supervisor exporta las gestiones del día → descarga el CSV con las filas de la demo.
8. Cierre: se muestra que la línea de prueba funcionó sin uContact y que ningún dato se digitó dos veces.

## 15. Métricas de éxito

- **Segundos de doble registro y redigitación por llamada**: 45 [EST] → 0, al final del prototipo, medido con cronómetro en las llamadas de prueba por calidad.
- **Gestiones registradas en ≤ 3 segundos después de colgar**: Por medir → 100% de las llamadas de prueba, medido con `registrada_en − hora_fin`.
- **Llamadas de prueba completadas de principio a fin sin uContact**: 0 → al menos 50 entre los 5 agentes durante la semana 4.
- **TMO de las llamadas de prueba de asignación**: 3,9 minutos (Imagenología, septiembre de 2026) → menor, como referencia, medido por la plataforma.

## 16. Riesgos

- **Riesgo (dependencia)**: los accesos de prueba a Everest o LUMIER llegan tarde. *Mitigación*: pedirlos el primer día; avanzar con tablas con la misma estructura y conectar cuando lleguen.
- **Riesgo (telefonía)**: conseguir número o troncal de prueba en Colombia puede tomar más de una semana. *Mitigación*: decidir el componente de telefonía en los primeros 2 días; si hace falta, usar un número de prueba de otro país solo para la demo.
- **Riesgo (sobrestimar lo que resuelve)**: el piloto demuestra el flujo en una línea con tráfico de prueba, no que la plataforma aguante 172 agentes y 286.000 llamadas al mes, ni que el nivel de servicio suba a 80%. *Mitigación*: presentar el resultado como prueba de concepto y planear una prueba con tráfico real con desborde a uContact.
- **Riesgo (cumplimiento)**: grabaciones y datos de salud en una plataforma nueva. *Mitigación*: solo datos de prueba en v1 (RN-12), acceso por rol y aviso de grabación (RN-6).
