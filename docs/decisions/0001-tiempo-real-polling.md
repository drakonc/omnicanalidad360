# 0001 · El tiempo real se construye preguntando, no esperando avisos

Fecha: 5 de noviembre de 2026
Estado: aceptada

## Contexto

La consola del agente y la vista de supervisor tienen que mostrar lo que está
pasando ahora mismo: quién está en llamada, cuántas esperan en cola, qué
llamada está sonando. Hay dos formas de lograrlo:

- **Preguntar cada cierto tiempo** ("¿hay algo nuevo?"), una y otra vez.
- **Esperar un aviso** del servidor cuando algo cambia (websocket).

Lo segundo suena mejor y es lo que uno elegiría por instinto.

## Lo que mostró el análisis de uContact

El análisis del tráfico de red de uContact
(`uContact-arquitectura.md`, capturado el 22 de octubre de 2026) encontró que
la plataforma que hoy usa Linkai hace las dos cosas, pero no por igual:

- Su dashboard pregunta constantemente: en una sesión de 15 a 20 minutos se
  contaron **492 consultas** de estado de campaña, 338 de grupos y 318 de
  agentes en línea.
- Su canal de avisos en vivo **está roto en producción**: se registraron 89
  intentos fallidos de conexión, porque el servidor no estaba bien configurado.
  Además, el tenant lo tiene apagado por configuración
  (`AgentStatusViaWebsocket: 0`).

Y aun así la plataforma funciona, y los agentes trabajan todos los días en
ella. Nadie notó que el canal en vivo llevaba meses caído, porque preguntar
cada cierto tiempo lo suplía en silencio.

## Decisión

**Preguntar es la base; el aviso en vivo será una mejora encima, nunca al
revés.**

En concreto:

1. Las pantallas le piden el estado a la plataforma cada segundo, a través de
   una sola pieza (`nucleo/sincronizacion.ts`). Ninguna pantalla maneja su
   propio temporizador.
2. Si la plataforma deja de responder, la pantalla **conserva el último dato
   bueno** en vez de quedarse en blanco, y se recupera sola cuando vuelve.
3. El dato viejo **se ve**: un indicador dice "En vivo" o "No se pudo
   actualizar; último dato hace N segundos". Esto es justo lo que le faltó a
   uContact para darse cuenta de que su canal estaba caído, y es además lo que
   el PRD pide textualmente para la vista de supervisor.
4. El día que se agregue un canal de avisos en vivo, entra por debajo de esa
   misma pieza y las pantallas no se tocan.

## Lo que esto nos cuesta

Preguntar cada segundo genera más tráfico que esperar un aviso. En el piloto
es irrelevante: 5 agentes y un supervisor en una red local. Si algún día la
plataforma crece a los 172 agentes de Linkai, habrá que revisar el intervalo o
agregar el canal de avisos; el diseño ya deja el espacio para hacerlo sin
reescribir las pantallas.

## Lo que NO tomamos de uContact

- **Credenciales en las respuestas**: uno de sus endpoints devolvía al
  navegador el usuario y la contraseña de sus bases de datos en texto plano.
  Nuestras respuestas nunca llevan credenciales.
- **Separar base operativa de base de reportes**: es un buen patrón suyo, pero
  no aplica al piloto, que corre sobre una sola base local.
- **No reinventar la central telefónica**: aplica al backend de las 4 semanas,
  no al frontend. Queda anotado para esa conversación.
